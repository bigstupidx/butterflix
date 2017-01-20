using UnityEngine;

public class CameraFacingBillboard : MonoBehaviour
{
	[SerializeField]
	private bool _isReversed;
	private Transform _thisTransform;
	private Camera _camera;

	//=====================================================

	void OnEnable()
	{
		_thisTransform = this.transform;
		_camera = Camera.main;
	}

	//=====================================================

	void Update()
	{
		// Used to face object toward or away from the camera
		var multiplier = _isReversed ? Vector3.forward : Vector3.back;

		if( _camera != null)
			_thisTransform.LookAt(	_thisTransform.position + _camera.transform.rotation * multiplier, _camera.transform.rotation * Vector3.up );
	}

	//=====================================================
}
