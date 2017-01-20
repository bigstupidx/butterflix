using UnityEngine;

public class DoneCameraMovement : MonoBehaviour
{
	[SerializeField]
	private float _smoothMovement = 2.5f;	// The relative speed at which the camera will catch up.
	[SerializeField]
	private float _smoothLookAt = 1.5f;		// The relative speed at which the camera will catch up.
	[SerializeField]
	private Transform _player;				// Reference to the player's transform.

	private Vector3 _relCameraPos;			// The relative position of the camera from the player.
	private float _relCameraPosMag;			// The distance of the camera from the player.
	private Vector3 _newPos;				// The position the camera is trying to reach.
	private float _newPosMag;
	private Vector3 _lookAtOffset;
	private int _curCheckPoint;				// Index of latest camera checkPoint
	//private Vector3 _lastPlayerPos;

	//=====================================================

	void Start()
	{
		// Setting up the reference.
		//player = GameObject.FindGameObjectWithTag( UnityTags.Player ).transform;

		// Setting the relative position as the initial relative position of the camera in the scene.
		_relCameraPos = transform.position - _player.position;
		_relCameraPosMag = _relCameraPos.magnitude - 0.5f;

		_lookAtOffset = new Vector3( 0.0f, 1.5f, 0.0f );
	}

	//=====================================================

	void FixedUpdate()
	{
		// The standard position of the camera is the relative position of the camera from the player.
		var standardPos = _player.position + _relCameraPos;

		// The abovePos is directly above the player at the same distance as the standard position.
		var abovePos = _player.position + Vector3.up * _relCameraPosMag;
		var leftPos = _player.position - Vector3.right * _relCameraPosMag;
		var rightPos = _player.position + Vector3.right * _relCameraPosMag;

		// An array of 5 points to check if the camera can see the player.
		var checkPoints = new Vector3[5];

		// The first is the standard position of the camera.
		checkPoints[0] = standardPos;

		// The next three are 25%, 50% and 75% of the distance between the standard position and abovePos.
		checkPoints[1] = Vector3.Lerp( standardPos, abovePos, 0.25f );
		checkPoints[2] = Vector3.Lerp( standardPos, abovePos, 0.5f );
		//checkPoints[3] = Vector3.Lerp(standardPos, abovePos, 0.75f);

		// The last is the abovePos.
		//checkPoints[4] = Vector3.Lerp(standardPos, abovePos, 0.9f); // abovePos;

		// Left and right psoitions
		checkPoints[3] = Vector3.Lerp( standardPos, leftPos, 0.25f );
		checkPoints[4] = Vector3.Lerp( standardPos, rightPos, 0.25f );

		// Run through the check points...
		for( var i = 0; i < checkPoints.Length; i++ )
		{
			// ... if the camera can see the player...
			if( ViewingPosCheck( checkPoints[i] ) )
			{
				_curCheckPoint = i;
				break;
			}
		}

		//Debug.Log("got here");
		_newPos = checkPoints[_curCheckPoint];

		// If new camera postion is unchanged but player has moved then track towards player
		//if( _newPos == transform.position && _player.position != _lastPlayerPos )
		//{
		//	Debug.Log( "got here" );
		//	_newPos = _player.position + ((_newPos - _player.position).normalized) * _newPosMag;
		//}

		// Track player movement
		if( (transform.position - _newPos).sqrMagnitude > 0.005f )
		{
			// Lerp the camera's position between it's current position and it's new position.
			transform.position = Vector3.Lerp( transform.position, _newPos, _smoothMovement * Time.deltaTime );
		}
		else
		{
			// Updating the new position to match current camera pos for checks elsewhere
			_newPos = transform.position;
		}

		// Make sure the camera is looking at the player.
		SmoothLookAt();

		// Store player position
		//_lastPlayerPos = _player.position;
	}

	//=====================================================

	private bool ViewingPosCheck( Vector3 checkPos )
	{
		RaycastHit hit;

		// If a raycast from the check position to the player hits something...
		if( Physics.Raycast( checkPos, _player.position - checkPos, out hit, _relCameraPosMag ) )
			// ... if it is not the player...
			if( hit.transform != _player )
				// This position isn't appropriate.
				return false;

		// If we haven't hit anything or we've hit the player, this is an appropriate position.
		_newPos = checkPos;
		//_newPosMag = (_player.position - _newPos).magnitude;

		return true;
	}

	//=====================================================

	private void SmoothLookAt()
	{
		// Create a vector from the camera towards the player.
		var relPlayerPosition = (_player.position + _lookAtOffset) - transform.position;

		// Create a rotation based on the relative position of the player being the forward vector.
		var lookAtRotation = Quaternion.LookRotation( relPlayerPosition, Vector3.up );

		// Lerp the camera's rotation between it's current rotation and the rotation that looks at the player.
		transform.rotation = Quaternion.Lerp( transform.rotation, lookAtRotation, _smoothLookAt * Time.deltaTime );
	}

	//=====================================================
}
