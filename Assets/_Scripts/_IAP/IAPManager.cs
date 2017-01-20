using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using Prime31;
using UnityEngine.Analytics;

#if UNITY_WP8 || UNITY_WP_8_1 || UNITY_WINRT || UNITY_METRO
using Prime31;
using Prime31.WinPhoneStore;
#endif
#if YANDEX
using OnePF;
#endif

//=============================================================================

[System.Serializable]
public class IAPItem
{
	public enum eIAPType
	{
		CONSUMABLE,
		NON_CONSUMABLE
	}
	
	public	string			GameID;
	public	eIAPType		Type;
	public	int				Quantity;
	public	string			InventoryID;
	public	bool			Locked;

	public	string			ID_WP8;
	public	string			ID_IOS;
	public	string			ID_Android_Google;
	public	string			ID_Android_Samsung;
	
	public	string			ItemTitle;
	public	string			ItemDescription;
	public	string			ItemPrice;

	public	float			ItemPriceNumeric;
	public	string			ItemCurrencyCode;
	
	public	bool			ReadQuantityFromTitle;
	public	bool			UseInGameCurrency;
	public	bool			ValidIAP;
}

//=============================================================================

[System.Serializable]
public class InventoryDefaultTemplate
{
	public	string			InventoryID;
	public	int				DefaultQuantity;
}

//=============================================================================

[System.Serializable]
public class InventoryItem
{
	public	string			InventoryID;
	public	int				CurrentQuantity;
}

//=============================================================================

[System.Serializable]
public enum eIAPReturnCode
{
	OK,
	FAILED,
	NO_CONNECTION,
	CANCELLED
}

//=============================================================================

public class IAPManager : MonoBehaviour 
{
	// Delegate callbacks
	public delegate void IAPManagerPurchaseEventDelegate( bool bSuccess , IAPItem PurchasedItem , eIAPReturnCode ErrorCode );
	public delegate void IAPManagerRestoreEventDelegate( bool bSuccess , eIAPReturnCode ErrorCode );
	public delegate void IAPManagerBillingSupportedDelegate( bool bSuccess );

	public static IAPManagerPurchaseEventDelegate 		DelegatePurchaseCompleted;
	public static IAPManagerRestoreEventDelegate	 	DelegateRestoreCompleted;
	public static IAPManagerBillingSupportedDelegate	DelegateBillingSupported;


	public enum eIAPPlatform
	{
		IOS,
		ANDROID_GOOGLE,
		ANDROID_YANDEX,
		ANDROID_SAMSUNG,
		WP8
	}

	public 	static IAPManager		instance;

	public	bool					UseInventoryOnly = false;
	public	eIAPPlatform			Platform;
	public	bool					EditorIsStoreAvailable = true;
	public	eIAPReturnCode			EditorSimulate_PurchaseReturnCode = eIAPReturnCode.OK;
	public	eIAPReturnCode			EditorSimulate_RestorePurchaseReturnCode = eIAPReturnCode.OK;

	public	string					Key_Android_Google;
	public	string					Key_Android_Yandex;
	public	string					Key_Android_Samsung;
	
	public	IAPItem[]				IAP_ItemList;
	private	List< InventoryItem >	Inventory_ItemList = new List< InventoryItem >();
	public 	InventoryDefaultTemplate[] Inventory_DefaultTemplateList;
	
	#if YANDEX
	private	Inventory 				YandexInventory = null;
	#endif
	
	//=============================================================================

	private	bool					bIAPAvailableOnDevice = false;
	private	bool					bIAPInfoRetrieved = false;
	private	bool					bIAPRetrievingInfo = false;
	private	string					CurTransactionID;

	//=============================================================================

	void Awake()
	{
		// IAP manager needs to be available in all scenes so it doesn't continually
		// reload the IAP information from apple/google/kindle servers
		DontDestroyOnLoad( transform.gameObject );
	}

	//=============================================================================

	public bool IsStoreAvailable()
	{
		#if UNITY_EDITOR
		return( EditorIsStoreAvailable );
		#else
		if( bIAPAvailableOnDevice && bIAPInfoRetrieved )
			return( true );
		else
			return( false );
		#endif
	}
	
	//=============================================================================

	public bool WasIAPInfoRetrieved()
	{
		#if UNITY_EDITOR
		return( true );
		#else
		if( bIAPInfoRetrieved )
			return( true );
		else
			return( false );
		#endif
	}
	
	//=============================================================================
	
	public void RetryProductInfoRetrieval()
	{
		if( bIAPRetrievingInfo == false )
		{
			RequestProductInfo();
		}
	}

	//=============================================================================

	public IEnumerator EditorFakePurchase( string GameID )
	{
		yield return new WaitForSeconds( 1.5f );

		IAPItem CurItem = GetIAPItem( GameID );

		// Simulate failure/success
		if( EditorSimulate_PurchaseReturnCode == eIAPReturnCode.OK )
		{
			// Use product ID
			string ProductToPurchaseID = CurItem.ID_IOS;
			#if UNITY_ANDROID
			ProductToPurchaseID = CurItem.ID_Android_Google;
			#endif
			#if UNITY_WP8 || UNITY_WP_8_1 || UNITY_WINRT || UNITY_METRO
			ProductToPurchaseID = CurItem.ID_WP8;
			#endif
			ApplyPurchase( ProductToPurchaseID , true );
			if( DelegatePurchaseCompleted != null )
			{
				DelegatePurchaseCompleted( true , GetIAPItem( GameID ) , EditorSimulate_PurchaseReturnCode );
			}
		}
		else
		{
			if( DelegatePurchaseCompleted != null )
			{
				DelegatePurchaseCompleted( false , null , EditorSimulate_PurchaseReturnCode );
			}
		}
	}
	
	//=============================================================================

	public IEnumerator InGameCurrencyPurchase( string GameID )
	{
		yield return new WaitForSeconds( 0.2f );

		IAPItem CurItem = GetIAPItem( GameID );

		// Use product ID
		string ProductToPurchaseID = CurItem.ID_IOS;
		#if UNITY_ANDROID
		ProductToPurchaseID = CurItem.ID_Android_Google;
		#endif
		#if UNITY_WP8 || UNITY_WP_8_1 || UNITY_WINRT || UNITY_METRO
		ProductToPurchaseID = CurItem.ID_WP8;
		#endif
		ApplyPurchase( ProductToPurchaseID , true );
		
		if( DelegatePurchaseCompleted != null )
		{	
			DelegatePurchaseCompleted( true , GetIAPItem( GameID ) , eIAPReturnCode.OK );
		}
	}
	
	//=============================================================================

