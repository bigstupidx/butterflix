using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent( typeof( Image ) )]
public class GuiPanelTutorial : MonoBehaviour
{
	[SerializeField] private Sprite[] _sprTutorials;

	private static Transform _thisTransform;
	private Image _imgBackground;
	private Text _text;
	private int _currentTutorial;

	//=====================================================

	public void OnButtonClick()
	{
		// Go to next tutorial screen or close
		if( ++_currentTutorial < _sprTutorials.Length )
		{
			_imgBackground.sprite = _sprTutorials[_currentTutorial];

			var text = "TUTORIAL_INTRO_" + ( _currentTutorial + 1 ).ToString( "00" );
			_text.text = TextManager.GetText( text );
		}
		else
		{
			_thisTransform.gameObject.SetActive( false );

			// Un-block player input
			if( InputManager.Instance != null )
				InputManager.Instance.OnBlockInput( false );
		}
	}

	//=====================================================

	void Awake()
	{
		_thisTransform = transform;
		_imgBackground = _thisTransform.GetComponent<Image>();
		_text = _thisTransform.GetComponentInChildren<Text>();
	}

	//=====================================================

	void OnEnable()
	{
		_currentTutorial = 0;
		_imgBackground.sprite = _sprTutorials[_currentTutorial];
		_text.text = TextManager.GetText( "TUTORIAL_INTRO_01" );

		// Block player input
		if( InputManager.Instance != null )
			InputManager.Instance.OnBlockInput( true );
	}

	//=====================================================
}
