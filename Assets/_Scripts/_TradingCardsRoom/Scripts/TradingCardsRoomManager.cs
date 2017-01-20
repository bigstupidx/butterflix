using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Analytics;

public enum eSelectedCardMode
{
	Buying,
	Selling
};

public class TradingCardsRoomManager : MonoBehaviourEMS
{
	enum eRoomMode
	{
		ViewingBook,
		BoosterPacks,
		BoosterPacksPreview,
		SelectedCard,
		BuySellCardConfirm,
		BuyPackConfirm
	};
	
	public	Camera					m_BookCamera;
	public	GraphicRaycaster		m_RaycasterViewingBook;
	public	GraphicRaycaster		m_RaycasterBoosterPacks;
	public	GraphicRaycaster		m_RaycasterBoosterPacksPreview;
	public	GraphicRaycaster		m_RaycasterSelectedCard;
	public	GraphicRaycaster		m_RaycasterBuySellCardConfirm;
	public	GraphicRaycaster		m_RaycasterBuyPackConfirm;
	public	GameObject 				m_pfbShopPopup;

	public	GameObject[]			m_sprPageTypes; // Winx,wild,story,standard
	
	
	private	eTradingCardCondition	m_CurCardCondition;
	private	eRoomMode				m_RoomMode;
	private	eSelectedCardMode		m_SelectedCardMode;
	private	bool					m_bSwiping = false;
	private	float					m_SwipingStartX = 0.0f;
	private	float					m_SwipingAccel = 0.0f;
	private	float					m_SwipeTime = 0.0f;
	private	float					m_SwipeTimeAccum = 0.0f;
	private	Vector3[]				m_LastWorldPos = new Vector3[ 4 ];
	
	//=====================================================
	
	void Awake()
	{
		SetRoomMode( eRoomMode.ViewingBook );
	}
	
	//=====================================================

	void Start()
	{
		StartCoroutine( OpenBook() );
	}

	//=====================================================
	
	IEnumerator OpenBook()
	{	
		yield return new WaitForSeconds( 1.0f );
		
		// Open book to page 1
		BookManager.instance.TurnToPage( 1 );
		
		SoundFXManager.instance.PlaySound( "PageTurn" );
	}

	//=====================================================
	
	void SetRoomMode( eRoomMode Mode )
	{
		m_RoomMode = Mode;

		switch( m_RoomMode )
		{
			case eRoomMode.ViewingBook:
				m_RaycasterViewingBook.enabled = true;
				m_RaycasterBoosterPacks.enabled = false;
				m_RaycasterBoosterPacksPreview.enabled = false;
				m_RaycasterSelectedCard.enabled = false;
				m_RaycasterBuySellCardConfirm.enabled = false;
				m_RaycasterBuyPackConfirm.enabled = false;
				break;
			
			case eRoomMode.BoosterPacks:
				m_RaycasterViewingBook.enabled = false;
				m_RaycasterBoosterPacks.enabled = true;
				m_RaycasterBoosterPacksPreview.enabled = false;
				m_RaycasterSelectedCard.enabled = false;
				m_RaycasterBuySellCardConfirm.enabled = false;
				m_RaycasterBuyPackConfirm.enabled = false;
				break;
				
			case eRoomMode.BoosterPacksPreview:
				m_RaycasterViewingBook.enabled = false;
				m_RaycasterBoosterPacks.enabled = false;
				m_RaycasterBoosterPacksPreview.enabled = true;
				m_RaycasterSelectedCard.enabled = false;
				m_RaycasterBuySellCardConfirm.enabled = false;
				m_RaycasterBuyPackConfirm.enabled = false;
				break;

			case eRoomMode.SelectedCard:
				m_RaycasterViewingBook.enabled = false;
				m_RaycasterBoosterPacks.enabled = false;
				m_RaycasterBoosterPacksPreview.enabled = false;
				m_RaycasterSelectedCard.enabled = true;
				m_RaycasterBuySellCardConfirm.enabled = false;
				m_RaycasterBuyPackConfirm.enabled = false;
				break;

			case eRoomMode.BuySellCardConfirm:
				m_RaycasterViewingBook.enabled = false;
				m_RaycasterBoosterPacks.enabled = false;
				m_RaycasterBoosterPacksPreview.enabled = false;
				m_RaycasterSelectedCard.enabled = false;
				m_RaycasterBuySellCardConfirm.enabled = true;
				m_RaycasterBuyPackConfirm.enabled = false;
				break;

			case eRoomMode.BuyPackConfirm:
				m_RaycasterViewingBook.enabled = false;
				m_RaycasterBoosterPacks.enabled = false;
				m_RaycasterBoosterPacksPreview.enabled = false;
				m_RaycasterSelectedCard.enabled = false;
				m_RaycasterBuySellCardConfirm.enabled = false;
				m_RaycasterBuyPackConfirm.enabled = true;
				break;
		}
	}
	
