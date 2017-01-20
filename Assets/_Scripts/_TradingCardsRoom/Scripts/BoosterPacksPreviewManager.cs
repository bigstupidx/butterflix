using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class BoosterPacksPreviewManager : MonoBehaviour
{
	static	public	BoosterPacksPreviewManager		instance;
	
	public	GameObject								m_MainPanel;
	public	GameObject								m_PreviewPanel;
	public	GameObject								m_pfbCardFrame;

	// Button presses
	private	bool									m_bButtonPressed;
	private	int										m_ButtonIndex;
	
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

	public void ShowPanel( bool bActive , List< TradingCardHeldItem > CardsInPack = null )
	{
		if( bActive )
		{
			m_bButtonPressed = false;
			m_MainPanel.SetActive( true );
			
			// Add card previews
			int NumCards = CardsInPack.Count;
			float fWidth = 17.0f + ( ( NumCards - 1 ) * 300.0f ) + 240.0f + 17.0f;
			for( int Idx = 0 ; Idx < NumCards ; Idx++ )
			{
				GameObject NewCard = Instantiate( m_pfbCardFrame , new Vector3( (float)Idx * 300.0f , 15.0f , 0.0f ) , Quaternion.identity ) as GameObject;
				NewCard.transform.parent = m_PreviewPanel.transform;
				NewCard.transform.localPosition = new Vector3( 17.0f + ( (float)Idx * 300.0f ) , -170.0f , 0.0f );
				
				// Setup card sprite image
				TradingCardSpreadsheetItem CurSpreadsheetCard = TradingCardItemsManager.GetTradingCardItem( CardsInPack[ Idx ].id );
				UnityEngine.UI.Image RenderCardImage = NewCard.transform.GetChild( 0 ).gameObject.GetComponent( typeof( UnityEngine.UI.Image ) ) as UnityEngine.UI.Image;
				Sprite RenderSprite = (Sprite)Resources.Load( "Cards/LargeGUITextures/" + CurSpreadsheetCard.largeGuiTexture2D , typeof( Sprite ) );
				
				RenderCardImage.sprite = RenderSprite;
			}
			
			//Rect NewRect = new Rect( m_PreviewPanel.GetComponent<RectTransform>().rect );
			//NewRect.width = fWidth;
			//m_PreviewPanel.GetComponent<RectTransform>().rect = NewRect;
			m_PreviewPanel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal , fWidth );
		}
		else
		{
			// Delete all child objects of preview panel
			for( int i = m_PreviewPanel.transform.childCount-1 ; i >= 0 ; --i )
			{
				GameObject child = m_PreviewPanel.transform.GetChild( i ).gameObject;
				Destroy( child );
			}

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
}
