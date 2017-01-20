using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class SceneManager : MonoBehaviour
{
	public static event Action<int> RedGemEvent;

	// Local Player Fields
	private static int _maxPlayerGems;
	private static int _maxPlayerRedGems;
	private static PlayerData _localPlayerData = new PlayerData();
	private static eSwitchItem _currentSwitchItem = eSwitchItem.NULL;

	// Local Scene Fields
	private int _numMicroVillains;
	private static bool _is100GemKeyActive;
	private static bool _isRedGemKeyActive;

	//=====================================================

	#region Public Interface

	public static eSwitchItem CurrentSwitchItem { get { return _currentSwitchItem; } set { _currentSwitchItem = value; } }

	public static int NumGemsCollectedInScene { get { return _localPlayerData.Gems; } }

	public static int NumRedGemsCollectedInScene { get { return _localPlayerData.RedGems; } }

	//=====================================================

	public static void UpdateGameDataManager( bool clearLocalData = true )
	{
		GameDataManager.Instance.AddPlayerGems( _localPlayerData.Gems );
		GameDataManager.Instance.AddPlayerRedGems( _localPlayerData.RedGems );

		if( clearLocalData == false ) return;
		
		_localPlayerData.Gems = 0;
		_localPlayerData.RedGems = 0;

		// Force gui update
		AddPlayerGems( 0 );
	}

	//=====================================================

	public static void AddPlayerGems( int gems )
	{
		if( gems != 0 )
		{
			_localPlayerData.Gems = Mathf.Clamp( _localPlayerData.Gems + gems, 0, _maxPlayerGems );

			// Show popup then unlock puzzle-room key after collecting 100 gems
			if( _localPlayerData.Gems >= 100 && _is100GemKeyActive == false )
			{
				if( PopupKeyUnlocked.Instance != null )
				{
					PopupKeyUnlocked.Instance.Show( ePuzzleKeyType.KEY_GEM_100 );

					_is100GemKeyActive = true;
				}
			}
		}

		// In puzzle-rooms we display the SceneManagers gems total rather than the GameDataManager->player's running total
		if( GuiManager.Instance != null )
		{
			var location = GameManager.Instance.CurrentLocation;

			// Puzzle Room enums range from 0+ (eLocation.PUZZLE_ROOM_01 == 0)
			if( location != eLocation.NULL && (int)location < (int)eLocation.NUM_PUZZLE_LOCATIONS )
				GuiManager.Instance.TxtGems = "" + _localPlayerData.Gems;
			else
				GuiManager.Instance.TxtGems = "" + ( GameDataManager.Instance.PlayerGems + _localPlayerData.Gems );
		}

		// Add bonus to wild magic rate and population
		if( gems != 0 )
			GameDataManager.Instance.AddWildMagicAndPopulation( WildMagicItemsManager.GetWildMagicItem( "WM_RATE_COLLECT_GEM" ), gems );
	}

	//=====================================================

	public static void AddPlayerRedGems( int redGems )
	{
		_localPlayerData.RedGems = Mathf.Clamp( _localPlayerData.RedGems + redGems, 0, _maxPlayerRedGems );

		// Ping player to display bouncing red gem text
		if( RedGemEvent != null )
			RedGemEvent( _localPlayerData.RedGems );

		// Show popup then unlock puzzle-room key after collecting 8 red gems
		if( _localPlayerData.RedGems >= 8 && _isRedGemKeyActive == false )
		{
			if( PopupKeyUnlocked.Instance != null )
			{
				PopupKeyUnlocked.Instance.Show( ePuzzleKeyType.KEY_GEM_RED );

				_isRedGemKeyActive = true;
			}
		}

		// Add bonus to wild magic rate and population
		GameDataManager.Instance.AddWildMagicAndPopulation( WildMagicItemsManager.GetWildMagicItem( "WM_RATE_COLLECT_GEM" ), redGems );
	}

	//=====================================================

	public static void AwardPlayer( ChestReward reward )
	{
		if( reward == null ) return;

		if( reward.Gems > 0 )
		{
			AddPlayerGems( reward.Gems );
			//Debug.Log( "Gems collected: " + reward.Gems );
		}
		else if( string.IsNullOrEmpty( reward.Card.id ) == false )
		{
			// Add card to players collection
			var heldCard = GameDataManager.Instance.AddTradingCard( reward.Card.id, reward.CardCondition );

			// If card rarity is TEACHER unlock special NPC e.g. Faragonda
			if( reward.Card.rarity == eTradingCardRarity.TEACHER && reward.Card.id != "NULL" )
			{
				var npc = NPCItemsManager.GetNPCItemIDFromCardId( reward.Card.id );

				if( npc != eNPC.NULL)
					GameDataManager.Instance.UnlockPlayerNPC( npc );
			}

			// Show popup
			if( PopupChestReward.instance != null )
				PopupChestReward.instance.Show( heldCard, 1.5f );

			//Debug.Log( "Card collected: " + reward.Card._id );
		}
		else if( reward.SwitchItem != eSwitchItem.NULL )
		{
			_currentSwitchItem = reward.SwitchItem;
			//Debug.Log( "Switch Item received: " + _currentSwitchItem );
		}

		// Play player's celebrate animation
		PlayerManager.OnCelebrate();
	}

	//=====================================================
	// Returns common card from randomly selected card classification
	public static TradingCardSpreadsheetItem GetNPCReward()
	{
		var classification = (eTradingCardClassification)Random.Range( 1, (int)eTradingCardClassification.NUM_ITEMS );

		return TradingCardItemsManager.GetCard( eChestType.SMALL, classification, eTradingCardRarity.COMMON, eTradingCardCondition.MINT );
	}

	//=====================================================

	public static ChestReward GetChestReward( eChestType type,
											  eTradingCardClassification cardClassification,
											  eTradingCardRarity cardRarity,
											  eTradingCardCondition cardCondition )
	{
		var reward = new ChestReward();

		var rewardCard = (type == eChestType.LARGE &&
						  cardClassification != eTradingCardClassification.NULL &&
						  cardRarity != eTradingCardRarity.NULL);

		// 50% chance of awarding a card or gem
		if( rewardCard == false && Random.Range( 0, 99 ) % 2 == 0 )
			rewardCard = true;

		if( rewardCard == true )
		{
			reward.Card = TradingCardItemsManager.GetCard( type, cardClassification, cardRarity, cardCondition );
			reward.CardCondition = cardCondition;
			//Debug.Log( "SceneManager: Reward card" );
		}
		else
		{
			// Small, medium or large number of gems
			var maxGems = Convert.ToInt32( SettingsManager.GetSettingsItem( "AWARD_GEMS_CHEST_" + type, -1 ) );

			reward.Gems = Random.Range( maxGems / 2, maxGems );
			//Debug.Log( "SceneManager: Reward gems" );
		}

		return reward;
	}

	#endregion

	//=====================================================

	#region Private Methods

	private void Awake()
	{
		_maxPlayerGems = Convert.ToInt32( SettingsManager.GetSettingsItem( "PLAYER_MAX_GEMS", -1 ) );
		_maxPlayerRedGems = Convert.ToInt32( SettingsManager.GetSettingsItem( "PLAYER_MAX_RED_GEMS", -1 ) );

		// Reset
		_localPlayerData = new PlayerData();
		_currentSwitchItem = eSwitchItem.NULL;
		_is100GemKeyActive = false;
		_isRedGemKeyActive = false;
	}

	//=====================================================

	void Start()
	{
		// Using local playerData instance for holding gems / redGems
		_localPlayerData.Reset( false );

		// Clear gui in puzzle-rooms
		var location = GameManager.Instance.CurrentLocation;
		if( location != eLocation.MAIN_HALL && location != eLocation.COMMON_ROOM )
		{
			//Debug.Log( "SceneManager->Start : local data only : " + _localPlayerData.Gems );
			GuiManager.Instance.TxtGems = "" + _localPlayerData.Gems;
		}
		else
		{
			//Debug.Log( "SceneManager->Start : global and local data: " + GameDataManager.Instance.PlayerGems + " : " + _localPlayerData.Gems );
			GuiManager.Instance.TxtGems = "" + (GameDataManager.Instance.PlayerGems + _localPlayerData.Gems);
		}

		// Start music for scene
		if( AudioManager.Instance != null )
			AudioManager.Instance.PlayMusic( location );

		// Init player
		GameManager.Instance.InitFairy();

		// Start Everyplay recording
		Everyplay.ReadyForRecording += OnReadyForRecording;
		Everyplay.Initialize();
		// Start fade-in
		if( GameManager.Instance.CurrentLocation == eLocation.BOSS_ROOM )
		{
			// Start intro cutscene
			GameManager.Instance.OnBossStartEvent();
		}
		else
		{
			// Show tutorial screens?
			if( GameManager.Instance.CurrentLocation == eLocation.TUTORIAL ) // ToDo: && NOT VIEWED TUTORIAL
			{
				ScreenManager.FadeInCompleteEvent += OnFadeInCompleteEvent;	
			}

			Invoke( "FadeIn", 0.5f );
		}
	}
		
	public void OnReadyForRecording(bool enabled) {
		if(enabled) {
			Debug.Log("OnReadyForRecording. Everyplay: I am ready for recording");
			EveryplayWrapper.StartRecording();
		}
		else
		{
			Debug.Log("OnReadyForRecording. Everyplay: NOT SUPPORT");
		}
	}
	//=====================================================

	private void FadeIn()
	{
		ScreenManager.FadeIn( 0.6f );
	}

	//=====================================================

	private static void OnFadeInCompleteEvent()
	{
		ScreenManager.FadeInCompleteEvent -= OnFadeInCompleteEvent;

		// Show tutorial screens
		//GuiManager.Instance.OnShowTutorial();
		if( GameManager.Instance.CurrentLocation == eLocation.TUTORIAL )
			PopupTutorial.Instance.Show( eTutorial.INTRO );
	}

	#endregion

	//=====================================================
}
