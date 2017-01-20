using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Analytics;

public class ClothingRoomManager : MonoBehaviourEMS
{
	public	static ClothingRoomManager	instance;

	enum eRoomMode
	{
		ViewingOutfits,
		BuyingOutfitFairy,
		FairyRequirements,
		FairyLocked
	};

	public	Camera					m_GUICamera;
	public	GraphicRaycaster		m_RaycasterMainGUI;
	public	GraphicRaycaster		m_RaycasterConfirmPurchase;
	public	GraphicRaycaster		m_RaycasterUpgradeFairy;
	public	GraphicRaycaster		m_RaycasterFairyLocked;
	public	GameObject 				m_pfbShopPopup;

	private	eRoomMode				m_RoomMode;

	// Fairy model
	public	GameObject				m_FairyPosition;
	private	GameObject				m_CurrentFairyObj;

	// Selected fairy GUI
	public	GameObject[]			m_sprFairyLevels;
	public	GameObject[]			m_sprFairyLevelsLocked;

	public	GameObject				m_btnPrevFairy;
	public	GameObject				m_btnNextFairy;
	public	GameObject				m_sprPurchaseOverlayGems;
	public	GameObject				m_sprPurchaseOverlayDiamonds;
	public	Text					m_txtPurchaseCostGems;
	public	Text					m_txtPurchaseCostDiamonds;
	public	Text					m_txtCurrentFairyName;
	private	int						m_CurrentFairyIndex;
	private	bool					m_bBuyOutfitActive;
	private	ClothingItemData		m_BuyOutfit;

	private	float					m_InspectOufitTimer;

	//=====================================================

	void Awake()
	{
		instance = this;
		m_CurrentFairyIndex = GameDataManager.Instance.PlayerCurrentFairy;
		m_CurrentFairyIndex = Mathf.Clamp( m_CurrentFairyIndex , 0 , 6 );
		m_bBuyOutfitActive = false;
		m_InspectOufitTimer = UnityEngine.Random.Range( 3.0f , 6.0f );
		SetRoomMode( eRoomMode.ViewingOutfits );
		//GameManager.Instance.CurrentLocation = eLocation.CLOTHING_ROOM;
	}
	
	//=====================================================

	void Start()
	{
		UpdateSelectedFairy();
	}

	//=====================================================
	
	void SetRoomMode( eRoomMode Mode )
	{
		m_RoomMode = Mode;

		switch( m_RoomMode )
		{
			case eRoomMode.ViewingOutfits:
				m_RaycasterMainGUI.enabled = true;
				m_RaycasterConfirmPurchase.enabled = false;
				m_RaycasterUpgradeFairy.enabled = false;
				m_RaycasterFairyLocked.enabled = false;
				break;
			
			case eRoomMode.BuyingOutfitFairy:
				m_RaycasterMainGUI.enabled = false;
				m_RaycasterConfirmPurchase.enabled = true;
				m_RaycasterUpgradeFairy.enabled = false;
				m_RaycasterFairyLocked.enabled = false;
				break;
				
			case eRoomMode.FairyRequirements:
				m_RaycasterMainGUI.enabled = false;
				m_RaycasterConfirmPurchase.enabled = false;
				m_RaycasterUpgradeFairy.enabled = true;
				m_RaycasterFairyLocked.enabled = false;
				break;

			case eRoomMode.FairyLocked:
				m_RaycasterMainGUI.enabled = false;
				m_RaycasterConfirmPurchase.enabled = false;
				m_RaycasterUpgradeFairy.enabled = false;
				m_RaycasterFairyLocked.enabled = true;
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
		
		// If we own the currently selected fairy then make that the 'current fairy'
		FairyData CurFairy = GameDataManager.Instance.GetFairyData( (eFairy)m_CurrentFairyIndex );
		if( CurFairy != null )
		{
			GameDataManager.Instance.PlayerCurrentFairy = m_CurrentFairyIndex;
		}
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
			case eRoomMode.ViewingOutfits:
				UpdateViewingOutfits();
				break;
			
			case eRoomMode.BuyingOutfitFairy:
				UpdateBuyingOutfitFairy();
				break;
				
			case eRoomMode.FairyRequirements:
				UpdateFairyRequirements();
				break;

			case eRoomMode.FairyLocked:
				UpdateFairyLocked();
				break;
		}
	}

	//=====================================================

