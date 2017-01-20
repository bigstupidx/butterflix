using UnityEngine;
using System.Collections;

[RequireComponent( typeof(SphereCollider) )]
public class MagicalTrapGlobe : MonoBehaviourEMS, IPauseListener
{
	[SerializeField] private ParticleSystem _psIceFx;
	[SerializeField] private ParticleSystem _psWindFx;

	private GameObject _psDestroyedFx;
	private MagicalTrap _parent;
	private SphereCollider _collider;
	private eObstacleType _type = eObstacleType.NULL;

	//=====================================================

	public void Init( MagicalTrap parent, eObstacleType type )
	{
		_parent = parent;
		_type = type;

		// ToDo: select / activate particle emmisions
		switch( _type )
		{
			case eObstacleType.MAGICAL_TRAP_ICE:
				_psIceFx.enableEmission = true;
				break;
			case eObstacleType.MAGICAL_TRAP_WIND:
				_psWindFx.enableEmission = true;
				break;
		}
	}

	//=====================================================

	public bool IsActive()
	{
		return ( _collider.enabled );
	}

	//=====================================================

	public void OnHitEvent()
	{
		OnTriggerDeath();

		// Tell trap this globe has been deactivated
		_parent.OnGlobeHitEvent();
	}

	//=====================================================

	public void OnTriggerDeath()
	{
		// Disable collider
		_collider.enabled = false;

		// Disable particle emmision
		switch( _type )
		{
			case eObstacleType.MAGICAL_TRAP_ICE:
				if( _psIceFx != null )
					_psIceFx.enableEmission = false;
				break;
			case eObstacleType.MAGICAL_TRAP_WIND:
				if( _psWindFx != null )
					_psWindFx.enableEmission = false;
				break;
		}

		// Play destroyed fx
		_psDestroyedFx.SetActive( true );
	}

	//=====================================================

	public void OnPauseEvent( bool isPaused )
	{
		// ToDo: pause particles?
	}

	//=====================================================

	void Awake()
	{
		_collider = this.transform.GetComponent<SphereCollider>();
		_psDestroyedFx = this.transform.FindChild( "psDestroyedFX" ).gameObject;
	}

	//=====================================================

	void OnEnable()
	{
		if( Application.isPlaying )
			GameManager.Instance.PauseEvent += OnPauseEvent;
	}

	//=====================================================

	void OnDisable()
	{
		if( Application.isPlaying )
		{
			if( _isAppQuiting == false )
				GameManager.Instance.PauseEvent -= OnPauseEvent;
		}
	}

	//=====================================================
}
