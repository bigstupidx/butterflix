using UnityEngine;
using System;
using System.Collections;
using JetBrains.Annotations;

[ExecuteInEditMode]
[Serializable]
[RequireComponent( typeof( SphereCollider ) )]
[RequireComponent( typeof( AudioSource ) )]
public class Collectable : MonoBehaviourEMS, IPauseListener, ICutsceneObject
{
	private enum eState { ACTIVE = 0, COLLECTED, LOCKED, EATEN }

	public event Action<int> ConsumedEvent;

	[SerializeField] protected eCollectable _type;
	[SerializeField] private int _model = 0;
	[SerializeField] private eState	_state;
	[SerializeField] private ePuzzleKeyType	_keyId;

	// Audio
	[SerializeField] private AudioClip _clipGem;
	[SerializeField] private AudioClip _clipRedGem;
	[SerializeField] private AudioClip _clipKey;

	// Other vars
	[SerializeField] private CutsceneContainer _cutsceneContainer;
	[SerializeField] private Transform _thisTransform;
	[SerializeField] private Transform _collectable;
	[SerializeField] private Animation _animation;
	[SerializeField] private Material _matKeyIsCollected;

	// Non-serialized
	private SphereCollider _collider;
	private ParticleSystem _particleSystem;
	private Transform _shadow;
	private AudioSource _audioSource;
	private int _gridIndex;

	//=====================================================

	#region Public Interface

	public eCollectable Type { get { return _type; } set { _type = value; } }

	public int Model { get { return _model; } set { _model = value; } }

	public bool IsAvailable { get { return (_state == eState.ACTIVE) ? true : false; } }

	public bool Unlock { set { if( _state == eState.LOCKED ) _state = eState.ACTIVE; } }

	public ePuzzleKeyType KeyId { get { return _keyId; } set { _keyId = value; } }

	public int GridIndex { get { return _gridIndex; } set { _gridIndex = value; } }

	//=====================================================

	public void Init( GameObject collectableModel )
	{
		// Remove previous model Instance
		if( _collectable != null )
			DestroyImmediate( _collectable.gameObject );

		_collectable = collectableModel.transform;
		_collectable.parent = _thisTransform;
		_collectable.name = "Collectable";

		// Get collectable's animation component
		_animation = _collectable.GetComponent<Animation>();

		// Store this platform-prefab's rotation then zero it out before updating model
		var rot = _thisTransform.rotation;
		_thisTransform.rotation = Quaternion.identity;

		// Position model
		_collectable.localPosition = Vector3.zero;
		_collectable.localRotation = Quaternion.Euler( Vector3.zero );

		// Reset the prefab rotation
		_thisTransform.rotation = rot;

		// Position above / on floor (world origin)
		_thisTransform.position = new Vector3( _thisTransform.position.x, 1.0f, _thisTransform.position.z );

		// Set tags
		switch( _type )
		{
			case eCollectable.GEM:
				_thisTransform.tag = UnityTags.Gem;
				break;

			case eCollectable.RED_GEM:
				_thisTransform.tag = UnityTags.Gem;
				break;

			case eCollectable.KEY:
				_thisTransform.tag = UnityTags.Untagged;

				var id = _keyId.ToString();
				_thisTransform.name = "Key" + id.Substring( id.Length - 3 );
				break;
		}
	}

	//=====================================================

	public void InitGemPrefab()
	{
		_collectable = _thisTransform.FindChild( "Collectable" );

		ChangeState( eState.ACTIVE );
	}

	//=====================================================
	// Eaten by Enemy
	public void OnConsumed()
	{
		if( _type == eCollectable.GEM || _type == eCollectable.RED_GEM )
			ChangeState( eState.EATEN );
	}

	//=====================================================
	// Unlock puzzle-room key once 100 gems or 8 red-gems have been collected
	public void OnUnlockSpecialKey()
	{
		if( _type == eCollectable.KEY &&
			_keyId == ePuzzleKeyType.KEY_GEM_100 ||
			_keyId == ePuzzleKeyType.KEY_GEM_RED )
		{
			ChangeState( eState.ACTIVE );
		}
	}

	//=====================================================

