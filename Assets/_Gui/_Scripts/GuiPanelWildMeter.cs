using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent( typeof( Animator ) )]
[RequireComponent( typeof( AudioSource ) )]
[RequireComponent( typeof( Image ) )]
public class GuiPanelWildMeter : MonoBehaviourEMS, IPauseListener
{
	[SerializeField] private Sprite[] _fairyGemsSelected;
	[SerializeField] private GameObject[] _fairyGemsOwned;
	[SerializeField] private AudioClip _clipWMPositive;
	[SerializeField] private AudioClip _clipWMNegative;
	[SerializeField] private AudioClip _clipBtnClick;
	[SerializeField] private AudioClip _clipAishaIntro;
	[SerializeField] private AudioClip _clipBloomIntro;
	[SerializeField] private AudioClip _clipFloraIntro;
	[SerializeField] private AudioClip _clipMusaIntro;
	[SerializeField] private AudioClip _clipStellaIntro;
	[SerializeField] private AudioClip _clipTecnaIntro;

	private RectTransform _wildMagicDial;
	private Animator _animator;
	private Image _imgCurrentFairyGem;
	private AudioSource _audioSource;
	private bool _isPanelOpen;
	private bool _isPaused;
	private float _wildMagicRate;
	private float _lastWildMagicRate;
	private float _lastTimeFairySelected;
	private float _maxWildMagicRate;

	//=====================================================

	public void OnPauseEvent( bool isPaused )
	{
		_isPaused = isPaused;
	}

	//=====================================================

	public void OnOpenPanel()
	{
		_isPanelOpen = !_isPanelOpen;

		// Open / close panel
		_animator.SetBool( HashIDs.IsOpen, _isPanelOpen );

		if( _audioSource != null )
			_audioSource.PlayOneShot( _clipBtnClick );
	}

	//=====================================================

	public void OnBloomSelected()
	{
		OnSelectFairy( eFairy.BLOOM );
	}

	//=====================================================

	public void OnStellaSelected()
	{
		OnSelectFairy( eFairy.STELLA );
	}

	//=====================================================

	public void OnFloraSelected()
	{
		OnSelectFairy( eFairy.FLORA );
	}

	//=====================================================

	public void OnMusaSelected()
	{
		OnSelectFairy( eFairy.MUSA );
	}

	//=====================================================

	public void OnTecnaSelected()
	{
		OnSelectFairy( eFairy.TECNA );
	}

	//=====================================================

	public void OnAishaSelected()
	{
		OnSelectFairy( eFairy.AISHA );
	}

	//=====================================================

	void Awake()
	{
		_animator = transform.GetComponent<Animator>();
		_audioSource = transform.GetComponent<AudioSource>();
		_wildMagicDial = transform.FindChild( "ImgWildMagicDial" ).GetComponent<RectTransform>();
		_imgCurrentFairyGem = transform.FindChild( "BtnCurrentFairyGem" ).GetComponent<Image>();

		_isPanelOpen = false;
		_isPaused = false;
		_wildMagicRate = _lastWildMagicRate = 0.0f;
		_lastTimeFairySelected = 0.0f;
	}

	//=====================================================

	private void OnEnable()
	{
		//GameManager.Instance.PauseEvent += OnPauseEvent;
		GameDataManager.Instance.PlayerWildMagicEvent += OnPlayerWildMagicEvent;

		// Set current gem to match player's current fairy
		_imgCurrentFairyGem.sprite = _fairyGemsSelected[GameDataManager.Instance.PlayerCurrentFairy];
	}

	//=====================================================

	private void OnDisable()
	{
		if( _isAppQuiting == true ) return;

		//	GameManager.Instance.PauseEvent -= OnPauseEvent;
		GameDataManager.Instance.PlayerWildMagicEvent -= OnPlayerWildMagicEvent;
	}

	//=====================================================

	void Start()
	{
		_maxWildMagicRate = WildMagicItemsManager.GetWildMagicItemValue( "WILD_MAGIC_RATE_MAX" );

		SetFairyGemsOwned();
	}

	//=====================================================