	public void PurchaseProduct( string GameID )
	{
		#if UNITY_EDITOR
		// In the editor just simulate a transaction
		StartCoroutine( EditorFakePurchase( GameID ) );
		#else
		// Find IAP item to purchase
		IAPItem CurItem = GetIAPItem( GameID );

		// If we're using in game currency then assume it was successful
		if( CurItem != null )
		{
			if( CurItem.UseInGameCurrency )
			{
				StartCoroutine( InGameCurrencyPurchase( GameID ) );
				return;
			}
		}

		if( IsStoreAvailable() == false )
		{
			Debug.Log( string.Format( "Store not available" ) );
			
			// Show Apple dialog
			#if UNITY_IPHONE
			string[] buttons = new string[] { "OK" };
			EtceteraBinding.showAlertWithTitleMessageAndButtons( "Error", "The shop is currently unavailable. Please try again later.", buttons );
			#endif
			
			if( DelegatePurchaseCompleted != null )
			{
				DelegatePurchaseCompleted( false , CurItem , eIAPReturnCode.NO_CONNECTION );
			}
			return;
		}
		
		// For WP8 always return a failed purchase in trial mode
		#if UNITY_WP8
		// Trial game?
		LicenseInformation StoreInfo = Store.getLicenseInformation();
		if( ( StoreInfo != null ) && StoreInfo.isTrial )
		{
			Debug.Log( string.Format( "Can't purchase item {0} in trial mode" , GameID ) );
			if( DelegatePurchaseCompleted != null )
			{
				DelegatePurchaseCompleted( false , null , eIAPReturnCode.FAILED );
			}
			return;
		}
		#endif
		
		
		if( CurItem == null )
		{
			Debug.Log( string.Format( "Can't find item {0} to purchase" , GameID ) );
			if( DelegatePurchaseCompleted != null )
			{
				DelegatePurchaseCompleted( false , null , eIAPReturnCode.FAILED );
			}
			return;
		}
		
		switch( Platform )
		{
			case eIAPPlatform.IOS:
			{
				#if UNITY_IPHONE
				string ProductToPurchaseID = null;
				ProductToPurchaseID = CurItem.ID_IOS;
				
				StoreKitBinding.purchaseProduct( ProductToPurchaseID, 1 );
				#endif
			}
			break;

			case eIAPPlatform.ANDROID_GOOGLE:
			{
				#if UNITY_ANDROID
				string ProductToPurchaseID = null;
				ProductToPurchaseID = CurItem.ID_Android_Google;
				
				GoogleIAB.purchaseProduct( ProductToPurchaseID );
				#endif
			}
			break;
			
			case eIAPPlatform.ANDROID_YANDEX:
			{
				#if UNITY_ANDROID && YANDEX
				string ProductToPurchaseID = null;
				ProductToPurchaseID = CurItem.ID_Android_Google;
				
				OpenIAB.purchaseProduct( ProductToPurchaseID );
				#endif
			}
			break;

			case eIAPPlatform.WP8:
			{
				#if UNITY_WP8 || UNITY_WP_8_1 || UNITY_WINRT || UNITY_METRO
				string ProductToPurchaseID = null;
				ProductToPurchaseID = CurItem.ID_WP8;
				
				CurTransactionID = ProductToPurchaseID;
				Store.requestProductPurchase( ProductToPurchaseID , OnPurchaseCompleteEvent );
				#endif
			}
			break;			
		}
		#endif
	}
	
	//=============================================================================

	public IEnumerator EditorFakeRestoreTransactions()
	{
		yield return new WaitForSeconds( 1.5f );

		// Simulate failure/success
		if( EditorSimulate_RestorePurchaseReturnCode == eIAPReturnCode.OK )
		{
			// Simulate purchasing all non consumable items
			if( DelegateRestoreCompleted != null )
			{
				DelegateRestoreCompleted( true , EditorSimulate_RestorePurchaseReturnCode );
			}
		}
		else
		{
			if( DelegateRestoreCompleted != null )
			{
				DelegateRestoreCompleted( false , EditorSimulate_RestorePurchaseReturnCode );
			}
		}
	}
	
	//=============================================================================
	
	public void RestoreCompletedTransactions()
	{
		#if UNITY_EDITOR
		// In the editor just simulate a transaction
		StartCoroutine( EditorFakeRestoreTransactions() );
		#else
		switch( Platform )
		{
			case eIAPPlatform.IOS:
			{
				#if UNITY_IPHONE
				StoreKitBinding.restoreCompletedTransactions();
				#endif
			}
			break;

			case eIAPPlatform.ANDROID_GOOGLE:
			{
				#if UNITY_ANDROID
				// Not available on Android
				if( DelegateRestoreCompleted != null )
				{
					DelegateRestoreCompleted( true , eIAPReturnCode.OK );
				}
				#endif
			}
			break;

			case eIAPPlatform.ANDROID_YANDEX:
			{
				#if UNITY_ANDROID
				// Not available on Android
				if( DelegateRestoreCompleted != null )
				{
					DelegateRestoreCompleted( true , eIAPReturnCode.OK );
				}
				#endif
			}
			break;

			case eIAPPlatform.WP8:
			{
				#if UNITY_WP8 || UNITY_WP_8_1 || UNITY_WINRT || UNITY_METRO
				// Restore previous transactions
				foreach( IAPItem CurItem in IAP_ItemList )
				{
					string ProductToPurchaseID = null;
					ProductToPurchaseID = CurItem.ID_WP8;
					
					ProductLicense CurLicense = Store.getProductLicense( ProductToPurchaseID );
					if( CurLicense == null )
						continue;
					
					if( CurLicense.isActive )
					{
						if( CurItem.Type == IAPItem.eIAPType.CONSUMABLE )
						{
							// Fulfill purchase
							Store.reportProductFulfillment( ProductToPurchaseID );
						}
						
						ApplyPurchase( ProductToPurchaseID , true );
					}
				}
				if( DelegateRestoreCompleted != null )
				{
					DelegateRestoreCompleted( true , eIAPReturnCode.OK );
				}
				#endif
			}
			break;
		}
		#endif
	}
	
	//=============================================================================

	public IAPItem GetIAPItem( string GameID )
	{
		foreach( IAPItem CurItem in IAP_ItemList )
		{
			if( CurItem.GameID.Equals( GameID ) )
			{
				// For real IAP items return null if store isnt available
				if( CurItem.UseInGameCurrency == false )
				{
					if( IsStoreAvailable() == false )
						return( null );
					else
						return CurItem;
				}
				else
				{
					return CurItem;
				}
			}
		}
		
		return null;
	}

	//=============================================================================

	public IAPItem GetIAPItemToNearestQuantity( int QuantityRequired )
	{
		foreach( IAPItem CurItem in IAP_ItemList )
		{
			if( CurItem.UseInGameCurrency == true )
				continue;
			
			if( CurItem.Quantity >= QuantityRequired )
			{
				return CurItem;
			}
		}
		
		return null;
	}

	//=============================================================================

	private IAPItem GetIAPItemFromProductID( string ProductID )
	{
		// Find corresponding IAP Item
		foreach( IAPItem CurItem in IAP_ItemList )
		{
			string IDMatch = null;
			
			switch( Platform )
			{
				case eIAPPlatform.IOS:
				{
					IDMatch = CurItem.ID_IOS;
				}
				break;

				case eIAPPlatform.WP8:
				{
					IDMatch = CurItem.ID_WP8;
				}
				break;

				case eIAPPlatform.ANDROID_GOOGLE:
				{
					IDMatch = CurItem.ID_Android_Google;
				}
				break;

				case eIAPPlatform.ANDROID_YANDEX:
				{
					IDMatch = CurItem.ID_Android_Google;
				}
				break;
			}
			
			if( ProductID.Equals( IDMatch ) )
			{
				return( CurItem );
			}
		}
		
		return( null );
	}

	//=============================================================================

