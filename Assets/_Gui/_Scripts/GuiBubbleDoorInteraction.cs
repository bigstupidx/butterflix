using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GuiBubbleDoorInteraction : MonoBehaviour
{
	[SerializeField] private CameraFacingBillboard _billboardController;
	[SerializeField] private Sprite[] _spriteIcons;							// List of fairies then key

	private Transform _thisTransform;
	private Canvas _canvas;
	private Image _icon;
	private Text _text;
	private Animator _animator;

	private bool _isEnabled;

	//=====================================================

	#region Public Interface

	public void OnButtonClick()
	{
		HideBubble();
	}

	//=====================================================

	public void ShowBubble( eDoorType doorType, int numKeysRequired, eFairy fairyRequired )
	{
		if( _isEnabled == true ) return;

		var showBubble = false;

		if( _icon != null && _text != null )
		{
			if( numKeysRequired > 0 )
			{
				// Num keys
				_icon.sprite = _spriteIcons[_spriteIcons.Length - 3];
				_text.text = TextManager.GetText( "DOOR_INTERACT_KEY" ) + " " + numKeysRequired + " :";
				showBubble = true;
			}
			else if( fairyRequired != eFairy.NULL )
			{
				// Fairy
				_icon.sprite = _spriteIcons[(int)fairyRequired];
				_text.text = TextManager.GetText( "DOOR_INTERACT_FAIRY" ) + " :";
				showBubble = true;
			}
			else
			{
				switch( doorType )
				{
					case eDoorType.BOSS:
						_icon.sprite = _spriteIcons[_spriteIcons.Length - 1];
						_text.text = TextManager.GetText( "DOOR_INTERACT_BOSS" ) + " " + (int)eFairy.NUM_FAIRIES + " :";
						showBubble = true;
						break;

					default:
						// Lever / switch / pushable block
						_icon.sprite = _spriteIcons[_spriteIcons.Length - 2];
						_text.text = TextManager.GetText( "DOOR_INTERACT_SWITCH" ) + " :";
						showBubble = true;
						break;
				}
			}
		}

		if( showBubble == false ) return;
		
		_isEnabled = true;

		if( _animator != null && _animator.GetBool( HashIDs.IsEnabled ) == false )
			_animator.SetBool( HashIDs.IsEnabled, true );

		StartCoroutine( AutoHideBubble() );

		// Face bubble towards player-camera
		if( _billboardController != null )
			_billboardController.enabled = true;
	}

	//=====================================================

	public void HideBubble()
	{
		_isEnabled = false;

		if( _animator != null && _animator.GetBool( HashIDs.IsEnabled ) == true )
			_animator.SetBool( HashIDs.IsEnabled, false );

		// Stop updating bubble rotations (towards player-camera)
		if( _billboardController != null )
			_billboardController.enabled = false;
	}

	#endregion

	//=====================================================

	#region Private Methods

	void Awake()
	{
		_thisTransform = this.transform;

		_canvas = _thisTransform.FindChild( "Canvas" ).GetComponent<Canvas>();

		if( _canvas != null )
		{
			var button = _canvas.GetComponentInChildren<Button>();

			if( button != null )
			{
				_animator = button.GetComponent<Animator>();

				var image = button.transform.FindChild( "Image" );
				_icon = image.GetComponent<Image>();

				var text = button.transform.Find( "Text" );
				_text = text.GetComponent<Text>();
			}
		}

		_isEnabled = false;
	}

	//=====================================================

	private IEnumerator AutoHideBubble()
	{
		yield return new WaitForSeconds( 4.0f );

		HideBubble();
	}

	#endregion

	//=====================================================
}
