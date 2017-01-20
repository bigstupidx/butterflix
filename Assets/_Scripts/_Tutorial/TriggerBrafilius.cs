using UnityEngine;

[RequireComponent( typeof( Collider ) )]
public class TriggerBrafilius : MonoBehaviour
{
	[SerializeField] private eTutorial _nextTutorial;
	[SerializeField] private Door _bossDoor;
	private Transform _enemyLocation;
	private Animator _animator;
	private bool _isNextTutorialActive;

	//=====================================================

	void Awake()
	{
		_enemyLocation = transform.GetComponentInChildren<PathNode>().transform;
		_animator = null;
		_isNextTutorialActive = false;
	}

	//=====================================================

	void OnTriggerEnter( Collider other )
	{
		if( _isNextTutorialActive == true ) return;

		// Brafilius enters trigger - snap and rotate into position
		if( other.tag == UnityTags.Enemy )
		{
			_animator = other.transform.GetComponent<Animator>();
			if( _animator )
				_animator.SetTrigger( HashIDs.Idle );

			other.transform.position = _enemyLocation.position;
			other.transform.rotation = _enemyLocation.rotation;

			return;
		}

		if( other.tag != UnityTags.Player ) return;

		if( _animator == null ) return;

		// Player enters trigger - start next Brafilius animation
		switch( _nextTutorial )
		{
			case eTutorial.TUTORIAL01:
				_animator.SetTrigger( HashIDs.Tutorial01 );
				break;

			case eTutorial.TUTORIAL02:
				_animator.SetTrigger( HashIDs.Tutorial02 );
				break;

			case eTutorial.TUTORIAL03:
				_animator.SetTrigger( HashIDs.Tutorial03 );
				break;

			case eTutorial.TUTORIAL04:
				_animator.SetTrigger( HashIDs.Tutorial04 );
				_bossDoor.OnPlayerInteraction();
				break;
		}

		_isNextTutorialActive = true;
	}

	//=====================================================

	void OnDrawGizmos()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = new Color( Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.5f );
		Gizmos.DrawCube( Vector3.zero, Vector3.one );
	}

	//=====================================================
}
