using UnityEngine;
using UnityEngine.UI;
using System;

public class BoosterPacksManager : MonoBehaviour
{
	static	public	BoosterPacksManager		instance;
	
	public	GameObject						m_MainPanel;
	public	Text							m_txtPackPrice1;
	public	Text							m_txtPackPrice2;
	public	Text							m_txtPackPrice3;

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

	public bool WasButtonPressed()
	{
		return( m_bButtonPressed );
	}
	
	//=============================================================================

	public void ShowPanel( bool bActive )
	{
		if( bActive )
		{
			m_bButtonPressed = false;
			m_MainPanel.SetActive( true );
			
			m_txtPackPrice1.text = "50";
			m_txtPackPrice2.text = "100";
			m_txtPackPrice3.text = "150";
		}
		else
		{
			m_MainPanel.SetActive( false );
		}
	}

	//=====================================================

	public void OnButtonPressed_Cancel()
	{
		m_bButtonPressed = true;
		m_ButtonIndex = 0;
	}

	//=====================================================

	public void OnButtonPressed_BuyPack1()
	{
		m_bButtonPressed = true;
		m_ButtonIndex = 1;
	}

	//=====================================================

	public void OnButtonPressed_BuyPack2()
	{
		m_bButtonPressed = true;
		m_ButtonIndex = 2;
	}

	//=====================================================

	public void OnButtonPressed_BuyPack3()
	{
		m_bButtonPressed = true;
		m_ButtonIndex = 3;
	}

	//=====================================================
}
