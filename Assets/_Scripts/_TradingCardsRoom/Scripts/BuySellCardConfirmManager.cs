using UnityEngine;
using UnityEngine.UI;
using System;

public class BuySellCardConfirmManager : MonoBehaviour
{
	static	public	BuySellCardConfirmManager	instance;
	
	public	GameObject							m_MainPanel;
	public	Text								m_txtDesc;
	
	private TradingCardSpreadsheetItem 			m_CurCard;

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

	public void ShowPanel( bool bActive , bool bBuy = true , TradingCardSpreadsheetItem CurCard = null , eTradingCardCondition Condition = eTradingCardCondition.MINT )
	{
		if( bActive )
		{
			m_bButtonPressed = false;
			m_CurCard = CurCard;
			m_MainPanel.SetActive( true );
			
			float fScuffedDiscount 	= Convert.ToSingle( SettingsManager.GetSettingsItem( "TRADINGCARD_SALEPRICE_SCUFFED", -1 ) );
			float fMintDiscount 	= Convert.ToSingle( SettingsManager.GetSettingsItem( "TRADINGCARD_SALEPRICE_MINT", -1 ) );
			
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
					txtDesc = txtDesc.Replace( "(Price)" , ((int)( CurCard.value * fMintDiscount )).ToString() );
				}
				else
				{
					txtDesc = TextManager.GetText( "TRADINGCARDS_SELLSCUFFED_CONFIRM" );
					txtDesc = txtDesc.Replace( "(Price)" , ((int)( CurCard.valueScuffed * fScuffedDiscount )).ToString() );
				}
			}

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
