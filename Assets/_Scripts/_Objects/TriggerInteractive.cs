using System.Collections;
using UnityEngine;

[RequireComponent( typeof( Collider ) )]
public class TriggerInteractive : MonoBehaviour
{
	[SerializeField] private ePlayerAction _currentAction;

	private Transform _thisTransform;
	private Color _currentColor;
	//private Color _particleColorStart;		// Used with Oblivion Portals
	private GameObject _particlePortalActive;	// Used with Oblivion Portals
	private bool _isActivated;

	//=====================================================

	public ePlayerAction TriggerAction { set { _currentAction = value; } }

	//=====================================================

	void Awake()
	{
		_thisTransform = this.transform;
		_isActivated = false;

		if( _currentAction != ePlayerAction.TELEPORT_OBLIVION_PORTAL ) return;

		var door = _thisTransform.parent.FindChild( "Door" );
		_particlePortalActive = door.FindChild( "psActivated" ).gameObject;

		if( _particlePortalActive != null )
		{
			// Deactivate extra particle fx
			_particlePortalActive.SetActive( false );
		}
	}

	//=====================================================

	void OnTriggerEnter( Collider other )
	{
		if( other.tag != UnityTags.PlayerActionTrigger ) return;

		switch( _currentAction )
		{
			default:
				PlayerManager.OnCancelAction();
				break;

			case ePlayerAction.ACTIVATE_RESPAWN:
				if( _thisTransform.parent.GetComponent<SpawnPoint>().IsActivated == true ) return;

				// Reset all spawn points to deactivate any fx that are active
				SpawnManager.Instance.ResetRespawnPoints();

				// Set this one as the current respawn point
				SpawnManager.Instance.SetCurrentRespawnPoint( _thisTransform.parent, true );
				break;

			case ePlayerAction.CLIMB_UP:
			case ePlayerAction.CLIMB_DOWN:
				PlayerManager.OnActionOk( _currentAction );
				break;

			case ePlayerAction.USE_FLOOR_LEVER:
				PlayerManager.OnActionOk( _currentAction, _thisTransform.parent.GetComponent<Switch>() );
				break;

			case ePlayerAction.USE_WALL_LEVER:
				PlayerManager.OnActionOk( _currentAction, _thisTransform.parent.GetComponent<Switch>() );
				break;

			case ePlayerAction.OPEN_DOOR:
			{
				var door = _thisTransform.parent.GetComponent<Door>();
				if( door.Type == eDoorType.PUZZLE_LOCKED && door.IsInteractionOk() )
				{
					// Override player action - auto-open door
					door.OnPlayerInteraction();
				}
				else
				{
					PlayerManager.OnActionOk( _currentAction, door );
				}
				break;
			}
			case ePlayerAction.CRAWL_THROUGH_DOOR:
			{
				var door = _thisTransform.parent.GetComponent<Door>();
				PlayerManager.OnActionOk( _currentAction, door, door.SpawnPoint );
				break;
			}
			case ePlayerAction.TELEPORT_OBLIVION_PORTAL:
			{
				if( _isActivated == true ) break;

				var portal = _thisTransform.parent.GetComponent<Door>();
				PlayerManager.OnActionOk( _currentAction, portal, portal.SpawnPoint );

				// Block trigger for short period to avoid player's collider hitting it multiple times
				// (conflict with auto-activation of portals and player running into it)
				_isActivated = true;
				StartCoroutine( DelayReset() );

				// Force teleport to activate immediately - simulates player interaction
				if( InputManager.Instance != null )
					InputManager.Instance.OnFakePerformAction();

				// Activate extra particle fx
				_particlePortalActive.SetActive( true );
				//var door = _thisTransform.parent.FindChild( "Door" );
				//var ps = door.GetComponentInChildren<ParticleSystem>();
				//_particleColorStart = ps.startColor;
				//ps.startColor = Color.green;
				break;
			}
			case ePlayerAction.ENTER_PUZZLE_ROOM:
			case ePlayerAction.LEAVE_PUZZLE_ROOM:
			case ePlayerAction.ENTER_PLAYER_HUB:
			case ePlayerAction.LEAVE_PLAYER_HUB:
			case ePlayerAction.ENTER_BOSS_ROOM:
			case ePlayerAction.LEAVE_BOSS_ROOM:
			{
				var door = _thisTransform.parent.GetComponent<Door>();
				PlayerManager.OnActionOk( _currentAction, door );
			}
				break;

			case ePlayerAction.PUSH_OBJECT:
				var pushableObject = _thisTransform.parent.GetComponent<Obstacle>();
				PlayerManager.OnActionOk( ePlayerAction.PUSH_OBJECT, pushableObject );
				break;

			//case ePlayerAction.CAST_SPELL_ATTACK:
			//case ePlayerAction.CAST_SPELL_MELT:
			//	var enemy = _thisTransform.parent.GetComponent<Enemy>();
			//	PlayerManager.OnActionOk( ePlayerAction.CAST_SPELL_ATTACK, enemy );
			//	break;

			case ePlayerAction.ESCAPE_FROM_TRAP:
				_thisTransform.parent.GetComponent<MagicalTrap>().OnActivateTrap();
				//PlayerManager.OnActionOk( _currentAction, _thisTransform.parent.GetComponent<MagicalTrap>() );
				break;

			case ePlayerAction.OPEN_CHEST:
				PlayerManager.OnActionOk( _currentAction, _thisTransform.parent.GetComponent<Chest>() );
				break;
		}
	}

