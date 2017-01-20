using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class PopupTutorial : MonoBehaviour
{
	public static event Action PopupStartTutorial;

	public static PopupTutorial Instance;

	[SerializeField] private GameObject _canvasIntro;
	[SerializeField] private GameObject _canvasTutorials;
	[SerializeField] private Image _imgInteractIcon;
	[SerializeField] private Sprite _sprPush;
	[SerializeField] private Sprite _sprJump;
	[SerializeField] private Text _txtKeyId;
	[SerializeField] private Text _txtCollected;
	private	GameObject _camera;
	private eTutorial _currentTutorial;

	//=====================================================

	void Awake()
	{
		Instance = this;
	}

	//=====================================================

	public void Show( eTutorial currentTutorial )
	{
		if( _camera == null )
			_camera = transform.FindChild( "GuiCamera" ).gameObject;

		if( _camera == null ) return;

		_currentTutorial = currentTutorial;

		StartCoroutine( DelayShowPopup() );
	}

	//=====================================================

	public void OnButtonPressedClose()
	{
		if( InputManager.Instance != null )
			InputManager.Instance.OnBlockInput( false );

		// Start Brafilius' cutscene
		switch( _currentTutorial )
		{
			case eTutorial.TUTORIAL01:
				if( PopupStartTutorial != null )
					PopupStartTutorial();
				break;

			case eTutorial.NULL:
			case eTutorial.TUTORIAL04:
				// Tutorial is completed
				PlayerPrefsWrapper.SetInt( "IsTutorialCompleted", 1 );

				// Go to Main Hall scene via cutscene
				GameManager.Instance.SetNextLocation( eLocation.MAIN_HALL );
				CutsceneManager.Instance.OnRestartEvent();
				break;
		}

		_camera.SetActive( false );
	}

	//=====================================================

	private IEnumerator DelayShowPopup()
	{
		if( InputManager.Instance != null )
			InputManager.Instance.OnBlockInput( true );

		yield return new WaitForSeconds( 0.35f );

		_imgInteractIcon.gameObject.SetActive( false );
		_camera.SetActive( true );
		_canvasIntro.SetActive( _currentTutorial == eTutorial.INTRO );
		_canvasTutorials.SetActive( _currentTutorial != eTutorial.INTRO );

		var description = "";
		switch( _currentTutorial )
		{
			case eTutorial.TUTORIAL01:
				description = TextManager.GetText( "TUTORIAL_01" );
				break;

			case eTutorial.TUTORIAL02:
				description = TextManager.GetText( "TUTORIAL_02" );

				// Show 'push' icon
				_imgInteractIcon.gameObject.SetActive( true );
				_imgInteractIcon.sprite = _sprPush;
				break;

			case eTutorial.TUTORIAL03:
				description = TextManager.GetText( "TUTORIAL_03" );

				// Show 'jump' icon
				_imgInteractIcon.gameObject.SetActive( true );
				_imgInteractIcon.sprite = _sprJump;
				break;

			case eTutorial.TUTORIAL04:
				description = TextManager.GetText( "TUTORIAL_04" );
				break;
		}

		_txtKeyId.text = description;
		//_txtCollected.text = TextManager.GetText( "POPUP_KEY_COLLECTED" );
	}

	//=====================================================
}
