using UnityEngine;

[ExecuteInEditMode]
public class CutsceneContainer : MonoBehaviour
{
	[SerializeField] private Transform[] _cameraPoints;
	[SerializeField] private Transform _cameraLookAt;
	[SerializeField] private Transform _playerPoint;
	[SerializeField] private bool _isPlayerVisible;
	[SerializeField] private bool _orientCameraToPath;
	[Range( 0.5f, 10.0f )]
	[SerializeField] private float _duration = 2.5f;

	[SerializeField] private CameraPathAnimator _flyThruAnimator;

	private LTSpline _spline;
	private Vector3[] _path;

	//=====================================================

	public Transform[] CameraPoints { get { return _cameraPoints; } }

	public LTSpline CameraPath { get { return _spline; } }

	public Transform CameraLookAt { get { return _cameraLookAt; } }

	public Transform PlayerPoint { get { return _playerPoint; } }

	public bool OrientCameraToPath { get { return _orientCameraToPath; } }

	public float Duration { get { return _duration; } }

	public bool IsFlyThruAvailable { get { return _flyThruAnimator != null; } }

	public CameraPathAnimator FlyThruAnimator { get { return _flyThruAnimator; } }

	//=====================================================

	private void OnEnable()
	{
		_playerPoint.gameObject.SetActive( _isPlayerVisible );

		if( IsFlyThruAvailable == false )
			InitCameraSpline();
	}

	//=====================================================

	void OnDrawGizmos()
	{
		if( IsFlyThruAvailable == true || _path == null || _path.Length <= 1 ) return;

		Gizmos.color = Color.grey;

		// Visualize the path / spline
		if( _spline != null )
			_spline.gizmoDraw();
	}

	//=====================================================
	// Move along spline following path nodes
	private void InitCameraSpline()
	{
		if( _cameraPoints.Length <= 1 || _cameraPoints[0] == null )
			return;

		// Build spline nodes - Duplicate first node and last for path array
		_path = new Vector3[_cameraPoints.Length + 2];
		_path[0] = _cameraPoints[0].position;

		for( var i = 0; i < _cameraPoints.Length; i++ )
		{
			if( _cameraPoints[i] != null )
				_path[i + 1] = _cameraPoints[i].position;
		}

		if( _cameraPoints.Length > 0 && _cameraPoints[_cameraPoints.Length - 1] != null )
			_path[_path.Length - 1] = _cameraPoints[_cameraPoints.Length - 1].position;

		// Create spline
		_spline = new LTSpline( _path );
	}

	//=====================================================
}
