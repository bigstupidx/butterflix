using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent( typeof( AudioSource ) )]
public class AudioManager : MonoBehaviour
{
	// Static Instance
	public static AudioManager	_instance;

	[SerializeField] private AudioClip[] _tracks;
	[SerializeField] private AudioMixerSnapshot	_audioSnapshotPaused;
	[SerializeField] private AudioMixerSnapshot	_audioSnapshotNormal;
	[SerializeField] private AudioMixerSnapshot	_audioSnapshotNoAudio;
	[SerializeField] private AudioMixerSnapshot	_audioSnapshotNoMusic;
	[SerializeField] private AudioMixerSnapshot	_audioSnapshotNoSFX;

	private	AudioSource _audioSource;
	
	private bool _isInitialized;
	private bool _isMusicOn;
	private bool _isSFXOn;
	private bool _isPaused;
	private int _currentTrack;
	//private bool _isInTransition;
	//private	int _transitionDir;
	//private	float _transitionVolume;


	//=====================================================

	#region Public Interface

	//public bool IsMusicOn { get { return _isMusicOn; } }

	//public bool IsSFXOn { get { return _isSFXOn; } }

	//=====================================================

	// Create an instance of the audio manager
	public static AudioManager Instance
	{
		get
		{
			if( _instance != null ) return _instance;

			// Look for existing AudioManager object in scene
			var gm = GameObject.FindGameObjectWithTag( UnityTags.AudioManager );
			if( gm != null )
			{
				var script = gm.GetComponent<AudioManager>();
				if( script != null )
				{
					_instance = script;
				}
			}
			// Otherwise, create new instance
			else
			{
				// Because the MusicManager is a component, we have to create a GameObject to attach it to.
				var managerObject = new GameObject( "AudioManager" ) { tag = UnityTags.AudioManager };

				// Add the DynamicObjectManager component, and set it as the defaultCenter
				_instance = (AudioManager)managerObject.AddComponent( typeof( AudioManager ) );
			}

			if( _instance != null )
				_instance.Init();

			return _instance;
		}
	}

	//=====================================================

	public void OnPauseEvent( bool isPaused)
	{
		if( isPaused == _isPaused ) return;

		_isPaused = isPaused;

		if( _isPaused == true && _isMusicOn == true )
		{
			if( _audioSnapshotPaused != null )
				_audioSnapshotPaused.TransitionTo( 0.25f );
			else
				Debug.LogError( "_audioSnapshotPaused is null in OnPauseEvent" );
		}
		else
		{
			ResetAudioState();
		}
	}

	//=====================================================

	public void SetMusicState( bool isOn )
	{
		if( isOn == _isMusicOn ) return;

		_isMusicOn = isOn;

		if( _isPaused == false )
			ResetAudioState();
	}

	//=====================================================

	public void SetSFXState( bool isOn )
	{
		if( isOn == _isSFXOn ) return;

		_isSFXOn = isOn;

		if( _isPaused == false )
			ResetAudioState();
	}

	//=====================================================

	public void PlayMusic( eLocation location )
	{
		// Puzzle Room enums range from 0+ (eLocation.PUZZLE_ROOM_01 == 0)
		if( location != eLocation.NULL && (int)location < (int)eLocation.NUM_PUZZLE_LOCATIONS )
		{
			PlayMusic( 3 );
		}
		else
		{
			switch( location )
			{
				default:
					PlayMusic( 1 );
					break;

				case eLocation.TUTORIAL:
					PlayMusic( 0 );
					break;

				case eLocation.COMMON_ROOM:
				case eLocation.CLOTHING_ROOM:
				case eLocation.HIGHSCORES_ROOM:
				case eLocation.TRADING_CARD_ROOM:
					PlayMusic( 2 );
					break;

				case eLocation.BOSS_ROOM:
					PlayMusic( 4 );
					break;
			}
		}
	}

	//=====================================================

	public void StopMusic()
	{
		_audioSource.Stop();
	}

	#endregion

	//=====================================================

	#region Private Methods

	void Awake()
	{
		Init();
	}

	//=====================================================

	void Update()
	{
		if( Input.GetKeyDown( KeyCode.Alpha1 ) )
			SetMusicState( false );
		else if( Input.GetKeyDown( KeyCode.Alpha2 ) )
			SetMusicState( true );
		else if( Input.GetKeyDown( KeyCode.Alpha3 ) )
			SetSFXState( false );
		else if( Input.GetKeyDown( KeyCode.Alpha4 ) )
			SetSFXState( true );
	}