	public InventoryItem GetInventoryItem( string InventoryID )
	{
		foreach( InventoryItem CurItem in Inventory_ItemList )
		{
			if( CurItem.InventoryID.Equals( InventoryID ) )
				return CurItem;
		}
		
		// Create a new inventory item
		InventoryItem NewItem = new InventoryItem();
		NewItem.InventoryID = InventoryID;
		NewItem.CurrentQuantity = GetDefaultQuantity( InventoryID );
		Inventory_ItemList.Add( NewItem );
		
		//SaveInventory();

		return( NewItem );
	}

	//=============================================================================
	
	int GetDefaultQuantity( string InventoryID )
	{
		if( Inventory_DefaultTemplateList == null )
			return( 0 );
		
		foreach( InventoryDefaultTemplate Template in Inventory_DefaultTemplateList )
		{
			if( Template.InventoryID.Equals( InventoryID ) )
				return Template.DefaultQuantity;
		}
		
		return( 0 );
	}

	//=============================================================================

	void OnEnable()
	{
		instance = this;

		#if UNITY_ANDROID
		Platform = eIAPPlatform.ANDROID_GOOGLE;
		#endif
		#if YANDEX
		Platform = eIAPPlatform.ANDROID_YANDEX;
		#endif
		

		if( UseInventoryOnly )
			return;

		// Setup callbacks
		switch( Platform )
		{
			case eIAPPlatform.IOS:
			{
				#if UNITY_IPHONE
				StoreKitManager.productListReceivedEvent += OnProductListReceivedEvent;
				StoreKitManager.productListRequestFailedEvent += OnProductListRequestFailedEvent;

				StoreKitManager.purchaseSuccessfulEvent += OnPurchaseSuccessfulEvent;
				StoreKitManager.purchaseCancelledEvent += OnPurchaseCancelledEvent;
				StoreKitManager.purchaseFailedEvent += OnPurchaseFailedEvent;

				//StoreKitManager.receiptValidationSuccessfulEvent += OnReceiptValidationSuccessful;
				//StoreKitManager.receiptValidationFailedEvent += OnReceiptValidationFailed;

				StoreKitManager.restoreTransactionsFinishedEvent += OnRestoreTransactionsFinished;
				StoreKitManager.restoreTransactionsFailedEvent += OnRestoreTransactionsFailed;
				#endif
			}
			break;

			case eIAPPlatform.ANDROID_GOOGLE:
			{
				#if UNITY_ANDROID
				#if !YANDEX
				GoogleIABManager.billingSupportedEvent += OnBillingSupportedEvent;
				GoogleIABManager.billingNotSupportedEvent += OnBillingNotSupportedEvent;
				GoogleIABManager.queryInventorySucceededEvent += OnQueryInventorySucceededEvent;
				GoogleIABManager.queryInventoryFailedEvent += OnQueryInventoryFailedEvent;
				GoogleIABManager.purchaseSucceededEvent += OnPurchaseSuccessfulEvent;
				GoogleIABManager.purchaseFailedEvent += OnPurchaseFailedEvent;
				GoogleIABManager.consumePurchaseSucceededEvent += OnConsumePurchaseSucceededEvent;
				GoogleIABManager.consumePurchaseFailedEvent += OnConsumePurchaseFailedEvent;
				#endif
				#endif
			}
			break;

			case eIAPPlatform.ANDROID_YANDEX:
			{
				#if UNITY_ANDROID
				#if YANDEX
				OpenIABEventManager.billingSupportedEvent += OnBillingSupportedEvent;
				OpenIABEventManager.billingNotSupportedEvent += OnBillingNotSupportedEvent;
				OpenIABEventManager.queryInventorySucceededEvent += OnQueryInventorySucceededEvent;
				OpenIABEventManager.queryInventoryFailedEvent += OnQueryInventoryFailedEvent;
				OpenIABEventManager.purchaseSucceededEvent += OnPurchaseSuccessfulEvent;
				OpenIABEventManager.purchaseFailedEvent += OnPurchaseFailedEvent;
				OpenIABEventManager.consumePurchaseSucceededEvent += OnConsumePurchaseSucceededEvent;
				OpenIABEventManager.consumePurchaseFailedEvent += OnConsumePurchaseFailedEvent;
				#endif
				#endif
			}
			break;
		}
	}
	
	//=============================================================================

	void OnDisable()
	{
		if( UseInventoryOnly )
			return;

		// Disable callbacks
		switch( Platform )
		{
			case eIAPPlatform.IOS:
			{
				#if UNITY_IPHONE
				StoreKitManager.productListReceivedEvent -= OnProductListReceivedEvent;
				StoreKitManager.productListRequestFailedEvent -= OnProductListRequestFailedEvent;
				StoreKitManager.purchaseSuccessfulEvent -= OnPurchaseSuccessfulEvent;
				StoreKitManager.purchaseCancelledEvent -= OnPurchaseCancelledEvent;
				StoreKitManager.purchaseFailedEvent -= OnPurchaseFailedEvent;
				//StoreKitManager.receiptValidationSuccessfulEvent -= OnReceiptValidationSuccessful;
				//StoreKitManager.receiptValidationFailedEvent -= OnReceiptValidationFailed;
				StoreKitManager.restoreTransactionsFinishedEvent -= OnRestoreTransactionsFinished;
				StoreKitManager.restoreTransactionsFailedEvent -= OnRestoreTransactionsFailed;
				#endif
			}
			break;

			case eIAPPlatform.ANDROID_GOOGLE:
			{
				#if UNITY_ANDROID
				#if !YANDEX
				GoogleIABManager.billingSupportedEvent -= OnBillingSupportedEvent;
				GoogleIABManager.billingNotSupportedEvent -= OnBillingNotSupportedEvent;
				GoogleIABManager.queryInventorySucceededEvent -= OnQueryInventorySucceededEvent;
				GoogleIABManager.queryInventoryFailedEvent -= OnQueryInventoryFailedEvent;
				GoogleIABManager.purchaseSucceededEvent -= OnPurchaseSuccessfulEvent;
				GoogleIABManager.purchaseFailedEvent -= OnPurchaseFailedEvent;
				GoogleIABManager.consumePurchaseSucceededEvent -= OnConsumePurchaseSucceededEvent;
				GoogleIABManager.consumePurchaseFailedEvent -= OnConsumePurchaseFailedEvent;
				#endif
				#endif
			}
			break;
			
			case eIAPPlatform.ANDROID_YANDEX:
			{
				#if UNITY_ANDROID
				#if YANDEX
				OpenIABEventManager.billingSupportedEvent -= OnBillingSupportedEvent;
				OpenIABEventManager.billingNotSupportedEvent -= OnBillingNotSupportedEvent;
				OpenIABEventManager.queryInventorySucceededEvent -= OnQueryInventorySucceededEvent;
				OpenIABEventManager.queryInventoryFailedEvent -= OnQueryInventoryFailedEvent;
				OpenIABEventManager.purchaseSucceededEvent -= OnPurchaseSuccessfulEvent;
				OpenIABEventManager.purchaseFailedEvent -= OnPurchaseFailedEvent;
				OpenIABEventManager.consumePurchaseSucceededEvent -= OnConsumePurchaseSucceededEvent;
				OpenIABEventManager.consumePurchaseFailedEvent -= OnConsumePurchaseFailedEvent;
				#endif
				#endif
			}
			break;
		}
	}
	
	//=============================================================================