	//=====================================================

	void OnDisable()
	{
		if( Application.isPlaying == false ) return;

		if( _isAppQuiting == true ) return;

		GameManager.Instance.CommonRoomEvent -= OnCommonRoomEvent;
		ScreenManager.FadeOutCompleteEvent -= OnFadeOutCompleteEvent;
	}
	
	//=====================================================

	void OnEnable()
	{
		GameManager.Instance.CommonRoomEvent += OnCommonRoomEvent;
		ScreenManager.FadeOutCompleteEvent += OnFadeOutCompleteEvent;
	}
	
	//=====================================================

	void Update()
	{
		switch( m_RoomMode )
		{
			case eRoomMode.ViewingBook:
				UpdateViewingBook();
				break;
			
			case eRoomMode.BoosterPacks:
				UpdateBoosterPacks();
				break;
				
			case eRoomMode.BoosterPacksPreview:
				UpdateBoosterPacksPreview();
				break;

			case eRoomMode.SelectedCard:
				UpdateSelectedCard();
				break;

			case eRoomMode.BuySellCardConfirm:
				UpdateBuySellCardConfirm();
				break;

			case eRoomMode.BuyPackConfirm:
				UpdateBuyPackConfirm();
				break;
		}
	}

	//=====================================================
	
	void UpdateBoosterPacks()
	{
		if( BoosterPacksManager.instance.WasButtonPressed() )
		{
			switch( BoosterPacksManager.instance.GetButtonPressed() )
			{
				case 0:
					// Cancel
					BoosterPacksManager.instance.ShowPanel( false );
					SetRoomMode( eRoomMode.ViewingBook );
					break;

				case 1:
				case 2:
				case 3:
					// Buy pack - enough money?
					int DiamondsRequired = BoosterPacksManager.instance.GetButtonPressed() * 50;
					int PlayerDiamonds = GameDataManager.Instance.PlayerDiamonds;
					if( PlayerDiamonds >= DiamondsRequired )
					{
						BuyPackConfirmManager.instance.ShowPanel( true , BoosterPacksManager.instance.GetButtonPressed() - 1 );
						BoosterPacksManager.instance.Reset();
						SetRoomMode( eRoomMode.BuyPackConfirm );
					}
					else
					{
						// Not enough money, show shop popup
						Instantiate( m_pfbShopPopup );
						ShopManager.instance.OnButtonPressed_ChangeType( 1 );
						BoosterPacksManager.instance.Reset();
					}
					break;
			}
		}
	}
	
	//======================================================
	
	void UpdateBoosterPacksPreview()
	{
		if( BoosterPacksPreviewManager.instance.WasButtonPressed() )
		{
			switch( BoosterPacksPreviewManager.instance.GetButtonPressed() )
			{
				case 0:
					// Cancel
					BoosterPacksPreviewManager.instance.ShowPanel( false );
					SetRoomMode( eRoomMode.ViewingBook );
					break;
			}
		}
	}
	
	//=====================================================

