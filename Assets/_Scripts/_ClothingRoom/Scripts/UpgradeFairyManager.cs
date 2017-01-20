using UnityEngine;
using UnityEngine.UI;
using System;

public class UpgradeFairyManager : MonoBehaviour
{
	static	public	UpgradeFairyManager			instance;
	
	public	GameObject							m_MainPanel;
	public	RequirementsPanel					m_RequirementsPanel;
	public	Text								m_txtFairyLevel;
	public	Text								m_txtHeader;
	public	Text								m_txtLevelUp;
	
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

	public void ShowPanel( bool bActive , int FairyLevel = 1 , eFairy Fairy = eFairy.BLOOM )
	{
		if( bActive )
		{
			m_bButtonPressed = false;
			m_MainPanel.SetActive( true );

			FairyItemData CurFairyInfo = FairyItemsManager.GetFairyItem( Fairy );
			m_RequirementsPanel.Setup( CurFairyInfo , FairyLevel );
			
			m_txtFairyLevel.text = FairyLevel.ToString();
			
			string txtHeader = TextManager.GetText( "BOUTIQUE_UPGRADEFAIRY_TITLE" );
			txtHeader = txtHeader.Replace( "(FairyName)" , Fairy.ToString().ToUpper() );
			m_txtHeader.text = txtHeader;
			
			string txtLevelUp = TextManager.GetText( "BOUTIQUE_UPGRADEFAIRY_LEVELUP" );
			txtLevelUp = txtLevelUp.Replace( "(Cost)" , String.Format( "{0:n0}", CurFairyInfo.GemsRequired[ FairyLevel ] ) );
			m_txtLevelUp.text = txtLevelUp;
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
