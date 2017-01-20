using UnityEngine;

[ExecuteInEditMode]
public class CutsceneCamPoint : MonoBehaviour
{
	private Transform _thisTransform;
	[SerializeField] private Transform _lookatTarget;
	[SerializeField] private bool _isStartPoint;

	//=====================================================

	void Awake()
	{
		_thisTransform = this.transform;
	}

	//=====================================================

	void OnEnable()
	{
		_thisTransform.LookAt( _lookatTarget );
	}

	//=====================================================

	void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawSphere( _thisTransform.position, 0.1f );
		DrawArrow.ForGizmo( _thisTransform.position, _thisTransform.forward, 0.4f );

		if( _isStartPoint )
		{
			Gizmos.DrawIcon( _thisTransform.position + new Vector3( 0.0f, 0.3f, 0.0f ), "CameraIcon.png", false );

			Gizmos.color = Color.white;
			Gizmos.DrawLine( _thisTransform.position, _lookatTarget.position );
		}
	}

	//=====================================================
}