	//=====================================================
	// Called after constructor
	private void Init()
	{
		if( _isInitialized == true ) return;

		DontDestroyOnLoad( this.gameObject );

		// Defaults
		_isInitialized = true;
		_isMusicOn = true;
		_isSFXOn = true;
		_isPaused = false;
		_currentTrack = 0;
		//_isInTransition = false;
		//_transitionDir = 0;
		//_transitionVolume = 0.0f;

		_audioSource = transform.GetComponent<AudioSource>();
		if( _audioSource == null ) return;

		_audioSource.loop = true;
	}

	//=====================================================

	private void ResetAudioState()
	{
		if( _isMusicOn == true && _isSFXOn == true )
		{
			if( _audioSnapshotNormal != null )
				_audioSnapshotNormal.TransitionTo( 0.25f );
		}
		else if( _isMusicOn == true && _isSFXOn == false )
		{
			if( _audioSnapshotNoSFX != null )
				_audioSnapshotNoSFX.TransitionTo( 0.25f );
		}
		else if( _isSFXOn == true )
		{
			if( _audioSnapshotNoMusic != null )
				_audioSnapshotNoMusic.TransitionTo( 0.25f );
		}
		else
		{
			if( _audioSnapshotNoAudio != null )
				_audioSnapshotNoAudio.TransitionTo( 0.25f );
		}
	}

	//=====================================================

	private void PlayMusic( int track )
	{
		if( _tracks == null || _tracks.Length == 0 ) return;

		track = Mathf.Clamp( track, 0, _tracks.Length - 1 );

		if( _currentTrack == track && _audioSource.isPlaying == true ) return;

		// Store next track index
		_currentTrack = track;

		// Fade into track if nothing is currently playing
		if( _audioSource.isPlaying == false )
		{
#if UNITY_IPHONE
		// If ipod music is playing then skip the audio start
		//if( GameCenterBinding.isMusicPlaying() == false )
		{
			_audioSource.clip = _tracks[_currentTrack];
			StartCoroutine( FadeIn() );
		}
#else
			_audioSource.clip = _tracks[_currentTrack];
			StartCoroutine( FadeIn() );
#endif
			return;
		}

		// Fade out from current track and into next tracks
		StartCoroutine( FadeOut( true, () => {
#if UNITY_IPHONE
		// If ipod music is playing then skip the audio start
		//if( GameCenterBinding.isMusicPlaying() == false )
		{
			_audioSource.clip = _tracks[_currentTrack];
			StartCoroutine( FadeIn() );
		}
#else
			_audioSource.clip = _tracks[_currentTrack];
			StartCoroutine( FadeIn() );
#endif
		} ) );
	}

	//=====================================================

	private IEnumerator FadeOut( bool autoFadeIn = true, Action onComplete = null )
	{
		// Fade out to zero volume
		while( _audioSource.volume > 0.0f )
		{
			yield return new WaitForSeconds( 0.1f );

			_audioSource.volume -= 0.1f;
		}

		_audioSource.volume = 0.0f;
		_audioSource.Stop();

		if( autoFadeIn == false ) yield return null;
		
		if( onComplete != null )
			onComplete();
	}

	//=====================================================

	private IEnumerator FadeIn()
	{
		_audioSource.volume = 0.0f;
		_audioSource.Play();

		// Fade in to full volume
		while( _audioSource.volume < 1.0f )
		{
			yield return new WaitForSeconds( 0.1f );

			_audioSource.volume += 0.05f;
		}

		_audioSource.volume = 1.0f;
	}

	#endregion

	//=====================================================

	//public void FadeMusicOut()
	//{
	//	_isInTransition = true;
	//	_transitionDir = -1;
	//	_transitionVolume = 1.0f;
	//}

	//=====================================================

	//public void FadeMusicIn()
	//{
	//	_isInTransition = true;
	//	_transitionDir = 1;
	//	_transitionVolume = 0.0f;
	//}

	//=====================================================

	//void Update()
	//{
	//	if( _isInTransition )
	//	{
	//		switch( _transitionDir )
	//		{
	//			case -1:
	//				_transitionVolume -= (Time.deltaTime * 2.3f);
	//				if( _transitionVolume <= 0.0f )
	//				{
	//					_isInTransition = false;
	//					_transitionVolume = 0.0f;
	//				}
	//				break;

	//			case 1:
	//				_transitionVolume += (Time.deltaTime * 2.3f);
	//				if( _transitionVolume >= 1.0f )
	//				{
	//					_isInTransition = false;
	//					_transitionVolume = 1.0f;
	//				}
	//				break;
	//		}
	//	}

	//	var fVolume = 0.0f;
	//	if( GameManager.Instance.IsMusicOn() == true )
	//		fVolume = 1.0f;

	//	_audioSource.volume = fVolume * _transitionVolume;
	//}

	//=====================================================
}