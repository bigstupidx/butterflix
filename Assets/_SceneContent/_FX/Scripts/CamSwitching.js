#pragma strict

 var cam1: GameObject;
 var cam2: GameObject;
 var cam3: GameObject;
 var cam4: GameObject;
 var cam5: GameObject;
 
 function Start() {
     cam1.SetActive (true);
     cam2.SetActive (false);
     cam3.SetActive (false);
     cam4.SetActive (false);
     cam5.SetActive (false);
 }
 
 function Update() {
 
 if (Input.GetKeyDown(KeyCode.C) && (cam1.activeSelf == true)) {
 	cam1.SetActive (false);
 	cam2.SetActive (true);
 	cam3.SetActive (false);
	cam4.SetActive (false);
 	cam5.SetActive (false);
 }
 else if (Input.GetKeyDown(KeyCode.C) && (cam2.activeSelf == true)) {
 	cam1.SetActive (false);
 	cam2.SetActive (false);
 	cam3.SetActive (true);
 	cam4.SetActive (false);
 	cam5.SetActive (false);
 }
 else if (Input.GetKeyDown(KeyCode.C) && (cam3.activeSelf == true)) {
 	cam1.SetActive (false);
 	cam2.SetActive (false);
 	cam3.SetActive (false);
 	cam4.SetActive (true);
 	cam5.SetActive (false);
 }
 else if (Input.GetKeyDown(KeyCode.C) && (cam4.activeSelf == true)) {
 	cam1.SetActive (false);
 	cam2.SetActive (false);
 	cam3.SetActive (false);
 	cam4.SetActive (false);
 	cam5.SetActive (true);
 }
 else if (Input.GetKeyDown(KeyCode.C) && (cam5.activeSelf == true)) {
 	cam1.SetActive (true);
 	cam2.SetActive (false);
 	cam3.SetActive (false);
 	cam4.SetActive (false);
 	cam5.SetActive (false);
 }
}