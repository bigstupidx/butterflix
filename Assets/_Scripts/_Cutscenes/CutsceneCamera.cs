using UnityEngine;
using System.Collections;

public class CutsceneCamera : MonoBehaviour
{
	private Transform _thisTransform;
	private Vector3 _lookAt;
	private Vector3 _lastPosition;

	//=====================================================

	public Vector3 LookAt
	{
		set
		{
			_lookAt = value;
			_thisTransform.LookAt( _lookAt );
		}
	}

	//=====================================================

	void Awake()
	{
		_thisTransform = this.transform;
		_lastPosition = _thisTransform.position;
		_lookAt = Vector3.forward;
	}

	//=====================================================

	void Update()
	{
		if( _thisTransform.position == _lastPosition ) return;
		
		_lastPosition = _thisTransform.position;

		_thisTransform.LookAt( _lookAt );
	}

	//=====================================================
}
