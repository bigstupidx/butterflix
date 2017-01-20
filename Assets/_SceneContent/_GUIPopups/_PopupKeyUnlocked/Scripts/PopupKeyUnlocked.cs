using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class PopupKeyUnlocked : MonoBehaviour
{
	public static event Action<ePuzzleKeyType> PopupSpecialKeyUnlocked;	// <key type>

	public static PopupKeyUnlocked Instance;

	[SerializeField] private Image _imgKey;
	[SerializeField] private Sprite _8RedGems;
	[SerializeField] private Sprite _100Gems;
	[SerializeField] private Text _txtKeyId;
	[SerializeField] private Text _txtCollected;
	
	private	GameObject _camera;
	private ePuzzleKeyType _keyType;

	//=====================================================

	void Awake()
	{
		Instance = this;
	}

	//=====================================================

	public void Show( ePuzzleKeyType keyType )
	{
		if( _camera == null )
			_camera = transform.FindChild( "GuiCamera" ).gameObject;

		if( _camera == null ) return;

		_keyType = keyType;

		StartCoroutine( DelayShowPopup() );
	}

	//=====================================================

	public void OnButtonPressedClose()
	{
		if( InputManager.Instance != null )
			InputManager.Instance.OnBlockInput( false );

		// Show cutscene leading to unlocked key: 100_gem or 8_redgem key
		if( PopupSpecialKeyUnlocked != null )
			PopupSpecialKeyUnlocked( _keyType );

		_camera.SetActive( false );
	}

	//=====================================================

	private IEnumerator DelayShowPopup()
	{
		if( InputManager.Instance != null )
			InputManager.Instance.OnBlockInput( true );

		yield return new WaitForSeconds( 1.0f );

		_camera.SetActive( true );

		var id = "";
		switch( _keyType )
		{
			case ePuzzleKeyType.KEY_GEM_RED:
				_imgKey.sprite = _8RedGems;
				_txtKeyId.text = TextManager.GetText( "POPUP_8_RED_GEMS_COLLECTED" );
				_txtCollected.text = TextManager.GetText( "POPUP_8_RED_GEMS_COLLECTED_SUB" );
				break;
			case ePuzzleKeyType.KEY_GEM_100:
				_imgKey.sprite = _100Gems;
				_txtKeyId.text = TextManager.GetText( "POPUP_100_GEMS_COLLECTED" );
				_txtCollected.text = TextManager.GetText( "POPUP_100_GEMS_COLLECTED_SUB" );
				break;
		}
	}

	//=====================================================
}
