/*
  Created by:
  Juan Sebastian Munoz
  naruse@gmail.com
  All rights reserved

 */
namespace ProDrawCall {
	using UnityEngine;
	using UnityEditor;
	using System;
	using System.IO;
	using System.Collections;
	using System.Collections.Generic;

	public static class MaterialCombiner {

	    private static List<Texture2D> texturesToAtlas;
	    private static List<Vector2> scales;//used for tiling of the textures
	    private static List<Vector2> offsets;//used for tiling of the textures.


	    //objToCombine should be correctly assembled,meaning has MeshRenderer,filter and shares the same type of shader across materials
	    //combines the number of materials in the mesh renderer, not the submesh count.
	    public static GameObject CombineMaterials(GameObject objToCombine, string shaderUsed) {
	        List<string> shaderDefines = ShaderManager.Instance.GetShaderTexturesDefines(shaderUsed);
	        Material[] materialsToCombine = objToCombine.GetComponent<MeshRenderer>().sharedMaterials;
	        GetTexturesScalesAndOffsetsForShaderDefine(objToCombine, shaderDefines[0]);

	        int atlasSize = CalculateAproxAtlasSizeForMaterials(objToCombine, shaderUsed);
	        Atlasser atlas = new Atlasser(atlasSize, atlasSize);
	        // generate atlas for the initial textures
	        int resizeTimes = 1;
	        for(int i = 0; i < texturesToAtlas.Count; i++) {
	            Node resultNode = atlas.Insert(texturesToAtlas[i].width, texturesToAtlas[i].height);
	            if(resultNode == null) {
	                int resizedAtlasSize = atlasSize + Mathf.RoundToInt((float)atlasSize * Constants.AtlasResizeFactor * resizeTimes);
	                atlas = new Atlasser(resizedAtlasSize, resizedAtlasSize);
	                i = -1;//at the end of the loop 1 will be added and it will start in 0
	                resizeTimes++;
	            }
	        }

	        //with the generated atlas, save the textures and load them and add them to the combinedMaterial
	        string pathToAtlas = CreateFolderForCombinedObject(objToCombine);
	        string fileName = "MaterialAtlas " + shaderUsed.Replace('/','_');
	        string atlasTexturePath = pathToAtlas + Path.DirectorySeparatorChar + fileName;

	        //create material and fill with the combined to be textures in the material
	        Material combinedMaterial = new Material(Shader.Find(shaderUsed));
	        AssetDatabase.CreateAsset(combinedMaterial, atlasTexturePath + "Mat.mat");
            AssetDatabase.ImportAsset(atlasTexturePath + "Mat.mat");
	        //AssetDatabase.Refresh();
	        combinedMaterial = (Material) AssetDatabase.LoadAssetAtPath(atlasTexturePath + "Mat.mat", typeof(Material));

	        for(int i = 0; i < shaderDefines.Count; i++) {
	            GetTexturesScalesAndOffsetsForShaderDefine(objToCombine, shaderDefines[i]);
//				Debug.Log ("Creating atlas from multiple materials on gameobject");
				atlas.SaveAtlasToFile(atlasTexturePath + i.ToString() + ".png", texturesToAtlas, scales, offsets);
                AssetDatabase.ImportAsset(atlasTexturePath + i.ToString() + ".png");
	            //AssetDatabase.Refresh();
	            Texture2D savedAtlasTexture = (Texture2D) AssetDatabase.LoadAssetAtPath(atlasTexturePath + i.ToString() + ".png", typeof(Texture2D));
	            combinedMaterial.SetTexture(shaderDefines[i], savedAtlasTexture);
	        }
	        //AssetDatabase.Refresh();

	        Mesh masterMesh = objToCombine.GetComponent<MeshFilter>().sharedMesh;
	        Mesh[] subMeshes = new Mesh[materialsToCombine.Length];
	        for(int i = 0; i < subMeshes.Length; i++) {
	            subMeshes[i] = ExtractMesh(masterMesh, i);
	            Vector2[] remappedUVs = subMeshes[i].uv;
	            for(int j = 0; j < remappedUVs.Length; j++) {
	                remappedUVs[j] = Utils.ReMapUV(remappedUVs[j], atlas.AtlasWidth, atlas.AtlasHeight, atlas.TexturePositions[i], objToCombine.name);
	            }
	            subMeshes[i].uv = remappedUVs;
	        }
	        GameObject combinedObj = GameObject.Instantiate(objToCombine,
                                                            objToCombine.transform.position,
                                                            objToCombine.transform.rotation) as GameObject;
	        combinedObj.GetComponent<MeshRenderer>().sharedMaterials = new Material[] { combinedMaterial };
	        combinedObj.GetComponent<MeshFilter>().sharedMesh = Utils.CombineMeshes(subMeshes);

            combinedObj.transform.parent = objToCombine.transform.parent;
            combinedObj.transform.localScale = objToCombine.transform.localScale;
	        combinedObj.name = objToCombine.name;
	        return combinedObj;
	    }

