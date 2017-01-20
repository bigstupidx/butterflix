using UnityEngine;
using UnityEngine.UI;
using System;

public class FairyLockedManager : MonoBehaviour
{
	static	public	FairyLockedManager			instance;
	
	public	GameObject							m_MainPanel;
	public	GameObject							m_BackgroundLock;
	public	RequirementsPanel					m_RequirementsPanel;
	public	Text								m_txtDesc;
	
	// Button presses
	private	bool								m_bButtonPressed;
	private	int									m_ButtonIndex;
	
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

	public bool WasButtonPressed()
	{
		return( m_bButtonPressed );
	}
	
	//=============================================================================

	public void ShowPanel( bool bActive , FairyItemData CurFairyInfo = null )
	{
		if( bActive )
		{
			m_bButtonPressed = false;
			m_BackgroundLock.SetActive( true );
			m_MainPanel.SetActive( true );
			m_RequirementsPanel.Setup( CurFairyInfo , 0 );
			
			/*
			// Fill in description
			string txtDesc = null;
			if( bBuy )
			{
				if( Condition == eTradingCardCondition.MINT )
				{
					txtDesc = TextManager.GetText( "TRADINGCARDS_BUYMINT_CONFIRM" );
					txtDesc = txtDesc.Replace( "(Price)" , CurCard.value.ToString() );
				}
				else
				{
					txtDesc = TextManager.GetText( "TRADINGCARDS_BUYSCUFFED_CONFIRM" );
					txtDesc = txtDesc.Replace( "(Price)" , CurCard.valueScuffed.ToString() );
				}
			}
			else
			{
				if( Condition == eTradingCardCondition.MINT )
				{
					txtDesc = TextManager.GetText( "TRADINGCARDS_SELLMINT_CONFIRM" );
					txtDesc = txtDesc.Replace( "(Price)" , ((int)( CurCard.value * 0.75f )).ToString() );
				}
				else
				{
					txtDesc = TextManager.GetText( "TRADINGCARDS_SELLSCUFFED_CONFIRM" );
					txtDesc = txtDesc.Replace( "(Price)" , ((int)( CurCard.valueScuffed * 0.75f )).ToString() );
				}
			}

			m_txtDesc.text = txtDesc;
			*/
		}
		else
		{
			m_BackgroundLock.SetActive( false );
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
