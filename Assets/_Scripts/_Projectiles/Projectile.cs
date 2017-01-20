using UnityEngine;

[RequireComponent( typeof( Rigidbody ) )]
public class Projectile : MonoBehaviour
{
	[SerializeField] private GameObject _pfbExplosion;
	[SerializeField] private float _lifetime = 3.0f;
	[SerializeField] private float _speed = .4f;

	private Transform _thisTransform;
	private ParticleSystem _particleActive;
	private float _startTime;
	private int _damage;

	//=====================================================

	public GameObject Explosion { set { _pfbExplosion = value; } }

	//=====================================================

	void OnEnable()
	{
		_thisTransform = this.transform;
		_startTime = Time.time;
		_damage = GameDataManager.Instance.PlayerCurrentFairySpellDamage;
	}

	//=====================================================

	void FixedUpdate()
	{
		if( Time.time - _startTime < _lifetime )
		{
			// Move projectile
			_thisTransform.position += _speed * _thisTransform.forward;
		}
		else
		{
			// Instantiate hit particle - destroy after set duration
			InstantiateHitParticle();

			// Destroy this particle
			Destroy( this.gameObject );
		}
	}

	//=====================================================

	void OnTriggerEnter( Collider other )
	{
		// Update enemy or boss that's been hit
		if( other.tag == UnityTags.Enemy )
		{
			var enemy = other.GetComponent<EnemyAI>();

			if( enemy != null )
			{
				enemy.GetComponent<EnemyAI>().OnHitEvent( _damage );
			}
			else
			{
				var boss = other.GetComponent<BossManager>();
					
				if( boss != null )
					boss.OnHitEvent( _damage );
			}

			// Instantiate hit particle
			InstantiateHitParticle();

			// Destroy this projectile
			Destroy( this.gameObject );
		}
		else if( other.tag == UnityTags.MagicalTrap )
		{
			other.GetComponent<MagicalTrap>().OnHitEvent( _damage );

			// Instantiate hit particle
			InstantiateHitParticle();

			// Destroy this projectile
			Destroy( this.gameObject );
		}
	}

	//=====================================================

	private void InstantiateHitParticle()
	{
		// Instantiate hit particle - destroy after set duration
		var ps = Instantiate( _pfbExplosion, _thisTransform.position, Quaternion.identity ) as GameObject;

		if( ps != null )
			Destroy( ps, ps.GetComponent<ParticleSystem>().duration );
	}

	//=====================================================
}