	public void Refresh()
	{
		UnityEngine.Object model = null;
		// Update platform-model
		if( _type == eCollectable.KEY )
		{
			var keyId = ( (int)_keyId < (int)ePuzzleKeyType.KEY_001 ) ? (int)_keyId : (int)ePuzzleKeyType.KEY_001;
			model = ResourcesCollectables.GetModel( _type, keyId );
		}
		else
		{
			model = ResourcesCollectables.GetModel( _type, _model );
		}
		
		if( model == null ) return;

		var instance = Instantiate( model ) as GameObject;
		Init( instance );
	}

	#endregion

	//=====================================================

	#region ICutsceneObject

	//=====================================================

	public void OnPlayCutsceneAnimation( int animationIndex = 0 )
	{
		// Do nothing
	}

	//=====================================================

	public LTSpline CameraPath()
	{
		if( _type == eCollectable.KEY )
			return (_cutsceneContainer != null) ? _cutsceneContainer.CameraPath : null;

		return null;
	}

	//=====================================================

	public Transform[] CutsceneCameraPoints()
	{
		if( _type == eCollectable.KEY )
			return (_cutsceneContainer != null) ? _cutsceneContainer.CameraPoints : null;

		return null;
	}

	//=====================================================

	public Transform CutsceneCameraLookAt()
	{
		if( _type == eCollectable.KEY )
			return (_cutsceneContainer != null) ? _cutsceneContainer.CameraLookAt : null;

		return null;
	}

	//=====================================================

	public Transform CutscenePlayerPoint()
	{
		if( _type == eCollectable.KEY )
			return (_cutsceneContainer != null) ? _cutsceneContainer.PlayerPoint : null;

		return null;
	}

	//=====================================================

	public float CutsceneDuration()
	{
		if( _type == eCollectable.KEY )
			return (_cutsceneContainer != null) ? _cutsceneContainer.Duration : 2.5f;

		return 0.0f;
	}

	//=====================================================

	public bool OrientCameraToPath()
	{
		if( _type == eCollectable.KEY )
			return (_cutsceneContainer != null) && _cutsceneContainer.OrientCameraToPath;

		return false;
	}

	//=====================================================

	public bool IsFlyThruAvailable()
	{
		if( _type == eCollectable.KEY )
			return (_cutsceneContainer != null) && _cutsceneContainer.IsFlyThruAvailable;

		return false;
	}

	//=====================================================

	public CameraPathAnimator GetFlyThruAnimator()
	{
		if( _type == eCollectable.KEY )
			return (_cutsceneContainer != null) ? _cutsceneContainer.FlyThruAnimator : null;

		return null;
	}

	#endregion

	//=====================================================

	#region IPauseListener

	public void OnPauseEvent( bool isPaused )
	{
		// Do nothing for now
	}

	#endregion

	//=====================================================

	#region Private Methods

	void Awake()
	{
		_thisTransform = this.transform;
		_collider = _thisTransform.GetComponentInChildren<SphereCollider>();
		_particleSystem = _thisTransform.GetComponentInChildren<ParticleSystem>();
		_shadow = _thisTransform.FindChild( "Shadow" );
		_audioSource = _thisTransform.GetComponent<AudioSource>();
	}

	//=====================================================

	protected virtual void OnEnable()
	{
		if( Application.isPlaying == false ) return;

		GameManager.Instance.PauseEvent += OnPauseEvent;

		// Set audioSource clip and get cutscene container
		switch( _type )
		{
			case eCollectable.GEM:
			case eCollectable.HEALTH_GEM:
				_audioSource.clip = _clipGem;
				_cutsceneContainer = null;
				break;

			case eCollectable.RED_GEM:
				_audioSource.clip = _clipRedGem;
				_cutsceneContainer = null;
				break;

			case eCollectable.KEY:
				_audioSource.clip = _clipKey;

				// For non-double objects (doors) find the cutscene container as a child of the 'door'
				_cutsceneContainer = _thisTransform.GetComponentInChildren<CutsceneContainer>();
				
				if( _cutsceneContainer == null )
				{
					// Otherwise, objects should have a parent that also includes a cutscene container
					_cutsceneContainer = (_thisTransform.parent != null) ?
										_thisTransform.parent.GetComponentInChildren<CutsceneContainer>() :
										null;
				}
				break;
		}

		// Set CurrentState
		switch( _type )
		{
			case eCollectable.GEM:
				ChangeState( eState.ACTIVE );
				break;

			case eCollectable.RED_GEM:
				ChangeState( eState.ACTIVE );
				break;

			case eCollectable.KEY:
				// Activate all keys except 100Gem and 8RedGem keys
				if( _keyId == ePuzzleKeyType.KEY_GEM_100 || _keyId == ePuzzleKeyType.KEY_GEM_RED )
					ChangeState( eState.LOCKED );
				else
					ChangeState( eState.ACTIVE );
				break;
		}

		// Place shadow in scene
		var layerMask = 1 << LayerMask.NameToLayer( "Collidable" ) | 1 << LayerMask.NameToLayer( "CollidableRaycast" );
		RaycastHit hit;

		if( Physics.Raycast( _thisTransform.position, Vector3.down, out hit, 100.0f, layerMask ) )
		{
			_shadow.position = _thisTransform.position - new Vector3( 0.0f, hit.distance - 0.05f, 0.0f );
		}
	}