	void UpdateSelectedCard()
	{
		int PlayerGems = GameDataManager.Instance.PlayerGems;
		int PlayerDiamonds = GameDataManager.Instance.PlayerDiamonds;
		
		if( SelectedCardManager.instance.WasButtonPressed() )
		{
			if( SelectedCardManager.instance.InTransition( -1 ) )
			{
				// Fading out
				if( SelectedCardManager.instance.IsPanelActive() == false )
				{
					BookManager.instance.MarkDirty();
					SetRoomMode( eRoomMode.ViewingBook );
				}
			}
			else
			{
				switch( SelectedCardManager.instance.GetButtonPressed() )
				{
					case 0:
						// Cancel
						SelectedCardManager.instance.ShowPanel( false );
						break;

					case 1:
						// Buy mint
						if( PlayerDiamonds >= SelectedCardManager.instance.GetSpreadsheetCard().value )
						{
							SelectedCardManager.instance.Reset();
							BuySellCardConfirmManager.instance.ShowPanel( true , true , SelectedCardManager.instance.GetSpreadsheetCard() , eTradingCardCondition.MINT );
							m_CurCardCondition = eTradingCardCondition.MINT;
							SetRoomMode( eRoomMode.BuySellCardConfirm );
						}
						else
						{
							// Not enough money, show shop popup
							SelectedCardManager.instance.Reset();
							Instantiate( m_pfbShopPopup );
							ShopManager.instance.OnButtonPressed_ChangeType( 1 );
						}
						break;

					case 2:
						// Buy scuffed
						if( PlayerDiamonds >= SelectedCardManager.instance.GetSpreadsheetCard().valueScuffed )
						{
							SelectedCardManager.instance.Reset();
							BuySellCardConfirmManager.instance.ShowPanel( true , true , SelectedCardManager.instance.GetSpreadsheetCard() , eTradingCardCondition.SCUFFED );
							m_CurCardCondition = eTradingCardCondition.SCUFFED;
							SetRoomMode( eRoomMode.BuySellCardConfirm );
						}
						else
						{
							// Not enough money, show shop popup
							SelectedCardManager.instance.Reset();
							Instantiate( m_pfbShopPopup );
							ShopManager.instance.OnButtonPressed_ChangeType( 1 );
						}
						break;

					case 3:
						// Sell mint
						if( HasCard( SelectedCardManager.instance.GetSpreadsheetCard() , eTradingCardCondition.MINT ) )
						{
							SelectedCardManager.instance.Reset();
							BuySellCardConfirmManager.instance.ShowPanel( true , false , SelectedCardManager.instance.GetSpreadsheetCard() , eTradingCardCondition.MINT );
							m_CurCardCondition = eTradingCardCondition.MINT;
							SetRoomMode( eRoomMode.BuySellCardConfirm );
						}
						break;

					case 4:
						// Sell scuffed
						if( HasCard( SelectedCardManager.instance.GetSpreadsheetCard() , eTradingCardCondition.SCUFFED ) )
						{
							SelectedCardManager.instance.Reset();
							BuySellCardConfirmManager.instance.ShowPanel( true , false , SelectedCardManager.instance.GetSpreadsheetCard() , eTradingCardCondition.SCUFFED );
							m_CurCardCondition = eTradingCardCondition.SCUFFED;
							SetRoomMode( eRoomMode.BuySellCardConfirm );
						}
						break;
				}
			}
		}
	}

	//=====================================================
	
