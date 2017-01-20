/*
  singleton class for managing the shader defines logic.

  Created by:
  Juan Sebastian Munoz
  naruse@gmail.com
  All rights reserved

 */

namespace ProDrawCall {
	using System.IO;
	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.Collections.Generic;


	public class ShaderManager {

	    //stores all the data from the Unity shaders
	    private List<string> knownShaders;
	    private List<List<string>> knownShadersTexturesDefines;//array that contains each shader properties for textures

	    //stores all the data from custom shaders.
	    private List<List<string>> customShadersTexturesDefines;
	    public List<List<string>> CustomShadersTexturesDefines { get {return customShadersTexturesDefines; } }

	    private List<string> customShaders;
	    public List<string> CustomShaders { get { return customShaders; } }


	    private static ShaderManager instance;
	    public static ShaderManager Instance {
	        get {
	            if(instance == null)
	                instance = new ShaderManager();
	            return instance;
	        }
	    }

	    private ShaderManager() {
	        knownShaders = new List<string>();
	        knownShadersTexturesDefines = new List<List<string>>();
	        customShaders = new List<string>();
	        customShadersTexturesDefines = new List<List<string>>();
	        LoadUnityShaders();
	        LoadCustomShaders();
	    }

	    public int GetTotalShaderDefines() {
	        int count = 0;
	        for(int i = 0; i < customShadersTexturesDefines.Count; i++)
	            count += customShadersTexturesDefines[i].Count;
	        return count;
	    }

	    public void SaveCustomShaders() {
	        string[] lines = new string[customShaders.Count];

	        for(int i = 0; i < customShaders.Count; i++) {
	            string lineToWrite = customShaders[i] + "|";
	            for(int j = 0; j < customShadersTexturesDefines[i].Count; j++) {
	                lineToWrite += customShadersTexturesDefines[i][j] + " ";
	            }
	            lineToWrite = lineToWrite.Remove(lineToWrite.Length-1);
	            lines[i] = lineToWrite;
	        }
			File.WriteAllLines(Application.dataPath + "/" + Constants.PathForCustomShaders, lines);
	        AssetDatabase.Refresh();
	    }
	    public void LoadCustomShaders() {
	        string filePath = Application.dataPath + "/" + Constants.PathForCustomShaders;
	        if(File.Exists(filePath)) {
	            customShaders = new List<string>();
	            customShadersTexturesDefines = new List<List<string>>();
	            string[] lines = File.ReadAllLines(filePath);
	            for(int i = 0; i < lines.Length; i++) {
	                string[] data = lines[i].Split('|');
	                string[] defines = data[1].Split(' ');
	                AddShader(data[0], defines,true);
	            }
	        }
	    }

	    public List<Texture2D> GetTexturesForObject(GameObject g, string shaderName, bool generateTexturesIfNecessary = false) {
	        List<string> defines = GetShaderTexturesDefines(shaderName);
	        List<Texture2D> materialTextures = new List<Texture2D>();
	        if(defines != null) {
	            for(int i = 0; i < defines.Count; i++) {
	                Texture2D tex = g.GetComponent<MeshRenderer>().sharedMaterials[0].GetTexture(defines[i]) as Texture2D;
	                if(tex == null && generateTexturesIfNecessary) {
	                    tex = Utils.GenerateTexture(g.GetComponent<MeshRenderer>().sharedMaterials[0].color);//TODO GET THE PROPER COLOR FOR EACH SHADER.
	                }
	                materialTextures.Add(tex);
	            }
	            return materialTextures;
	        }
	        return null;
	    }

	    public Texture2D GetTextureForObjectSpecificShaderDefine(GameObject g, string shaderDefine, bool generateTexturesIfNecessary = false) {
	        Texture2D result = g.GetComponent<MeshRenderer>().sharedMaterials[0].GetTexture(shaderDefine) as Texture2D;
	        if(result == null && generateTexturesIfNecessary) {
	            if(g.GetComponent<MeshRenderer>().sharedMaterials[0].HasProperty("_Color")) {
	                Color shaderColor = g.GetComponent<MeshRenderer>().sharedMaterials[0].GetColor("_Color");
	                result = Utils.GenerateTexture(shaderColor);
	            } else {
	                Debug.LogWarning("Shader for GameObject " + g.name + " doesnt have a '_Color' property, using white color by default");
	                result = Utils.GenerateTexture(Color.white);
	            }
	        }
	        return result;
	    }

