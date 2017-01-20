using UnityEngine;

[RequireComponent( typeof( SphereCollider ) )]
public class TriggerTargetInRange : MonoBehaviour
{
	private ITargetWithinRange _parent;
	private Transform _thisTransform;
	private SphereCollider _thisCollider;
	private Transform _playerInRange;
	private float _timer;
	private float _interval;
	private eLocation _location;

	//=====================================================

	public bool IsPlayerInRange { get { return CheckPlayerInRange(); } }

	//=====================================================

	public void Init( ITargetWithinRange parent )
	{
		_parent = parent;
	}

	//=====================================================

	void Awake()
	{
		_thisTransform = this.transform;
		_thisCollider = _thisTransform.GetComponent<SphereCollider>();

		_timer = 0.0f;
		_interval = 1.0f + Random.Range( 0.0f, 1.0f );
	}

	//=====================================================

	void Start()
	{
		_location = GameManager.Instance.CurrentLocation;
	}

	//=====================================================

	void Update()
	{
		if( _parent == null || _playerInRange == null ) return;

		if( (_timer -= Time.deltaTime) > 0.0f ) return;

		// Checking that player hasn't teleported out of range
		CheckPlayerInRange();

		_timer = _interval;
	}

	//=====================================================

	void OnTriggerEnter( Collider other )
	{
		if( _parent == null ) return;

		if( other.tag == UnityTags.PlayerActionTrigger )
		{
			if( other.transform.parent.tag != UnityTags.Player ) return;

			_playerInRange = other.transform;
			_parent.OnTargetWithinRange( _playerInRange, true );
		}
		else if( other.tag == UnityTags.Gem )
		{
			_parent.OnTargetWithinRange( other.transform );
		}
	}

	//=====================================================

	void OnTriggerStay( Collider other )
	{
		if( _parent == null ) return;

		// Don't update once player has been found
		if( _playerInRange != null ) return;

		if( _location == eLocation.MAIN_HALL ) return;

		// Has player teleported into range
		if( other.tag != UnityTags.PlayerActionTrigger ) return;
		
		if( other.transform.parent.tag != UnityTags.Player ) return;
		
		_playerInRange = other.transform;
		_parent.OnTargetWithinRange( _playerInRange, true );
	}

	//=====================================================

	void OnTriggerExit( Collider other )
	{
		if( _parent == null ) return;

		if( other.tag != UnityTags.PlayerActionTrigger ) return;

		_parent.OnTargetLost();
		_playerInRange = null;
	}

	//=====================================================

	//void OnDrawGizmos()
	//{
	//	Gizmos.matrix = transform.localToWorldMatrix;
	//	Gizmos.color = GetGizmoColor();
	//	Gizmos.DrawCube( Vector3.zero, Vector3.one );
	//}

	//=====================================================
	// Check if player is in range as player could teleport outwith range, not triggering OnTriggerExit
	private bool CheckPlayerInRange()
	{
		if( _playerInRange == null ) return false;

		var radius = _thisCollider.radius;

		// Reset if player outwith radius
		if( (_playerInRange.position - _thisTransform.position).sqrMagnitude > (radius * radius) )
		{
			_parent.OnTargetLost();
			_playerInRange = null;
		}

		return (_playerInRange != null);
	}

	//=====================================================
}
