using UnityEngine;
using System;

[ExecuteInEditMode]
[Serializable]
[RequireComponent( typeof( AudioSource ) )]
public class Chest : MonoBehaviourEMS, IPauseListener, IPlayerInteraction
{
	// Editable in inspector
	[SerializeField] private eChestType _type = eChestType.SMALL;
	[SerializeField] private int _model = 0;
	[SerializeField] private eTradingCardClassification _cardClassification = eTradingCardClassification.STANDARD;
	[SerializeField] private eTradingCardRarity _cardRarity = eTradingCardRarity.COMMON;
	[SerializeField] private eTradingCardCondition _cardCondition = eTradingCardCondition.MINT;
	[SerializeField] private eSwitchItem _rewardSwitchItem = eSwitchItem.NULL;	// Switch switchItem for large chest only

	// Audio
	[SerializeField] private AudioClip _clipOpen;
	[SerializeField] private AudioClip _clipReward;

	// Other vars
	[SerializeField] private Transform _chest;
	[SerializeField] private Transform _trigger;
	[SerializeField] private Collider _collider;
	[SerializeField] private Animator _animator;
	[SerializeField] private int _interactiveLevel = 0;
	[SerializeField] private ChestReward _reward;

	// Non-serialized
	private Transform _thisTransform;
	private AudioSource _audioSource;
	private Transform _interactPosition;
	private ParticleSystem _psChestOpenFx;
	private ChestItem _chestReward;
	private bool _isOpen;
	//private bool _checkAnimIsFinished;
	private bool _isRewardCollected;

	//=====================================================

	#region Public Interface

	public eChestType Type { get { return _type; } set { _type = value; } }

	public int Model { get { return _model; } set { _model = value; } }

	public eTradingCardClassification CardClassification { get { return _cardClassification; } set { _cardClassification = value; } }

	public eTradingCardRarity CardRarity { get { return _cardRarity; } set { _cardRarity = value; } }

	public eTradingCardCondition CardCondition { get { return _cardCondition; } set { _cardCondition = value; } }

	public bool IsOpen { get { return _isOpen; } private set { _isOpen = value; } }

	public int InteractiveLevel { get { return _interactiveLevel; } set { _interactiveLevel = value; } }

	//=====================================================

	public void Init( GameObject chestModel )
	{
		// Remove previous chest instance
		if( _chest != null )
			DestroyImmediate( _chest.gameObject );

		_chest = chestModel.transform;
		_chest.parent = _thisTransform;
		_chest.name = "Chest";

		CheckReferences();

		// Store this chest-prefab's rotation then zero it out before updating switch
		var rot = _thisTransform.rotation;
		_thisTransform.rotation = Quaternion.identity;

		// Position model
		_chest.localPosition = Vector3.zero;
		_chest.localRotation = Quaternion.Euler( Vector3.zero );

		// Initialize trigger
		InitTrigger();

		// Reset the door prefab rotation
		_thisTransform.rotation = rot;
	}

	//=====================================================

	public void Refresh()
	{
		CheckReferences();

		// Update chest-model
		//Debug.Log( "Model type: " + _type + " : " + _model );
		var mdlChest = ResourcesChests.GetModel( _type, _model );
		var chestModel = Instantiate( mdlChest ) as GameObject;
		Init( chestModel );

		// Special Case: editor changes only update _rewardSwitchItem because of enumerated type so forcing update on _reward
		// Do we need to set the chest-reward?
		if( _reward == null )
		{
			// Set reward (gems or a card). Special 'key' items only set in inspector for large chests (NULL forces normal reward setup)
			_rewardSwitchItem = eSwitchItem.NULL;
			SetRewardItem( _rewardSwitchItem, _cardClassification, _cardRarity, _cardCondition );
		}
		else
		{
			if( _reward.SwitchItem != _rewardSwitchItem )
				SetRewardItem( _rewardSwitchItem, _cardClassification, _cardRarity, _cardCondition );
		}
	}

	//=====================================================

	public void OnPlayCollectRewardSfx()
	{
		if( _isRewardCollected == true ) return;

		// Play sfx
		if( _audioSource != null )
			_audioSource.PlayOneShot( _clipReward );
	}

	//=====================================================

