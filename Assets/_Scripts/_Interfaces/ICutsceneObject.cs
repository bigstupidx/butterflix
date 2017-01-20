using UnityEngine;

public interface ICutsceneObject
{
	void OnPlayCutsceneAnimation( int animationIndex = 0 );
	LTSpline CameraPath();
	Transform[] CutsceneCameraPoints();
	Transform CutsceneCameraLookAt();
	Transform CutscenePlayerPoint();
	float CutsceneDuration();
	bool OrientCameraToPath();
	bool IsFlyThruAvailable();
	CameraPathAnimator GetFlyThruAnimator();
}
