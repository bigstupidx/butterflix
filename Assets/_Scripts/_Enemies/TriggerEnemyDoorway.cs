using UnityEngine;

[RequireComponent( typeof( Collider ) )]
public class TriggerEnemyDoorway : MonoBehaviour
{
	[SerializeField]
	private bool _isEntrance;
	private ITargetWithinRange _manager;
	private EnemyDoorway _parent;

	//=====================================================

	public void Init( ITargetWithinRange manager, EnemyDoorway parent )
	{
		_manager = manager;
		_parent = parent;
	}

	//=====================================================

	void OnTriggerEnter( Collider other )
	{
		if( other.tag == UnityTags.Player )
		{
			if( _parent == null ) return;
			if( _parent.HasEnemyEnteredDoorway == true ) return;

			// Is player entering EnemyManager's room
			if( _manager != null && _isEntrance == true )
			{
				Debug.Log( "ENTERING" );
				_parent.HasEnemyEnteredDoorway = true;
				_manager.OnTargetWithinRange( null, true );
			}
		}
	}

	//=====================================================

	void OnTriggerExit( Collider other )
	{
		if( other.tag == UnityTags.Player )
		{
			if( _parent == null ) return;
			if( _parent.HasEnemyEnteredDoorway == false ) return;

			// Is player exiting EnemyManager's room
			if(_manager != null && _isEntrance == false)
			{
				Debug.Log( "EXITING" );
				_parent.HasEnemyEnteredDoorway = false;
				_manager.OnTargetLost();
			}
		}
	}

	//=====================================================

	void OnDrawGizmos()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = _isEntrance ? new Color( Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.5f ) : new Color( Color.red.r, Color.red.g, Color.red.b, 0.5f );
		Gizmos.DrawCube( Vector3.zero, Vector3.one );
	}

	//=====================================================
}