	private void OnSelectFairy( eFairy fairy )
	{
		// Block these events for short period
		if( Time.time - _lastTimeFairySelected < 2.5f ) return;

		_lastTimeFairySelected = Time.time;

		_imgCurrentFairyGem.sprite = _fairyGemsSelected[(int)fairy];

		if( GameManager.Instance.OnChangeFairy( fairy ) == false ) return;

		// Play sfx
		if( _audioSource == null ) return;
		
		_audioSource.PlayOneShot( _clipBtnClick );

		switch( fairy )
		{
			case eFairy.AISHA:
				_audioSource.PlayOneShot( _clipAishaIntro );
				break;
			case eFairy.BLOOM:
				_audioSource.PlayOneShot( _clipBloomIntro );
				break;
			case eFairy.FLORA:
				_audioSource.PlayOneShot( _clipFloraIntro );
				break;
			case eFairy.MUSA:
				_audioSource.PlayOneShot( _clipMusaIntro );
				break;
			case eFairy.STELLA:
				_audioSource.PlayOneShot( _clipStellaIntro );
				break;
			case eFairy.TECNA:
				_audioSource.PlayOneShot( _clipTecnaIntro );
				break;
		}
	}

	//=====================================================

	public void OnPlayerWildMagicEvent( float maxWildMagicRate, float currentWildMagicRate )
	{
		if( _audioSource != null )
		{
			// Play positive wild magic sfx
			if( _lastWildMagicRate < 0 && currentWildMagicRate > 0 )				
				_audioSource.PlayOneShot( _clipWMPositive );
			// Play negative wild magic sfx
			else if( _lastWildMagicRate > 0 && currentWildMagicRate < 0 )
				_audioSource.PlayOneShot( _clipWMNegative );
		}

		_lastWildMagicRate = _wildMagicRate;
		_wildMagicRate = currentWildMagicRate;

		SetWildMagicMeter();
	}

	//=====================================================

	private void SetWildMagicMeter()
	{
		// Ensure we're not dividing by zero (assumes spreadsheet value exists / isn't zero)
		if( Math.Abs( _maxWildMagicRate ) < 0.01f )
			_maxWildMagicRate = WildMagicItemsManager.GetWildMagicItemValue( "WILD_MAGIC_RATE_MAX" );

		// Update meter rotation (meter range [-90 | 90]) (original wild magic rate range [-100 | 100])
		//Debug.Log( "Dial Z: " + (_wildMagicRate * (1.0f / _maxWildMagicRate) * 90.0f ) );
		_wildMagicDial.localEulerAngles = new Vector3( 0.0f, 0.0f, _wildMagicRate * ( 1.0f / _maxWildMagicRate ) * 90.0f );

		// DEBUG - REMOVE THIS
		//GuiManager.Instance.TxtWildMagicRate = _wildMagicRate.ToString( "##.0" );
	}

	//=====================================================

	private void SetFairyGemsOwned()
	{
		var fairiesOwned = GameDataManager.Instance.PlayerFairiesOwned;

		if( fairiesOwned == null ) { Debug.Log( "NO FAIRIES OWNED" ); return; }

		// Activate gui gems according to fairies currently owned
		for( var i = 0; i < fairiesOwned.Length; i++ )
		{
			//Debug.Log( ": " + (fairiesOwned[i] > 0 ? 1 : 0) );
			_fairyGemsOwned[i].SetActive( fairiesOwned[i] > 0 );
		}

		// *************************
		// ToDo: DEBUG - REMOVE THIS
		//GameDataManager.Instance.BuyFairy( eFairy.STELLA );
		//_fairyGemsOwned[1].SetActive( true );
		//GameDataManager.Instance.BuyFairy( eFairy.FLORA );
		//_fairyGemsOwned[2].SetActive( true );
		//GameDataManager.Instance.BuyFairy( eFairy.MUSA );
		//_fairyGemsOwned[3].SetActive( true );
		//GameDataManager.Instance.BuyFairy( eFairy.AISHA );
		//_fairyGemsOwned[4].SetActive( true );
		//GameDataManager.Instance.BuyFairy( eFairy.TECNA, true );
		//_fairyGemsOwned[5].SetActive( true );

		// *************************
	}

	//=====================================================
}
