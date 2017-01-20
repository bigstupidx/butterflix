using UnityEngine;
using UnityEngine.UI;
using System;

public class ScreenManager : MonoBehaviour
{
	public static event Action FadeInCompleteEvent;
	public static event Action FadeOutCompleteEvent;

	private Image			_fadePanel		= null;
	private static float	_fadeSpeed		= 3.0f;			// Speed that the screen fades to and from black.
	private static bool		_isFadingIn		= false;		// Whether or not the scene is still fading in.
	private static bool		_isFadingOut	= false;
	private static float	_fadeStart		= 0.0f;

	//=====================================================

	public static void FadeIn( float speed = 0.5f )
	{
		_fadeSpeed = speed;
		_fadeStart = Time.time;
		_isFadingIn = true;
	}

	//=====================================================

	public static void FadeOut( float speed = 0.5f )
	{
		_fadeSpeed = speed;
		_fadeStart = Time.time;
		_isFadingOut = true;
	}

	//=====================================================

	void Awake()
	{
		var fadePanel = GameObject.FindGameObjectWithTag( UnityTags.GuiFadePanel );
		if( fadePanel != null )
		{
			_fadePanel = fadePanel.GetComponent<Image>();

			if( _fadePanel != null )
			{
				_fadePanel.color = Color.black;
				_fadePanel.enabled = true;
			}
			else
			{
				Debug.LogError( "ScreenManager: Fade panel doesnt contain an image component!" );
			}
		}
		else
		{
			Debug.LogError( "ScreenManager: Fade panel not found. Check Gui prefab exists in scene." );
		}

		_isFadingIn = false;
		_isFadingOut = false;
	}

	//=====================================================

	void Start()
	{
		// Auto-fade-in for non-gameplay areas : look for GameLocation instance
		var gameLocation = GameObject.Find( "GameLocation" );
		if( gameLocation == null ) return;

		var locationScript = gameLocation.GetComponent<GameLocation>();
		if( locationScript == null || locationScript.Location == eLocation.NULL ) return;

		var location = locationScript.Location;

		// Puzzle Room enums range from 0+ (eLocation.PUZZLE_ROOM_01 == 0)
		if( location != eLocation.NULL && (int)location < (int)eLocation.NUM_PUZZLE_LOCATIONS )
		{
			// Do nothing
		}
		else
		{
			switch( location )
			{
				case eLocation.MAIN_HALL:
				case eLocation.BOSS_ROOM:
					// Do nothing
					break;
				default:
					// Delay then fade in at start
					Invoke( "FadeInDelayed", 0.5f );
					break;
			}
		}
	}

	//=====================================================

	void Update()
	{
		if( _fadePanel == null ) return;

		// If the scene is starting...
		if( _isFadingIn )
			FadeSceneIn();

		else if( _isFadingOut )
			FadeSceneOut();
	}

	//=====================================================

	private void FadeInDelayed()
	{
		FadeIn( 0.6f );
	}

	//=====================================================

	void FadeSceneIn()
	{
		// Make sure the texture is enabled.
		if( _fadePanel.enabled == false )
			_fadePanel.enabled = true;

		// Fade the texture to clear.
		FadeToClear();

		// If the texture is almost clear...
		if( _fadePanel.color.a > 0.015f ) return;

		// ... set the colour to clear and disable the GUITexture.
		_fadePanel.color = Color.clear;
		_fadePanel.enabled = false;

		// The scene is no longer starting.
		_isFadingIn = false;

		if( FadeInCompleteEvent != null )
			FadeInCompleteEvent();
	}

	//=====================================================

	void FadeSceneOut()
	{
		// Make sure the texture is enabled.
		if( _fadePanel.enabled == false )
			_fadePanel.enabled = true;

		// Start fading towards black.
		FadeToBlack();

		// If the screen is almost black...
		if( _fadePanel.color.a < 0.985f ) return;

		_fadePanel.color = Color.black;

		// The scene is no longer starting.
		_isFadingOut = false;

		if( FadeOutCompleteEvent != null )
			FadeOutCompleteEvent();
	}

	//=====================================================

	void FadeToClear()
	{
		// Lerp the colour of the texture between itself and transparent.
		_fadePanel.color = Color.Lerp( Color.black, Color.clear, (Time.time - _fadeStart) * _fadeSpeed );
	}

	//=====================================================

	void FadeToBlack()
	{
		// Lerp the colour of the texture between itself and black.
		_fadePanel.color = Color.Lerp( Color.clear, Color.black, (Time.time - _fadeStart) * _fadeSpeed );	// _fadeSpeed * Time.deltaTime
	}

	//=====================================================
}