	    //creates a folder where the scene resides and then creates a subfolder with the obj name and InstanceID
	    //returns the created path
	    private static string CreateFolderForCombinedObject(GameObject g) {
			string folderToSaveAssets = EditorApplication.currentScene;
			if(folderToSaveAssets == "") { //scene is not saved yet.
				folderToSaveAssets = Constants.NonSavedSceneFolderName + ".unity";
				Debug.LogWarning("WARNING: Scene has not been saved, saving baked objects to: " + Constants.NonSavedSceneFolderName + " folder");
			}
	        string path = folderToSaveAssets.Substring(0, folderToSaveAssets.Length-6) + "-Atlas";//rm .unity
	        if(!Directory.Exists(path)) {
	            Directory.CreateDirectory(path);
	            AssetDatabase.ImportAsset(path);
                //AssetDatabase.Refresh();
	        }
	        //create specific directory for the combined obj
	        path += Path.DirectorySeparatorChar + g.name + g.GetInstanceID();
	        if(!Directory.Exists(path)) {
	            Directory.CreateDirectory(path);
	            AssetDatabase.ImportAsset(path);
                //AssetDatabase.Refresh();
	        }
	        return path;
	    }


	    private static void GetTexturesScalesAndOffsetsForShaderDefine(GameObject obj, string shaderDefine) {
	        Material[] materials = obj.GetComponent<MeshRenderer>().sharedMaterials;
	        texturesToAtlas = new List<Texture2D>();
	        scales = new List<Vector2>();
	        offsets = new List<Vector2>();

	        for(int i = 0; i < materials.Length; i++) {
	            if(materials[i] != null) {
	                Texture2D extractedTexture = materials[i].GetTexture(shaderDefine) as Texture2D;
	                if(extractedTexture) {
	                    texturesToAtlas.Add(extractedTexture);
	                    scales.Add(materials[i].GetTextureScale(shaderDefine));
	                    offsets.Add(materials[i].GetTextureOffset(shaderDefine));
	                } else {//material doesnt have a texture with that define/there is no texture.lets generate a texture with the color.
	                    if(materials[i].HasProperty("_Color"))//check if mat has a color property
	                        texturesToAtlas.Add(Utils.GenerateTexture(materials[i].GetColor("_Color")));
	                    else
	                        texturesToAtlas.Add(Utils.GenerateTexture(Color.white));
	                    scales.Add(Vector2.one);
	                    offsets.Add(Vector2.zero);
	                }
	            } else {
	                //null material, generate a white texture.
	                texturesToAtlas.Add(Utils.GenerateTexture(Color.white));
	                scales.Add(Vector2.one);
	                offsets.Add(Vector2.zero);
	            }
	        }
	    }

	    public static int CalculateAproxAtlasSizeForMaterials(GameObject g, string shaderUsed) {
	        int atlasSize = 0;
            if(shaderUsed == "" || //the game object is not supported
               ShaderManager.Instance.GetShaderTexturesDefines(shaderUsed) == null)//shader is not recognized
                return 0;
	        string shaderTextureDefine = ShaderManager.Instance.GetShaderTexturesDefines(shaderUsed)[0];
	        Material[] materials = g.GetComponent<MeshRenderer>().sharedMaterials;
	        for(int i = 0; i < materials.Length; i++) {
	            if(materials[i] != null) {
	                Texture2D refTexture = materials[i].GetTexture(shaderTextureDefine) as Texture2D;
	                if(refTexture != null)
	                    atlasSize += (refTexture.width * refTexture.height);
	                else
	                    atlasSize += (Constants.NullTextureSize * Constants.NullTextureSize);
	            } else {
	                atlasSize += (Constants.NullTextureSize * Constants.NullTextureSize);
	            }
	        }
	        return Mathf.RoundToInt(Mathf.Sqrt(atlasSize));
	    }


	    private static Mesh ExtractMesh(Mesh masterMesh, int subMeshToExtract) {
	        Dictionary<int, int> indexMap = new Dictionary<int, int>();
	        int[] meshIndices = masterMesh.GetIndices(subMeshToExtract);
	        int counter = 0;
	        //get unique indexes
	        for(int i = 0; i < meshIndices.Length; i++) {
	            if(!indexMap.ContainsKey(meshIndices[i])) {
	                indexMap.Add(meshIndices[i], counter);
	                counter++;
	            }/* else {
	                Debug.LogError("Index exists already! " + meshIndices[i]);
	            }*/
	        }

	        List<Vector3> extractedMeshVertices = new List<Vector3>();
	        List<Vector2> extractedMeshUvs = new List<Vector2>();
	        List<Vector3> extractedMeshNormals = new List<Vector3>();
	        //start filling the vertices,uvs and normals for the acquired indexes
	        foreach(KeyValuePair<int, int> pair in indexMap) {
	            extractedMeshVertices.Add(masterMesh.vertices[pair.Key]);
	            extractedMeshUvs.Add(masterMesh.uv[pair.Key]);
	            extractedMeshNormals.Add(masterMesh.normals[pair.Key]);
	        }

	        int[] subMeshTriangles = masterMesh.GetTriangles(subMeshToExtract);
	        int[] extractedMeshTriangles = new int[subMeshTriangles.Length];
	        for(int i = 0; i < subMeshTriangles.Length; i++) {
	            extractedMeshTriangles[i] = indexMap[subMeshTriangles[i]];
	        }
	        Mesh extractedMesh = new Mesh();
	        extractedMesh.vertices = extractedMeshVertices.ToArray();
	        extractedMesh.uv = extractedMeshUvs.ToArray();
	        extractedMesh.normals = extractedMeshNormals.ToArray();
	        extractedMesh.triangles = extractedMeshTriangles;

	        return extractedMesh;
	    }
	}
}