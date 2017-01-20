using UnityEngine;
using UnityEngine.UI;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SelectedCardManager : MonoBehaviour
{
	static	public	SelectedCardManager		instance;
	
	public	GameObject						m_MainPanel;
	public	UnityEngine.UI.Image 			m_sprCardFrame;
	public	UnityEngine.UI.Image 			m_sprBG;
	public	UnityEngine.UI.Image 			m_sprScuffedOverlay;
	public	UnityEngine.UI.Image[]			m_sprPanelImages;
	public	UnityEngine.UI.Text[] 			m_sprPanelTexts;

	public	Text							m_txtPriceBuyScuffed;
	public	Text							m_txtPriceBuyMint;
	public	Text							m_txtPriceSellScuffed;
	public	Text							m_txtPriceSellMint;
	public	Text							m_txtPriceSellScuffedNumCards;
	public	Text							m_txtPriceSellMintNumCards;
	public	Text							m_txtCardPosition;
	public	Text							m_txtCardRarity;

	public	GameObject						m_btnBuyScuffed;
	public	GameObject						m_btnBuyMint;
	
	public	GameObject						m_btnSellScuffed;
	public	GameObject						m_btnSellMint;

	public	Sprite							m_HiddenCardImage;
	public	Sprite							m_HeldCardImage;
	public	UnityEngine.UI.Image 			m_RenderCardImage;

	private TradingCardSpreadsheetItem 		m_CurCard;
	private	int								m_NumMint;
	private	int								m_NumScuffed;
	private	int								m_CurrentPosition;
	
	// Button presses
	private	bool							m_bButtonPressed;
	private	int								m_ButtonIndex;
	
	// Transition
	private	eSelectedCardMode				m_SelectedCardMode;
	private	bool							m_bCardZoomed = false;
	private	float							m_CurCardZoom = 1.0f;
	private	bool							m_bPlayerHasCard = false;
	private	bool							m_bInTransition;
	private	float							m_TransitionTimer;
	private	int								m_bTransitionDir;
	private	Vector3							m_TransitionStartCardPos;
	private	Vector3							m_TransitionEndCardPos;
	
	//=====================================================
	
	void Awake()
	{
		instance = this;
		
		m_bInTransition = false;
	}

	//=====================================================

	public void Reset()
	{
		m_bButtonPressed = false;
		m_ButtonIndex = 0;
	}
	
	//=============================================================================
	
	public TradingCardSpreadsheetItem GetSpreadsheetCard()
	{
		return( m_CurCard );
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
	
	public void SetPlayerHasCard( bool bHasCard )
	{
		m_bPlayerHasCard = bHasCard;
	}

	//=============================================================================
	
	public void SetupPanel( eSelectedCardMode SelectedCardMode , TradingCardSpreadsheetItem CurCard , int CurrentPosition , int NumMint , int NumScuffed )
	{
		m_NumMint = NumMint;
		m_NumScuffed = NumScuffed;
		m_CurrentPosition = CurrentPosition;
		m_CurCard = CurCard;
		m_SelectedCardMode = SelectedCardMode;
	}

	//=============================================================================

	public void ShowPanel( bool bActive )
	{
		if( bActive )
		{
			m_bButtonPressed = false;
			m_MainPanel.SetActive( true );
			
			// Start transition
			m_bInTransition = true;
			m_TransitionTimer = 0.0f;
			m_bTransitionDir = 1;
			m_bCardZoomed = false;
			m_CurCardZoom = 1.0f;
			
			// Setup transition start position
			m_TransitionStartCardPos = new Vector3( -225.0f , 136.0f + 45.0f , 0.5f ); // 244,-220
			m_TransitionStartCardPos.y -= ( m_CurrentPosition / 4 ) * 179.0f;
			m_TransitionStartCardPos.x += ( m_CurrentPosition & 3 ) * 156.0f;
			m_TransitionEndCardPos = new Vector3( 0.0f , 15.0f , 1.0f );
			
			// Does player have the card
			if( m_SelectedCardMode == eSelectedCardMode.Selling )
				m_bPlayerHasCard = true;
			else
				m_bPlayerHasCard = false;
			
			// Show scuffed overlay?
			if( ( m_NumMint <= 0 ) && ( m_NumScuffed > 0 ) )
			{
				m_sprScuffedOverlay.gameObject.SetActive( true );
			}
			else
			{
				m_sprScuffedOverlay.gameObject.SetActive( false );
			}
			
			// Setup card image
			try
			{
				Sprite RenderSprite = (Sprite)Resources.Load( "Cards/LargeGUITextures/" + m_CurCard.largeGuiTexture2D , typeof( Sprite ) );
				m_HeldCardImage = RenderSprite;
			}
			catch
			{
				Debug.LogError( "The card texture '" + m_CurCard.largeGuiTexture2D + "' needs to be set to a 'Sprite (UI)' type without mipmaps" );
				#if UNITY_EDITOR
				EditorUtility.DisplayDialog( "ERROR" , "The card texture '" + m_CurCard.largeGuiTexture2D + "' needs to be set to a 'Sprite (UI)' type without mipmaps", "OK");
				#endif
			}
			
			// Setup card text/rarity and buttons
			m_txtCardPosition.text = ( m_CurCard.globalPosition + 1 ).ToString();
			m_txtCardRarity.text = TextManager.GetText( "POPUP_CARDRARITY_" + (int)m_CurCard.rarity );
			m_btnBuyMint.SetActive( false );
			m_btnBuyScuffed.SetActive( false );
			m_btnSellMint.SetActive( false );
			m_btnSellScuffed.SetActive( false );
			
			float fScuffedDiscount 	= Convert.ToSingle( SettingsManager.GetSettingsItem( "TRADINGCARD_SALEPRICE_SCUFFED", -1 ) );
			float fMintDiscount 	= Convert.ToSingle( SettingsManager.GetSettingsItem( "TRADINGCARD_SALEPRICE_MINT", -1 ) );
			
			switch( m_SelectedCardMode )
			{
				case eSelectedCardMode.Buying:
				{
					m_txtPriceBuyScuffed.text = m_CurCard.valueScuffed.ToString();
					m_txtPriceBuyMint.text = m_CurCard.value.ToString();
					m_btnBuyMint.SetActive( true );
					m_btnBuyScuffed.SetActive( true );
				}
				break;
				
				case eSelectedCardMode.Selling:
				{
					m_txtPriceSellScuffed.text = ((int)( m_CurCard.valueScuffed * fScuffedDiscount )).ToString();
					m_txtPriceSellMint.text = ((int)( m_CurCard.value * fMintDiscount )).ToString();
					m_btnSellMint.SetActive( true );
					m_btnSellScuffed.SetActive( true );
					m_txtPriceSellMintNumCards.text = m_NumMint.ToString();
					m_txtPriceSellScuffedNumCards.text = m_NumScuffed.ToString();
				}
				break;
			}

			UpdateTransition( 0.0f );
		}
		else
		{
			// Start transition
			m_bInTransition = true;
			m_TransitionTimer = 1.0f;
			m_bTransitionDir = -1;
			UpdateTransition( 0.0f );
		}
	}

	//=====================================================

	public void OnButtonPressed_Cancel()
	{
		if( m_bCardZoomed == false )
		{
			m_bButtonPressed = true;
			m_ButtonIndex = 0;
		}
		else
		{
			m_bCardZoomed = false;
		}
	}

	//=====================================================
	
	public void OnButtonPressed_BuyMint()
	{
		m_bButtonPressed = true;
		m_ButtonIndex = 1;
	}

	//=====================================================

	public void OnButtonPressed_BuyScuffed()
	{
		m_bButtonPressed = true;
		m_ButtonIndex = 2;
	}

	//=====================================================

	public void OnButtonPressed_SellMint()
	{
		m_bButtonPressed = true;
		m_ButtonIndex = 3;
	}

	//=====================================================

	public void OnButtonPressed_SellScuffed()
	{
		m_bButtonPressed = true;
		m_ButtonIndex = 4;
	}

	//=====================================================

	public void OnButtonPressed_ToggleCardZoom()
	{
		if( m_bCardZoomed == false )
			m_bCardZoomed = true;
		else
			m_bCardZoomed = false;
	}

	//=====================================================

	public bool InTransition( int Dir )
	{
		if( Dir == m_bTransitionDir )
			return( m_bInTransition );
		else
			return( false );
	}
	
	//=====================================================
	
	public bool IsPanelActive()
	{
		return( m_TransitionTimer > 0.07f );
	}

	//=====================================================

	void UpdateTransition( float fDeltaTime )
	{
		if( m_bInTransition == false )
			return;
		
		switch( m_bTransitionDir )
		{
			case 1:
			{
				// In
				m_TransitionTimer += fDeltaTime * 2.0f;
				
				if( m_TransitionTimer >= 1.0f )
				{
					m_bInTransition = false;
				}
			}
			break;
			
			case -1:
			{
				// Out
				m_TransitionTimer -= fDeltaTime * 2.0f;

				if( m_TransitionTimer <= 0.0f )
				{
					m_bInTransition = false;
					m_MainPanel.SetActive( false );
				}
			}
			break;
		}
		
		float CurTimer = Mathf.Clamp( m_TransitionTimer , 0.0f , 1.0f );
		CurTimer = EaseT( CurTimer , 0.2f , 1.0f );
		
		// Transition UI elements
		Vector3 CurCardPos = Vector3.Lerp( m_TransitionStartCardPos , m_TransitionEndCardPos , CurTimer );
		m_RenderCardImage.gameObject.transform.localPosition = new Vector3( CurCardPos.x , CurCardPos.y , 0.0f );
		
		float fXScale = 1.0f;
		if( m_bPlayerHasCard )
		{
			m_RenderCardImage.sprite = m_HeldCardImage;
		}
		else
		{
			if( CurTimer < 0.5f )
			{
				m_RenderCardImage.sprite = m_HiddenCardImage;
				fXScale = ( 0.5f - CurTimer ) * 2.0f;
			}
			else
			{
				m_RenderCardImage.sprite = m_HeldCardImage;
				fXScale = ( CurTimer - 0.5f ) * 2.0f;
			}
		}
		
		m_RenderCardImage.gameObject.transform.localScale = new Vector3( CurCardPos.z * fXScale , CurCardPos.z , 1.0f );

		float fPanelAlpha = 0.0f;
		if( CurTimer > 0.75f )
		{
			fPanelAlpha = ( CurTimer - 0.75f ) * 4.0f;
		}
		m_sprBG.color = new Color( 1.0f , 1.0f , 1.0f , 0.62f * fPanelAlpha );

		foreach( UnityEngine.UI.Image CurImage in m_sprPanelImages )
		{
			CurImage.color = new Color( 1.0f , 1.0f , 1.0f , 1.0f * fPanelAlpha );
		}
		foreach( UnityEngine.UI.Text CurText in m_sprPanelTexts )
		{
			CurText.color = new Color( 1.0f , 1.0f , 1.0f , 1.0f * fPanelAlpha );
		}
	}

	//=====================================================
	
	void Update()
	{
		UpdateTransition( Time.deltaTime );
		
		// Update card zoom
		if( m_bInTransition == false )
		{
			if( m_bCardZoomed )
				PreHelpers.DeltaTend( ref m_CurCardZoom, 2.0f , 8.0f , Time.deltaTime );
			else
				PreHelpers.DeltaTend( ref m_CurCardZoom, 1.0f , 8.0f , Time.deltaTime );
			
			m_sprCardFrame.gameObject.transform.localScale = new Vector3( m_CurCardZoom , m_CurCardZoom , 1.0f );
		}
	}
	
	//=============================================================================

	float EaseT( float PosT , float EaseDuration , float TotalDuration )
	{
		float OutT = 0.0f;
		
		PosT *= 2.0f;
		if( PosT < 1.0f ) OutT = 0.5f * PosT * PosT;
		else
		{
			PosT -= 1.0f;
			OutT = -0.5f * ( PosT * ( PosT - 2.0f ) - 1.0f );
		}
		
		return OutT;
	}

	//=============================================================================
}