	void UpdateViewingOutfits()
	{
		// Show selected fairy name and fairy level information/buttons
		{
			if(	m_CurrentFairyIndex <= 0 )
				m_btnPrevFairy.SetActive( false );
			else
				m_btnPrevFairy.SetActive( true );

			if(	m_CurrentFairyIndex >= 5 )
				m_btnNextFairy.SetActive( false );
			else
				m_btnNextFairy.SetActive( true );
			
			m_txtCurrentFairyName.text = TextManager.GetText( "FAIRYNAME_" + ((eFairy)m_CurrentFairyIndex).ToString() );
			
			// Fairy owned?
			FairyData CurFairy = GameDataManager.Instance.GetFairyData( (eFairy)m_CurrentFairyIndex );
			if( CurFairy == null )
			{
				// Not owned, do we meet the requirements to buy this fairy?
				FairyItemData CurFairyInfo = FairyItemsManager.GetFairyItem( (eFairy)m_CurrentFairyIndex );
				bool bRequirementsMet = AreRequirementsMet( CurFairyInfo );
				
				string txtPurchaseCost = TextManager.GetText( "BOUTIQUE_PURCHASE" );
				txtPurchaseCost = txtPurchaseCost.Replace( "(Cost)" , CurFairyInfo.GemsRequired[ 0 ].ToString() );
				m_txtPurchaseCostGems.text = txtPurchaseCost;
				m_txtPurchaseCostDiamonds.text = txtPurchaseCost;
				
				if( bRequirementsMet )
				{
					// Requirements met, show purchase button
					m_sprPurchaseOverlayGems.SetActive( true );
					m_sprPurchaseOverlayDiamonds.SetActive( false );

					FairyLockedManager.instance.ShowPanel( false , CurFairyInfo );
				}
				else
				{
					// Requirements not met, show requirements panel
					m_sprPurchaseOverlayGems.SetActive( false );
					m_sprPurchaseOverlayDiamonds.SetActive( false );

					FairyLockedManager.instance.ShowPanel( true , CurFairyInfo );
				}

				// Hide all fairy levels
				foreach( GameObject Obj in m_sprFairyLevels )
					Obj.SetActive( false );
				foreach( GameObject Obj in m_sprFairyLevelsLocked )
					Obj.SetActive( false );
			}
			else
			{
				// Owned, show current fairy level
				m_sprPurchaseOverlayGems.SetActive( false );
				m_sprPurchaseOverlayDiamonds.SetActive( false );
				FairyLockedManager.instance.ShowPanel( false );
				
				int FairyLevel = CurFairy.Level;

				int Idx = 1;
				foreach( GameObject Obj in m_sprFairyLevels )
				{
					Obj.SetActive( Idx <= FairyLevel ? true : false );
					Idx++;
				}

				Idx = 1;
				foreach( GameObject Obj in m_sprFairyLevelsLocked )
				{
					Obj.SetActive( Idx == ( FairyLevel + 1 ) ? true : false );
					Idx++;
				}
				
				// Buying outfit button active?
				if( m_bBuyOutfitActive )
				{
					m_sprPurchaseOverlayGems.SetActive( false );
					m_sprPurchaseOverlayDiamonds.SetActive( true );

					string txtPurchaseCost = TextManager.GetText( "BOUTIQUE_PURCHASE" );
					txtPurchaseCost = txtPurchaseCost.Replace( "(Cost)" , m_BuyOutfit.cost.ToString() );
					m_txtPurchaseCostGems.text = txtPurchaseCost;
					m_txtPurchaseCostDiamonds.text = txtPurchaseCost;
				}
			}
		}
		
		// Random outfit inspection anim
		m_InspectOufitTimer -= Time.deltaTime;
		if( m_InspectOufitTimer < 0.0f )
		{
			m_InspectOufitTimer = UnityEngine.Random.Range( 3.0f , 6.0f );
			m_CurrentFairyObj.GetComponent<Animator>().SetTrigger( "IsInspectingOutfit" );
		}
	}

	//=====================================================
	
	bool AreRequirementsMet( FairyItemData CurFairyInfo )
	{
		if( GameDataManager.Instance.PlayerPopulation >= CurFairyInfo.PopulationRequired[ 0 ] )
		{
			if( GameDataManager.Instance.GetNumKeysCollected() >= CurFairyInfo.KeysRequired[ 0 ] )
			{
				return( true );
			}
		}
		
		return( false );
	}

	//=====================================================

	public void OnPrevFairyButton()
	{
		if( m_CurrentFairyIndex > 0 )
		{
			m_CurrentFairyIndex--;
			m_CurrentFairyIndex = Mathf.Clamp( m_CurrentFairyIndex , 0 , 5 );
			UpdateSelectedFairy();
			m_bBuyOutfitActive = false;
		}
	}

	//=====================================================

