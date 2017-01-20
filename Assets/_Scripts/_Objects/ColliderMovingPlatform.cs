using UnityEngine;

public class ColliderMovingPlatform : MonoBehaviour
{
	private Transform _player						= null;
	private float _timer							= 0.0f;
	private const float _delayBeforeParentingPlayer = 0.3f;

	//=====================================================

	void OnCollisionEnter( Collision collisionInfo )
	{
		if( collisionInfo.transform.tag == UnityTags.Player )
		{
			_player = collisionInfo.transform;
			_timer = _delayBeforeParentingPlayer;
		}
	}

	//=====================================================

	void OnCollisionStay( Collision collisionInfo )
	{
		if( _timer <= 0.0f ) return;
		
		_timer -= Time.deltaTime;

		if( _timer > 0.0f ) return;
		
		if( collisionInfo.transform.tag != UnityTags.Player ) return;
		
		if( _player != null )
			_player.parent = this.transform;
	}

	//=====================================================

	void OnCollisionExit( Collision collisionInfo )
	{
		if( collisionInfo.transform.tag != UnityTags.Player ) return;
		
		if( _player != null )
			_player.GetComponent<PlayerMovement>().ResetParentTransform();
			
		_player = null;
		_timer = 0.0f;
	}

	//=====================================================
}
