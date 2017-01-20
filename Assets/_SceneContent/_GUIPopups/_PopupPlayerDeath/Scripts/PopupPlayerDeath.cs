using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.Analytics;
#if UNITY_IPHONE
using Prime31;
#endif


public class PopupPlayerDeath : MonoBehaviour
{
	public static event Action PopupPlayerRevived;
	public static event Action PopupPlayerDead;

	public static PopupPlayerDeath Instance;

	[SerializeField]
	private GameObject	_fadePanel;
	[SerializeField]
	private Text		_txtCountdown;

	private	GameObject	_camera;
	private	float		_countdown;
	private	bool		_isCountdownActive;

	//=====================================================

	void Awake()
	{
		Instance = this;
		_countdown = 10.0f;
		_isCountdownActive = false;
	}

	//=====================================================

	public void Show()
	{
		if( _camera == null )
			_camera = transform.FindChild( "GuiCamera" ).gameObject;

		if( _camera == null ) return;

		_camera.SetActive( true );
		_countdown = 10.0f;
		_isCountdownActive = true;
	}

	//=====================================================

	public void OnButtonPressedBuy()
	{
		_isCountdownActive = false;

		// Do we have enough diamonds?
		var playerDiamonds = GameDataManager.Instance.PlayerDiamonds;
		if( playerDiamonds >= 5 )
		{
			// Analytics event
			var eventDictionary = new Dictionary<string, object>();
			eventDictionary["diamondsCount"] = playerDiamonds;
			//Analytics.CustomEvent( "RevivePlayer", eventDictionary );

			RevivePlayer();
		}
		else
		{
			// Offer to purchase diamonds in the shop
			if( (IAPManager.instance != null) && IAPManager.instance.IsStoreAvailable() )
			{
				var iapID = "diamondpack1";
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
			_fadePanel.SetActive( true );
		}
		else
		{
#if UNITY_IPHONE
			EtceteraBinding.hideActivityView();
#endif

			//UIManager.instance.UnlockInput();
			_fadePanel.SetActive( false );
		}
	}

	//=============================================================================

	public void PurchaseEventComplete( bool bSuccess, IAPItem PurchasedItem, eIAPReturnCode ErrorCode )
	{
		LockView( false );

		if( bSuccess )
		{
			// Add items to our total
			if( PurchasedItem != null )
			{
				Debug.Log( "Product Purchased: " + PurchasedItem.ItemTitle + "  (" + PurchasedItem.Quantity + ")" );
				GameDataManager.Instance.AddPlayerDiamonds( PurchasedItem.Quantity, true );
				GameDataManager.Instance.BroadcastGuiData();

				// Continue game
				RevivePlayer();
			}
		}
		else
		{
			// Log purchase of pack cancellation
			Debug.Log( "Purchase failed!" );
			
			// Restart timer
			_isCountdownActive = true;
		}
	}

	//=============================================================================

	void RevivePlayer()
	{
		GameDataManager.Instance.AddPlayerDiamonds( -5, true );
		GameDataManager.Instance.BroadcastGuiData();

		GameDataManager.Instance.AddPlayerLife();

		if( PopupPlayerRevived != null )
			PopupPlayerRevived();

		if( _camera != null )
		{
			_camera.SetActive( false );
		}
	}

	//=============================================================================

	public void OnButtonPressedEveryplay()
	{
		_isCountdownActive = false;

		EveryplayWrapper.ShareRecording( GameDataManager.Instance.PlayerCurrentFairyName.ToString() );
	}

	//=====================================================

	public void OnButtonPressedClose()
	{
		_isCountdownActive = false;
		if( PopupPlayerDead != null )
			PopupPlayerDead();

		_camera.SetActive( false );
	}

	//=====================================================

	void Update()
	{
		if( _isCountdownActive )
		{
			_countdown -= Time.deltaTime;
			if( _countdown < 0.0f )
			{
				_countdown = 0.0f;
				OnButtonPressedClose();
			}
		}

		// Update revive counter
		_txtCountdown.text = "00:" + ((int)_countdown).ToString( "D2" );
	}

	//=====================================================
}