	    public Vector2 GetScaleForObjectSpecificShaderDefine(GameObject g, string shaderDefine) {
	        return g.GetComponent<MeshRenderer>().sharedMaterials[0].GetTextureScale(shaderDefine);
	    }

	    public Vector2 GetOffsetForObjectSpecificShaderDefine(GameObject g, string shaderDefine) {
	        return g.GetComponent<MeshRenderer>().sharedMaterials[0].GetTextureOffset(shaderDefine);
	    }

	    public List<string> GetShaderTexturesDefines(string shaderName) {
	        for(int i = 0; i < knownShaders.Count; i++) {
	            if(shaderName == knownShaders[i]) {
	                return knownShadersTexturesDefines[i];
	            }
	        }
	        for(int i = 0; i < customShaders.Count; i++) {
	            if(shaderName == customShaders[i]) {
	                return customShadersTexturesDefines[i];
	            }
	        }
	        //Debug.Log("Not known shader:" + shaderName);
	        return null;
	    }

	    private void PrintCustomShaders() {
	        Debug.Log("LOADED: " + customShadersTexturesDefines.Count);
	        for(int i = 0; i < customShadersTexturesDefines.Count; i++) {
	            string line = "";
	            for(int j = 0; j < customShadersTexturesDefines[i].Count; j++)
	                line += " " + customShadersTexturesDefines[i][j];
	            Debug.Log(i + ": " + line);
	        }
	    }

	    public bool ShaderExists(string shaderName) {
	        for(int i = 0; i < knownShaders.Count; i++) {
	            if(shaderName == knownShaders[i])
	                return true;
	        }
	        for(int i = 0; i < customShaders.Count; i++) {
	            if(shaderName == customShaders[i])
	                return true;
	        }
	        return false;
	    }


	    public void AddShader(string shaderName, string[] shaderTextures, bool custom) {
	        if(custom) {
	            customShaders.Add(shaderName);
	            customShadersTexturesDefines.Add(new List<string>());
	            for(int i = 0; i < shaderTextures.Length; i++) {
	                customShadersTexturesDefines[customShadersTexturesDefines.Count-1].Add(shaderTextures[i]);
	            }
	        } else {
	            knownShaders.Add(shaderName);
	            knownShadersTexturesDefines.Add(new List<string>());
	            for(int i = 0; i < shaderTextures.Length; i++) {
	                knownShadersTexturesDefines[knownShadersTexturesDefines.Count-1].Add(shaderTextures[i]);
	            }
	        }
	    }