	void LocaliseDescriptions()
	{
		// Localise text if required
		foreach( IAPItem CurItem in IAP_ItemList )
		{
			if( ( CurItem.ItemTitle != null ) && ( CurItem.ItemTitle.Length >= 2 ) && ( CurItem.ItemTitle[ 0 ] == '=' ) )
			{
				string LocaleTextID = CurItem.ItemTitle.TrimStart( '=' );
				CurItem.ItemTitle = TextManager.GetText( LocaleTextID );
			}
			if( ( CurItem.ItemDescription != null ) && ( CurItem.ItemDescription.Length >= 2 ) && ( CurItem.ItemDescription[ 0 ] == '=' ) )
			{
				string LocaleTextID = CurItem.ItemDescription.TrimStart( '=' );
				CurItem.ItemDescription = TextManager.GetText( LocaleTextID );
			}
			
			// Convert android IDs to lower case
			CurItem.ID_Android_Google = CurItem.ID_Android_Google.ToLower();
		}
	}
	
	//=============================================================================

	void Start() 
	{
		#if UNITY_ANDROID
		Platform = eIAPPlatform.ANDROID_GOOGLE;
		#endif
		#if YANDEX
		Platform = eIAPPlatform.ANDROID_YANDEX;
		#endif
		#if UNITY_WP8 || UNITY_WP_8_1 || UNITY_WINRT || UNITY_METRO
		Platform = eIAPPlatform.WP8;
		#endif
		
		// Load inventory
		LoadInventory();
		
		if( UseInventoryOnly )
			return;
		
		// Localise any text titles/descriptions
		LocaliseDescriptions();
		
		RequestProductInfo();
	}
		
	//=============================================================================

	void RequestProductInfo()
	{
		bIAPRetrievingInfo = true;
		
		switch( Platform )
		{
			case eIAPPlatform.IOS:
			{
				#if UNITY_IPHONE
				bIAPAvailableOnDevice = StoreKitBinding.canMakePayments();
				bIAPInfoRetrieved = false;
				
				// If store is available then start retrieving the product information
				if( bIAPAvailableOnDevice && ( IAP_ItemList.Length > 0 ) )
				{
					string[] ProductIdList = new string[ IAP_ItemList.Length ];
					int ID_Idx = 0;
					foreach( IAPItem CurItem in IAP_ItemList )
					{
						ProductIdList[ ID_Idx ] = CurItem.ID_IOS;

						ID_Idx++;
					}
					
					StoreKitBinding.requestProductData( ProductIdList );
				}
				#endif
			}
			break;

			case eIAPPlatform.ANDROID_GOOGLE:
			{
				#if UNITY_ANDROID
				// The callback for 'billing supported' sets the IAP available flag
				bIAPAvailableOnDevice = true;
				bIAPInfoRetrieved = false;
				
				// Initialise billing system
				GoogleIAB.init( Key_Android_Google );
				#endif
			}
			break;
			
			case eIAPPlatform.ANDROID_YANDEX:
			{
				#if UNITY_ANDROID
				#if YANDEX
				// The callback for 'billing supported' sets the IAP available flag
				bIAPAvailableOnDevice = true;
				bIAPInfoRetrieved = false;
				
				// Initialise billing system
				var options = new Options();
				options.checkInventoryTimeoutMs = Options.INVENTORY_CHECK_TIMEOUT_MS * 2;
				options.discoveryTimeoutMs = Options.DISCOVER_TIMEOUT_MS * 2;
				options.checkInventory = false;
				options.verifyMode = OptionsVerifyMode.VERIFY_SKIP;

				options.availableStoreNames = new string[] { OpenIAB_Android.STORE_YANDEX };
				options.storeSearchStrategy = SearchStrategy.INSTALLER_THEN_BEST_FIT;

				options.prefferedStoreNames = new string[] {OpenIAB_Android.STORE_YANDEX };
				
				options.storeKeys = new Dictionary<string, string> { {"com.yandex.store", Key_Android_Yandex } };
					
				// Transmit options and start the service
				OpenIAB.init( options );				
				#endif
				#endif
			}
			break;

			case eIAPPlatform.WP8:
			{
				#if UNITY_WP8 || UNITY_WP_8_1 || UNITY_WINRT || UNITY_METRO
				//LicenseInformation StoreInfo = Store.getLicenseInformation();
				
				bIAPAvailableOnDevice = true; //StoreKitBinding.canMakePayments();
				bIAPInfoRetrieved = false;
				
				// If store is available then start retrieving the product information
				if( bIAPAvailableOnDevice && ( IAP_ItemList.Length > 0 ) )
				{
					string[] ProductIdList = new string[ IAP_ItemList.Length ];
					int ID_Idx = 0;
					foreach( IAPItem CurItem in IAP_ItemList )
					{
						ProductIdList[ ID_Idx ] = CurItem.ID_WP8;

						ID_Idx++;
					}
					
					Store.loadListingInformation( OnProductListReceivedEvent );
				}
				#endif
			}
			break;
			
		}
	}
	
	//=============================================================================

	void Update() 
	{
	
	}
	
	//=============================================================================

	private void LoadInventory()
	{
		// Reset inventory
		Inventory_ItemList.Clear();
		
		// Read inventory from prefs
		if( PlayerPrefsWrapper.HasKey( "IAP_Inventory_IDList" ) && PlayerPrefsWrapper.HasKey( "IAP_Inventory_QuantityList" ) )
		{
			string[] IDList = PlayerPrefsX.GetStringArray( "IAP_Inventory_IDList" , ',' );
			int[] QuantityList = PlayerPrefsX.GetIntArray( "IAP_Inventory_QuantityList" );
		
			if( ( IDList.Length > 0 ) && ( QuantityList.Length > 0 ) )
			{
				for( int Idx = 0 ; Idx < IDList.Length ; Idx++ )
				{
					InventoryItem NewItem = new InventoryItem();
					NewItem.InventoryID = IDList[ Idx ];
					NewItem.CurrentQuantity = QuantityList[ Idx ];
					
					Inventory_ItemList.Add( NewItem );
				}
			}
		}
	}
	
	//=============================================================================

	public void SaveInventory()
	{
		// Convert inventory to strings
		PlayerPrefsWrapper.DeleteKey( "IAP_Inventory_IDList" );
		PlayerPrefsWrapper.DeleteKey( "IAP_Inventory_QuantityList" );
		
		int InventoryLength = Inventory_ItemList.Count;
		if( InventoryLength > 0 )
		{
			string[] IDList = new string[ InventoryLength ];
			int[] QuantityList = new int[ InventoryLength ];
			
			int IDIdx = 0;
			foreach( InventoryItem CurItem in Inventory_ItemList )
			{
				IDList[ IDIdx ] = CurItem.InventoryID;
				QuantityList[ IDIdx ] = CurItem.CurrentQuantity;
				IDIdx++;
			}
			
			PlayerPrefsX.SetStringArray( "IAP_Inventory_IDList", ',' , IDList );
			PlayerPrefsX.SetIntArray( "IAP_Inventory_QuantityList", QuantityList );
		}
	}

	//=============================================================================