	//=====================================================

	void OnTriggerStay( Collider other )
	{
		if( other.tag != UnityTags.PlayerActionTrigger ) return;
		
		if( _currentAction != ePlayerAction.PUSH_OBJECT ) return;
		
		_thisTransform.parent.GetComponent<Rigidbody>().isKinematic = !PlayerManager.IsPushing;
	}

	//=====================================================

	void OnTriggerExit( Collider other )
	{
		if( other.tag != UnityTags.PlayerActionTrigger ) return;
		
		PlayerManager.OnCancelAction();

		switch( _currentAction )
		{
			case ePlayerAction.TELEPORT_OBLIVION_PORTAL:
				StartCoroutine( DelayResetParticleFx() );
				break;

			case ePlayerAction.PUSH_OBJECT:
				// Disable box movement as player leaves box trigger
				_thisTransform.parent.GetComponent<Rigidbody>().isKinematic = true;
				break;
		}
	}

	//=====================================================

	void OnDrawGizmos()
	{
		if( _currentAction != ePlayerAction.ACTIVATE_RESPAWN &&
			_currentAction != ePlayerAction.CAST_SPELL_ATTACK &&
			_currentAction != ePlayerAction.CAST_SPELL_MELT &&
			_currentAction != ePlayerAction.CAST_SPELL_DISABLE_TRAP &&
			_currentAction != ePlayerAction.ESCAPE_FROM_TRAP )
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = GetGizmoColor();
			Gizmos.DrawCube( Vector3.zero, Vector3.one );
		}
	}

	//=====================================================

	private IEnumerator DelayReset()
	{
		yield return new WaitForSeconds( 2.0f );

		_isActivated = false;
	}

	//=====================================================

	private IEnumerator DelayResetParticleFx()
	{
		yield return new WaitForSeconds( 6.0f );

		// Deactivate extra particle fx
		_particlePortalActive.SetActive( false );

		//var door = _thisTransform.parent.FindChild( "Door" );

		//// Affect particle colour for now
		//var ps = door.GetComponentInChildren<ParticleSystem>();
		//ps.startColor = _particleColorStart;
	}

	//=====================================================

	private Color GetGizmoColor()
	{
		switch( _currentAction )
		{
			case ePlayerAction.CLIMB_UP:
				return new Color( Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.5f );
			case ePlayerAction.CLIMB_DOWN:
				return new Color( Color.green.r, Color.green.g, Color.green.b, 0.5f );
			case ePlayerAction.USE_FLOOR_LEVER:
				return new Color( Color.blue.r, Color.blue.g, Color.blue.b, 0.5f );
			case ePlayerAction.OPEN_DOOR:
			case ePlayerAction.CRAWL_THROUGH_DOOR:
			case ePlayerAction.TELEPORT_OBLIVION_PORTAL:
				return new Color( Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.5f );
			default:
				return new Color( Color.white.r, Color.white.g, Color.white.b, 0.5f );
		}
	}

	//=====================================================
}
