using UnityEngine;

[ExecuteInEditMode]
public class CutsceneCamLookAt : MonoBehaviour {

	private Transform _thisTransform;

	//=====================================================

	void Awake()
	{
		_thisTransform = this.transform;
	}

	//=====================================================

	void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere( _thisTransform.position, 0.1f );
	}

	//=====================================================
}
