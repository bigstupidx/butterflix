using UnityEngine;
using System.Collections;

public class PopupGenericMessage : MonoBehaviour
{
	public 	static PopupGenericMessage			instance					= null;

	public	GameObject				m_GUICamera;
	public	UnityEngine.UI.Text 	m_GenericText;

	//=====================================================

	void Awake()
	{
		instance = this;
	}
	
	//=====================================================
	
	public void Show( string TextID )
	{
		if( InputManager.Instance != null )
			InputManager.Instance.OnBlockInput( true );

		StartCoroutine( ShowPopup( TextManager.GetText( TextID ) ) );
	}

	//=====================================================

	public void OnButtonPressed_OK()
	{
		m_GUICamera.SetActive( false );

		if( InputManager.Instance != null )
			InputManager.Instance.OnBlockInput( false );
	}

	//=====================================================

	private IEnumerator ShowPopup( string Text )
	{
		yield return new WaitForSeconds( 0.1f );

		m_GenericText.text = Text;

		m_GUICamera.SetActive( true );
	}

	//=====================================================
}
