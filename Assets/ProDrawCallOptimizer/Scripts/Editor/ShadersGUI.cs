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

    public class ShadersGUI {
        private static List<bool> unfoldedKnownShaders;

        static readonly ShadersGUI instance = new ShadersGUI();
        public static  ShadersGUI Instance {
            get {
                return instance;
            }
        }
        private ShadersGUI() {
            Initialize();
        }

        public void Initialize() {
            unfoldedKnownShaders = new List<bool>();
        }

        private Vector2 arraysShadersScrollPos = Vector2.zero;

        //TODO pass parameter where the window can be drawn
        public void DrawGUI(ProDrawCallOptimizerMenu window) {
            if(GUI.Button(new Rect(5,35, 80, 28), "Add Custom\nShader")) {
                ShaderManager.Instance.CustomShaders.Insert(0,"");
                ShaderManager.Instance.CustomShadersTexturesDefines.Insert(0,new List<string>());
                ShaderManager.Instance.CustomShadersTexturesDefines[0].Add("");
            }
            if(GUI.Button(new Rect(window.position.width/2 - 46,35, 85, 28), "Save Custom\nShaders")) {
                ShaderManager.Instance.SaveCustomShaders();
            }
            if(GUI.Button(new Rect(window.position.width - 100,35, 95, 28), "Reload Custom\nShader")) {
                ShaderManager.Instance.LoadCustomShaders();
            }

            AdjustFoldedArraySizeWithShaderManager();
            arraysShadersScrollPos = GUI.BeginScrollView(new Rect(0, 90, window.position.width, window.position.height - 127),
                                                         arraysShadersScrollPos,
                                                         new Rect(0,0, window.position.width - 20, (ShaderManager.Instance.GetTotalShaderDefines() + ShaderManager.Instance.CustomShaders.Count)*(32.5f)));
            int drawingPos = 0;
            for(int i = 0; i < ShaderManager.Instance.CustomShaders.Count; i++) {
                unfoldedKnownShaders[i] = EditorGUI.Foldout(new Rect(3, drawingPos*30+25, 10, 15),
                                                            unfoldedKnownShaders[i],
                                                            (i+1).ToString() + ". ");
                ShaderManager.Instance.CustomShaders[i] = EditorGUI.TextField(new Rect(32, drawingPos*30+27, 260, 15),
                                                                              ShaderManager.Instance.CustomShaders[i]);

                if(GUI.Button(new Rect(window.position.width-40, drawingPos*30+26, 24,15),"X")) {
                    ShaderManager.Instance.CustomShaders.RemoveAt(i);
                    ShaderManager.Instance.CustomShadersTexturesDefines.RemoveAt(i);
                    return;//return to recalculate the unfolded array.
                }
                drawingPos++;
                if(unfoldedKnownShaders[i]) {
                    for(int j = 0; j < ShaderManager.Instance.CustomShadersTexturesDefines[i].Count; j++) {
                        GUI.Label(new Rect(20, drawingPos*30+20 + 6, 30, 25), (j+1).ToString() +":");
                        ShaderManager.Instance.CustomShadersTexturesDefines[i][j] = EditorGUI.TextField(new Rect(41, drawingPos*30 + 24, 200, 17),
                                                                                                        ShaderManager.Instance.CustomShadersTexturesDefines[i][j]).Replace(" ", string.Empty);

                        if(GUI.Button(new Rect(250, drawingPos*30+20, 18,22), "-")) {
                            if(ShaderManager.Instance.CustomShadersTexturesDefines[i].Count > 1) {
                                ShaderManager.Instance.CustomShadersTexturesDefines[i].RemoveAt(j);
                            } else {
                                ShaderManager.Instance.CustomShadersTexturesDefines.RemoveAt(i);
                                ShaderManager.Instance.CustomShaders.RemoveAt(i);
                            }
                            return;
                        }
                        if(j == ShaderManager.Instance.CustomShadersTexturesDefines[i].Count-1) {
                            if(GUI.Button(new Rect(273, drawingPos*30+20, 22,22), "+")) {
                                ShaderManager.Instance.CustomShadersTexturesDefines[i].Add("");
                            }
                        }
                        drawingPos++;
                    }
                }
            }
            GUI.EndScrollView();
        }
        
        private void AdjustFoldedArraySizeWithShaderManager() {
            if(unfoldedKnownShaders.Count != ShaderManager.Instance.CustomShaders.Count) {
                int offset = ShaderManager.Instance.CustomShaders.Count - unfoldedKnownShaders.Count;
                bool removing = false;
                if(offset < 0) {
                    offset *= -1;
                    removing = true;
                }
                for(int i = 0; i < (offset < 0 ? offset*-1 : offset); i++) {
                    if(removing)
                        unfoldedKnownShaders.RemoveAt(unfoldedKnownShaders.Count-1);
                    else
                        unfoldedKnownShaders.Add(true);
                }
            }
        }
    }
}