	private void ApplyPurchase( string ProductId , bool bApplyConsumables )
	{
		// Find IAP item
		IAPItem CurItem = GetIAPItemFromProductID( ProductId );
		
		if( CurItem == null )
		{
			Debug.Log( string.Format( "Attempting to apply purchase for {0} but can't find it in the IAP list." , ProductId ) );
			return;
		}
		
		if( ( CurItem.Type == IAPItem.eIAPType.CONSUMABLE ) && ( bApplyConsumables == false ) )
			return;

		// Find inventory item
		InventoryItem CurInventoryItem = GetInventoryItem( CurItem.InventoryID );
		if( CurInventoryItem == null )
		{
			CurInventoryItem = new InventoryItem();
			Inventory_ItemList.Add( CurInventoryItem );

			CurInventoryItem.InventoryID = CurItem.InventoryID;
		}
		
		if( CurItem.Type == IAPItem.eIAPType.CONSUMABLE )
		{
			CurInventoryItem.CurrentQuantity += CurItem.Quantity;
		}
		else
		{
			CurInventoryItem.CurrentQuantity = 1;
		}
		
		SaveInventory();
	}

	//=============================================================================

	private void RevertPurchase( string ProductId )
	{
		// Find IAP item
		IAPItem CurItem = GetIAPItemFromProductID( ProductId );
		
		if( CurItem == null )
		{
			Debug.Log( string.Format( "Attempting to revert purchase for {0} but can't find it in the IAP list." , ProductId ) );
			return;
		}
		
		// Find inventory item
		InventoryItem CurInventoryItem = GetInventoryItem( CurItem.InventoryID );
		if( CurInventoryItem == null )
		{
			Debug.Log( string.Format( "Attempting to revert purchase for {0} but can't find it in the inventory." , ProductId ) );
			return;
		}
		
		if( CurItem.Type == IAPItem.eIAPType.CONSUMABLE )
		{
			CurInventoryItem.CurrentQuantity -= CurItem.Quantity;
			if( CurInventoryItem.CurrentQuantity < 0 )
				CurInventoryItem.CurrentQuantity = 0;
		}
		else
		{
			CurInventoryItem.CurrentQuantity = 0;
		}
		
		SaveInventory();
	}
	
	//=============================================================================

	public static int GetValueFromString( string InString )
	{
		string OutString = string.Empty;
		int val = -1;
		bool bParsingValue = false;
		for( int i = 0 ; i< InString.Length ; i++ )
		{
			if( Char.IsDigit(InString[i]) )
			{
				if( bParsingValue == false )
					bParsingValue = true;
				OutString += InString[i];
			}
			else
			{
				if( bParsingValue )
					break;
			}
		}
		
		if (OutString.Length>0)
		{
			val = int.Parse( OutString );
			
			if( val <= 0 )
				val = -1;
		}
		
		return( val );
	}

	//=============================================================================
	// IOS Callbacks
	//=============================================================================
	#if UNITY_IPHONE
	private void OnProductListReceivedEvent( List< StoreKitProduct > ProductList )
	{
		if( ProductList != null )
		{
			if( ProductList.Count > 0)
			{
				Debug.Log( string.Format( "Product list count: {0}" , ProductList.Count ) );
				
				foreach( StoreKitProduct CurProduct in ProductList )
				{
					// Find corresponding IAP Item
					foreach( IAPItem CurItem in IAP_ItemList )
					{
						string IDMatch = null;
						IDMatch = CurItem.ID_IOS;
						
						if( CurProduct.productIdentifier.Equals( IDMatch ) )
						{
							// Matching product, fill in information
							CurItem.ItemTitle = CurProduct.title;
							CurItem.ItemDescription = CurProduct.description;
							CurItem.ItemPrice = CurProduct.formattedPrice;
							CurItem.ItemPriceNumeric = float.Parse( CurProduct.price );
							CurItem.ItemCurrencyCode = CurProduct.currencyCode;
							
							// Read quantity from title if required
							if( CurItem.ReadQuantityFromTitle )
							{
								int TitleQuantity = GetValueFromString( CurItem.ItemTitle );
								if( TitleQuantity != -1 )
									CurItem.Quantity = TitleQuantity;
							}
						}
					}
				}
				
				// Allow use of the store
				bIAPInfoRetrieved = true;
			}
			else
			{
				Debug.Log( string.Format( "Product list was empty") );
			}
		}
		else
		{
			Debug.Log( string.Format( "Product list was null") );
		}

		bIAPRetrievingInfo = false;
	}
	
	//=============================================================================
	
	private void OnProductListRequestFailedEvent( string Error )
	{
		// Failed to retrieve the product information, let the user know there's been a problem
		if( Error != null )
		{
			if( Error.Length > 0 )
			{
				Debug.Log( string.Format( "ProductListRequestFailed: {0}" , Error ) );
			}
		}

		bIAPInfoRetrieved = false;
		bIAPRetrievingInfo = false;
	}
	
	//=============================================================================
	
	private void OnPurchaseSuccessfulEvent( StoreKitTransaction transaction )
	{
		Debug.Log( string.Format( "purchased product: {0}, quantity: {1}" , transaction.productIdentifier, transaction.quantity) );
		
		ApplyPurchase( transaction.productIdentifier , true );

		IAPItem CurItem = GetIAPItemFromProductID( transaction.productIdentifier );
		if( DelegatePurchaseCompleted != null )
		{
			DelegatePurchaseCompleted( true , CurItem , eIAPReturnCode.OK );
		}
		
		Analytics.Transaction( transaction.productIdentifier , (decimal)CurItem.ItemPriceNumeric , CurItem.ItemCurrencyCode , transaction.base64EncodedTransactionReceipt , transaction.transactionIdentifier );
		// Prime31 Plugin: validate receipt (use true for testing on sandbox server)
		//StoreKitBinding.validateReceipt(transaction.base64EncodedTransactionReceipt, /*true*/false);
	}
	
	//=============================================================================
	
	private void OnPurchaseFailedEvent( string Error )
	{
		Debug.Log( string.Format( "Purchase failed with error: {0}" , Error ) );
		if( DelegatePurchaseCompleted != null )
		{
			if( Error.Contains("Cannot connect") )
				DelegatePurchaseCompleted( false , null , eIAPReturnCode.NO_CONNECTION );
			else
				DelegatePurchaseCompleted( false , null , eIAPReturnCode.FAILED );
		}
	}
	
	//=============================================================================
	
	private void OnPurchaseCancelledEvent( string Error )
	{
		Debug.Log( string.Format( "Purchase cancelled with error: {0}" , Error ) );
		if( DelegatePurchaseCompleted != null )
		{
			DelegatePurchaseCompleted( false , null , eIAPReturnCode.CANCELLED );
		}
	}
	
	//=============================================================================
	/*
	private void OnReceiptValidationSuccessful()
	{		
		Debug.Log( string.Format( "Receipt validation successful!" ) );
	}
		
	//=============================================================================
	
	private void OnReceiptValidationFailed( string Error )
	{
		Debug.Log( string.Format( "Receipt validation failed with error: {0}" , Error ) );
	}
	*/
	//=============================================================================
	
	private void OnRestoreTransactionsFinished()
	{
		Debug.Log( string.Format( "Restoring Transactions Finished!") );
		
		List< StoreKitTransaction > TransactionsList = StoreKitBinding.getAllSavedTransactions();
		
		foreach( StoreKitTransaction RestoredTransaction in TransactionsList )
		{
			Debug.Log( string.Format( "Restoring product with Id: {0}" , RestoredTransaction.productIdentifier ) );
		
			ApplyPurchase( RestoredTransaction.productIdentifier , false );	
		}

		if( DelegateRestoreCompleted != null )
		{
			DelegateRestoreCompleted( true , eIAPReturnCode.OK );
		}
	}
	