	void UpdateBuySellCardConfirm()
	{
		if( BuySellCardConfirmManager.instance.WasButtonPressed() )
		{
			switch( BuySellCardConfirmManager.instance.GetButtonPressed() )
			{
				case 0:
					// OK
					BuySellCardConfirmManager.instance.ShowPanel( false );
					SetRoomMode( eRoomMode.SelectedCard );
				
					switch( m_SelectedCardMode )
					{
						case eSelectedCardMode.Buying:
						{
							// Buy card
							TradingCardSpreadsheetItem CurCard = SelectedCardManager.instance.GetSpreadsheetCard();
							TradingCardHeldItem BoughtCard = GameDataManager.Instance.AddTradingCard( CurCard.id , m_CurCardCondition , true );
							BoughtCard.notifyTimer = 5.0f;
							SelectedCardManager.instance.SetPlayerHasCard( true );
			
							// If this is a teacher card unlock any NPC
							UnlockNPC( CurCard.id );
							
							// Use diamonds
							if( m_CurCardCondition == eTradingCardCondition.SCUFFED )
								GameDataManager.Instance.AddPlayerDiamonds( -SelectedCardManager.instance.GetSpreadsheetCard().valueScuffed );
							else
								GameDataManager.Instance.AddPlayerDiamonds( -SelectedCardManager.instance.GetSpreadsheetCard().value );

							GameDataManager.Instance.BroadcastGuiData();
							
							// Analytics event
							Dictionary<string, object> EventDictionary = new Dictionary<string, object>();
							EventDictionary["cardID"] = CurCard.id;
							EventDictionary["cardCondition"] = m_CurCardCondition.ToString();
							Analytics.CustomEvent("BuyCard", EventDictionary);				
						}
						break;

						case eSelectedCardMode.Selling:
						{
							// Sell card
							TradingCardSpreadsheetItem CurCard = SelectedCardManager.instance.GetSpreadsheetCard();
							GameDataManager.Instance.RemoveTradingCard( CurCard.id , m_CurCardCondition , true );
							SelectedCardManager.instance.SetPlayerHasCard( true );

							float fScuffedDiscount 	= Convert.ToSingle( SettingsManager.GetSettingsItem( "TRADINGCARD_SALEPRICE_SCUFFED", -1 ) );
							float fMintDiscount 	= Convert.ToSingle( SettingsManager.GetSettingsItem( "TRADINGCARD_SALEPRICE_MINT", -1 ) );
							
							// Add gems
							if( m_CurCardCondition == eTradingCardCondition.SCUFFED )
								GameDataManager.Instance.AddPlayerDiamonds( ((int)( SelectedCardManager.instance.GetSpreadsheetCard().valueScuffed * fScuffedDiscount )) );
							else
								GameDataManager.Instance.AddPlayerDiamonds( ((int)( SelectedCardManager.instance.GetSpreadsheetCard().value * fMintDiscount )) );

							GameDataManager.Instance.BroadcastGuiData();

							// Analytics event
							Dictionary<string, object> EventDictionary = new Dictionary<string, object>();
							EventDictionary["cardID"] = CurCard.id;
							EventDictionary["cardCondition"] = m_CurCardCondition.ToString();
							Analytics.CustomEvent("SellCard", EventDictionary);				
						}
						break;
					}
				
					// Dismiss card panel
					SelectedCardManager.instance.OnButtonPressed_Cancel();
					break;

				case 1:
					// Cancel
					BuySellCardConfirmManager.instance.ShowPanel( false );
					SetRoomMode( eRoomMode.SelectedCard );
					break;
			}
		}
	}

	//=====================================================

