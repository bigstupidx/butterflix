using UnityEngine;

public class ScrollingTexture : MonoBehaviourEMS, IPauseListener
{

	[SerializeField] private float	_scrollSpeedU	= 0.0f;
	[SerializeField] private float	_scrollSpeedV	= 0.02f;

	private Material _rendererMaterial;
	private float _offsetU;
	private float _offsetV;
	private bool _isPaused;
	private float _startTime;
	private float _pauseTime;

	//=====================================================

	public void OnPauseEvent( bool isPaused )
	{
		_isPaused = isPaused;

		if( _isPaused )
		{
			_pauseTime = Time.time;
		}
		else
		{
			var pauseDuration = Time.time - _pauseTime;
			_startTime += pauseDuration;
		}
	}

	//=====================================================

	public void Reset()
	{
		_isPaused = false;
	}

	//=====================================================

	void OnEnable()
	{
		_isPaused = false;

		_rendererMaterial = GetComponent<Renderer>().material;

		if( _scrollSpeedU == 0.0f )
			_offsetU = 0;

		if( _scrollSpeedU == 0.0f )
			_offsetV = 0;
	}

	//=====================================================

	void Start()
	{
		GameManager.Instance.PauseEvent += OnPauseEvent;
	}

	//=====================================================

	void OnDisable()
	{
		if( _isAppQuiting == false )
			GameManager.Instance.PauseEvent -= OnPauseEvent;
	}

	//=====================================================

	void FixedUpdate()
	{
		if( _isPaused == false )	// GameManager.Instance.IsGameActive && 
		{
			if( _scrollSpeedU != 0 )
				_offsetU = ((Time.time - _startTime) * _scrollSpeedU) % 1;
			if( _scrollSpeedV != 0 )
				_offsetV = ((Time.time - _startTime) * _scrollSpeedV) % 1;

			_rendererMaterial.mainTextureOffset = new Vector2( _offsetU, _offsetV );
		}
	}

	//=====================================================
}