	//=============================================================================
	
	private void OnRestoreTransactionsFailed( string Error )
	{
		Debug.Log( string.Format( "Restore transactions failed with error: {0}" , Error ) );

		if( DelegateRestoreCompleted != null )
		{
			if( Error.Contains("Cannot connect") )
				DelegateRestoreCompleted( false , eIAPReturnCode.NO_CONNECTION );
			else
				DelegateRestoreCompleted( false , eIAPReturnCode.FAILED );
		}
	}
	
	//=============================================================================
	#endif
	


	//=============================================================================
	// Android Callbacks
	//=============================================================================
	#if UNITY_ANDROID

	//=============================================================================

	#if YANDEX
	private void OnBillingSupportedEvent()
	{
		Debug.Log( "OnBillingSupportedEvent" );

		// Billing supported
		bIAPAvailableOnDevice = true;
		bIAPRetrievingInfo = false;
		
		// Find valid IAP count
		int ValidIAPCount = 0;
		foreach( IAPItem CurItem in IAP_ItemList )
		{
			if( CurItem.ValidIAP )
				ValidIAPCount++;
		}
		
		if( ValidIAPCount > 0 )
		{
			// Billing supported, retrieve product list
			string[] ProductIdList = new string[ ValidIAPCount ];
			int ID_Idx = 0;
			foreach( IAPItem CurItem in IAP_ItemList )
			{
				if( CurItem.ValidIAP )
				{
					ProductIdList[ ID_Idx ] = CurItem.ID_Android_Google;
		
					ID_Idx++;
				}
			}
			
			/*
			foreach( string IDString in ProductIdList )
			{
				Debug.Log( "ID: " + IDString );
			}
			*/
		
			OpenIAB.queryInventory( ProductIdList );
		}
	}
	
	//=============================================================================

	private void OnBillingNotSupportedEvent( string Error )
	{
		Debug.Log( string.Format( "OnBillingNotSupportedEvent: {0}" , Error ) );

		// Billing not supported
		bIAPAvailableOnDevice = false;
	}
	
	//=============================================================================

	private void OnQueryInventorySucceededEvent( Inventory ProductList )
	{
		Debug.Log( string.Format( "OnQueryInventorySucceededEvent" ) );

		// Fill in item descriptions/titles/prices 
		YandexInventory = ProductList;
		if( ProductList != null )
		{
			List<SkuDetails> SkuList = ProductList.GetAllAvailableSkus();
			if( SkuList.Count > 0 )
			{
				Debug.Log( string.Format( "Product list count: {0}" , SkuList.Count ) );
				
				foreach( SkuDetails CurProduct in SkuList )
				{
					// Find corresponding IAP Item
					foreach( IAPItem CurItem in IAP_ItemList )
					{
						string IDMatch = null;
						IDMatch = CurItem.ID_Android_Google;
						
						if( CurProduct.Sku.Equals( IDMatch ) )
						{
							// Matching product, fill in information
							CurItem.ItemTitle = CurProduct.Title;
							CurItem.ItemDescription = CurProduct.Description;
							CurItem.ItemPrice = CurProduct.Price;

							// Strip brackets from title
							int BracketIdx = CurItem.ItemTitle.IndexOf( '(' , 0 );
							if( BracketIdx != -1 )
							{
								CurItem.ItemTitle = CurItem.ItemTitle.Remove( BracketIdx );
							}
							CurItem.ItemTitle = CurItem.ItemTitle.Trim();

							// Read quantity from title if required
							if( CurItem.ReadQuantityFromTitle )
							{
								int TitleQuantity = GetValueFromString( CurItem.ItemTitle );
								if( TitleQuantity != -1 )
									CurItem.Quantity = TitleQuantity;
							}
						}
					}
				}
				
				// Allow use of the store
				bIAPInfoRetrieved = true;
				
				/*
				// Consume all 'consumable' items to make sure they're all used up
				string[] ConsumableItemList = GetConsumableItemList();
				
				if( ConsumableItemList != null )
					GoogleIAB.consumeProducts( ConsumableItemList );
				*/
			}
			else
			{
				Debug.Log( string.Format( "Product list was empty") );
			}
		}
		else
		{
			Debug.Log( string.Format( "Product list was null") );
		}
		
		// Restore previous transactions
		foreach( Purchase PreviousPurchase in ProductList.GetAllPurchases() )
		{
			IAPItem CurItem = GetIAPItemFromProductID( PreviousPurchase.Sku );
			if( CurItem != null )
			{
				if( CurItem.Type == IAPItem.eIAPType.CONSUMABLE )
				{
					// Consume product
					if( YandexInventory != null && YandexInventory.HasPurchase( PreviousPurchase.Sku ) )
					{
						OpenIAB.consumeProduct( PreviousPurchase );
					}
				}
				else
				{
					ApplyPurchase( PreviousPurchase.Sku , true );
				}
			}
		}
		
		if( DelegateBillingSupported != null )
		{
			DelegateBillingSupported( true );
		}
	}
	
	//=============================================================================
	
	private string[] GetConsumableItemList()
	{
		string[] ProductIdList = null;
		
		// Find valid IAP count
		int ValidIAPCount = 0;
		foreach( IAPItem CurItem in IAP_ItemList )
		{
			if( CurItem.ValidIAP && ( CurItem.Type == IAPItem.eIAPType.CONSUMABLE ) )
				ValidIAPCount++;
		}
		
		if( ValidIAPCount > 0 )
		{
			// Billing supported, retrieve product list
			ProductIdList = new string[ ValidIAPCount ];
			int ID_Idx = 0;
			foreach( IAPItem CurItem in IAP_ItemList )
			{
				if( CurItem.ValidIAP && ( CurItem.Type == IAPItem.eIAPType.CONSUMABLE ) )
				{
					ProductIdList[ ID_Idx ] = CurItem.ID_Android_Google;
		
					ID_Idx++;
				}
			}
		}
		
		return( ProductIdList );
	}

	//=============================================================================

	private void OnQueryInventoryFailedEvent( string Error )
	{
		Debug.Log( string.Format( "OnQueryInventoryFailedEvent: {0}" , Error ) );

		// Billing not supported
		bIAPAvailableOnDevice = false;
		bIAPInfoRetrieved = false;
	}
	
	//=============================================================================

	private void OnPurchaseSuccessfulEvent( Purchase CurPurchase )
	{
		Debug.Log( string.Format( "OnPurchaseSuccessfulEvent" ) );
		
		if( CurPurchase == null )
		{
			Debug.Log( "CurPurchase is null!" );
			if( DelegatePurchaseCompleted != null )
			{
				DelegatePurchaseCompleted( false , null , eIAPReturnCode.FAILED );
			}
		}
		
		Debug.Log( string.Format( "Purchased product: {0}", CurPurchase.Sku ) );
		ApplyPurchase( CurPurchase.Sku , true );
		
		// Attempt to consume purchase if required
		IAPItem CurIAPItem = GetIAPItemFromProductID( CurPurchase.Sku );

		if( CurIAPItem != null )
		{
			if( CurIAPItem.Type == IAPItem.eIAPType.CONSUMABLE )
			{
				OpenIAB.consumeProduct( CurPurchase );
				//if( bDebugMode )
					//OpenIAB.consumeProduct( YandexInventory.GetPurchase( CurIAPItem.DebugID_Android_Google ) );
				//else
					//OpenIAB.consumeProduct( YandexInventory.GetPurchase( CurIAPItem.ID_Android_Google ) );
			}
		}
		else
		{
			Debug.Log( "CurIAPItem is null!" );
			if( DelegatePurchaseCompleted != null )
			{
				DelegatePurchaseCompleted( false , null , eIAPReturnCode.FAILED );
			}
		}

		if( DelegatePurchaseCompleted != null )
		{
			DelegatePurchaseCompleted( true , CurIAPItem , eIAPReturnCode.OK );
		}
	}
	