	void UpdateBuyPackConfirm()
	{
		if( BuyPackConfirmManager.instance.WasButtonPressed() )
		{
			switch( BuyPackConfirmManager.instance.GetButtonPressed() )
			{
				case 0:
					// OK
					BuyPackConfirmManager.instance.ShowPanel( false , 0 );
					SetRoomMode( eRoomMode.BoosterPacks );
				
					// Buy card pack
					int PackType = BuyPackConfirmManager.instance.GetPackType();
					int NumCardsInPack = ( PackType + 1 ) * 5;
					int DiamondsRequired = ( PackType + 1 ) * 50;
				
					// Deduct diamonds
					GameDataManager.Instance.AddPlayerDiamonds( -DiamondsRequired );
					GameDataManager.Instance.BroadcastGuiData();
				
					// Analytics event
					Dictionary<string, object> EventDictionary = new Dictionary<string, object>();
					EventDictionary["cardsInPack"] = NumCardsInPack;
					Analytics.CustomEvent("BuyBoosterPack", EventDictionary);				
				
					List< TradingCardHeldItem > CardsInPack = new List< TradingCardHeldItem >();
					for( int Idx = 0 ; Idx < NumCardsInPack ; Idx++ )
					{
						TradingCardSpreadsheetItem RandomCard = new TradingCardSpreadsheetItem();

						switch( UnityEngine.Random.Range( 0 , 3 ) )
						{
							case 0:
								// Return common card
								//RandomCard = TradingCardItemsManager.GetRandomCard( eTradingCardRarity.COMMON , 30.0f , eTradingCardRarity.VERYCOMMON , 70.0f );
								RandomCard = TradingCardItemsManager.GetRandomCard( eTradingCardRarity.COMMON , 30.0f , eTradingCardRarity.COMMON , 70.0f );
								break;
							case 1:
								// Return common or uncommon card
								RandomCard = TradingCardItemsManager.GetRandomCard( eTradingCardRarity.RARE , 100.0f , eTradingCardRarity.NULL , 0.0f );
								break;
							case 2:
								// Return uncmmon or rare card
								//RandomCard = TradingCardItemsManager.GetRandomCard( eTradingCardRarity.VERYRARE , 80.0f , eTradingCardRarity.UNIQUE , 20.0f );
								RandomCard = TradingCardItemsManager.GetRandomCard( eTradingCardRarity.VERYRARE , 80.0f , eTradingCardRarity.VERYRARE , 20.0f );
								break;
						}

						eTradingCardCondition CurCardCondition = eTradingCardCondition.MINT;
						if( UnityEngine.Random.Range( 0.0f , 100.0f ) < 50.0f )
							CurCardCondition = eTradingCardCondition.SCUFFED;

						bool bSaveData = false;
						if( Idx == ( NumCardsInPack - 1 ) )
							bSaveData = true;
						
						TradingCardHeldItem BoughtCard = GameDataManager.Instance.AddTradingCard( RandomCard.id , CurCardCondition , bSaveData );
						BoughtCard.notifyTimer = UnityEngine.Random.Range( 4.5f , 5.5f );
			
						// If this is a teacher card unlock any NPC
						UnlockNPC( RandomCard.id );
			
						CardsInPack.Add( BoughtCard );
					}
				
					// Dismiss booster pack panel
					BoosterPacksManager.instance.ShowPanel( false );
					//BoosterPacksManager.instance.OnButtonPressed_Cancel();
					BookManager.instance.MarkDirty();
					
					// Show preview of cards
					BoosterPacksPreviewManager.instance.ShowPanel( true , CardsInPack );
					SetRoomMode( eRoomMode.BoosterPacksPreview );
					break;

				case 1:
					// Cancel
					BuyPackConfirmManager.instance.ShowPanel( false , 0 );
					SetRoomMode( eRoomMode.BoosterPacks );
					break;
			}
		}
	}

	//=====================================================
	
	void UnlockNPC( string NPCID )
	{
		int NumNPCs = NPCItemsManager.GetNumItems();
		NPCItemData FoundNPCItem = null;
		
		for( int Idx = 0 ; Idx < NumNPCs ; Idx++ )
		{
			NPCItemData CurNPCItem = NPCItemsManager.GetNPCItem( Idx );
			
			if( CurNPCItem.CardId == NPCID )
			{
				// Card we just bought matches an NPC, unlock the NPC
				Debug.Log( "Found NPC to unlock for this card: " + CurNPCItem.Id );
				
				eNPC FoundNPCId = eNPC.NULL;
				try	{ FoundNPCId = (eNPC)Enum.Parse(typeof(eNPC), CurNPCItem.Id); }
				catch 
				{
					Debug.Log("Warning: FoundNPCId state not recognised!");
				}
				
				GameDataManager.Instance.UnlockPlayerNPC( FoundNPCId , true );
				return;
			}
		}
	}

	//=====================================================

	string GetCategoryText( eTradingCardClassification Category )
	{
		switch( Category )
		{
			case eTradingCardClassification.WINX:
				return( TextManager.GetText( "TRADINGCARDS_WINXMAGIC" ) );
			case eTradingCardClassification.WILD:
				return( TextManager.GetText( "TRADINGCARDS_WILDMAGIC" ) );
			case eTradingCardClassification.STORY:
				return( "" ); //TextManager.GetText( "TRADINGCARDS_STORY" ) );
			case eTradingCardClassification.STANDARD:
				return( TextManager.GetText( "TRADINGCARDS_STANDARD" ) );
		}

		return( "" );
	}

