using UnityEngine;
using UnityEngine.UI;
using System;

public class BuyPackConfirmManager : MonoBehaviour
{
	static	public	BuyPackConfirmManager	instance;
	
	public	GameObject						m_MainPanel;
	public	Text							m_txtDesc;
	
	private int								m_CurPackType;

	// Button presses
	private	bool							m_bButtonPressed;
	private	int								m_ButtonIndex;
	
	//=====================================================
	
	void Awake()
	{
		instance = this;
	}

	//=====================================================

	public void Reset()
	{
		m_bButtonPressed = false;
		m_ButtonIndex = 0;
	}
	
	//=============================================================================

	public int GetButtonPressed()
	{
		return( m_ButtonIndex );
	}
	
	//=============================================================================

	public int GetPackType()
	{
		return( m_CurPackType );
	}
	
	//=============================================================================

	public bool WasButtonPressed()
	{
		return( m_bButtonPressed );
	}
	
	//=============================================================================

	public void ShowPanel( bool bActive , int PackType )
	{
		if( bActive )
		{
			m_bButtonPressed = false;
			m_CurPackType = PackType;
			m_MainPanel.SetActive( true );
			
			// Fill in description
			string txtDesc = null;
			txtDesc = TextManager.GetText( "TRADINGCARDS_BUYPACK_CONFIRM" );
			txtDesc = txtDesc.Replace( "(Price)" , ( ( PackType + 1 ) * 50 ).ToString() );
			
			m_txtDesc.text = txtDesc;
		}
		else
		{
			m_MainPanel.SetActive( false );
		}
	}

	//=====================================================

	public void OnButtonPressed_OK()
	{
		m_bButtonPressed = true;
		m_ButtonIndex = 0;
	}

	//=====================================================
	
	public void OnButtonPressed_Cancel()
	{
		m_bButtonPressed = true;
		m_ButtonIndex = 1;
	}

	//=====================================================

	public void OnButtonPressed_Dummy()
	{
	}

	//=====================================================

	void Update()
	{
	}
	
	//=============================================================================
}
