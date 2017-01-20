using UnityEngine;
using UnityEngine.UI;
using System;

public class PopupKeyCollected : MonoBehaviour
{
	public static event Action<ICutsceneObject, bool> PopupKeyExitScene;	// <object, exitScene>

	public static PopupKeyCollected Instance;

	[SerializeField] private Image _imgKey;
	[SerializeField] private Sprite _keyDefault;
	[SerializeField] private Sprite _keyRedGems;
	[SerializeField] private Sprite _key100Gems;
	[SerializeField] private Text _txtKeyId;
	[SerializeField] private Text _txtCollected;
	private	GameObject _camera;
	private ICutsceneObject _keyCutsceneObject;
	private ePuzzleKeyType _keyType;
	
	//=====================================================

	void Awake()
	{
		Instance = this;
	}
	
	//=====================================================

	public void Show( ICutsceneObject obj, ePuzzleKeyType keyType, bool _isFinalKey )
	{
		// ToDo: show additional info for collecting all keys in scene

		if( _camera == null )
			_camera = transform.FindChild( "GuiCamera" ).gameObject;

		if( _camera == null ) return;

		// Store cutscene object - pass back to GameManager when closing popup
		_keyCutsceneObject = obj;

		_camera.SetActive( true );

		_keyType = keyType;

		//var id = "";
		switch( _keyType )
		{
			default:
				_imgKey.sprite = _keyDefault;
				_txtKeyId.text = TextManager.GetText( "POPUP_KEY" ) + " " + ((int)_keyType - 1);
				break;
			case ePuzzleKeyType.KEY_GEM_RED:
				_imgKey.sprite = _keyRedGems;
				_txtKeyId.text = TextManager.GetText( "POPUP_KEY_RED_GEMS" );
				break;
			case ePuzzleKeyType.KEY_GEM_100:
				_imgKey.sprite = _key100Gems;
				_txtKeyId.text = TextManager.GetText( "POPUP_KEY_100_GEMS" );
				break;
		}

		//_txtKeyId.text = TextManager.GetText( "POPUP_KEY" ) + " " + id;
		_txtCollected.text = TextManager.GetText( "POPUP_KEY_COLLECTED" );
	}

	//=====================================================

	public void OnButtonPressedShare()
	{
		EveryplayWrapper.ShareRecording( GameDataManager.Instance.PlayerCurrentFairyName.ToString() );
	}

	//=====================================================

	public void OnButtonPressedClose()
	{
		// Player exits scene after collecting all key-types IGNORE: except KEY_GEM_100
		if( PopupKeyExitScene != null )
			PopupKeyExitScene( _keyCutsceneObject, true ); // _keyType != ePuzzleKeyType.KEY_GEM_100 );

		_camera.SetActive( false );
	}

	//=====================================================
}
