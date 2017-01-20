using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;

[ExecuteInEditMode]
[Serializable]
[RequireComponent( typeof( SphereCollider ) )]
public class MagicalTrap : MonoBehaviourEMS, IPauseListener
{
	// Serializable
	[SerializeField] private eObstacleType _type = eObstacleType.NULL;
	[SerializeField] private eDamageType _damageType = eDamageType.NULL;
	[SerializeField] private int _difficultyLevel;
	[SerializeField] private float _activeDuration = 10.0f;

	// Serializable - globes
	[SerializeField] private float _radiusX = 1.5f;
	[SerializeField] private float _radiusZ = 1.5f;
	[SerializeField] private bool _isCircular = true;
	[SerializeField] private bool _isElipse = true;

	// Audio
	[SerializeField] private AudioClip _clipActivate;
	[SerializeField] private AudioClip _clipBubbleBurst;
	[SerializeField] private AudioClip _clipDestroy;

	// Non-serialized
	private Transform _thisTransform;
	private Collider _mainCollider;
	private AudioSource _audioSource;
	private ParticleSystem _psMainFX;
	private GameObject _psDestroyedFX;
	private Transform _containerGlobes;
	private int _maskMagicalTrap;
	private bool _isPaused;
	private bool _isActivated;
	private bool _isDestroyed;
	private float _timer;

	// Non-serialzed - globes
	private List<MagicalTrapGlobe> _globes;

	//=====================================================

	#region Public Interface

	public eObstacleType Type { get { return _type; } set { _type = value; } }

	public bool IsActivated { get { return _isActivated; } private set { _isActivated = value; } }

	public int DifficultyLevel { get { return _difficultyLevel; } set { _difficultyLevel = value; } }

	public float ActiveDuration { get { return _activeDuration; } set { _activeDuration = value; } }

	public float RadiusX { get { return _radiusX; } set { _radiusX = value; } }

	public float RadiusZ { get { return _radiusZ; } set { _radiusZ = value; } }

	public bool IsCircular { get { return _isCircular; } set { _isCircular = value; } }

	public bool IsElipse { get { return _isElipse; } set { _isElipse = value; } }

	//=====================================================

	//public void Refresh()
	//{
	//	ResetName();
	//}

	//=====================================================

	public void OnHitEvent( int damage )
	{
		if( _isActivated == false )
			Destroy( this.gameObject );
	}

	//=====================================================

	public void OnGlobeHitEvent()
	{
		// Check if any globes are still active
		foreach( var globe in _globes )
		{
			if( globe.IsActive() == true )
				return;
		}

		// Player has escaped
		PlayerManager.OnTrapEscaped();

		// Destroy trap
		StartCoroutine( DestroyTrap() );
	}

	//=====================================================

	public void OnActivateTrap()
	{
		// Deactivate main collider
		_mainCollider.enabled = false;

		// Deactivate player input - taps on globes are monitored in Update()
		if( InputManager.Instance != null )
			InputManager.Instance.OnBlockInput( true );

		// Activate main particle fx
		_psMainFX.enableEmission = true;

		// Play audio fx
		if( _audioSource != null )
			_audioSource.PlayOneShot( _clipActivate );

		// Init spinning globes according to difficulty
		InitGlobes();

		// Set game duration according to difficulty
		_activeDuration -= _difficultyLevel * 1.0f;

		// Set start time
		_timer = _activeDuration;

		// Set player to trapped state and snap to centre of trap
		PlayerManager.OnTrapEntered( _thisTransform.position );

		// Point player at camera
		//var dir = Camera.mainCamera.transform.position - _thisTransform.position;
		//PlayerManager.Direction = new Vector3( dir.x, 0.0f, dir.z );

		// Activate trap
		_isActivated = true;
	}

	#endregion

	//=====================================================

	#region IPauseListener

	public void OnPauseEvent( bool isPaused )
	{
		_isPaused = isPaused;
	}

	#endregion

	//=====================================================

	#region Private Methods

	void Awake()
	{
		_thisTransform = this.transform;
		_mainCollider = _thisTransform.GetComponent<SphereCollider>();
		_audioSource = _thisTransform.GetComponent<AudioSource>();
		_psMainFX = _thisTransform.FindChild( "psMainFX" ).GetComponent<ParticleSystem>();
		_psDestroyedFX = _thisTransform.FindChild( "psDestroyedFX" ).gameObject;
		_containerGlobes = _thisTransform.FindChild( "ContainerGlobes" );
	}

	//=====================================================

