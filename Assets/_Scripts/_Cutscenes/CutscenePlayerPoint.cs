using UnityEngine;

[ExecuteInEditMode]
public class CutscenePlayerPoint : MonoBehaviour {

	private Transform _thisTransform;

	//=====================================================

	void Awake()
	{
		_thisTransform = this.transform;
	}

	//=====================================================

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawSphere( _thisTransform.position, 0.1f );
	}

	//=====================================================
}