	//=====================================================

	void OnDisable()
	{
		if( Application.isPlaying == false ) return;

		if( _isAppQuiting == true ) return;

		GameManager.Instance.PauseEvent -= OnPauseEvent;
	}

	//=====================================================

	void OnTriggerEnter( Collider other )
	{
		if( _state == eState.ACTIVE && other.tag == UnityTags.PlayerActionTrigger )
		{
			// Change to COLLECTED state
			ChangeState( eState.COLLECTED );
		}
	}

	//=====================================================

	void OnDrawGizmos()
	{
		if( _state != eState.ACTIVE || _collider == null ) return;

		switch( _type )
		{
			case eCollectable.GEM:
				Gizmos.color = new Color( Color.blue.r, Color.blue.g, Color.blue.b, 0.5f );
				break;
			case eCollectable.RED_GEM:
				Gizmos.color = new Color( Color.red.r, Color.red.g, Color.red.b, 0.5f );
				break;
			case eCollectable.KEY:
				Gizmos.color = new Color( Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.5f );
				break;
			case eCollectable.ANIMAL:
				Gizmos.color = new Color( Color.magenta.r, Color.magenta.g, Color.magenta.b, 0.5f );
				break;
			case eCollectable.HEALTH_GEM:
				Gizmos.color = new Color( Color.green.r, Color.green.g, Color.green.b, 0.5f );
				break;
		}

		Gizmos.DrawSphere( _collider.bounds.center, _collider.radius );
	}

	//=====================================================
	// Having problems with prefabs instantiated from Resources losing their private references
	private void CheckReferences()
	{
		// Try to find model if reference has been lost
		if( _collectable == null )
			_collectable = _thisTransform.FindChild( "Collectable" );

		if( _collectable == null )
		{
			Debug.Log( "CheckReferences: Collectable not found" );
			_animation = null;
			return;
		}

		// Get collectable's animation component
		_animation = _collectable.GetComponent<Animation>();
	}

	//=====================================================