	public void OnCollectReward()
	{
		if( _isRewardCollected == true ) return;

		// Update SceneManager's data with chest-reward
		SceneManager.AwardPlayer( _reward );

		// Disable reward
		_chestReward.OnCollectReward();

		_isRewardCollected = true;
	}

	#endregion

	//=====================================================

	#region IPlayerInteraction

	public bool IsInteractionOk()
	{
		// Block player-object interaction if current fairy level is too low
		if( _interactiveLevel > GameDataManager.Instance.PlayerCurrentFairyLevel )
			return false;

		// Allow if chest is not open
		return !_isOpen;
	}

	//=====================================================

	public Transform OnPlayerInteraction()
	{
		PlayerManager.Position = _interactPosition.position;
		PlayerManager.Direction = _chest.transform.position - _interactPosition.position;

		OnOpenChest();

		// Managing player position relative to chest - transform not required
		return null;
	}

	//=====================================================

	public void OnPlayCutsceneAnimation( int animationIndex = 1 )
	{
		// No cutscene for this object
	}

	//=====================================================

	public LTSpline CameraPath()
	{
		return null;
	}

	//=====================================================

	public Transform[] CutsceneCameraPoints()
	{
		// No cutscene for this object
		return null;
	}

	//=====================================================

	public Transform CutsceneCameraLookAt()
	{
		// No cutscene for this object
		return null;
	}

	//=====================================================

	public Transform CutscenePlayerPoint()
	{
		// No cutscene for this object
		return null;
	}

	//=====================================================

	public float CutsceneDuration()
	{
		// No cutscene for this object
		return 2.5f;
	}

	//=====================================================

	public bool OrientCameraToPath()
	{
		// No cutscene for this object
		return false;
	}

	//=====================================================

	public bool IsFlyThruAvailable()
	{
		// No cutscene for this object
		return false;
	}

	//=====================================================

	public CameraPathAnimator GetFlyThruAnimator()
	{
		// No cutscene for this object
		return null;
	}

	#endregion

	//=====================================================

	#region IPauseListener

	public void OnPauseEvent( bool isPaused )
	{
		CheckReferences();

		_animator.speed = (isPaused) ? 0.0f : 1.0f;
		_chestReward.OnPause( isPaused );
	}

	#endregion

	//=====================================================

	#region Private Methods

	void Awake()
	{
		_thisTransform = this.transform;
		_audioSource = _thisTransform.GetComponent<AudioSource>();
		_interactPosition = _thisTransform.FindChild( "InteractPosition" );
		_psChestOpenFx = _thisTransform.FindChild( "psChestOpenFx" ).GetComponent<ParticleSystem>();
		_chestReward = _thisTransform.FindChild( "ChestItem" ).GetComponent<ChestItem>();

		_isRewardCollected = false;

		// Set interactive position (offset for player from chest)
		switch( _type )
		{
			case eChestType.SMALL:
				_interactPosition.localPosition = new Vector3( 0.0f, 0.0f, 1.0f );
				break;
			case eChestType.MEDIUM:
				_interactPosition.localPosition = new Vector3( 0.0f, 0.0f, 1.15f );
				break;
			case eChestType.LARGE:
				_interactPosition.localPosition = new Vector3( 0.0f, 0.0f, 1.3f );
				break;
		}
	}

	//=====================================================

	void OnEnable()
	{
		if( Application.isPlaying == false ) return;

		GameManager.Instance.PauseEvent += OnPauseEvent;

		_audioSource.clip = _clipOpen;

		_isOpen = false;
		//_checkAnimIsFinished = false;
	}

	//=====================================================

	void OnDisable()
	{
		if( Application.isPlaying == false ) return;

		if( _isAppQuiting == true ) return;

		GameManager.Instance.PauseEvent -= OnPauseEvent;
	}

	//=====================================================

	void Start()
	{
		// Do we need to set the chest-reward?
		if( _reward != null ) return;

		// Set reward (gems or a card). Special 'key' items only set in inspector for large chests (NULL forces normal reward setup)
		SetRewardItem( _rewardSwitchItem, _cardClassification, _cardRarity, _cardCondition );
	}

	//=====================================================

	void Update()
	{
#if UNITY_EDITOR
		// DEBUG - REMOVE THIS
		if( Input.GetKeyDown( KeyCode.H ) )
			OnOpenChest();
#endif
		// When switch anim completes send message to attched door
		//if( _checkAnimIsFinished )
		//	CheckSwitchAnimCompleted();
	}

