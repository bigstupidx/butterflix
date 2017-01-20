using UnityEngine;
using System.Collections;

public class FxTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
	}
	
	// Update is called once per frame
	void Update () {
		if (Application.targetFrameRate != 60)
			Application.targetFrameRate = 60;
	}
}