	    private void LoadUnityShaders() {
	        AddShader("Transparent/Bumped Diffuse",
	                  new string[] { "_MainTex", "_BumpMap" }, false);
	        AddShader("Transparent/Bumped Specular",
	                  new string[] { "_MainTex", "_BumpMap" }, false);
	        AddShader("Transparent/Diffuse",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Transparent/Specular",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Transparent/Parallax Diffuse",
	                  new string[] { "_MainTex", "_BumpMap", "_ParallaxMap" }, false);
	        AddShader("Transparent/Parallax Specular",
	                  new string[] { "_MainTex", "_BumpMap", "_ParallaxMap" }, false);
	        AddShader("Transparent/VertexLit",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Transparent/Cutout/Bumped Diffuse",
	                  new string[] { "_MainTex", "_BumpMap" }, false);
	        AddShader("Transparent/Cutout/Bumped Specular",
	                  new string[] { "_MainTex", "_BumpMap" }, false);
	        AddShader("Transparent/Cutout/Diffuse",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Transparent/Cutout/Specular",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Transparent/Cutout/Soft Edge Unlit",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Transparent/Cutout/VertexLit",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Decal",
	                  new string[] { "_MainTex", "_DecalTex" }, false);
	        AddShader("FX/Flare",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Self-Illumin/Bumped Diffuse",
	                  new string[] { "_MainTex", "_Illum", "_BumpMap" }, false);
	        AddShader("Self-Illumin/Bumped Specular",
	                  new string[] { "_MainTex", "_Illum", "_BumpMap" }, false);
	        AddShader("Self-Illumin/Diffuse",
	                  new string[] { "_MainTex", "_Illum" }, false);
	        AddShader("Self-Illumin/Specular",
	                  new string[] { "_MainTex", "_Illum" }, false);
	        AddShader("Self-Illumin/Parallax Diffuse",
	                  new string[] { "_MainTex", "_Illum", "_BumpMap", "_ParallaxMap" }, false);
	        AddShader("Self-Illumin/Parallax Specular",
	                  new string[] { "_MainTex", "_Illum", "_BumpMap", "_ParallaxMap" }, false);
	        AddShader("Self-Illumin/VertexLit",
	                  new string[] { "_MainTex", "_Illum" }, false);
	        AddShader("Legacy Shaders/Lightmapped/Bumped Diffuse",
	                  new string[] { "_MainTex", "_BumpMap", "_LightMap" }, false);
	        AddShader("Legacy Shaders/Lightmapped/Bumped Specular",
	                  new string[] { "_MainTex", "_BumpMap", "_LightMap" }, false);
	        AddShader("Legacy Shaders/Lightmapped/Diffuse",
	                  new string[] { "_MainTex", "_LightMap" }, false);
	        AddShader("Legacy Shaders/Lightmapped/Specular",
	                  new string[] { "_MainTex", "_LightMap" }, false);
	        AddShader("Legacy Shaders/Lightmapped/VertexLit",
	                  new string[] { "_MainTex", "_LightMap" }, false);

	        //Mobile
	        AddShader("Mobile/Bumped Diffuse",
	                  new string[] { "_MainTex", "_BumpMap" }, false);
	        AddShader("Mobile/Bumped Specular",
	                  new string[] { "_MainTex", "_BumpMap" }, false);
	        AddShader("Mobile/Bumped Specular (1 Directional Light)",
	                  new string[] { "_MainTex", "_BumpMap" }, false);
	        AddShader("Mobile/Diffuse",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Mobile/Unlit (Supports Lightmap)",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Mobile/Particles/Additive",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Mobile/Particles/Alpha Blended",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Mobile/Particles/VertexLit Blended",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Mobile/Particles/Multiply",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Mobile/Skybox",
	                  new string[] { "_FrontTex", "_BackTex", "_LeftTex", "_RightTex", "_UpTex", "_DownTex" }, false);
	        AddShader("Mobile/VertexLit",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Mobile/VertexLit (Only Directional Lights)",
	                  new string[] { "_MainTex" }, false);

	        //Nature
	        AddShader("Nature/Tree Soft Occlusion Bark",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Nature/Tree Soft Occlusion Leaves",
	                  new string[] { "_MainTex" }, false);
	        //"Nature/Terrain/Bumped Specular" uses some HideInInspector so not optimizing
	        //"Nature/Terrain/Diffuse" uses some HideInInspector so not optimizing
	        AddShader("Nature/Tree Creator Bark",
	                  new string[] { "_MainTex", "_BumpMap", "_GlossMap" }, false);
	        AddShader("Nature/Tree Creator Leaves",
	                  new string[] { "_MainTex", "_BumpMap", "_GlossMap", "_TranslucencyMap", "_ShadowOffset" }, false);
	        AddShader("Nature/Tree Creator Leaves Fast",
	                  new string[] { "_MainTex" }, false);


	        AddShader("Bumped Diffuse",
	                  new string[] { "_MainTex", "_BumpMap" }, false);
	        AddShader("Bumped Specular",
	                  new string[] { "_MainTex", "_BumpMap" }, false);
	        AddShader("Diffuse",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Diffuse Detail",
	                  new string[] { "_MainTex", "_Detail" }, false);
	        AddShader("Legacy Shaders/Diffuse Fast",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Specular",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Parallax Diffuse",
	                  new string[] { "_MainTex", "_BumpMap", "_ParallaxMap" }, false);
	        AddShader("Parallax Specular",
	                  new string[] { "_MainTex", "_BumpMap", "_ParallaxMap" }, false);
	        AddShader("VertexLit",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Particles/Additive",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Particles/~Additive-Multiply",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Particles/Additive (Soft)",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Particles/Alpha Blended",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Particles/Blend",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Particles/Multiply",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Particles/Multiply (Double)",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Particles/Alpha Blended Premultiply",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Particles/VertexLit Blended",
	                  new string[] { "_MainTex" }, false);
	        //reflective not suported
	        AddShader("RenderFX/Skybox",
	                  new string[] { "_FrontTex", "_BackTex", "_LeftTex", "_RightTex", "_UpTex", "_DownTex" }, false);
	        AddShader("Sprites/Alpha Blended",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Sprites/Pixel Snap/Alpha Blended",
	                  new string[] { "_MainTex" }, false);
	        //Unlit
	        AddShader("Unlit/Transparent",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Unlit/Transparent Cutout",
	                  new string[] { "_MainTex" }, false);
	        AddShader("Unlit/Texture",
	                  new string[] { "_MainTex" }, false);
	    }
	}
}