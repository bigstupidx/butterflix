using UnityEngine;
using System;

[ExecuteInEditMode]
[Serializable]
public class PathNode : MonoBehaviour
{
	[SerializeField] private bool _showDirection = false;

	private Transform _thisTransform;

	//=====================================================

	void Awake()
	{
		_thisTransform = this.transform;
	}

	//=====================================================

	void OnDrawGizmos()
	{
		// Allow for rotations
		//Gizmos.DrawIcon( _thisTransform.position + new Vector3( 0.0f, 0.3f, 0.0f ), "TickIcon.png", false );

		Gizmos.color = Color.white;
		Gizmos.DrawSphere( _thisTransform.position, 0.1f );

		if( _showDirection == true )
			DrawArrow.ForGizmo( _thisTransform.position + new Vector3( 0.0f, 0.05f, 0.0f ), _thisTransform.forward, 0.4f );
	}

	//=====================================================
}