	void OnEnable()
	{
		if( Application.isPlaying == true )
		{
			GameManager.Instance.PauseEvent += OnPauseEvent;

			if( _audioSource != null )
				_audioSource.clip = _clipBubbleBurst;

			_maskMagicalTrap = 1 << LayerMask.NameToLayer( "MagicalTrap" );

			// Clear or create globes-list
			if( _globes != null && _globes.Count > 0 )
				_globes.Clear();
			else
				_globes = new List<MagicalTrapGlobe>();

			_isPaused = false;
			_isActivated = false;
			_isDestroyed = false;
		}
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

	void Update()
	{
		if( _isPaused == true || _isDestroyed == true ) return;

		if( _isActivated == false ) return;
		
		// Rotate the globe container
		_containerGlobes.Rotate( Vector3.up, 90.0f * Time.deltaTime );

		// Is player attempting to tap the magical trap (fire at it)
		if( Camera.main == null ) return;

#if UNITY_EDITOR || UNITY_STANDALONE
		// Check for mouse-down events
		if( Input.GetMouseButtonDown( 0 ) )
		{
			var ray = Camera.main.ScreenPointToRay( Input.mousePosition );

			// Is player trying to attack an enemy or magical trap
			CheckForGlobeTouched( ray );
		}
#elif UNITY_ANDROID || UNITY_IPHONE
		// Check for 'touch' events
		if( Input.touchCount > 0 )
		{
			var ray = Camera.main.ScreenPointToRay( Input.GetTouch( 0 ).position );
			
			// Is player trying to attack an enemy or magical trap
			CheckForGlobeTouched( ray );
		}
#endif
		_timer -= Time.deltaTime;

		// Has time run out for player disabling trap?
		if( _timer > 0.0f ) return;
			
		// Damage player
		PlayerManager.OnObstacleHit( _damageType, Vector3.zero );

		// Destroy trap
		StartCoroutine( DestroyTrap() );
	}

	//=====================================================

	IEnumerator DestroyTrap()
	{
		_isDestroyed = true;

		// Activate player input
		if( InputManager.Instance != null )
			InputManager.Instance.OnBlockInput( false );

		// Fire remaining globe destroyed fx
		foreach( var globe in _globes )
		{
			if( globe.IsActive() == true )
				globe.OnTriggerDeath();
		}

		// Activate destroyed fx
		_psDestroyedFX.SetActive( true );

		// Play audio fx
		if( _audioSource != null )
			_audioSource.PlayOneShot( _clipDestroy );

		yield return new WaitForSeconds( 0.5f );

		// Destroy all globes
		foreach( var globe in _globes )
			Destroy( globe.gameObject );

		_globes.Clear();

		yield return new WaitForSeconds( 0.5f );

		// Destroy trap
		Destroy( this.gameObject );
	}

	//=====================================================

	//private void ResetName()
	//{
	//	// Update switch name with index e.g. Switch-Floor-0, Switch-Wall-1
	//	var name = _thisTransform.name;
	//	name = name.Substring( 0, name.LastIndexOf( "-", StringComparison.Ordinal ) + 1 ) + _index;
	//	_thisTransform.name = name;
	//}

	//=====================================================

	private void InitGlobes()
	{
		var	prefab = Resources.Load( "Prefabs/Obstacles/MagicalTraps/pfbMagicalGlobe" );

		if( prefab == null )
		{
			Debug.Log( "Globe prefab not found in Resources." );
			return;
		}

		var globePos = Vector3.zero;
		const float maxHeight = 2.0f;

		// Keeps radius on both axes the same if circular
		if( _isCircular )
		{
			_radiusZ = _radiusX;
		}

		// Set number of globes according to trap's difficulty level
		var _numGlobes = 4 + _difficultyLevel;

		// Create globes positioned around centre point
		for( var i = 0; i < _numGlobes; i++ )
		{
			// Multiply 'i' by '1.0f' to ensure the result is a fraction
			var globeDivision = (i * 1.0f) / _numGlobes;

			// Angle along the unit circle for placing points
			var angle = globeDivision * Mathf.PI * 2;

			// Axis position
			var x = Mathf.Sin( angle ) * _radiusX;
			var z = Mathf.Cos( angle ) * _radiusZ;

			// Position for the globe prefab
			if( _isElipse == true )
			{
				// Height
				var y = 0.25f + (maxHeight / _numGlobes * i);

				globePos = new Vector3( x, y, z );
			}
			else
			{
				globePos = new Vector3( x, 0.5f, z );
			}

			// Place the prefab at given position
			var globe = Instantiate( prefab, Vector3.zero, Quaternion.identity ) as GameObject;

			// Parent globe in container and store reference in globe-list
			if( globe != null )
			{
				globe.transform.parent = _containerGlobes;
				globe.transform.localPosition = globePos;

				var script = globe.GetComponent<MagicalTrapGlobe>();

				if( script != null )
				{
					// Set globe type to match this trap
					script.Init( this, _type );
					_globes.Add( script );
				}
			}
		}
	}

	//=====================================================

	private void CheckForGlobeTouched( Ray ray )
	{
		RaycastHit hit;

		// Is player trying to attack an magical trap
		if( Physics.Raycast( ray, out hit, 100.0f, _maskMagicalTrap ) )
		{
			if( hit.collider.tag != UnityTags.MagicalTrapGlobe ) return;
			
			hit.collider.GetComponent<MagicalTrapGlobe>().OnHitEvent();

			// Play audio fx
			if( _audioSource != null )
				_audioSource.Play();
		}
	}

	#endregion

	//=====================================================
}
