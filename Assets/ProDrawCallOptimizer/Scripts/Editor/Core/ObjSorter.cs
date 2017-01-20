namespace ProDrawCall {
	/*
	  This class is in charge of sorting game objects depending on the shader.
	  and then sorts them by size;

	  Created by:
	  Juan Sebastian Munoz Arango
	  naruse@gmail.com
	  All rights reserved
	 */
	using UnityEngine;
	using UnityEditor;
	using System.IO;
	using System.Collections;
	using System.Collections.Generic;

	public static class ObjSorter {

	    private static List<string> shadersUsed;
	    private static List<List<OptimizableObject>> sortedObjects;

	    public static void Initialize() {
	        //Debug.Log("Initializing sorter...");
	        shadersUsed = new List<string>();
	        sortedObjects = new List<List<OptimizableObject>>();
	        sortedObjects.Add(new List<OptimizableObject>());
	    }

	    public static void AddObject(GameObject g) {
	        sortedObjects[0].Add(new OptimizableObject(g));
	    }

	    public static void AdjustArraysSize(int newSize) {
	        int totalObjsSize = GetTotalSortedObjects();
	        if(totalObjsSize == newSize) {
	            for(int i = 0; i < sortedObjects.Count; i++)//remove empty lists
	                if(sortedObjects[i].Count == 0)
	                    sortedObjects.RemoveAt(i);
	            return;
	        }
	        int offset = newSize - totalObjsSize;
	        if(offset > 0) {
	            for(int i = 0; i < offset; i++) {
	                sortedObjects[sortedObjects.Count-1].Add(null);
	            }
	        } else {
	            offset *= -1;
	            for(int i = 0; i < offset; i++) {
	                if(sortedObjects[sortedObjects.Count-1].Count > 0) {
	                    sortedObjects[sortedObjects.Count-1].RemoveAt(sortedObjects[sortedObjects.Count-1].Count-1);
	                } else if(sortedObjects[sortedObjects.Count-1].Count == 0) {//array is empty so remove last position
	                    sortedObjects.RemoveAt(sortedObjects.Count-1);
	                }
	            }
	        }
	    }

	    public static void Remove(int index) {
	        sortedObjects.RemoveAt(index);
	        shadersUsed.RemoveAt(index);
	    }

	    public static void RemoveAtPosition(int i, int j) {
	        sortedObjects[i].RemoveAt(j);
	    }


	    private static void Print(List<List<OptimizableObject>> o, string title) {
	        string s = title + ": " + o.Count +"\n";
	        for(int i = 0; i < o.Count; i++) {
	            s+= i+":";
	            for(int j = 0; j < o[i].Count; j++) {
	                s += " " + ((o[i][j] != null) ? (o[i][j].ShaderName == "") ? "NNN" : o[i][j].ShaderName : "NNN");
	            }
	            s+="\n";
	        }
	        Debug.Log(s);
	    }

	    public static void PrintSortedObjs() {
	        Print(sortedObjects, "Sorted Objects");
	    }

	    public static void OptimizeDrawCalls(ref Material atlasMaterial, int shaderIndex, float atlasWidth, float atlasHeight, List<Rect> texturePos, bool reuseTextures, TextureReuseManager texReuseMgr, bool generatePrefabsForObjects) {
	        GameObject trash = new GameObject("Trash");//stores unnecesary objects that might be cloned and are children of objects

			/***** when generating prefabs *****/
			string folderToSavePrefabs = EditorApplication.currentScene;
			if(generatePrefabsForObjects) {
				if(folderToSavePrefabs == "") { //scene is not saved yet.
					folderToSavePrefabs = Constants.NonSavedSceneFolderName + ".unity";
				}
				folderToSavePrefabs = folderToSavePrefabs.Substring(0, folderToSavePrefabs.Length-6) + "-Atlas";//remove the ".unity"
				folderToSavePrefabs += Path.DirectorySeparatorChar + "Prefabs";
				if(!Directory.Exists(folderToSavePrefabs)) {
					Directory.CreateDirectory(folderToSavePrefabs);
					AssetDatabase.Refresh();
				}
			}
			/************************************/

	        for(int i = 0; i < sortedObjects[shaderIndex].Count; i++) {
	            string optimizedObjID = sortedObjects[shaderIndex][i].GameObj.name + Constants.OptimizedObjIdentifier;

	            sortedObjects[shaderIndex][i].GameObj.GetComponent<MeshRenderer>().enabled = true;//activate renderers for instantiating
	            GameObject instance = GameObject.Instantiate(sortedObjects[shaderIndex][i].GameObj,
	                                                         sortedObjects[shaderIndex][i].GameObj.transform.position,
	                                                         sortedObjects[shaderIndex][i].GameObj.transform.rotation) as GameObject;
				Undo.RegisterCreatedObjectUndo(instance,"CreateObj" + optimizedObjID);

	            //remove children of the created instance.
	            Transform[] children = instance.GetComponentsInChildren<Transform>();
	            for(int j = 0; j < children.Length; j++)
	                children[j].transform.parent = trash.transform;

	            instance.transform.parent = sortedObjects[shaderIndex][i].GameObj.transform.parent;
	            instance.transform.localScale = sortedObjects[shaderIndex][i].GameObj.transform.localScale;
	            instance.GetComponent<Renderer>().sharedMaterial = atlasMaterial;
	            instance.name = optimizedObjID;

	            instance.GetComponent<MeshFilter>().sharedMesh = Utils.CopyMesh(sortedObjects[shaderIndex][i].GameObj.GetComponent<MeshFilter>().sharedMesh);

	            //Remap uvs
	            Mesh remappedMesh = instance.GetComponent<MeshFilter>().sharedMesh;
	            Vector2[] remappedUVs = instance.GetComponent<MeshFilter>().sharedMesh.uv;
	            for(int j = 0; j < remappedUVs.Length; j++) {
	                if(reuseTextures) {
	                    remappedUVs[j] = Utils.ReMapUV(remappedUVs[j],
	                                                   atlasWidth,
	                                                   atlasHeight,
	                                                   texReuseMgr.GetTextureRefPosition(sortedObjects[shaderIndex][i]),
	                                                   instance.name);
	                } else {
	                    remappedUVs[j] = Utils.ReMapUV(remappedUVs[j], atlasWidth, atlasHeight, texturePos[i], instance.name);
	                }
	            }
	            remappedMesh.uv = remappedUVs;

	            instance.GetComponent<MeshFilter>().sharedMesh = remappedMesh;

				Undo.RecordObject (sortedObjects[shaderIndex][i].GameObj.GetComponent<MeshRenderer>(), "Active Obj");

	            //if the gameObject has multiple materials, search for the original one (the uncombined) in order to deactivate it
	            if(sortedObjects[shaderIndex][i].ObjWasCombined) {
	                sortedObjects[shaderIndex][i].UncombinedObject.GetComponent<MeshRenderer>().enabled = false;
	            } else {
	                sortedObjects[shaderIndex][i].GameObj.GetComponent<MeshRenderer>().enabled = false;
	            }

				if(generatePrefabsForObjects) {
					string prefabName = Utils.GetValiName(instance.name) + " " + instance.GetInstanceID();
					string  assetPath = folderToSavePrefabs + Path.DirectorySeparatorChar + prefabName;
					AssetDatabase.CreateAsset(instance.GetComponent<MeshFilter>().sharedMesh, assetPath + ".asset");
					PrefabUtility.CreatePrefab(assetPath + ".prefab", instance, ReplacePrefabOptions.ConnectToPrefab);
				}
	        }
	        GameObject.DestroyImmediate(trash);
	    }

	    //this method returns a list of texture2D by the textures defines of the shader of each object.
	    public static List<Texture2D> GetTexturesToAtlasForShaderDefine(int index, string shaderDefine) {
	        List<Texture2D> textures = new List<Texture2D>();
	        for(int i = 0; i < sortedObjects[index].Count; i++) {//for each object lets get the shaderDefine texture.
	            Texture2D texToAdd = ShaderManager.Instance.GetTextureForObjectSpecificShaderDefine(sortedObjects[index][i].GameObj, shaderDefine, true/*if null generate texture*/);
	            textures.Add(texToAdd);
	        }
	        return textures;
	    }

	    public static List<Vector2> GetScalesToAtlasForShaderDefine(int index, string shaderDefine) {
	        List<Vector2> scales = new List<Vector2>();
	        for(int i = 0; i < sortedObjects[index].Count; i++) {//for each object lets get the shaderDefine texture.
	            Vector2 scale = ShaderManager.Instance.GetScaleForObjectSpecificShaderDefine(sortedObjects[index][i].GameObj, shaderDefine);
	            scales.Add(scale);
	        }
	        return scales;
	    }
	    public static List<Vector2> GetOffsetsToAtlasForShaderDefine(int index, string shaderDefine) {
	        List<Vector2> offsets = new List<Vector2>();
	        for(int i = 0; i < sortedObjects[index].Count; i++) {//for each object lets get the shaderDefine texture.
	            Vector2 offset = ShaderManager.Instance.GetOffsetForObjectSpecificShaderDefine(sortedObjects[index][i].GameObj, shaderDefine);
	            offsets.Add(offset);
	        }
	        return offsets;
	    }

	    //NOTE: always the list at sortedObjects[0] contains the not
	    //optimizable objects (which its shader is "") in case there are any
	    public static void SortObjects() {
	        List<List<OptimizableObject>> objs = new List<List<OptimizableObject>>(sortedObjects);
	        sortedObjects.Clear();
	        shadersUsed.Clear();

	        sortedObjects.Add(new List<OptimizableObject>());
	        shadersUsed.Add("");//used for not optimizable objects

	        //organize objects by shader name.
	        for(int i = 0; i < objs.Count; i++) {
	            for(int j = 0; j < objs[i].Count; j++) {
	                string shaderName = "";//object is not optimizable by default

	                if(objs[i][j] != null && objs[i][j].IsCorrectlyAssembled) {
	                    shaderName = objs[i][j].ShaderName;
	                }

	                int index = GetIndexForShader(shaderName);
	                if(index == -1) {//no shader found for this object so add a new position
	                    shadersUsed.Add(shaderName);

	                    sortedObjects.Add(new List<OptimizableObject>());
	                    sortedObjects[sortedObjects.Count-1].Add(objs[i][j]);
	                } else {//shader already exists so place it in the sorted objs
	                    sortedObjects[index].Add(objs[i][j]);
	                }
	            }
	        }
	        //after the array has been re organized (with removed or added objects) lets sort each in-array by texture sizes
	        for(int i = 0; i < sortedObjects.Count; i++)
	            if(sortedObjects[i].Count > 1)
	                if(sortedObjects[i][0] != null && sortedObjects[i][0].IsCorrectlyAssembled)//ShaderName != "")
	                    SortTexturesBySize(0, sortedObjects[i].Count-1, i);

	        if(sortedObjects.Count > 1 && sortedObjects[0].Count == 0) {
	            sortedObjects.RemoveAt(0);
	            shadersUsed.RemoveAt(0);
	        }
	    }

	    public static int GetAproxAtlasSize(int index, bool reuseTextures) {
	        int atlasSize = 0;
	        if(shadersUsed[index] == "")//we dont need to calculate atlas size on non-optimizable objects
	            return atlasSize;
	        if(reuseTextures) {
	            TextureReuseManager textureReuseManager = new TextureReuseManager();
	            for(int i = 0; i < sortedObjects[index].Count; i++) {
	                if(sortedObjects[index][i] != null) {
	                    if(!textureReuseManager.TextureRefExists(sortedObjects[index][i])) {
	                        textureReuseManager.AddTextureRef(sortedObjects[index][i]);
	                        atlasSize += sortedObjects[index][i].TextureArea;
	                    }
	                }
	            }
	        } else {
	            for(int i = 0; i < sortedObjects[index].Count; i++) {
	                if(sortedObjects[index][i] != null)
	                    atlasSize += sortedObjects[index][i].TextureArea;
	            }
	        }
	        return Mathf.RoundToInt(Mathf.Sqrt(atlasSize));
	    }


	    //given a shader name, checks on the sortedObjects list for the position
	    //where the objects with same shader exist.
	    //if no shader name matches, returns -1;
	    private static int GetIndexForShader(string s) {
	        for(int i = 0; i < shadersUsed.Count; i++)
	            if(s == shadersUsed[i])
	                return i;
	        return -1;
	    }

	    public static List<List<OptimizableObject>> GetObjs() {
	        return sortedObjects;
	    }

	    public static int GetRecognizableShadersCount() {
	        int shadersRecognized = 0;
	        for(int i = 0; i < shadersUsed.Count; i++) {
	            if(ShaderManager.Instance.ShaderExists(shadersUsed[i]))
	               shadersRecognized++;
	        }

	        return shadersRecognized;
	    }


	    public static int GetTotalSortedObjects() {
	        int count = 0;
	        for(int i = 0; i < sortedObjects.Count; i++)
	            count += sortedObjects[i].Count;
	        return count;
	    }

	    //Quick sort for organizing textures sizes.
	    //ascending order
	    private static void SortTexturesBySize(int left, int right, int position) {
	        int leftHold = left;
	        int rightHold = right;
	        OptimizableObject pivotObj = sortedObjects[position][left];
	        int pivot = pivotObj.TextureArea;

	        while (left < right) {
	            while ((sortedObjects[position][right].TextureArea >= pivot) && (left < right))
	                right--;

	            if (left != right) {
	                sortedObjects[position][left] = sortedObjects[position][right];
	                left++;
	            }

	            while ((sortedObjects[position][left].TextureArea <= pivot) && (left < right))
	                left++;

	            if (left != right) {
	                sortedObjects[position][right] = sortedObjects[position][left];
	                right--;
	            }
	        }
	        sortedObjects[position][left] = pivotObj;
	        pivot = left;
	        left = leftHold;
	        right = rightHold;

	        if (left < pivot)
	            SortTexturesBySize(left, pivot - 1, position);

	        if (right > pivot)
	            SortTexturesBySize(pivot + 1, right, position);
	    }
	}
}