	//=====================================================

	private void OnOpenChest()
	{
		CheckReferences();

		// Play chest-open animation
		_animator.SetTrigger( HashIDs.IsChestOpen );

		// Play reward animation
		_chestReward.gameObject.SetActive( true );
		_chestReward.OnChestOpen();

		// Play particle and audio fx
		_psChestOpenFx.Play();
		_audioSource.Play();

		// Set flags
		_isOpen = true;
		//_checkAnimIsFinished = true;

		// Disable trigger to avoid player's attempting to interact with chest after it's been opened
		_trigger.gameObject.SetActive( false );
	}

	//=====================================================

	//private void CheckSwitchAnimCompleted()
	//{
	//	if( _animator == null )
	//		_animator = _chest.GetComponent<Animator>();

	//	// Get current state's info (0 : on Base Layer)
	//	var currentStateInfo = _animator.GetCurrentAnimatorStateInfo( 0 );

	//	if( currentStateInfo.normalizedTime >= 1 && !_animator.IsInTransition( 0 ) )
	//	{
	//		if( currentStateInfo.nameHash == HashIDs.StateChestOpenHash )
	//		{
	//			// Update SceneManager's temp data with Chest-reward
	//			SceneManager.AwardPlayer( _reward );

	//			_checkAnimIsFinished = false;
	//		}
	//	}
	//}

	//=====================================================

	private void InitTrigger()
	{
		var triggerSize = new Vector3( 1.0f, 0.25f, 1.2f );

		_trigger = _thisTransform.FindChild( "TriggerOpenChest" );

		if( _collider != null )
		{
			var chestWidth = _collider.bounds.size.x;
			triggerSize = new Vector3( chestWidth, triggerSize.y, triggerSize.z );
		}

		// Set trigger position
		_trigger.localScale = triggerSize;
		_trigger.localPosition = new Vector3( 0.0f, _trigger.localScale.y * 0.5f, _trigger.localScale.z * 0.6f );
	}

	//=====================================================

	private void SetRewardItem( eSwitchItem switchItem,
								eTradingCardClassification cardClassification,
								eTradingCardRarity cardRarity,
								eTradingCardCondition cardCondition )
	{
		if( switchItem == eSwitchItem.NULL )
		{
			// Reset reward to gems or a card
			_reward = SceneManager.GetChestReward( _type, cardClassification, cardRarity, cardCondition );
		}
		else
		{
			if( _reward == null )
				_reward = new ChestReward();

			// Clear other reward items if switchItem is set
			_reward.Gems = 0;
			_reward.Card = null;
			_reward.SwitchItem = switchItem;
		}
		//Debug.Log( "Chest: " + _type + " : " + _reward );

		// Initialise chest switchItem as reward
		if( _chestReward != null && _reward != null )
			_chestReward.Init( _reward );
	}

	//=====================================================
	// Having problems with prefabs instantiated from Resources losing their private references
	private void CheckReferences()
	{
		_thisTransform = transform;

		// Try to find chest if reference has been lost
		if( _chest == null )
			_chest = _thisTransform.FindChild( "Chest" );

		if( _chest != null )
		{
			// Get animator to control chest animations
			if( _animator == null )
			{
				_animator = _chest.GetComponent<Animator>();
				if( _animator == null )
					Debug.Log( "CheckReferences: Chest Animator not found" );
			}

			// Get collider to determine trigger size
			if( _collider != null ) return;
			
			_collider = _chest.GetComponent<Collider>();
			if( _collider != null ) return;
			
			// Check children for collider
			var colliders = _chest.GetComponentsInChildren<Collider>();
			if( colliders[0] != null )
				_collider = colliders[0];

			if( _collider == null )
				Debug.Log( "CheckReferences: Chest Collider not found" );
		}
		else
		{
			Debug.Log( "CheckReferences: Chest not found" );
		}
	}

	//=====================================================

	//private Color GetGizmoColor()
	//{
	//	switch( _switchType )
	//	{
	//		case eSwitchType.FLOOR_LEVER:
	//		case eSwitchType.WALL_SWITCH:
	//			return Color.blue;
	//		default:
	//			return Color.white;
	//	}
	//}

	#endregion
}