	//=====================================================

	void UpdateViewingBook()
	{
		// Update sprite that shows which page type we're on
		eTradingCardClassification VisibleClassification = TradingCardItemsManager.GetPageClassification( BookManager.instance.GetCurrentPage() );
		foreach( GameObject sprClassification in m_sprPageTypes )
		{
			if( sprClassification != null )
				sprClassification.SetActive( false );
		}
		m_sprPageTypes[ (int)VisibleClassification ].SetActive( true );
		
		// Set the page shortcuts for this classification (prev/next)
		int PrevCategory = (int)VisibleClassification - 1;
		int NextCategory = (int)VisibleClassification + 1;
		
		// Skip 'story' class
		if( VisibleClassification == eTradingCardClassification.STANDARD )
			PrevCategory--;
		if( VisibleClassification == eTradingCardClassification.WILD )
			NextCategory++;
		
		//m_txtCategoryPrev.text = GetCategoryText( (eTradingCardClassification)PrevCategory );
		//m_txtCategoryNext.text = GetCategoryText( (eTradingCardClassification)NextCategory );
		
		// Check for swiping
		if( BookManager.instance.IsPageTurning() )
			return;
		
		// Update area scrolling list position
		if( m_bSwiping )
		{
			m_SwipeTime += Time.deltaTime;
			m_SwipeTimeAccum += Time.deltaTime;
			Vector3 WorldPos = m_BookCamera.ScreenToViewportPoint( Input.mousePosition );
			WorldPos *= 384.0f;
			float SwipingDeltaX = WorldPos.x - m_SwipingStartX;
			
			m_SwipingAccel = ( WorldPos - m_LastWorldPos[ 3 ] ).x;
			
			if( m_SwipeTimeAccum > ( 1.0f / 60.0f ) )
			{
				for( int MIdx = 3 ; MIdx > 0 ; MIdx-- )
				{
					m_LastWorldPos[ MIdx ] = m_LastWorldPos[ MIdx - 1 ]; 
				}
				m_LastWorldPos[ 0 ] = WorldPos;
				m_SwipeTimeAccum -= ( 1.0f / 60.0f );
			}
			
			if( Input.GetMouseButtonUp( 0 ) )
			{
				m_bSwiping = false;
				
				m_SwipingAccel = Mathf.Clamp( m_SwipingAccel , -12.0f , 12.0f );
				
				bool bMinimumDistTravelled = true;
				
				if( Mathf.Abs( SwipingDeltaX ) < 28.0f )
					bMinimumDistTravelled = false;
				
				//Debug.Log( m_SwipingAccel + " " + bMinimumDistTravelled );
				if( ( Mathf.Abs( m_SwipingAccel ) < 7.0f ) || ( bMinimumDistTravelled == false ) )
				{
					// No swipe - selection of a card?
					m_SwipingAccel = 0.0f;
					if( ( m_SwipeTime < 0.25f ) && ( bMinimumDistTravelled == false ) )
					{
						// Selected card - check collision
						int CurrentPage = BookManager.instance.GetCurrentPage();
						int CurrentPosition = BookManager.instance.GetPositionAtMouse();
						
						// For first and last pages don't allow card selection
						if( BookManager.instance.IsValidPage() == false )
							CurrentPosition = -1;
						
						if( CurrentPosition != -1 )
						{
							// Valid card?
							bool bValidCard = true;
							TradingCardSpreadsheetItem CurSpreadsheetCard = TradingCardItemsManager.GetTradingCardItem( CurrentPage , CurrentPosition );
							if( CurSpreadsheetCard == null )
								bValidCard = false;

							if( bValidCard )
							{
								// Set buy/sell mode based on whether we have the card or not
								int NumMint = 0;
								int NumScuffed = 0;
								TradingCardHeldItem CurHeldCard = GameDataManager.Instance.GetHeldTradingCard( CurSpreadsheetCard.id , ref NumMint , ref NumScuffed );
								
								if( CurHeldCard != null )
									m_SelectedCardMode = eSelectedCardMode.Selling;
								else
									m_SelectedCardMode = eSelectedCardMode.Buying;
								
								// Show selected card panel
								SelectedCardManager.instance.SetupPanel( m_SelectedCardMode , CurSpreadsheetCard , CurrentPosition , NumMint , NumScuffed );
								SelectedCardManager.instance.ShowPanel( true );
								SetRoomMode( eRoomMode.SelectedCard );
								
								SoundFXManager.instance.PlaySound( "CardSelect" );

								
								// Hide this card on the book so we can zoom into it
								BookManager.instance.HideCard( CurrentPage , CurrentPosition );
							}
						}
					}
				}
				else
				{
					// Quick Swipe
					if( m_SwipingAccel < 0.0f )
					{
						BookManager.instance.TurnPage( 1 );
						SoundFXManager.instance.PlaySound( "PageTurn" );
					}
					else
					{
						BookManager.instance.TurnPage( -1 );
						SoundFXManager.instance.PlaySound( "PageTurn" );
					}
					
					m_SwipingAccel = 0.0f;
				}
			}
		}
		else
		{
			// Not swiping
			Vector3 WorldPos = m_BookCamera.ScreenToViewportPoint( Input.mousePosition );
			WorldPos *= 384.0f;
			for( int MIdx = 3 ; MIdx > 0 ; MIdx-- )
			{
				m_LastWorldPos[ MIdx ] = WorldPos;
			}
			
			m_SwipingStartX = WorldPos.x;
			m_SwipeTimeAccum = 0.0f;

			// Make sure pause menu or shop menu isn't active
			if( ( PopupPause.Instance.IsActive == false ) && ( ShopManager.instance == false ) )
			{
				if( Input.GetMouseButtonDown( 0 ) )
				{
					if( ( WorldPos.y > 48.0f ) && ( WorldPos.y < 323.0f ) )
					{
						m_bSwiping = true;
						m_SwipingAccel = 0.0f;
						m_SwipeTime = 0.0f;
					}
				}
			}
		}
	}