	//=============================================================================
	
	private void OnPurchaseFailedEvent( int ErrorCode , string Error )
	{
		Debug.Log( string.Format( "OnPurchaseFailedEvent" ) );
		Debug.Log( string.Format("Purchase failed with error: {0}", Error ) );

		if( DelegatePurchaseCompleted != null )
		{
			if( Error.ToLower().Contains("unavail") )
				DelegatePurchaseCompleted( false , null , eIAPReturnCode.NO_CONNECTION );
			else if( Error.ToLower().Contains("cancel") )
				DelegatePurchaseCompleted( false , null , eIAPReturnCode.CANCELLED );
			else
				DelegatePurchaseCompleted( false , null , eIAPReturnCode.FAILED );
		}
	}
	
	//=============================================================================

	private void OnConsumePurchaseSucceededEvent( Purchase CurPurchase )
	{
		Debug.Log( string.Format( "OnConsumePurchaseSucceededEvent" ) );
		//Debug.Log( string.Format( "Purchased product: {0}", Purchase.productId ) );
	}
	
	//=============================================================================
	
	private void OnConsumePurchaseFailedEvent( string Error )
	{
		Debug.Log( string.Format( "OnConsumePurchaseFailedEvent" ) );
		//Debug.Log( string.Format( "Purchase failed with error: {0}", Error ) );
	}
	
	//=============================================================================
	#else
	private void OnBillingSupportedEvent()
	{
		Debug.Log( "OnBillingSupportedEvent" );

		// Billing supported
		bIAPAvailableOnDevice = true;
		bIAPRetrievingInfo = false;
		
		// Find valid IAP count
		int ValidIAPCount = 0;
		foreach( IAPItem CurItem in IAP_ItemList )
		{
			if( CurItem.ValidIAP )
				ValidIAPCount++;
		}
		
		if( ValidIAPCount > 0 )
		{
			// Billing supported, retrieve product list
			string[] ProductIdList = new string[ ValidIAPCount ];
			int ID_Idx = 0;
			foreach( IAPItem CurItem in IAP_ItemList )
			{
				if( CurItem.ValidIAP )
				{
					ProductIdList[ ID_Idx ] = CurItem.ID_Android_Google;
		
					ID_Idx++;
				}
			}
			
			/*
			foreach( string IDString in ProductIdList )
			{
				Debug.Log( "ID: " + IDString );
			}
			*/
		
			GoogleIAB.queryInventory( ProductIdList );
		}
	}
	
	//=============================================================================

	private void OnBillingNotSupportedEvent( string Error )
	{
		Debug.Log( string.Format( "OnBillingNotSupportedEvent: {0}" , Error ) );

		// Billing not supported
		bIAPAvailableOnDevice = false;
	}
	
	//=============================================================================

	private void OnQueryInventorySucceededEvent( List< GooglePurchase > Purchases , List< GoogleSkuInfo > ProductList )
	{
		Debug.Log( string.Format( "OnQueryInventorySucceededEvent" ) );

		// Fill in item descriptions/titles/prices 
		if( ProductList != null )
		{
			if( ProductList.Count > 0 )
			{
				Debug.Log( string.Format( "Product list count: {0}" , ProductList.Count ) );
				
				foreach( GoogleSkuInfo CurProduct in ProductList )
				{
					// Find corresponding IAP Item
					foreach( IAPItem CurItem in IAP_ItemList )
					{
						string IDMatch = null;
						IDMatch = CurItem.ID_Android_Google;
						
						if( CurProduct.productId.Equals( IDMatch ) )
						{
							// Matching product, fill in information
							CurItem.ItemTitle = CurProduct.title;
							CurItem.ItemDescription = CurProduct.description;
							CurItem.ItemPrice = CurProduct.price;
							CurItem.ItemPriceNumeric = ((float)CurProduct.priceAmountMicros)/1000000.0f;
							CurItem.ItemCurrencyCode = CurProduct.priceCurrencyCode;

							// Strip brackets from title
							int BracketIdx = CurItem.ItemTitle.IndexOf( '(' , 0 );
							if( BracketIdx != -1 )
							{
								CurItem.ItemTitle = CurItem.ItemTitle.Remove( BracketIdx );
							}
							CurItem.ItemTitle = CurItem.ItemTitle.Trim();

							// Read quantity from title if required
							if( CurItem.ReadQuantityFromTitle )
							{
								int TitleQuantity = GetValueFromString( CurItem.ItemTitle );
								if( TitleQuantity != -1 )
									CurItem.Quantity = TitleQuantity;
							}
						}
					}
				}
				
				// Allow use of the store
				bIAPInfoRetrieved = true;
				
				// Consume all 'consumable' items to make sure they're all used up
				string[] ConsumableItemList = GetConsumableItemList();
				
				if( ConsumableItemList != null )
					GoogleIAB.consumeProducts( ConsumableItemList );
			}
			else
			{
				Debug.Log( string.Format( "Product list was empty") );
			}
		}
		else
		{
			Debug.Log( string.Format( "Product list was null") );
		}
		
		// Restore previous transactions
		foreach( GooglePurchase PreviousPurchase in Purchases )
		{
			IAPItem CurItem = GetIAPItemFromProductID( PreviousPurchase.productId );
			if( CurItem != null )
			{
				if( CurItem.Type == IAPItem.eIAPType.CONSUMABLE )
					continue;
				
				if( PreviousPurchase.purchaseState == GooglePurchase.GooglePurchaseState.Purchased )
					ApplyPurchase( PreviousPurchase.productId , true );
				if( PreviousPurchase.purchaseState == GooglePurchase.GooglePurchaseState.Refunded )
					RevertPurchase( PreviousPurchase.productId );
			}
		}
		
		if( DelegateBillingSupported != null )
		{
			DelegateBillingSupported( true );
		}
	}
	
	//=============================================================================
	
	private string[] GetConsumableItemList()
	{
		string[] ProductIdList = null;
		
		// Find valid IAP count
		int ValidIAPCount = 0;
		foreach( IAPItem CurItem in IAP_ItemList )
		{
			if( CurItem.ValidIAP && ( CurItem.Type == IAPItem.eIAPType.CONSUMABLE ) )
				ValidIAPCount++;
		}
		
		if( ValidIAPCount > 0 )
		{
			// Billing supported, retrieve product list
			ProductIdList = new string[ ValidIAPCount ];
			int ID_Idx = 0;
			foreach( IAPItem CurItem in IAP_ItemList )
			{
				if( CurItem.ValidIAP && ( CurItem.Type == IAPItem.eIAPType.CONSUMABLE ) )
				{
					ProductIdList[ ID_Idx ] = CurItem.ID_Android_Google;
		
					ID_Idx++;
				}
			}
		}
		
		return( ProductIdList );
	}