	public void OnNextFairyButton()
	{
		if( m_CurrentFairyIndex < 5 )
		{
			m_CurrentFairyIndex++;
			m_CurrentFairyIndex = Mathf.Clamp( m_CurrentFairyIndex , 0 , 5 );
			UpdateSelectedFairy();
			m_bBuyOutfitActive = false;
		}
	}

	//=====================================================

	void UpdateSelectedFairy( string PreviewOutfitID = null )
	{
		if( m_CurrentFairyObj != null )
		{
			GameObject.Destroy( m_CurrentFairyObj );
			m_CurrentFairyObj = null;
		}
		
		string path = "Fairies/";

		switch( (eFairy)m_CurrentFairyIndex )
		{
			case eFairy.BLOOM:
				path += "Bloom/Prefabs/";
				break;
			case eFairy.STELLA:
				path += "Stella/Prefabs/";
				break;
			case eFairy.FLORA:
				path += "Flora/Prefabs/";
				break;
			case eFairy.MUSA:
				path += "Musa/Prefabs/";
				break;
			case eFairy.TECNA:
				path += "Tecna/Prefabs/";
				break;
			case eFairy.AISHA:
				path += "Aisha/Prefabs/";
				break;
		}
		
		// Fairy owned?
		eFairy CurFairyType = (eFairy)m_CurrentFairyIndex;
		RackScroller.instance.SetCurrentFairy( CurFairyType );
		RackRenderer.instance.SetCurrentFairy( CurFairyType );
		RackRenderer.instance.MarkDirty();

		FairyData CurFairy = GameDataManager.Instance.GetFairyData( CurFairyType );
		if( CurFairy == null )
		{
			// Not owned, show default outfit (unless we have an override)
			string DefaultItem = ClothingItemsManager.GetClothingDefaultItem( (eFairy)m_CurrentFairyIndex );
			
			if( PreviewOutfitID != null )
				DefaultItem = PreviewOutfitID;
			
			if( !String.IsNullOrEmpty( DefaultItem ) )
			{
				path += ClothingItemsManager.GetClothingItem( DefaultItem ).prefabName;
			}
		}
		else
		{
			// Owned, show current outfit
			string CurrentOutfit = CurFairy.Outfit;
			if( PreviewOutfitID != null )
				CurrentOutfit = PreviewOutfitID;
			
			if( !String.IsNullOrEmpty( CurrentOutfit ) )
			{
				ClothingItemData itemData = ClothingItemsManager.GetClothingItem(CurrentOutfit);
				if(itemData == null)
				{
					// Not owned, show default outfit (unless we have an override)
					string DefaultItem = ClothingItemsManager.GetClothingDefaultItem( (eFairy)m_CurrentFairyIndex );

					if( PreviewOutfitID != null )
						DefaultItem = PreviewOutfitID;

					if( !String.IsNullOrEmpty( DefaultItem ) )
					{
						path += ClothingItemsManager.GetClothingItem( DefaultItem ).prefabName;
					}
				}
				else
				{
					path += itemData.prefabName;
				}
			}
		}
		
		// Load prefab
		GameObject prefab = Resources.Load( path.ToString() ) as GameObject;

		if(prefab == null)
		{
			
		}

		if( prefab != null )
		{
			m_CurrentFairyObj = Instantiate( prefab ) as GameObject;
			m_CurrentFairyObj.transform.parent = m_FairyPosition.transform;
			m_CurrentFairyObj.transform.localPosition = new Vector3( 0.0f , 0.0f , 0.0f );
			m_CurrentFairyObj.transform.localEulerAngles = new Vector3( 0.0f , 0.0f , 0.0f );
		}
	}

	//=====================================================

	public void OnUpgradeFairyLevelButton1()
	{
		UpgradeFairyManager.instance.ShowPanel( true , 1 , (eFairy)m_CurrentFairyIndex );
		SetRoomMode( eRoomMode.FairyRequirements );
	}

	//=====================================================

	public void OnUpgradeFairyLevelButton2()
	{
		UpgradeFairyManager.instance.ShowPanel( true , 2 , (eFairy)m_CurrentFairyIndex );
		SetRoomMode( eRoomMode.FairyRequirements );
	}

	//=====================================================

	public void OnUpgradeFairyLevelButton3()
	{
		UpgradeFairyManager.instance.ShowPanel( true , 3 , (eFairy)m_CurrentFairyIndex );
		SetRoomMode( eRoomMode.FairyRequirements );
	}

	//=====================================================

