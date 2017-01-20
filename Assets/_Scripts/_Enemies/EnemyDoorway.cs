using System.Linq;
using UnityEngine;

public class EnemyDoorway : MonoBehaviour
{
	[SerializeField] private TriggerEnemyDoorway[] _doorways;

	//=====================================================

	public bool HasEnemyEnteredDoorway { get; set; }

	//=====================================================

	public void Init( ITargetWithinRange manager )
	{
		if(_doorways.Length > 0)
		{
			foreach( var doorway in _doorways )
			{
				doorway.Init( manager, this );
			}
		}
		else
		{
			Debug.LogWarning( "EnemyDoorway is missing door triggers." );
		}
	}

	//=====================================================

	void OnEnable()
	{
		HasEnemyEnteredDoorway = false;
	}

	//=====================================================

	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;

		// Draw lines between door and referenced switches
		if( _doorways.Length == 2 )
			Gizmos.DrawLine( _doorways[0].transform.position, _doorways[1].transform.position );
	}

	//=====================================================
}
