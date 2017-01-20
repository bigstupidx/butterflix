/*
  Created by:
  Juan Sebastian Munoz Arango
  naruse@gmail.com
  All rights reserved
 */

namespace ProDrawCall {
    using UnityEngine;
    using UnityEditor;
    using System.Collections;
    using System.Collections.Generic;

    public sealed class AdvancedMenuGUI {
        static readonly AdvancedMenuGUI instance = new AdvancedMenuGUI();
        public static  AdvancedMenuGUI Instance { get { return instance; } }
        private AdvancedMenuGUI() { Initialize(); }

        int selectedLayer = 0;
        string selectedTag = "";
        private string consoleStatus = "";

        private bool reuseTextures = true;
        public bool ReuseTextures { get { return reuseTextures; } }
		private bool createPrefabsForObjects = false;
        public bool CreatePrefabsForObjects { get { return createPrefabsForObjects; } }
        private bool removeObjectsBeforeBaking = true;
        public bool RemoveObjectsBeforeBaking { get { return removeObjectsBeforeBaking; } }

        public void Initialize() {
            selectedLayer = 0;
            selectedTag = "";
            consoleStatus = "";
        }

        public void ClearConsole() {
            consoleStatus = "";
        }

        public void DrawGUI(ProDrawCallOptimizerMenu window) {
            DrawAdvancedSearch(window);
            DrawAdvancedOptions(window);
            EditorGUI.HelpBox(new Rect(5,window.position.height - 80,window.position.width-10, 40), "Status:\n" + consoleStatus, MessageType.None);
        }

        private void DrawAdvancedOptions(ProDrawCallOptimizerMenu window) {
            GUILayout.BeginArea(new Rect(5, 115, window.position.width - 10, 170));
            GUILayout.Label("Options:");
            GUILayout.BeginHorizontal();
                GUILayout.Space(35);
                reuseTextures = GUILayout.Toggle(reuseTextures, "Reuse Textures");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
                GUILayout.Space(35);
                EditorGUILayout.HelpBox("Makes generated atlas smaller by reusing shared textures among objects", MessageType.None);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
                GUILayout.Space(35);
                reuseTextures = GUILayout.Toggle(reuseTextures, "Remove atlassed before bake");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
                GUILayout.Space(35);
                EditorGUILayout.HelpBox("Remove optimized objects (if any) from the hierarchy and textures and materials from the project view before optimizing the current objects.", MessageType.None);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
                GUILayout.Space(35);
                createPrefabsForObjects = GUILayout.Toggle(createPrefabsForObjects, "Generate prefabs for objects (SLOW)");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
                GUILayout.Space(35);
                EditorGUILayout.HelpBox("Creates a prefab for each optimized object, this is really slow as it needs to create a mesh and the prefab each time an object is created and optimized.", MessageType.None);
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void DrawAdvancedSearch(ProDrawCallOptimizerMenu window) {
            GUILayout.BeginArea(new Rect(5, 40, window.position.width - 10, 70));
                EditorGUILayout.BeginVertical();
                    GUILayout.Label("Advanced search:");
                    EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(35);
                        EditorGUILayout.BeginVertical();
                            GUILayout.Space(5);
                            selectedLayer = EditorGUILayout.LayerField("Select Objects By Layer:", selectedLayer);
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.BeginHorizontal();
                            if(GUILayout.Button("Select", GUILayout.Width(60))) {
                                List<GameObject> selectedObjects = new List<GameObject>();
                                GameObject[] allObjs = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
                                foreach(GameObject g in allObjs) {
                                    if(g.layer == selectedLayer)
                                        selectedObjects.Add(g);
                                }
                                Selection.objects = selectedObjects.ToArray();
                                consoleStatus = "Selected " + Selection.objects.Length + " Game Objects with layer: '" + LayerMask.LayerToName(selectedLayer) + "'";
                            }
                            if(GUILayout.Button("Add Selected", GUILayout.Width(100))) {
                                ObjectsGUI.Instance.FillArrayWithSelectedObjects(Selection.gameObjects);
                                consoleStatus = "Added " + Selection.gameObjects.Length + " Game objects to be optimized.";
                            }
                        EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(35);
                        EditorGUILayout.BeginVertical();
                            GUILayout.Space(5);
                            selectedTag = EditorGUILayout.TagField("Select Objects By Tag:", selectedTag);
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.BeginHorizontal();
                            if(GUILayout.Button("Select", GUILayout.Width(60))) {
                                List<GameObject> selectedObjects = new List<GameObject>();
                                if(selectedTag == "Untagged") {//when  there are untagged objs the selection has to be done manually
                                    GameObject[] allObjs = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
                                    foreach(GameObject g in allObjs) {
                                        if(g.tag == "Untagged")
                                            selectedObjects.Add(g);
                                    }
                                    Selection.objects = selectedObjects.ToArray();
                                } else {
                                    Selection.objects = GameObject.FindGameObjectsWithTag(selectedTag);
                                }
                                consoleStatus = "Selected " + Selection.objects.Length + " Game Objects with tag: '" + selectedTag + "'";
                            }
                            if(GUILayout.Button("Add Selected", GUILayout.Width(100))) {
                                ObjectsGUI.Instance.FillArrayWithSelectedObjects(Selection.gameObjects);
                                consoleStatus = "Added " + Selection.gameObjects.Length + " Game objects to be optimized.";
                            }
                        EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}