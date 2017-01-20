using UnityEngine;
using System.Collections;

public class PlayerMovement_Old : MonoBehaviour
{
	[SerializeField]
	private float _turnSmoothing = 5.0f;
	[SerializeField]
	private float _speedDampTime = 0.5f;
	[SerializeField]
	private float _maxTimeJumpAnim = 0.8f;

	private Transform _defaultParent;
	private Animator _animator;
	private CapsuleCollider _collider;

	private LayerMask _mask;
	private float _timer;

	public float CurrentMovementH { private get; set; }
	public float CurrentMovementV { private get; set; }
	public bool IsActionRequired { private get; set; }
	public bool IsClimbingUpOk { private get; set; }
	public bool IsClimbingDownOk { private get; set; }

	//[SerializeField]
	//private GameObject _target;
	//private float _rotateSpeed = 5.0f;
	//public float CurrentMouseX;

	//=====================================================

	public void ResetParentTransform()
	{
		transform.parent = _defaultParent;
	}

	//=====================================================

	void Awake()
	{
		_defaultParent = (transform.parent != null) ? transform.parent : null;
		_collider = GetComponent<CapsuleCollider>();
		_animator = GetComponent<Animator>();
		//_animator.SetLayerWeight (1, 1f);

		_mask = 1 << LayerMask.NameToLayer( "CollidableRaycast" );
		_timer = 0.0f;

		ResetActionFlags();
	}

	//=====================================================

	void FixedUpdate()
	{
		var currentStateInfo = _animator.GetCurrentAnimatorStateInfo( 0 );

		// CurrentState Idling
		if( currentStateInfo.nameHash == HashIDs.StateIdling )
		{
			if( IsActionRequired == true )
			{
				if( IsClimbingUpOk )
					EnterClimbingUpState();
				else if( IsClimbingDownOk )
					EnterClimbingDownState();
				else
					EnterJumpingState();

				IsActionRequired = false;
			}
		}
		// CurrentState Walking / Running
		else if( currentStateInfo.nameHash == HashIDs.StateLocomotion )
		{
			if( IsActionRequired == true )
			{
				if( IsClimbingUpOk )
					EnterClimbingUpState();
				else
					EnterJumpingState();

				IsActionRequired = false;
			}
		}
		else if( currentStateInfo.nameHash == HashIDs.StateJumping )
		{
			var isJumping = true;
			_timer -= Time.deltaTime;

			if( _timer > 0.0f )
			{
				if( GetComponent<Rigidbody>().velocity.y < 0.0f )
				{
					Vector3 direction = transform.TransformDirection( Vector3.down );
					RaycastHit hit;
					if( Physics.Raycast( transform.position, direction, out hit, 200.0f, _mask ) )
					{
						Debug.DrawLine( transform.position, transform.position + direction * 10.0f, Color.cyan );
						if( hit.distance < 0.25f )
							isJumping = false;
					}
				}
			}
			else
			{
				isJumping = false;
				_timer = 0.0f;
			}

			if( isJumping == false )
			{
				EnterLandingState();
			}
		}
		// CurrentState Landing
		else if( currentStateInfo.nameHash == HashIDs.StateLanding )
		{
			if( IsActionRequired == true )
			{
				//Debug.Log("Landing - Jump");
				EnterJumpingState();
				IsActionRequired = false;
			}
		}
		// CurrentState ClimbingUp
		else if( currentStateInfo.nameHash == HashIDs.StateClimbingUp )
		{
			if( !_animator.IsInTransition( 0 ) )
			{
				_collider.height = _animator.GetFloat( HashIDs.ColliderHeight );
				_collider.center = new Vector3( 0, _animator.GetFloat( HashIDs.ColliderY ), 0 );
			}

			return;
		}

		// Update player position / rotation
		MovementManagement();

		// Play step loops
		AudioManagement( currentStateInfo );
	}

	//=====================================================

	private void ResetActionFlags()
	{
		IsClimbingUpOk = false;
		IsClimbingDownOk = false;
	}

	//=====================================================

	private void EnterJumpingState()
	{
		_timer = _maxTimeJumpAnim;
		_animator.SetTrigger( HashIDs.IsJumping );
	}

	//=====================================================

	private void EnterLandingState()
	{
		_timer = 0.0f;
		_animator.SetTrigger( HashIDs.IsLanding );
	}

	//=====================================================

	private void EnterClimbingUpState()
	{
		_timer = 0.0f;
		_animator.SetTrigger( HashIDs.IsClimbingUp );

		ResetActionFlags();
	}

	//=====================================================

	private void EnterClimbingDownState()
	{
		_timer = 0.0f;
		_animator.SetTrigger( HashIDs.IsClimbingDown );

		ResetActionFlags();
	}

	//=====================================================

	private void MovementManagement()
	{
		// Monitor Input
		CurrentMovementH = Input.GetAxis( "Horizontal" );
		CurrentMovementV = Input.GetAxis( "Vertical" );
		IsActionRequired = Input.GetButtonDown( "Jump" );
		//CurrentMouseX = Input.GetAxis( "Mouse X" );

		Debug.Log(CurrentMovementH + " : " + CurrentMovementV);	// + " : " + CurrentMouseX);

		if( CurrentMovementH != 0.0f || CurrentMovementV != 0.0f )
		{
			// Direction
			var inputMagSqr = new Vector2( CurrentMovementH, CurrentMovementV ).sqrMagnitude;

			// Convert to rough range [0.0 to 1.0] - tweaked slightly (5 + 0.5) to increase lower values slightly
			inputMagSqr = Mathf.Clamp( inputMagSqr * 5.5f, 0.0f, 1.0f );

			// Rotate
			RotatePlayer( CurrentMovementH, CurrentMovementV );

			// Move - speed range [0 - 5]
			var maxSpeed = 5.0f;
			_animator.SetFloat( HashIDs.Speed, inputMagSqr * maxSpeed, _speedDampTime, Time.deltaTime );
		}
		else
		{
			_animator.SetFloat( HashIDs.Speed, 0.0f, _speedDampTime * 0.05f, Time.deltaTime );
		}
	}

	//=====================================================

	private void RotatePlayer( float horizontal, float vertical )
	{
		Vector3 targetDirection = new Vector3( horizontal, 0.0f, vertical );
		Quaternion targetRotation = Quaternion.LookRotation( targetDirection, Vector3.up );
		Quaternion newRotation = Quaternion.Lerp( GetComponent<Rigidbody>().rotation, targetRotation, _turnSmoothing * Time.deltaTime );
		GetComponent<Rigidbody>().MoveRotation( newRotation );
	}

	//=====================================================

	void AudioManagement( AnimatorStateInfo currentStateInfo )
	{
		if( currentStateInfo.nameHash == HashIDs.StateLocomotion ||
			currentStateInfo.nameHash == HashIDs.StateLanding )
		{
			// Play footstep audioclip
			if( !GetComponent<AudioSource>().isPlaying )
				GetComponent<AudioSource>().Play();
		}
		else
		{
			GetComponent<AudioSource>().Stop();
		}
	}

	//=====================================================
}