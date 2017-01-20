using UnityEngine;

public class TriggerBossShieldEnemy : MonoBehaviour {

	public void OnTriggerEnter( Collider other )
	{
		if( other.tag != UnityTags.Enemy ) return;

		// Stun any enemy that bumps into the shield
		var enemy = other.GetComponent<EnemyAI>();
		enemy.OnHitEvent( 0 );
	}
}
