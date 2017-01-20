using UnityEngine;

[RequireComponent( typeof( Collider ) )]
public class ColliderObstacle : MonoBehaviour
{
	[SerializeField] private eDamageType _damageType = eDamageType.NULL;

	private ICanAttackTarget _parent;

	//=====================================================

	public void Init( ICanAttackTarget parent )
	{
		_parent = parent;
	}

	//=====================================================

	void OnCollisionEnter( Collision collisionInfo )
	{
		if( collisionInfo.transform.tag == UnityTags.Player )
		{
			// If parent object has registered check if it's ok to attck the player
			if(_parent != null)
			{
				if(_parent.IsAttackingTargetOk())
					PlayerManager.OnObstacleHit( _damageType, collisionInfo.contacts[0].point );
			}
			else
			{
				PlayerManager.OnObstacleHit( _damageType, collisionInfo.contacts[0].point );
			}
		}
	}

	//=====================================================

	//void OnCollisionStay( Collision collisionInfo )
	//{
		
	//}

	//=====================================================

	void OnCollisionExit( Collision collisionInfo )
	{
		if( collisionInfo.transform.tag == UnityTags.Player )
			PlayerManager.OnObstacleHit( eDamageType.NULL, Vector3.zero );
	}

	//=====================================================
}
