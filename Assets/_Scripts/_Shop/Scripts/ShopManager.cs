using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Prime31;
using UnityEngine.Analytics;

public class ShopManager : MonoBehaviour
{
	static	public	ShopManager						instance;

	enum eShopPageType
	{
		GEMS = 0,
		DIAMONDS,
		MOREGAMES
	}
	
	public	GameObject								m_MainPanel;
	public	GameObject								m_FadePanel;
	public	GameObject[]							m_ScrollingLists;
	public	GameObject[]							m_TypeHighlights;

	private	eShopPageType							m_CurrentPage;

	// Button presses
	private	bool									m_bButtonPressed;
	private	int										m_ButtonIndex;
	
	//=====================================================
	
	void Awake()
	{
		instance = this;
		m_CurrentPage = eShopPageType.GEMS;
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
		}
		else
		{
			m_MainPanel.SetActive( false );
		}
	}

	//=====================================================
	
	public void Update()
	{
		// Show correct page scrolling list and highlight icon
		foreach( GameObject ScrollList in m_ScrollingLists )
			ScrollList.SetActive( false );
		
		foreach( GameObject TypeHighlight in m_TypeHighlights )
			TypeHighlight.SetActive( false );

		m_ScrollingLists[ (int)m_CurrentPage ].SetActive( true );
		m_TypeHighlights[ (int)m_CurrentPage ].SetActive( true );
	}

	//=====================================================

	public void OnButtonPressed_Cancel()
	{
		m_bButtonPressed = true;
		m_ButtonIndex = 0;
		
		ShowPanel( false );
		GameObject.Destroy( this.gameObject );
	}

	//=====================================================

	public void OnButtonPressed_ChangeType( int Type )
	{
		m_CurrentPage = (eShopPageType)Type;

		// Analytics event
		Dictionary<string, object> EventDictionary = new Dictionary<string, object>();
		EventDictionary["pageType"] = ((eShopPageType)Type).ToString();
		Analytics.CustomEvent("ShopChangePage", EventDictionary);				
	}
	
	//=====================================================
	
	public void OnButtonPressed_BuyItem( string iapID )
	{
		if( ( IAPManager.instance != null ) && IAPManager.instance.IsStoreAvailable() )
		{
			Debug.Log( "Buying IAP item: " + iapID );

			LockView( true );
			IAPManager.DelegatePurchaseCompleted = PurchaseEventComplete;
			IAPManager.instance.PurchaseProduct( iapID );
		}
		else
		{
			Debug.LogError( "IAPManager not available!" );
		}	
	}
	
	//=====================================================
	
	void LockView( bool bLock )
	{
		if( bLock )
		{
			#if UNITY_IPHONE
			EtceteraBinding.showActivityView();
			#endif

			//UIManager.instance.LockInput();
			m_FadePanel.SetActive( true );
		}
		else
		{
			#if UNITY_IPHONE
			EtceteraBinding.hideActivityView();
			#endif

			//UIManager.instance.UnlockInput();
			m_FadePanel.SetActive( false );
		}
	}
	
	//=============================================================================

	public void PurchaseEventComplete( bool bSuccess , IAPItem PurchasedItem , eIAPReturnCode ErrorCode )
	{
		LockView( false );
		
		if ( bSuccess )
		{
			// Add items to our total
			if ( PurchasedItem != null )
			{
				Debug.Log( "Product Purchased: " + PurchasedItem.ItemTitle + "  (" + PurchasedItem.Quantity + ")" );
				switch( m_CurrentPage )
				{
				case eShopPageType.GEMS:
					GameDataManager.Instance.AddPlayerGems( PurchasedItem.Quantity , true );
					break;
				
				case eShopPageType.DIAMONDS:
					GameDataManager.Instance.AddPlayerDiamonds( PurchasedItem.Quantity , true );
					break;
				}
				
				GameDataManager.Instance.BroadcastGuiData();
			}
		}
		else
		{
			// Log purchase of pack cancellation
			Debug.Log("Purchase failed!");
		}
	}
	
	//=============================================================================

	#if UNITY_IPHONE || UNITY_EDITOR
    string iTunesURL		= "http://www.appstore.com/tsumangastudiosltd";
    string iTunesURLShredIt	= "https://itunes.apple.com/app/id884964321";
	#endif

	#if UNITY_ANDROID
    string KindleURL 		= "http://www.amazon.com/s/ref=bl_sr_mobile-apps?_encoding=UTF8&field-brandtextbin=Tsumanga%20Studios&node=2350149011";
    string PlayURL 			= "https://play.google.com/store/apps/developer?id=tsumanga+studios+limited";
    string PlayURLShredIt 	= "https://play.google.com/store/apps/details?id=com.emstudios.shredit";
	#endif
	
    public void OnButtonPressed_MoreGames( int AppIndex )
    {
		// Analytics event
		string gameLink = "Unknown";
		switch( AppIndex )
		{
			case 0:	gameLink = "Fairy School";	break;
			case 1:	gameLink = "Sirenix";	break;
			case 2:	gameLink = "Abyss";	break;
			case 3:	gameLink = "Sing It Laurie";	break;
			case 4:	gameLink = "Shred It";	break;
		}
		
		Dictionary<string, object> EventDictionary = new Dictionary<string, object>();
		EventDictionary["gameLink"] = gameLink;
		Analytics.CustomEvent("MoreGames", EventDictionary);				

		
        string url = "";
		if( AppIndex == 4 )
		{
			// Shred It
			#if UNITY_IPHONE
			url = iTunesURLShredIt;
			#elif UNITY_EDITOR
			url = iTunesURLShredIt;
			#elif UNITY_ANDROID
			url = PlayURLShredIt;
			#endif
		}
		else
		{
			// Tsumanga Games
			#if UNITY_IPHONE
			url = iTunesURL;
			#elif UNITY_EDITOR
			url = iTunesURL;
			#elif UNITY_ANDROID
			AndroidJavaClass build = new AndroidJavaClass("android.os.Build");
			if (build.GetStatic<string>("MANUFACTURER") == "Amazon")
				url = KindleURL;
			else
				url = PlayURL;
			#endif
		}
        Application.OpenURL( url );
    }
}
