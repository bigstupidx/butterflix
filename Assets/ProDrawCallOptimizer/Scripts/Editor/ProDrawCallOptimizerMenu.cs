/*
  Created by:
  Juan Sebastian Munoz Arango
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

	public sealed class ProDrawCallOptimizerMenu : EditorWindow {
	    private Atlasser generatedAtlas;

	    private static int selectedMenuOption = 0;
	    private static string[] menuOptions;

	    private static ProDrawCallOptimizerMenu window;
	    [MenuItem("Window/ProDrawCallOptimizer")]
	    private static void Init() {
	        ObjSorter.Initialize();

	        window = (ProDrawCallOptimizerMenu) EditorWindow.GetWindow(typeof(ProDrawCallOptimizerMenu));
	        window.minSize = new Vector2(445, 200);
	        window.Show();

            ShadersGUI.Instance.Initialize();
            ObjectsGUI.Instance.Initialize();
            AdvancedMenuGUI.Instance.Initialize();

	        menuOptions = new string[] { "Objects", "Custom Shaders", "Advanced" };
	        selectedMenuOption = 0;
	    }

	    void OnGUI() {
	        if(NeedToReload())
	            ReloadDataStructures();
	        selectedMenuOption = GUI.SelectionGrid(new Rect(5,8,window.position.width-10, 20), selectedMenuOption, menuOptions, 3);
	        switch(selectedMenuOption) {
	            case 0:
	                ObjectsGUI.Instance.DrawGUI(window);
                    AdvancedMenuGUI.Instance.ClearConsole();
                    menuOptions[0] = "Objects";
	                break;
	            case 1:
                    ShadersGUI.Instance.DrawGUI(window);
                    AdvancedMenuGUI.Instance.ClearConsole();
                    menuOptions[0] = "Objects(" + ObjSorter.GetTotalSortedObjects() + ")";
	                break;
                case 2:
                    AdvancedMenuGUI.Instance.DrawGUI(window);
                    menuOptions[0] = "Objects(" + ObjSorter.GetTotalSortedObjects() + ")";
                    break;
                default:
	                Debug.LogError("Unrecognized menu option: " + selectedMenuOption);
	                break;
	        }

	        if(GUI.Button(new Rect(5, window.position.height - 35, window.position.width/2 - 10, 33), "Clear Atlas")) {
	            GameObject[] objsInHierarchy = Utils.GetAllObjectsInHierarchy();
	            foreach(GameObject obj in objsInHierarchy) {
	                if(obj.name.Contains(Constants.OptimizedObjIdentifier))
	                    DestroyImmediate(obj);
                    else
                        if(obj.GetComponent<MeshRenderer>() != null)
	                        obj.GetComponent<MeshRenderer>().enabled = true;
	            }
	            // delete the folder where the atlas reside.
	            string folderOfAtlas = EditorApplication.currentScene;
				if(folderOfAtlas == "") { //scene is not saved yet.
					folderOfAtlas = Constants.NonSavedSceneFolderName + ".unity";
					Debug.LogWarning("WARNING: Scene has not been saved, clearing baked objects from NOT_SAVED_SCENE folder");
				}
	            folderOfAtlas = folderOfAtlas.Substring(0, folderOfAtlas.Length-6) + "-Atlas";//remove the ".unity"
	            if(Directory.Exists(folderOfAtlas)) {
	                FileUtil.DeleteFileOrDirectory(folderOfAtlas);
	                AssetDatabase.Refresh();
	            }
	        }

	        GUI.enabled = CheckEmptyArray(); //if there are no textures deactivate the GUI
	        if(GUI.Button(new Rect(window.position.width/2 , window.position.height - 35, window.position.width/2 - 5, 33), "Bake Atlas")) {
	            //Remove objects that are already optimized and start over.
	            if(AdvancedMenuGUI.Instance.RemoveObjectsBeforeBaking) {
	                GameObject[] objsInHierarchy = Utils.GetAllObjectsInHierarchy();
	                foreach(GameObject obj in objsInHierarchy) {
	                    if(obj.name.Contains(Constants.OptimizedObjIdentifier))
	                        GameObject.DestroyImmediate(obj);
	                }
	            }

	            List<Rect> texturePositions = new List<Rect>();//creo que esto puede morir porque el atlasser tiene adentro un rect.
	            string progressBarInfo = "";
	            float pace = 1/(float)ObjSorter.GetRecognizableShadersCount();
	            float progress = pace;

	            Node resultNode = null;//nodes for the tree for atlasing
	            for(int shaderIndex = 0; shaderIndex < ObjSorter.GetObjs().Count; shaderIndex++) {
					EditorUtility.DisplayProgressBar("Optimization in progress... " +
					                                 (AdvancedMenuGUI.Instance.CreatePrefabsForObjects ? " Get coffee this will take some time..." : ""), progressBarInfo, progress);
	                progress += pace;

	                texturePositions.Clear();
	                TextureReuseManager textureReuseManager = new TextureReuseManager();

	                string shaderToAtlas = (ObjSorter.GetObjs()[shaderIndex][0] != null && ObjSorter.GetObjs()[shaderIndex][0].IsCorrectlyAssembled) ? ObjSorter.GetObjs()[shaderIndex][0].ShaderName : "";
	                progressBarInfo = "Processing shader " + shaderToAtlas + "...";
	                int atlasSize = ObjSorter.GetAproxAtlasSize(shaderIndex, AdvancedMenuGUI.Instance.ReuseTextures);

	                if(ShaderManager.Instance.ShaderExists(shaderToAtlas) &&
                       (ObjSorter.GetObjs()[shaderIndex].Count > 1 ||
                        (ObjSorter.GetObjs()[shaderIndex].Count == 1 && ObjSorter.GetObjs()[shaderIndex][0] != null && ObjSorter.GetObjs()[shaderIndex][0].ObjHasMoreThanOneMaterial)) &&//more than 1 obj or 1obj wth multiple mat
					   atlasSize < Constants.MaxAtlasSize ) { //check the generated atlas size doesnt exceed max supported texture size
	                    generatedAtlas = new Atlasser(atlasSize, atlasSize);
	                    int resizeTimes = 1;

	                    for(int j = ObjSorter.GetObjs()[shaderIndex].Count-1; j >= 0; j--) {//start from the largest to the shortest textures
	                        //before atlassing multiple materials obj, combine it.
	                        if(ObjSorter.GetObjs()[shaderIndex][j].ObjHasMoreThanOneMaterial) {
	                            progressBarInfo = "Combining materials...";
	                            ObjSorter.GetObjs()[shaderIndex][j].ProcessAndCombineMaterials();//mirar esto, aca esta el problema  de multiple materiales y reimportacion
	                        }

							Vector2 textureToAtlasSize = ObjSorter.GetObjs()[shaderIndex][j].TextureSize;
	                        if(AdvancedMenuGUI.Instance.ReuseTextures) {
	                            //if texture is not registered already
	                            if(!textureReuseManager.TextureRefExists(ObjSorter.GetObjs()[shaderIndex][j])) {
	                                //generate a node
	                                resultNode = generatedAtlas.Insert(Mathf.RoundToInt((textureToAtlasSize.x != Constants.NULLV2.x) ? textureToAtlasSize.x : Constants.NullTextureSize),
	                                                                   Mathf.RoundToInt((textureToAtlasSize.y != Constants.NULLV2.y) ? textureToAtlasSize.y : Constants.NullTextureSize));
	                                if(resultNode != null) { //save node if fits in atlas
	                                    textureReuseManager.AddTextureRef(ObjSorter.GetObjs()[shaderIndex][j], resultNode.NodeRect, j);
	                                }
	                            }
	                        } else {
	                            resultNode = generatedAtlas.Insert(Mathf.RoundToInt((textureToAtlasSize.x != Constants.NULLV2.x) ? textureToAtlasSize.x : Constants.NullTextureSize),
	                                                               Mathf.RoundToInt((textureToAtlasSize.y != Constants.NULLV2.y) ? textureToAtlasSize.y : Constants.NullTextureSize));
	                        }

	                        if(resultNode == null) {
	                            int resizedAtlasSize = atlasSize + Mathf.RoundToInt((float)atlasSize * Constants.AtlasResizeFactor * resizeTimes);
	                            generatedAtlas = new Atlasser(resizedAtlasSize, resizedAtlasSize);
	                            j = ObjSorter.GetObjs()[shaderIndex].Count;//Count and not .Count-1 bc at the end of the loop it will be substracted j-- and we want to start from Count-1

	                            texturePositions.Clear();
	                            textureReuseManager.ClearTextureRefs();
	                            resizeTimes++;
	                        } else {
	                            if(AdvancedMenuGUI.Instance.ReuseTextures) {
	                                texturePositions.Add(textureReuseManager.GetTextureRefPosition(ObjSorter.GetObjs()[shaderIndex][j]));
	                            } else {
	                                texturePositions.Add(resultNode.NodeRect);//save the texture rectangle
	                            }
	                        }
	                    }
	                    progressBarInfo = "Saving textures to atlas...";
	                    Material atlasMaterial = CreateAtlasMaterialAndTexture(shaderToAtlas, shaderIndex, textureReuseManager);
	                    progressBarInfo = "Remapping coordinates...";

	                    ObjSorter.OptimizeDrawCalls(ref atlasMaterial,
	                                                shaderIndex,
	                                                generatedAtlas.GetAtlasSize().x,
	                                                generatedAtlas.GetAtlasSize().y,
	                                                texturePositions,
	                                                AdvancedMenuGUI.Instance.ReuseTextures,
	                                                textureReuseManager,
						                            AdvancedMenuGUI.Instance.CreatePrefabsForObjects);
	                }
	            }

	            //after the game object has been organized, remove the combined game objects.
	            for(int shaderIndex = 0; shaderIndex < ObjSorter.GetObjs().Count; shaderIndex++) {
	                for(int j = ObjSorter.GetObjs()[shaderIndex].Count-1; j >= 0; j--) {
	                    if(ObjSorter.GetObjs()[shaderIndex][j].ObjWasCombined)
	                        ObjSorter.GetObjs()[shaderIndex][j].ClearCombinedObject();
	                }
	            }
	            EditorUtility.ClearProgressBar();
	            AssetDatabase.Refresh();//reimport the created atlases so they get displayed in the editor.
	        }
	    }

	    private Material CreateAtlasMaterialAndTexture(string shaderToAtlas, int shaderIndex, TextureReuseManager textureReuseManager) {
	        string fileName = ((ObjectsGUI.CustomAtlasName == "") ? "Atlas " : (ObjectsGUI.CustomAtlasName + " ")) + shaderToAtlas.Replace('/','_');
	        string folderToSaveAssets = EditorApplication.currentScene;
			if(folderToSaveAssets == "") { //scene is not saved yet.
				folderToSaveAssets = Constants.NonSavedSceneFolderName + ".unity";
				Debug.LogWarning("WARNING: Scene has not been saved, saving baked objects to: " + Constants.NonSavedSceneFolderName + " folder");
			}

	        folderToSaveAssets = folderToSaveAssets.Substring(0, folderToSaveAssets.Length-6) + "-Atlas";//remove the ".unity" and add "-Atlas"
	        if(!Directory.Exists(folderToSaveAssets)) {
	            Directory.CreateDirectory(folderToSaveAssets);
                AssetDatabase.ImportAsset(folderToSaveAssets);
	        }

	        string atlasTexturePath = folderToSaveAssets + Path.DirectorySeparatorChar + fileName;
	        //create the material in the project and set the shader material to shaderToAtlas
	        Material atlasMaterial = new Material(Shader.Find(shaderToAtlas));
	        //save the material to the project view
	        AssetDatabase.CreateAsset(atlasMaterial, atlasTexturePath + "Mat.mat");
			AssetDatabase.ImportAsset(atlasTexturePath + "Mat.mat");
	        //load a reference from the project view to the material (this is done to be able to set the texture to the material in the project view)
	        atlasMaterial = (Material) AssetDatabase.LoadAssetAtPath(atlasTexturePath + "Mat.mat", typeof(Material));

	        List<string> shaderDefines = ShaderManager.Instance.GetShaderTexturesDefines(shaderToAtlas);
	        for(int k = 0; k < shaderDefines.Count; k++) {//go trough each property of the shader.
	            List<Texture2D> texturesOfShader = ObjSorter.GetTexturesToAtlasForShaderDefine(shaderIndex, shaderDefines[k]);//Get thtextures for the property shderDefines[k] to atlas them
	            List<Vector2> scales = ObjSorter.GetScalesToAtlasForShaderDefine(shaderIndex, shaderDefines[k]);
	            List<Vector2> offsets = ObjSorter.GetOffsetsToAtlasForShaderDefine(shaderIndex, shaderDefines[k]);
	            if(AdvancedMenuGUI.Instance.ReuseTextures) {
	                texturesOfShader = Utils.FilterTexsByIndex(texturesOfShader, textureReuseManager.GetTextureIndexes());
	                scales = Utils.FilterVec2ByIndex(scales, textureReuseManager.GetTextureIndexes());
	                offsets = Utils.FilterVec2ByIndex(offsets, textureReuseManager.GetTextureIndexes());
	            }
	            generatedAtlas.SaveAtlasToFile(atlasTexturePath + k.ToString() + ".png", texturesOfShader, scales, offsets);//save the atlas with the retrieved textures
				AssetDatabase.ImportAsset(atlasTexturePath + k.ToString() + ".png");
	            Texture2D tex = (Texture2D) AssetDatabase.LoadAssetAtPath(atlasTexturePath + k.ToString() + ".png", typeof(Texture2D));

	            atlasMaterial.SetTexture(shaderDefines[k], //set property shderDefines[k] for shader shaderToAtlas
	                                     tex);
	        }
	        return atlasMaterial;
	    }

        //used to deactivate the "Bake Atlas" button if we dont have anything to bake
	    private bool CheckEmptyArray() {
	        for(int i = 0; i < ObjSorter.GetObjs().Count; i++)
	            if(ObjSorter.GetObjs()[i].Count > 1 ||//check that at least there are 2 objects (regardless if tex are null) OR
                   (ObjSorter.GetObjs()[i].Count == 1 && (ObjSorter.GetObjs()[i][0] != null && ObjSorter.GetObjs()[i][0].ObjHasMoreThanOneMaterial)))//there is at least 1 object that has multiple materials
	                return true;
	        return false;
	    }

	    void OnInspectorUpdate() {
	        Repaint();
	    }

	    private void OnDidOpenScene() {
	        //unfold all the objects to automatically clear objs from other scenes
            ObjectsGUI.Instance.UnfoldObjects();
	    }

	    private static void ReloadDataStructures() {
	        Init();

	    }

	    private bool NeedToReload() {
	        if(ObjSorter.GetObjs() == null)
	            return true;
	        else
	            return false;
	    }
	}
}