	public void OnOutfitSelected( int Index )
	{
		// Move into 'buy?' overlay mode
		// Only allow outfits to be selected on bought fairies
		FairyData CurFairy = GameDataManager.Instance.GetFairyData( (eFairy)m_CurrentFairyIndex );
		if( CurFairy != null )
		{
			List< ClothingItemData > CurClothingList = ClothingItemsManager.GetClothingItems( (eFairy)m_CurrentFairyIndex );
			m_BuyOutfit = CurClothingList[ Index ];
			
			// If we own this outfit just wear it, otherwise show the 'buy' overlay
			if( CurFairy.OutfitOwned( m_BuyOutfit.id ) )
			{
				// Twirl anim
				bool bDoTwirl = false;
				if( CurFairy.Outfit != m_BuyOutfit.id )
					bDoTwirl = true;
				
				// Wear
				CurFairy.Outfit = m_BuyOutfit.id;
				m_bBuyOutfitActive = false;
				UpdateSelectedFairy();
				GameDataManager.Instance.SaveGameData();
				
				// Analytics event
				Dictionary<string, object> EventDictionary = new Dictionary<string, object>();
				EventDictionary["outfitID"] = m_BuyOutfit.id;
				Analytics.CustomEvent("WearOutfit", EventDictionary);				
				
				if( bDoTwirl )
				{
					m_InspectOufitTimer = UnityEngine.Random.Range( 3.0f , 6.0f );
					m_CurrentFairyObj.GetComponent<Animator>().SetTrigger( "IsChangingOutfit" );
				}
			}
			else
			{
				// Buy - show preview of outfit
				UpdateSelectedFairy( m_BuyOutfit.id );
				m_bBuyOutfitActive = true;
			}
		}
		else
		{
			// Fairy not owned, allow outfits to be tested anyway
			List< ClothingItemData > CurClothingList = ClothingItemsManager.GetClothingItems( (eFairy)m_CurrentFairyIndex );
			m_BuyOutfit = CurClothingList[ Index ];
			UpdateSelectedFairy( m_BuyOutfit.id );
		}
	}
	
	//=====================================================

	public void OnBuyOutfitFairy()
	{
		int PlayerGems = GameDataManager.Instance.PlayerGems;
		int PlayerDiamonds = GameDataManager.Instance.PlayerDiamonds;
		
		if( m_bBuyOutfitActive )
		{
			// Only show popup if player has enough diamonds to buy
			if( PlayerDiamonds >= m_BuyOutfit.cost )
			{
				ConfirmPurchaseManager.instance.ShowPanel( true , true , eFairy.BLOOM , m_BuyOutfit.cost );
				SetRoomMode( eRoomMode.BuyingOutfitFairy );
			}
			else
			{
				// Not enough money, show shop popup
				Instantiate( m_pfbShopPopup );
				ShopManager.instance.OnButtonPressed_ChangeType( 1 );
			}
		}
		else
		{
			FairyItemData CurFairyInfo = FairyItemsManager.GetFairyItem( (eFairy)m_CurrentFairyIndex );
			
			// Only show popup if player has enough gems to buy
			if( PlayerGems >= CurFairyInfo.GemsRequired[ 0 ] )
			{
				ConfirmPurchaseManager.instance.ShowPanel( true , false , (eFairy)m_CurrentFairyIndex , CurFairyInfo.GemsRequired[ 0 ] );
				SetRoomMode( eRoomMode.BuyingOutfitFairy );
			}
			else
			{
				// Not enough money, show shop popup
				Instantiate( m_pfbShopPopup );
				ShopManager.instance.OnButtonPressed_ChangeType( 0 );
			}
		}
	}

	//=====================================================