	private void ChangeState( eState state )
	{
		CheckReferences();

		_state = state;

		switch( _type )
		{
			case eCollectable.GEM:
				switch( _state )
				{
					case eState.ACTIVE:
						_collectable.gameObject.SetActive( true );
						StartCoroutine( DelayedSpinCollectable() );
						break;

					case eState.COLLECTED:
						_collectable.gameObject.SetActive( false );
						_shadow.gameObject.SetActive( false );
						_particleSystem.enableEmission = true;
						_particleSystem.Play();
						_audioSource.Play();

						// Update SceneManager's temp data with collected gems
						SceneManager.AddPlayerGems( 1 );

						// Update GameDataManager's player data with health
						GameDataManager.Instance.AddPlayerHealth( Convert.ToInt32( SettingsManager.GetSettingsItem( "HEALTH_PACK_SMALL", -1 ) ) );

						// Destroy
						StartCoroutine( DestroyCollectable() );
						break;

					case eState.LOCKED:
						_collectable.gameObject.SetActive( true );
						break;

					case eState.EATEN:
						// Used by GemManager in Boss Room
						if( ConsumedEvent != null )
							ConsumedEvent( _gridIndex );

						_collectable.gameObject.SetActive( false );
						_particleSystem.enableEmission = true;
						_particleSystem.Play();

						// Destroy
						StartCoroutine( DestroyCollectable() );
						break;
				}
				break;

			case eCollectable.RED_GEM:
				switch( _state )
				{
					case eState.ACTIVE:
						_collectable.gameObject.SetActive( true );
						StartCoroutine( DelayedSpinCollectable() );
						break;

					case eState.COLLECTED:
						_collectable.gameObject.SetActive( false );
						_shadow.gameObject.SetActive( false );
						_particleSystem.enableEmission = true;
						_particleSystem.Play();
						_audioSource.Play();

						// Update SceneManager's temp data with collected gems
						SceneManager.AddPlayerRedGems( 1 );

						// Update GameDataManager's player data with health
						GameDataManager.Instance.AddPlayerHealth( Convert.ToInt32( SettingsManager.GetSettingsItem( "HEALTH_PACK_SMALL", -1 ) ) );

						// Destroy
						StartCoroutine( DestroyCollectable() );
						break;

					case eState.LOCKED:
						_collectable.gameObject.SetActive( true );
						break;

					case eState.EATEN:
						_collectable.gameObject.SetActive( false );
						_particleSystem.enableEmission = true;
						_particleSystem.Play();

						// Destroy
						StartCoroutine( DestroyCollectable() );
						break;
				}
				break;

			case eCollectable.KEY:
				switch( _state )
				{
					case eState.ACTIVE:
						_collectable.gameObject.SetActive( true );
						_shadow.gameObject.SetActive( true );

						// Switch material if key has already been collected
						if( GameDataManager.Instance.PlayerKeyIsOwned( GameManager.Instance.CurrentLocation, _keyId ) )
						{
							var materials = _collectable.GetComponentInChildren<MeshRenderer>().materials;
							materials[0] = _matKeyIsCollected;
							_collectable.GetComponentInChildren<MeshRenderer>().materials = materials;
						}

						StartCoroutine( DelayedSpinCollectable() );
						break;

					case eState.COLLECTED:
						_collectable.gameObject.SetActive( false );
						_shadow.gameObject.SetActive( false );
						_particleSystem.enableEmission = true;
						_particleSystem.Play();
						_audioSource.Play();

						// Keys are stored directly in GameDataManager
						// If new key, trigger GameManager to play cutscene then boot player from puzzle-room
						if( GameDataManager.Instance.AddPlayerKey( GameManager.Instance.CurrentLocation, _keyId ) == true )
						{
							GameManager.Instance.OnKeyCollected( this, _keyId );
							// Note: Don't destroy key's because we need to show a popup and then show a cutscene
						}
						else
						{
							// Player already has key so no cutscene and ok to destroy
							StartCoroutine( DestroyCollectable() );
						}
						break;

					case eState.LOCKED:
						_collectable.gameObject.SetActive( false );
						_shadow.gameObject.SetActive( false );
						break;
				}
				break;

			case eCollectable.HEALTH_GEM:
				switch( _state )
				{
					case eState.ACTIVE:
					case eState.LOCKED:
						_collectable.gameObject.SetActive( true );
						StartCoroutine( DelayedSpinCollectable() );
						break;

					case eState.COLLECTED:
						_collectable.gameObject.SetActive( false );
						_shadow.gameObject.SetActive( false );
						_particleSystem.enableEmission = true;
						_particleSystem.Play();
						_audioSource.Play();

						// Update GameDataManager's player data with health
						GameDataManager.Instance.AddPlayerHealth( Convert.ToInt32( SettingsManager.GetSettingsItem( "HEALTH_PACK_MEDIUM", -1 ) ) );

						// Destroy
						StartCoroutine( DestroyCollectable() );
						break;

					//case eState.EATEN:
					//	_collectable.gameObject.SetActive( false );

					//	// Destroy
					//	StartCoroutine( DestroyCollectable() );
					//	break;
				}
				break;
		}
	}

	//=====================================================

	[CanBeNull]
	private IEnumerator DelayedSpinCollectable()
	{
		yield return new WaitForSeconds( 1.0f );

		if( _animation != null )
			_animation.Play( "Spinning" );
	}

	//=====================================================

	[CanBeNull]
	private IEnumerator DestroyCollectable()
	{
		yield return new WaitForSeconds( 2.0f );

		Destroy( gameObject );
	}

	#endregion

	//=====================================================
}