	//=====================================================

	bool HasCard( TradingCardSpreadsheetItem Card , eTradingCardCondition Condition )
	{
		int NumMint = 0;
		int NumScuffed = 0;
		TradingCardHeldItem CurHeldCard = GameDataManager.Instance.GetHeldTradingCard( Card.id , ref NumMint , ref NumScuffed );
		
		switch( Condition )
		{
			case eTradingCardCondition.MINT:
				if( NumMint > 0 )
					return( true );
				break;
			case eTradingCardCondition.SCUFFED:
				if( NumScuffed > 0 )
					return( true );
				break;
		}

		return( false );
	}
								
	//=====================================================

	void OnFadeOutCompleteEvent()
	{
		GameManager.UploadPlayerDataToServer();
		
		Application.LoadLevel( "CommonRoom" );
	}
	
	//=====================================================
	
	public void OnCommonRoomEvent()
	{
		ScreenManager.FadeOut();
	}

	//=====================================================

	public void OnButtonPressed_BoosterPacks()
	{
		// Show booster packs GUI
		BoosterPacksManager.instance.ShowPanel( true );
		SetRoomMode( eRoomMode.BoosterPacks );
	}

	//=====================================================

	public void OnButtonPressed_TypeWinx()
	{
		BookManager.instance.TurnToPage( TradingCardItemsManager.GetPageOffset( eTradingCardClassification.WINX ) );
	}

	//=====================================================

	public void OnButtonPressed_TypeWild()
	{
		BookManager.instance.TurnToPage( TradingCardItemsManager.GetPageOffset( eTradingCardClassification.WILD ) );
	}

	//=====================================================

	public void OnButtonPressed_TypeStandard()
	{
		BookManager.instance.TurnToPage( TradingCardItemsManager.GetPageOffset( eTradingCardClassification.STANDARD ) );
	}

	//=====================================================
}