	void UpdateBuyingOutfitFairy()
	{
		// Twirl anim
		bool bDoTwirl = false;
		
		if( ConfirmPurchaseManager.instance.WasButtonPressed() )
		{
			switch( ConfirmPurchaseManager.instance.GetButtonPressed() )
			{
				case 0:
					// Buy outfit/fairy
					if( m_bBuyOutfitActive )
					{
						// Outfit
						FairyData CurFairy = GameDataManager.Instance.GetFairyData( (eFairy)m_CurrentFairyIndex );
						if( CurFairy != null )
						{
							CurFairy.BuyOutfit( m_BuyOutfit.id );
							CurFairy.Outfit = m_BuyOutfit.id;
							GameDataManager.Instance.SaveGameData();
							
							// Use diamonds
							GameDataManager.Instance.AddPlayerDiamonds( -m_BuyOutfit.cost );
							GameDataManager.Instance.BroadcastGuiData();
							
							// Analytics event
							Dictionary<string, object> EventDictionary = new Dictionary<string, object>();
							EventDictionary["outfitID"] = m_BuyOutfit.id;
							Analytics.CustomEvent("BuyOutfit", EventDictionary);				
							
							bDoTwirl = true;
						}
					}
					else
					{
						// Fairy
						GameDataManager.Instance.BuyFairy( (eFairy)m_CurrentFairyIndex , true );

						// Use gems
						FairyItemData CurFairyInfo = FairyItemsManager.GetFairyItem( (eFairy)m_CurrentFairyIndex );
						GameDataManager.Instance.AddPlayerGems( -CurFairyInfo.GemsRequired[ 0 ] );
						GameDataManager.Instance.BroadcastGuiData();

						// Analytics event
						Dictionary<string, object> EventDictionary = new Dictionary<string, object>();
						EventDictionary["fairyID"] = ( (eFairy)m_CurrentFairyIndex ).ToString();
						Analytics.CustomEvent("BuyFairy", EventDictionary);				
						
						bDoTwirl = true;
					}
					UpdateSelectedFairy();
					ConfirmPurchaseManager.instance.ShowPanel( false );
					SetRoomMode( eRoomMode.ViewingOutfits );
					m_bBuyOutfitActive = false;
					
					if( bDoTwirl )
					{
						m_InspectOufitTimer = UnityEngine.Random.Range( 3.0f , 6.0f );
						m_CurrentFairyObj.GetComponent<Animator>().SetTrigger( "IsChangingOutfit" );
					}
					break;
				
				case 1:
					// Cancel
					ConfirmPurchaseManager.instance.ShowPanel( false );
					SetRoomMode( eRoomMode.ViewingOutfits );
					m_bBuyOutfitActive = false;
					break;
			}
		}
	}

	//=====================================================

	void UpdateFairyRequirements()
	{
		int PlayerGems = GameDataManager.Instance.PlayerGems;

		if( UpgradeFairyManager.instance.WasButtonPressed() )
		{
			switch( UpgradeFairyManager.instance.GetButtonPressed() )
			{
				case 0:
					// Upgrade fairy level if we have enough diamonds
					bool bCanUpgradeFairy = true;
					bool bRequirementsWereMet = true;
					int UpgradeGemCost = 0;
				
					FairyItemData CurFairyInfo = FairyItemsManager.GetFairyItem( (eFairy)m_CurrentFairyIndex );
					FairyData CurFairy = GameDataManager.Instance.GetFairyData( (eFairy)m_CurrentFairyIndex );
					if( CurFairy != null )
					{
						UpgradeGemCost = CurFairyInfo.GemsRequired[ CurFairy.Level + 1 ];
						
						if( UpgradeGemCost > PlayerGems )
							bCanUpgradeFairy = false;

						if( GameDataManager.Instance.PlayerPopulation < CurFairyInfo.PopulationRequired[ CurFairy.Level + 1 ] )
						{
							bRequirementsWereMet = false;
							bCanUpgradeFairy = false;
						}
						
						if( GameDataManager.Instance.GetNumKeysCollected() < CurFairyInfo.KeysRequired[ CurFairy.Level + 1 ] )
						{
							bRequirementsWereMet = false;
							bCanUpgradeFairy = false;
						}
					}
					else
					{
						bCanUpgradeFairy = false;
					}
					
					if( bCanUpgradeFairy )
					{
						CurFairy.Level++;
						GameDataManager.Instance.SaveGameData();
						GameDataManager.Instance.AddPlayerGems( -UpgradeGemCost );
						GameDataManager.Instance.BroadcastGuiData();
						UpgradeFairyManager.instance.ShowPanel( false );
						SetRoomMode( eRoomMode.ViewingOutfits );
						
						// Analytics event
						Dictionary<string, object> EventDictionary = new Dictionary<string, object>();
						EventDictionary["fairyID"] = ((eFairy)m_CurrentFairyIndex).ToString();
						EventDictionary["fairyLevel"] = CurFairy.Level;
						Analytics.CustomEvent("UpgradeFairy", EventDictionary);				
					}
					else
					{
						UpgradeFairyManager.instance.Reset();
						
						// Not enough money, show shop popup (if requirements are met)
						if( bRequirementsWereMet )
						{
							Instantiate( m_pfbShopPopup );
							ShopManager.instance.OnButtonPressed_ChangeType( 0 );
						}
					}
					break;
				
				case 1:
					// Cancel
					UpgradeFairyManager.instance.ShowPanel( false );
					SetRoomMode( eRoomMode.ViewingOutfits );
					break;
			}
		}
	}

	//=====================================================

	void UpdateFairyLocked()
	{
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
}