	//=============================================================================

	private void OnQueryInventoryFailedEvent( string Error )
	{
		Debug.Log( string.Format( "OnQueryInventoryFailedEvent: {0}" , Error ) );

		// Billing not supported
		bIAPAvailableOnDevice = false;
		bIAPInfoRetrieved = false;
	}
	
	//=============================================================================

	private void OnPurchaseSuccessfulEvent( GooglePurchase Purchase )
	{
		Debug.Log( string.Format( "OnPurchaseSuccessfulEvent" ) );
		Debug.Log( string.Format( "Purchased product: {0}", Purchase.productId ) );
		ApplyPurchase( Purchase.productId , true );
		
		// Attempt to consume purchase if required
		IAPItem CurIAPItem = GetIAPItemFromProductID( Purchase.productId );
		
		if( CurIAPItem.Type == IAPItem.eIAPType.CONSUMABLE )
		{
			GoogleIAB.consumeProduct( CurIAPItem.ID_Android_Google );
		}

		if( DelegatePurchaseCompleted != null )
		{
			DelegatePurchaseCompleted( true , CurIAPItem , eIAPReturnCode.OK );
		}
		
		Analytics.Transaction( Purchase.productId , (decimal)CurIAPItem.ItemPriceNumeric , CurIAPItem.ItemCurrencyCode , Purchase.orderId , Purchase.signature );
	}
	
	//=============================================================================
	
	private void OnPurchaseFailedEvent( string Error , int Response )
	{
		Debug.Log( string.Format( "OnPurchaseFailedEvent" ) );
		Debug.Log( string.Format("Purchase failed with error: {0}", Error ) );

		if( DelegatePurchaseCompleted != null )
		{
			if( Error.ToLower().Contains("unavail") )
				DelegatePurchaseCompleted( false , null , eIAPReturnCode.NO_CONNECTION );
			else if( Error.ToLower().Contains("cancel") )
				DelegatePurchaseCompleted( false , null , eIAPReturnCode.CANCELLED );
			else
				DelegatePurchaseCompleted( false , null , eIAPReturnCode.FAILED );
		}
	}
	
	//=============================================================================

	private void OnConsumePurchaseSucceededEvent( GooglePurchase Purchase )
	{
		Debug.Log( string.Format( "OnConsumePurchaseSucceededEvent" ) );
		//Debug.Log( string.Format( "Purchased product: {0}", Purchase.productId ) );
	}
	
	//=============================================================================
	
	private void OnConsumePurchaseFailedEvent( string Error )
	{
		Debug.Log( string.Format( "OnConsumePurchaseFailedEvent" ) );
		//Debug.Log( string.Format( "Purchase failed with error: {0}", Error ) );
	}
	
	//=============================================================================
	#endif
	#endif
	
	
	
	//=============================================================================
	// WP8 Callbacks
	//=============================================================================
	#if UNITY_WP8 || UNITY_WP_8_1 || UNITY_WINRT || UNITY_METRO
	private void OnProductListReceivedEvent( ListingInformation ProductList )
	{
		if( ProductList != null ) 
		{
			if( ProductList.productListings.Count > 0)
			{
				Debug.Log( string.Format( "Product list count: {0}" , ProductList.productListings.Count ) );
				
				foreach( KeyValuePair< string , ProductListing > CurProduct in ProductList.productListings )
				{
					// Find corresponding IAP Item
					foreach( IAPItem CurItem in IAP_ItemList )
					{
						string IDMatch = null;
						IDMatch = CurItem.ID_WP8;
						
						if( CurProduct.Value.productId.Equals( IDMatch ) )
						{
							// Matching product, fill in information
							CurItem.ItemTitle = CurProduct.Value.name;
							CurItem.ItemDescription = CurProduct.Value.name;
							CurItem.ItemPrice = CurProduct.Value.formattedPrice;
							
							// Read quantity from title if required
							if( CurItem.ReadQuantityFromTitle )
							{
								int TitleQuantity = GetValueFromString( CurItem.ItemTitle );
								if( TitleQuantity != -1 )
									CurItem.Quantity = TitleQuantity;
							}
						}
					}
				}
				
				// Allow use of the store
				bIAPInfoRetrieved = true;
			}
			else
			{
				Debug.Log( string.Format( "Product list was empty") );
			}
		}
		else
		{
			Debug.Log( string.Format( "Product list was null - assuming error") );
		}

		bIAPRetrievingInfo = false;
	}
	
	//=============================================================================
	
	private void OnPurchaseCompleteEvent( string receipt , Exception error )
	{
		if( error != null )
		{
			// Failed
			Debug.Log( string.Format( "Purchase failed with error: {0}" , error ) );
			if( DelegatePurchaseCompleted != null )
			{
				DelegatePurchaseCompleted( false , null , eIAPReturnCode.FAILED );
			}
		}
		else
		{
			// Success
			Debug.Log( string.Format( "purchased product: {0} {1}" , CurTransactionID , receipt ) );
			
			ApplyPurchase( CurTransactionID , true );
			
			// Attempt to consume purchase if required
			IAPItem CurIAPItem = GetIAPItemFromProductID( CurTransactionID );
		
			if( ( CurIAPItem != null ) && ( CurIAPItem.Type == IAPItem.eIAPType.CONSUMABLE ) )
			{
				Store.reportProductFulfillment( CurIAPItem.ID_WP8 );
			}
			
			if( DelegatePurchaseCompleted != null )
			{
				DelegatePurchaseCompleted( true , GetIAPItemFromProductID( CurTransactionID ) , eIAPReturnCode.OK );
			}
		}
	}
	
	//=============================================================================
	/*
	private void OnReceiptValidationSuccessful()
	{		
		Debug.Log( string.Format( "Receipt validation successful!" ) );
	}
		
	//=============================================================================
	
	private void OnReceiptValidationFailed( string Error )
	{
		Debug.Log( string.Format( "Receipt validation failed with error: {0}" , Error ) );
	}
	*/
	//=============================================================================
	
	/*
	private void OnRestoreTransactionsFinished()
	{
		Debug.Log( string.Format( "Restoring Transactions Finished!") );
		
		List< StoreKitTransaction > TransactionsList = StoreKitBinding.getAllSavedTransactions();
		
		foreach( StoreKitTransaction RestoredTransaction in TransactionsList )
		{
			Debug.Log( string.Format( "Restoring product with Id: {0}" , RestoredTransaction.productIdentifier ) );
		
			ApplyPurchase( RestoredTransaction.productIdentifier , false );	
		}

		if( DelegateRestoreCompleted != null )
		{
			DelegateRestoreCompleted( true , eIAPReturnCode.OK );
		}
	}
	
	//=============================================================================
	
	private void OnRestoreTransactionsFailed( string Error )
	{
		Debug.Log( string.Format( "Restore transactions failed with error: {0}" , Error ) );

		if( DelegateRestoreCompleted != null )
		{
			if( Error.Contains("Cannot connect") )
				DelegateRestoreCompleted( false , eIAPReturnCode.NO_CONNECTION );
			else
				DelegateRestoreCompleted( false , eIAPReturnCode.FAILED );
		}
	}
	*/
	
	//=============================================================================
	#endif
}

//=============================================================================
