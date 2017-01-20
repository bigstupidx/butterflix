using UnityEngine;
using UnityEngine.UI;
using System;

public class ConfirmPurchaseManager : MonoBehaviour
{
	static	public	ConfirmPurchaseManager		instance;
	
	public	GameObject							m_MainPanel;
	public	Text								m_txtHeader;
	public	Text								m_txtDesc;
	public	Text								m_txtCostGems;
	public	Text								m_txtCostDiamonds;
	public	GameObject							m_sprPurchaseOverlayGems;
	public	GameObject							m_sprPurchaseOverlayDiamonds;
	
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

	public void ShowPanel( bool bActive , bool bIsOutfit = false , eFairy Fairy = eFairy.BLOOM , int Cost = 0 )
	{
		if( bActive )
		{
			m_bButtonPressed = false;
			m_MainPanel.SetActive( true );
			
			if( bIsOutfit )
			{
				m_txtHeader.text = TextManager.GetText( "BOUTIQUE_PURCHASE_TITLE_OUTFIT" );
				m_txtDesc.text = TextManager.GetText( "BOUTIQUE_PURCHASE_DESC_OUTFIT" );
				
				m_sprPurchaseOverlayGems.SetActive( false );
				m_sprPurchaseOverlayDiamonds.SetActive( true );
			}
			else
			{
				string txtHeader = TextManager.GetText( "BOUTIQUE_PURCHASE_TITLE_FAIRY" );
				txtHeader = txtHeader.Replace( "(FairyName)" , Fairy.ToString().ToUpper() );
				m_txtHeader.text = txtHeader;

				string txtDesc = TextManager.GetText( "BOUTIQUE_PURCHASE_DESC_FAIRY" );
				txtDesc = txtDesc.Replace( "(FairyName)" , Fairy.ToString() );
				m_txtDesc.text = txtDesc;

				m_sprPurchaseOverlayGems.SetActive( true );
				m_sprPurchaseOverlayDiamonds.SetActive( false );
			}
			
			string txtCost = TextManager.GetText( "BOUTIQUE_PURCHASE" );
			txtCost = txtCost.Replace( "(Cost)" , String.Format( "{0:n0}", Cost ) );
			m_txtCostGems.text = txtCost;
			m_txtCostDiamonds.text = txtCost;
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
