using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent( typeof( Image ) )]
public class GuiPanelHealth : MonoBehaviourEMS, IPauseListener
{
	[SerializeField]
	private GameObject _pfbLifeIcon;

	private Transform _thisTransform;
	private RectTransform _healthBar;
	private List<GameObject> _lifeIcons;
	private Image _imgCurrentFairyGem;
	private bool _isPaused;
	private int _maxLives;
	private int _currentLives;
	private int _maxHealth;
	private int _currentHealth;

	private const int _maxHealthBarWidth = 350;

	//=====================================================

	public void OnUpdateL( bool isPaused )
	{
		_isPaused = isPaused;
	}

	//=====================================================

	public void OnPauseEvent( bool isPaused )
	{
		_isPaused = isPaused;
	}

	//=====================================================

	void Awake()
	{
		_thisTransform = this.transform;
		_healthBar = _thisTransform.FindChild( "ImgHealthBar" ).GetComponent<RectTransform>();

		_lifeIcons = new List<GameObject>();
		_isPaused = false;
	}

	//=====================================================

	void OnEnable()
	{
		//GameManager.Instance.PauseEvent += OnPauseEvent;
		GameDataManager.Instance.PlayerLifeEvent += OnPlayerLifeEvent;
		GameDataManager.Instance.PlayerHealthEvent += OnPlayerHealthEvent;
	}

	//=====================================================

	private void OnDisable()
	{
		if( _isAppQuiting == true ) return;

		//	GameManager.Instance.PauseEvent -= OnPauseEvent;
		GameDataManager.Instance.PlayerLifeEvent -= OnPlayerLifeEvent;
		GameDataManager.Instance.PlayerHealthEvent -= OnPlayerHealthEvent;
	}

	//=====================================================

	void Start()
	{
		_maxLives = GameDataManager.Instance.PlayerMaxLives;
		_maxHealth = GameDataManager.Instance.PlayerMaxHealth;

		SetLifeIcons();
		SetHealthBar();
	}

	//=====================================================

	private void OnPlayerLifeEvent( int maxLives, int currentLives )
	{
		_maxLives = maxLives;
		_currentLives = currentLives;

		SetLifeIcons();
	}

	//=====================================================

	private void OnPlayerHealthEvent( int maxHealth, int currentHealth )
	{
		_maxHealth = maxHealth;
		_currentHealth = currentHealth;

		SetHealthBar();
	}

	//=====================================================

	private void SetLifeIcons()
	{
		if( _lifeIcons == null )
			_lifeIcons = new List<GameObject>();

		if( _lifeIcons.Count > 0 )
		{
			foreach( var icon in _lifeIcons )
			{
				if( icon != null )
					Destroy( icon );
			}

			_lifeIcons.Clear();
		}

		var positions = new Vector3[_maxLives];

		// Position them all off of zero then move them into position
		for( var i = 0; i < positions.Length; i++ )
			positions[i] = new Vector3( i * ((float)_maxHealthBarWidth / _maxLives), 0.0f, 0.0f );

		// Offset from new positions mid-point
		var offset = Vector3.zero;
		if( _maxLives % 2 == 0 )
			offset = (positions[(_maxLives / 2) - 1] + positions[_maxLives / 2]) * 0.5f;
		else
			offset = positions[_maxLives / 2];

		// Apply offset
		for( var i = 0; i < positions.Length; i++ )
			positions[i] -= new Vector3( offset.x, 25.0f, 0.0f );

		// Instantiate icons for available lives and set position
		for( var i = 0; i < _currentLives; i++ )
		{
			_lifeIcons.Add( Instantiate( _pfbLifeIcon, Vector3.zero, Quaternion.identity ) as GameObject );

			_lifeIcons[i].name = "Life" + i.ToString( "00" );
			_lifeIcons[i].transform.SetParent( _thisTransform, false );
			_lifeIcons[i].transform.localPosition = positions[i];
		}
	}

	//=====================================================

	private void SetHealthBar()
	{
		// Set bar length
		_healthBar.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal,
											  (int)(_maxHealthBarWidth * ((float)_currentHealth / _maxHealth)) );
	}

	//=====================================================
}
