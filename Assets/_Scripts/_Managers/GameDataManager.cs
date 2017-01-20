using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Text;
using Object = UnityEngine.Object;

public class GameDataManager
{
	public event Action<int, int> PlayerLifeEvent;			// <Max lives, current lives>
	public event Action<int, int> PlayerHealthEvent;		// <Max health, current health>
	public event Action<float, float> PlayerWildMagicEvent;	// <Max wild magic rate, current wild magic rate>
	public event Action<int, int> PlayerPopulationEvent;	// <Max population, current population>
	public event Action<int> PlayerKeysEvent;				// <Current keys>
	public event Action<int> PlayerGemsEvent;				// <Current gems>
	public event Action<int> PlayerDiamondsEvent;			// <Current diamonds>
	public event Action<int> QualityChangeEvent;			// <Current quality level = 0-best/1-fast>

	private static GameDataManager _instance;

	private bool _isInitialized;

	// Player data
	public PlayerData PlayerData { get; private set; }
	private int _maxPlayerFairyLevel;
	private int _maxPlayerLives;
	private int _maxPlayerHealth;
	private int _maxPlayerGems;
	private int _maxPlayerRedGems;
	private int _maxPlayerDiamonds;
	private int _maxPlayerPopulation;

	// Wild magic
	private float _maxWildMagic;
	private float _minWildMagic;
	private float _wildMagicInterval = 5.0f;			// Note: now read from spreadsheet (originally 1.0f then 5.0f seconds)
	private int _wildMagicSaveInterval = 3;				// Num WMR intervals between data saves (seconds == _wildMagicInterval * _wildMagicSaveInterval)

	// Options
	private	bool _musicOn = true;
	public bool MusicOn { get { return _musicOn; } set { _musicOn = value; SaveGameData(); UpdateAudio(); } }

	private	bool _fxOn = true;
	public bool FXOn { get { return _fxOn; } set { _fxOn = value; SaveGameData(); UpdateAudio(); } }

	private	int _qualityLevel = 0;
	public int QualityLevel { get { return _qualityLevel; } set { _qualityLevel = value; SaveGameData(); UpdateQuality(); } }

	// Puzzle-room data
	public List<PuzzleRoomData> PuzzleRoomDataList { get; private set; }

	// Trading card data
	public List<TradingCardHeldItem> TradingCardDataList { get; private set; }

	// Achievement data
	public List<AchievementItem> AchievementDataList { get; private set; }

	//=====================================================

	#region Public Interface

	// Create an instance of the game data manager
	public static GameDataManager Instance
	{
		get
		{
			if( _instance != null ) return _instance;

			_instance = new GameDataManager();
			_instance.Init();
			return _instance;
		}
	}

	//public FairyData PlayerFairyData;

	public int PlayerMaxLives { get { return _maxPlayerLives; } }

	public int PlayerLives { get { return PlayerData.Lives; } }

	public int PlayerMaxHealth { get { return _maxPlayerHealth; } }

	public int PlayerHealth { get { return PlayerData.Health; } }

	public int PlayerGems { get { return PlayerData.Gems; } }

	public int PlayerRedGems { get { return PlayerData.RedGems; } }

	public int PlayerDiamonds { get { return PlayerData.Diamonds; } }

	public int PlayerPopulation { get { return PlayerData.Population; } }

	public int PlayerHighestEverPopulation { get { return PlayerData.HighestEverPopulation; } }

	public int LowestPopulationCap
	{
		get { return PlayerData.LowestPopulationCap; }
		set { PlayerData.LowestPopulationCap = Mathf.Clamp( value, 0, _maxPlayerPopulation ); }
	}

	public float PlayerWildMagicRate { get { return PlayerData.WildMagicRate; } }

	public int PlayerBossLevel
	{
		get { return PlayerData.BossLevel; }
		set { PlayerData.BossLevel = Mathf.Clamp( value, 1, PlayerMaxFairyLevel ); }
	}

	public int[] PlayerFairiesOwned { get { return PlayerData.FairiesOwned(); } }

	public int PlayerMaxFairyLevel { get { return _maxPlayerFairyLevel; } }

	public int PlayerCurrentFairy
	{
		get { return PlayerData.CurrentFairy; }
		set { Mathf.Clamp( PlayerData.CurrentFairy = value, 0, (int)eFairy.NUM_FAIRIES - 1 ); }
	}

	public eFairy PlayerCurrentFairyName { get { return (eFairy)PlayerCurrentFairy; } }

	public int PlayerCurrentFairyLevel
	{
		get
		{
			if( (PlayerData.FairyData != null) && (PlayerData.FairyData.Count > PlayerData.CurrentFairy) )
				return PlayerData.FairyData[PlayerData.CurrentFairy].Level;

			return 1;
		}
	}

	public int PlayerCurrentFairySpellDamage
	{
		get
		{
			var fairyDamage = "" + (eFairy)PlayerCurrentFairy + "_SPELL_DAMAGE";
			return Convert.ToInt32( SettingsManager.GetSettingsItem( fairyDamage, Mathf.Clamp( PlayerCurrentFairyLevel, 1, _maxPlayerFairyLevel ) ) );
		}
	}

	//=====================================================

	public void BroadcastGuiData()
	{
		// Update listeners e.g. gui
		if( PlayerLifeEvent != null )
			PlayerLifeEvent( _maxPlayerLives, PlayerLives );

		if( PlayerHealthEvent != null )
			PlayerHealthEvent( _maxPlayerHealth, PlayerHealth );

		if( PlayerWildMagicEvent != null )
			PlayerWildMagicEvent( _maxWildMagic, PlayerWildMagicRate );

		if( PlayerPopulationEvent != null )
			PlayerPopulationEvent( _maxPlayerPopulation, PlayerPopulation );

		if( PlayerDiamondsEvent != null )
			PlayerDiamondsEvent( PlayerDiamonds );

		var location = GameManager.Instance.CurrentLocation;

		// Puzzle Room enums range from 0+ (eLocation.PUZZLE_ROOM_01 == 0)
		if( location != eLocation.NULL && (int)location < (int)eLocation.NUM_PUZZLE_LOCATIONS )
		{
			if( PlayerKeysEvent != null )
				PlayerKeysEvent( GetNumKeysCollected( location ) );
		}
		else
		{
			if( PlayerGemsEvent != null )
				PlayerGemsEvent( PlayerGems );

			if( PlayerKeysEvent != null )
				PlayerKeysEvent( GetNumKeysCollected() );
		}
	}

	//=====================================================

	public int[] PlayerFairyLevels()
	{
		var levels = new int[PlayerData.FairyData.Count];

		for( var i = 0; i < PlayerData.FairyData.Count; i++ )
		{
			levels[i] = PlayerData.FairyData[i].Level;
		}

		return levels;
	}

	//=====================================================

	public int PlayerFairyMinLevel()
	{
		// Does player have all fairies?
		if( PlayerFairiesOwned.Length < (int)eFairy.NUM_FAIRIES ) return 1;

		var fairyLevel = 1;
		var fairies = PlayerData.FairyData;
		var count = 0;

		// Are any fairies at level 1?
		foreach( var fairy in fairies )
		{
			if( fairy.Level <= 1 )
				++count;
		}

		if( count > 0 ) return fairyLevel;

		// Are any fairies at level 2?
		fairyLevel = 2;
		count = 0;
		foreach( var fairy in fairies )
		{
			if( fairy.Level == 2 )
				++count;
		}

		if( count > 0 ) return fairyLevel;

		// All fairies must be level 3
		fairyLevel = 3;
		return fairyLevel;
	}

	//=====================================================

	public bool IsPlayerNPCUnlocked( eNPC npc )
	{
		//if( npc != eNPC.NPC_STUDENT )
		//	Debug.Log( "NPC: " + npc );

		// ToDo: GAME UPDATE (v1.1) - Add missing special character models
		switch( npc )
		{
			case eNPC.NPC_AVALON:
			case eNPC.NPC_PALLADIUM:
			case eNPC.NPC_WIZGIZ:
				return false;
		}

		return PlayerData.NPCs[(int)npc] >= 1;
	}

	//=====================================================

	public bool IsPlayerNPCVisited( eNPC npc )
	{
		switch( npc )
		{
			default:
				return PlayerData.NPCs[(int)npc] == 2;

			case eNPC.NPC_STUDENT:
				// ToDo: check student counter (of visits today against current population)
				return (PlayerData.NPCs[(int)npc] > NPCManager.Instance.NumStudentsAvailable());
		}
	}

	//=====================================================

	public void UnlockPlayerNPC( eNPC npc, bool saveGameData = false )
	{
		if( npc == eNPC.NPC_STUDENT || npc == eNPC.NULL || npc == eNPC.NUM_NPCS ) return;

		//Debug.Log( "Unlocked NPC: " + npc );

		PlayerData.NPCs[(int)npc] = 1;

		if( saveGameData == true )
			SaveGameData();
	}

	//=====================================================

	public void VisitPlayerNPC( eNPC npc, bool saveGameData = false )
	{
		if( npc == eNPC.NULL || npc == eNPC.NUM_NPCS ) return;

		switch( npc )
		{
			default:
				PlayerData.NPCs[(int)npc] = 2;
				break;

			case eNPC.NPC_STUDENT:
				// ToDo: update student counter (of visits today against current population)
				PlayerData.NPCs[(int)npc]++;
				break;
		}

		if( saveGameData == true )
			SaveGameData();
	}

	//=====================================================
	// Reset data on successful check for a new day e.g. NPC's visited
	public void ResetForNewDay( bool saveGameData = false )
	{
		// If unlocked NPC has previously been visited (== 2), reset to not visited (== 1)
		for( var i = 0; i < PlayerData.NPCs.Length; i++ )
		{
			if( PlayerData.NPCs[(int)i] > 1 )
				PlayerData.NPCs[(int)i] = 1;
		}

		if( saveGameData == true )
			SaveGameData();
	}

	//=====================================================

	public void ResetToPlayerDefaults( bool saveGameData = false )
	{
		Debug.Log( "ResetToPlayerDefaults" );

		// DEBUG - REMOVE THIS - Clear tutorial completed flag
		//PlayerPrefsWrapper.SetInt( "IsTutorialCompleted", 0 );

		// Set 'new game' defaults
		PlayerData.Reset();
		ResetPlayerLives();
		ResetPlayerHealth();
		PlayerData.Population = Convert.ToInt32( SettingsManager.GetSettingsItem( "POPN_NEW_GAME", -1 ) );

		// Reset options
		_musicOn = true;
		_fxOn = true;
		_qualityLevel = 0;

		// Clear gathered keys for all puzzle rooms
		foreach( var data in PuzzleRoomDataList )
			data.ResetKeys();

		// Clear trading cards held by the player
		TradingCardDataList.Clear();

		// Clear achievements held by the player
		AchievementDataList.Clear();

		// Reset NPCs visited
		ResetForNewDay();

		if( saveGameData == true )
			SaveGameData();

		BroadcastGuiData();
	}

	//=====================================================

	public void ResetPlayerLives( bool saveGameData = false )
	{
		PlayerData.Lives = _maxPlayerLives;

		// Update listeners e.g. gui
		if( PlayerLifeEvent != null )
			PlayerLifeEvent( _maxPlayerLives, PlayerLives );

		if( saveGameData == true )
			SaveGameData();
	}

	//=====================================================

	public void ResetPlayerHealth( bool saveGameData = false )
	{
		// ToDo: Note: think we're ignoring this feature now : set health according to current fairy level?
		PlayerData.Health = Convert.ToInt32( SettingsManager.GetSettingsItem( "PLAYER_HEALTH", -1 ) );

		// Update listeners e.g. gui
		if( PlayerHealthEvent != null )
			PlayerHealthEvent( _maxPlayerHealth, PlayerHealth );

		if( saveGameData == true )
			SaveGameData();
	}

	//=====================================================

	public bool IsPlayerHealthFull()
	{
		return (PlayerData.Health == Convert.ToInt32( SettingsManager.GetSettingsItem( "PLAYER_HEALTH", -1 ) ));
	}

	//=====================================================
	// Used for + only life values - also resets health
	public void AddPlayerLife( int lives = 1, bool saveGameData = false )
	{
		// Add lives (probably purchased from popupPlayerDeath or elsewhere)
		PlayerData.Lives += Mathf.Clamp( PlayerData.Lives + lives, 1, _maxPlayerLives );

		// Reset health
		PlayerData.Health = _maxPlayerHealth;

		if( saveGameData == true )
			SaveGameData();

		// Update listeners e.g. gui
		if( PlayerLifeEvent != null )
			PlayerLifeEvent( _maxPlayerLives, PlayerLives );

		// Update listeners e.g. gui
		if( PlayerHealthEvent != null )
			PlayerHealthEvent( _maxPlayerHealth, PlayerHealth );
	}

	//=====================================================
	// Used for + and - health values - updates life count if necessary
	public void AddPlayerHealth( int health, bool saveGameData = false )
	{
		if( health > 0 )
		{
			PlayerData.Health = Mathf.Clamp( PlayerData.Health + health, 0, _maxPlayerHealth );
		}
		else
		{
			if( PlayerData.Health + health <= 0 )
			{
				PlayerData.Health = 0;
				PlayerData.Lives = Mathf.Clamp( PlayerData.Lives - 1, 0, _maxPlayerLives );
			}
			else
			{
				PlayerData.Health += health;
			}
		}

		if( saveGameData == true )
			SaveGameData();

		// Update listeners e.g. gui
		if( PlayerLifeEvent != null )
			PlayerLifeEvent( _maxPlayerLives, PlayerLives );

		// Update listeners e.g. gui
		if( PlayerHealthEvent != null )
			PlayerHealthEvent( _maxPlayerHealth, PlayerHealth );
	}

	//=====================================================

	public void AddPlayerGems( int gems, bool saveGameData = false )
	{
		PlayerData.Gems = Mathf.Clamp( PlayerData.Gems + gems, 0, _maxPlayerGems );

		if( saveGameData == true )
			SaveGameData();

		// Note: In puzzle-rooms we display the SceneManagers gems total rather than the player's running total
	}

	//=====================================================

	public void AddPlayerRedGems( int redGems, bool saveGameData = false )
	{
		PlayerData.RedGems = Mathf.Clamp( PlayerData.RedGems + redGems, 0, _maxPlayerRedGems );

		if( saveGameData == true )
			SaveGameData();
	}

	//=====================================================

	public void AddPlayerDiamonds( int diamonds, bool saveGameData = false )
	{
		PlayerData.Diamonds = Mathf.Clamp( PlayerData.Diamonds + diamonds, 0, _maxPlayerDiamonds );

		if( saveGameData == true )
			SaveGameData();
	}

	//=====================================================

	public void AddWildMagicAndPopulation( WildMagicItemData item, int gemMultiplier = 1, bool saveGameData = false )
	{
		if( item == null ) return;

		AddWildMagicRate( item.Value * gemMultiplier, false, saveGameData );
		AddPlayerPopulation( item.PopulationBonus, saveGameData );
	}

	//=====================================================

	public void AddWildMagicRate( float rate, bool adjustPopulation = false, bool saveGameData = false )
	{
		PlayerData.WildMagicRate = Mathf.Clamp( PlayerData.WildMagicRate + rate, _minWildMagic, _maxWildMagic );
		//Debug.Log( "WMR penalty: " + rate );
		if( adjustPopulation == true )
			AddPlayerPopulation( (int)PlayerData.WildMagicRate, false );

		// Update listeners e.g. gui
		if( PlayerWildMagicEvent != null )
			PlayerWildMagicEvent( _maxWildMagic, PlayerWildMagicRate );

		if( saveGameData == true )
			SaveGameData();
	}

	//=====================================================

	public void AddPlayerPopulation( int pupils, bool saveGameData = false )
	{
		PlayerData.Population = Mathf.Clamp( PlayerData.Population + pupils, PlayerData.LowestPopulationCap, _maxPlayerPopulation );

		if( saveGameData == true )
			SaveGameData();

		// Update listeners e.g. gui
		if( PlayerPopulationEvent != null )
			PlayerPopulationEvent( _maxPlayerPopulation, PlayerPopulation );
	}

	//=====================================================

	public void DebugAwardPlayerAllKeys( eLocation location, bool saveGameData = false )
	{
		// Store all keys as collected for the current puzzle-room
		var numKeys = Convert.ToInt32( SettingsManager.GetSettingsItem( "NUM_PUZZLE_ROOM_KEYS", (int)location ) );
		for( var i = 0; i < numKeys; i++ )
		{
			PuzzleRoomDataList[(int)location].Keys[i] = 1;
		}

		// Add bonus to wild magic rate and population
		AddWildMagicAndPopulation( WildMagicItemsManager.GetWildMagicItem( "WM_RATE_COLLECT_KEY" ) );

		if( saveGameData == true )
			SaveGameData();

		// Update listeners e.g. gui
		if( PlayerKeysEvent != null )
			PlayerKeysEvent( GetNumKeysCollected( GameManager.Instance.CurrentLocation ) );
	}

	//=====================================================

	public bool AddPlayerKey( eLocation location, ePuzzleKeyType key, bool saveGameData = false )
	{
		var success = false;

		// Ensure we're being passed a puzzle-room key
		if( (int)location >= 0 && (int)location < (int)eLocation.NUM_PUZZLE_LOCATIONS )
		{
			// Check if key is already owned
			if( PuzzleRoomDataList[(int)location].Keys[(int)key] != 0 )
			{
				// Award player gems if key has already been collected - gems are stored by SceneManager until leaving scene
				SceneManager.AddPlayerGems( Convert.ToInt32( SettingsManager.GetSettingsItem( "AWARD_GEMS_FOR_KEY", -1 ) ) );
			}
			else
			{
				// Store latest collected key for the current puzzle-room
				PuzzleRoomDataList[(int)location].Keys[(int)key] = 1;

				// Update listeners e.g. gui
				// Update gui display for current scene
				if( PlayerKeysEvent != null )
					PlayerKeysEvent( GetNumKeysCollected( GameManager.Instance.CurrentLocation ) );

				// Add bonus to wild magic rate and population
				AddWildMagicAndPopulation( WildMagicItemsManager.GetWildMagicItem( "WM_RATE_COLLECT_KEY" ) );

				success = true;
			}

			if( saveGameData == true )
				SaveGameData();
		}
		else
		{
			Debug.LogWarning( "Attempting to store key from location other than puzzle-room." );
		}

		return success;
	}

	//=====================================================

	public bool PlayerKeyIsOwned( eLocation location, ePuzzleKeyType key )
	{
		if( (int)location >= 0 && (int)location < (int)eLocation.NUM_PUZZLE_LOCATIONS )
			return (PuzzleRoomDataList[(int)location].Keys[(int)key] == 1);

		return false;
	}

	//=====================================================

	public int GetNumKeysCollected( eLocation location )
	{
		var count = 0;

		if( location == eLocation.MAIN_HALL || location == eLocation.TUTORIAL )
		{
			count = GetNumKeysCollected();
		}
		// Ensure we're being passed a puzzle-room location
		else if( (int)location >= 0 && (int)location < (int)eLocation.NUM_PUZZLE_LOCATIONS )
		{
			var roomData = PuzzleRoomDataList[(int)location];
			var maxKeys = Convert.ToInt32( SettingsManager.GetSettingsItem( "NUM_PUZZLE_ROOM_KEYS", (int)location ) );

			for( var i = 0; i < maxKeys; i++ )
			{
				if( roomData.Keys[i] != 0 )
					++count;
			}
		}
		else
		{
			Debug.LogWarning( "Attempting to get key count from location other than puzzle-room." );
		}

		return count;
	}

	//=====================================================

	public int GetNumKeysCollected()
	{
		var count = 0;

		// Get total number of keys collected
		for( var location = 0; location < (int)eLocation.NUM_PUZZLE_LOCATIONS; location++ )
		{
			if(PuzzleRoomDataList.Count > location)
			{			
				var roomData = PuzzleRoomDataList[location];
				if( roomData == null ) continue;
				string nums = SettingsManager.GetSettingsItem("NUM_PUZZLE_ROOM_KEYS", location);
				var maxKeys = 0;
				try
				{
					maxKeys = Convert.ToInt32(nums);
				}
				catch(FormatException ex)
				{
					Debug.Log("Given string is : " + nums + " for location: " + location);
				}
				finally
				{
					for( var i = 0; i < maxKeys; i++ )
					{
						if( roomData.Keys[i] != 0 )
							++count;
					}
				}
			}

		}

		return count;
	}

	//=====================================================

	public TradingCardHeldItem GetHeldTradingCard( string id, ref int NumMint, ref int NumScuffed )
	{
		NumMint = 0;
		NumScuffed = 0;
		TradingCardHeldItem ReturnCard = null;

		foreach( var HeldCard in TradingCardDataList )
		{
			if( HeldCard.id == id )
			{
				if( ReturnCard != null )
				{
					if( HeldCard.notifyTimer > ReturnCard.notifyTimer )
						ReturnCard = HeldCard;
				}
				else
				{
					ReturnCard = HeldCard;
				}

				if( HeldCard.condition == eTradingCardCondition.MINT )
					NumMint++;
				if( HeldCard.condition == eTradingCardCondition.SCUFFED )
					NumScuffed++;
			}
		}

		return (ReturnCard);
	}

	//=====================================================

	public int GetNumHeldCards( eTradingCardClassification Classification, eTradingCardCondition Condition )
	{
		var NumCards = 0;

		// Only reports one of each card type
		var NumSpreadsheetCards = TradingCardItemsManager.GetNumItems();
		for( var Idx = 0; Idx < NumSpreadsheetCards; Idx++ )
		{
			var CurCard = TradingCardItemsManager.GetTradingCardItem( Idx );

			if( CurCard.classification != Classification )
				continue;

			var NumMint = 0;
			var NumScuffed = 0;
			GetHeldTradingCard( CurCard.id, ref NumMint, ref NumScuffed );

			if( Condition == eTradingCardCondition.MINT )
			{
				if( NumMint > 0 )
					NumCards++;
			}
			else if( Condition == eTradingCardCondition.SCUFFED )
			{
				if( NumScuffed > 0 )
					NumCards++;
			}
		}

		return (NumCards);
	}

	//=====================================================

	public TradingCardHeldItem GetHeldTradingCard( string id )
	{
		TradingCardHeldItem ReturnCard = null;

		foreach( var HeldCard in TradingCardDataList )
		{
			if( HeldCard.id == id )
			{
				if( ReturnCard != null )
				{
					if( HeldCard.notifyTimer > ReturnCard.notifyTimer )
						ReturnCard = HeldCard;
				}
				else
				{
					ReturnCard = HeldCard;
				}
			}
		}

		return (ReturnCard);
	}

	//=====================================================

	public TradingCardHeldItem AddTradingCard( string id, eTradingCardCondition condition, bool saveGameData = true )
	{
		TradingCardDataList.Add( new TradingCardHeldItem( id, condition ) );
		var heldCard = TradingCardDataList[TradingCardDataList.Count - 1];

		if( saveGameData == true )
			SaveGameData();

		return (heldCard);
	}

	//=====================================================

	public void RemoveTradingCard( string id, eTradingCardCondition condition, bool saveGameData = true )
	{
		foreach( var HeldCard in TradingCardDataList )
		{
			if( (HeldCard.id == id) && (HeldCard.condition == condition) )
			{
				TradingCardDataList.Remove( HeldCard );

				if( saveGameData == true )
					SaveGameData();
				return;
			}
		}

		Debug.Log( "Trying to remove card that we don't have!" );
	}

	//=====================================================

	public void UpdateAudio()
	{
		if( AudioManager.Instance != null )
		{
			AudioManager.Instance.SetMusicState( _musicOn );
			AudioManager.Instance.SetSFXState( _fxOn );
		}
	}

	//=====================================================

	public void UpdateQuality()
	{
		var names = QualitySettings.names;

		for( var i = 0; i < names.Length; i++ )
		{
			if( _qualityLevel == 0 )
			{
				if( names[i] == "Best" )
					QualitySettings.SetQualityLevel( i, false ); //true );
			}
			else
			{
				if( names[i] == "Fast" )
					QualitySettings.SetQualityLevel( i, false ); //true );
			}
		}

		if( QualityChangeEvent != null )
			QualityChangeEvent( _qualityLevel );
	}

	//=====================================================

	public void SetCameraGPUFlags()
	{
		if( Camera.main == null ) return;

		if( QualityLevel == 0 )
			Camera.main.cullingMask |= (1 << LayerMask.NameToLayer( "RenderOnFastGPU" ));
		else
			Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer( "RenderOnFastGPU" ));
	}

	//=====================================================

	public void MarkAchievementUploaded( int id )
	{
		// Do we already have the achievement?
		foreach( var curItem in AchievementDataList )
		{
			if( curItem.id != id ) continue;

			curItem.bHasUploadedToGCGP = true;
			SaveGameData();
			return;
		}
	}

	//=====================================================

	public int GetAchievementToUpload()
	{
		if( AchievementDataList == null ) return (-1);

		foreach( var curItem in AchievementDataList )
		{
			if( curItem.bHasUploadedToGCGP == false )
				return (curItem.id);
		}

		return (-1);
	}

	//=====================================================

	public void SetAchievement( int id )
	{
		// Do we already have the achievement?
		foreach( var curItem in AchievementDataList )
		{
			if( curItem.id == id )
				return;
		}

		// Add new achievement and save data
		var newItem = new AchievementItem { id = id, bHasUploadedToGCGP = false };
		AchievementDataList.Add( newItem );

		SaveGameData();
	}

	//=====================================================

	public void UpdateAchievements()
	{
		// Check each achievement status
		if( PlayerGems >= 100 )
			SetAchievement( 0 );

		if( PlayerRedGems >= 100 )
			SetAchievement( 1 );

		if( PlayerData.HighestEverPopulation >= 100000 )
			SetAchievement( 9 );

		if( PlayerData.HighestEverPopulation >= 500000 )
			SetAchievement( 10 );

		if( PlayerData.HighestEverPopulation >= 1000000 )
			SetAchievement( 11 );

		if( GetNumHeldCards( eTradingCardClassification.NULL, eTradingCardCondition.SCUFFED ) >= TradingCardItemsManager.GetNumCards( eTradingCardClassification.NULL ) )
			SetAchievement( 12 );

		if( GetNumHeldCards( eTradingCardClassification.WILD, eTradingCardCondition.SCUFFED ) >= TradingCardItemsManager.GetNumCards( eTradingCardClassification.WILD ) )
			SetAchievement( 13 );

		if( GetNumHeldCards( eTradingCardClassification.WINX, eTradingCardCondition.SCUFFED ) >= TradingCardItemsManager.GetNumCards( eTradingCardClassification.WINX ) )
			SetAchievement( 14 );

		if( GetNumHeldCards( eTradingCardClassification.STANDARD, eTradingCardCondition.SCUFFED ) >= TradingCardItemsManager.GetNumCards( eTradingCardClassification.STANDARD ) )
			SetAchievement( 15 );

		if( GetNumHeldCards( eTradingCardClassification.WILD, eTradingCardCondition.MINT ) >= TradingCardItemsManager.GetNumCards( eTradingCardClassification.WILD ) )
			SetAchievement( 16 );

		if( GetNumHeldCards( eTradingCardClassification.WINX, eTradingCardCondition.MINT ) >= TradingCardItemsManager.GetNumCards( eTradingCardClassification.WINX ) )
			SetAchievement( 17 );

		if( GetNumHeldCards( eTradingCardClassification.STANDARD, eTradingCardCondition.MINT ) >= TradingCardItemsManager.GetNumCards( eTradingCardClassification.STANDARD ) )
			SetAchievement( 18 );

		if( GetNumHeldCards( eTradingCardClassification.NULL, eTradingCardCondition.MINT ) >= TradingCardItemsManager.GetNumCards( eTradingCardClassification.NULL ) )
			SetAchievement( 19 );

		if( PlayerGems >= 1000 )
			SetAchievement( 26 );

		if( PlayerGems >= 10000 )
			SetAchievement( 27 );

		if( PlayerGems >= 100000 )
			SetAchievement( 28 );

		if( PlayerGems >= 1000000 )
			SetAchievement( 29 );
	}

	//=====================================================

	public FairyData GetFairyData( eFairy fairy )
	{
		foreach( var CurFairy in PlayerData.FairyData )
		{
			if( CurFairy.Fairy == (int)fairy )
				return (CurFairy);
		}

		return (null);
	}

	//=====================================================

	public void BuyFairy( eFairy fairy, bool saveGameData = false )
	{
		if( PlayerData.FairyData[(int)fairy].Fairy != (int)fairy )
		{
			// Set defaults
			PlayerData.FairyData[(int)fairy].Fairy = (int)fairy;
			PlayerData.FairyData[(int)fairy].Outfit = string.Empty;
			PlayerData.FairyData[(int)fairy].Level = 1;

			// Automatically buy default outfit for this fairy
			var defaultItem = ClothingItemsManager.GetClothingDefaultItem( fairy );

			if( defaultItem != null )
			{
				PlayerData.FairyData[(int)fairy].Outfit = defaultItem;
				PlayerData.FairyData[(int)fairy].BuyOutfit( defaultItem );
			}
			else
			{
				Debug.LogError( "Fairy: " + fairy + " has no default outfit!" );
			}

			if( saveGameData == true )
				SaveGameData();
		}
		else
		{
			Debug.LogError( "Trying to buy fairy that's already been bought - this will reset fairy data" );
		}
	}

	//=====================================================

	public Object ChangeFairy( eFairy fairy, bool saveGameData = false )
	{
		var pfb = GetPrefab( fairy, false );

		if( pfb == null ) return null;

		// Update PlayerData
		PlayerData.CurrentFairy = (int)fairy;

		if( saveGameData == true )
			SaveGameData();

		return pfb;
	}

	//=====================================================

	public void SaveGameData()
	{
		SavePlayer();
	}

	//=====================================================

	public IEnumerator UpdateWildMagic( Action onComplete )
	{
		// Determine time since last recorded time and apply the default reduction to the wild magic rate
		// Store start time for updating wild magic
		if( PlayerPrefsWrapper.HasKey( "WildMagicLastUpdate" ) == false )
			PlayerPrefsWrapper.SetDouble( "WildMagicLastUpdate", PreHelpers.UnixUtcNow() );

		var wildMagicRateAtStart = PlayerWildMagicRate;
		var defaultWildMagicPenalty = Convert.ToSingle( WildMagicItemsManager.GetWildMagicItemValue( "WM_RATE_DEFAULT" ) );
		var count = 0;

		// On apply penalty over time on first starting the app
		var applyPopPenaltyOverTime = true;

		// Short delay at start of game
		yield return new WaitForSeconds( 0.5f );

		while( true )
		{
			// Get time since last update in seconds
			var timeNow = PreHelpers.UnixUtcNow();
			var timePassed = timeNow - PlayerPrefsWrapper.GetDouble( "WildMagicLastUpdate" );

			// Determine penalty to be applied to wild magic rate
			var penalty = (float)((timePassed / _wildMagicInterval) * defaultWildMagicPenalty);

			// Tend wild magic rate towards full wild magic
			if( count++ < _wildMagicSaveInterval )
			{
				// Account for positive period incrementing population before applying penalty
				// New game session? Apply penalty on population for duration since last session
				if( applyPopPenaltyOverTime == true )
				{
					//Debug.Log( "pop: " + PlayerPopulation + " : wildMagicRateAtStart: " + wildMagicRateAtStart + " : penalty: " + penalty + " : timePassed: " + timePassed.ToString( "#.00" ) );

					var adjustedDuration = 0;
					var adjustedWMR = wildMagicRateAtStart;

					AddWildMagicRate( penalty );

					// ToDo: Penalties applied every 'x' seconds (_wildMagicInterval) - currently every second
					while( adjustedWMR > PlayerWildMagicRate )
					{
						// Using Abs for penalty to ensure we are decrementing adjustedWMR (incase the penalty has been changed to a positive value)
						adjustedWMR -= Mathf.Abs( defaultWildMagicPenalty );
						PlayerData.Population = Mathf.Clamp( PlayerData.Population + (int)adjustedWMR, PlayerData.LowestPopulationCap, _maxPlayerPopulation );
						adjustedDuration += (int)_wildMagicInterval;

						// Block further penalties exceeding time passed since last log-in
						if( adjustedDuration > timePassed )
						{
							//Debug.Log( "BREAK: adjusted duration for penalties exceeds time passed" );

							adjustedWMR = PlayerWildMagicRate;
						}
					}

					// Adjust for remaining time at fully negative wild magic rate
					if( adjustedDuration < timePassed )
					{
						PlayerData.Population =
							Mathf.Clamp(
								PlayerData.Population + ((int)((timePassed - adjustedDuration) / _wildMagicInterval) * (int)PlayerData.WildMagicRate),
								PlayerData.LowestPopulationCap, _maxPlayerPopulation );
					}

					//Debug.Log( "New Pop: " + PlayerPopulation + " : New WMR: " + PlayerWildMagicRate +
					//		   " : adjustedDuration: " + adjustedDuration + " : timePassed: " + timePassed.ToString( "#.00" ) );

					// Clear flag
					applyPopPenaltyOverTime = false;

					// Save changes to wild magic rate and population
					SaveGameData();
				}
				else
				{
					AddWildMagicRate( penalty, true );
				}
			}
			else
			{
				// Update GameDataManager with current SceneManager data (player's gems) and clear it's local data
				if( GameManager.Instance != null && GameManager.Instance.CurrentLocation == eLocation.MAIN_HALL )
					SceneManager.UpdateGameDataManager();

				// Save player data
				AddWildMagicRate( penalty, true, true );
				count = 0;
			}

			// Store current time
			PlayerPrefsWrapper.SetDouble( "WildMagicLastUpdate", timeNow );

			yield return new WaitForSeconds( _wildMagicInterval );

			// ToDo: use onComplete() ? - GameManager currently instantiates this Job and kills it during OnApplicationQuit()
		}
	}

	#endregion

	//=====================================================

	#region Private Methods

	// Constructor
	private GameDataManager() { }

	//=====================================================
	// Called after constructor
	private void Init()
	{
		if( _isInitialized == true ) return;

		// Set max player data values from spreadsheet
		_maxPlayerFairyLevel = Convert.ToInt32( SettingsManager.GetSettingsItem( "PLAYER_MAX_FAIRY_LEVEL", -1 ) );
		_maxPlayerLives = Convert.ToInt32( SettingsManager.GetSettingsItem( "PLAYER_MAX_LIVES", -1 ) );
		_maxPlayerHealth = Convert.ToInt32( SettingsManager.GetSettingsItem( "PLAYER_MAX_HEALTH", -1 ) );
		_maxPlayerGems = Convert.ToInt32( SettingsManager.GetSettingsItem( "PLAYER_MAX_GEMS", -1 ) );
		_maxPlayerRedGems = Convert.ToInt32( SettingsManager.GetSettingsItem( "PLAYER_MAX_RED_GEMS", -1 ) );
		_maxPlayerDiamonds = Convert.ToInt32( SettingsManager.GetSettingsItem( "PLAYER_MAX_DIAMONDS", -1 ) );
		_maxPlayerPopulation = Convert.ToInt32( SettingsManager.GetSettingsItem( "PLAYER_MAX_POPN", -1 ) );

		// Wild magic
		_maxWildMagic = (int)WildMagicItemsManager.GetWildMagicItemValue( "WILD_MAGIC_RATE_MAX" );
		_minWildMagic = (int)WildMagicItemsManager.GetWildMagicItemValue( "WILD_MAGIC_RATE_MIN" );
		_wildMagicInterval = (int)WildMagicItemsManager.GetWildMagicItemValue( "WILD_MAGIC_RATE_INTERVAL" );
		_wildMagicSaveInterval = (int)WildMagicItemsManager.GetWildMagicItemValue( "WILD_MAGIC_RATE_SAVE_INTERVAL" );

		// Load player data if it exists
		LoadPlayer();

		//GameObject ObjectStore = GameObject.Find( "ObjectTypesStore" );
		//m_ObjectStoreCmp = null;
		//if( ObjectStore )
		//{
		//	m_ObjectStoreCmp = ObjectStore.GetComponent( "LevelObjectTypes" ) as LevelObjectTypes;
		//}

		//if( m_ObjectStoreCmp == null ) Debug.Log( "Object store not found!" );

		//LoadPuzzleRoomData();

		_isInitialized = true;
	}

	//=====================================================

	public GameObject GetPrefab( eFairy fairy, bool isMenuPrefab )//= false )
	{
		GameObject prefab = null;
		var path = new StringBuilder();
		path.Append( "Fairies/" );

		switch( fairy )
		{
			case eFairy.BLOOM:
				path.Append( "Bloom/Prefabs/" );
				break;
			case eFairy.STELLA:
				path.Append( "Stella/Prefabs/" );
				break;
			case eFairy.FLORA:
				path.Append( "Flora/Prefabs/" );
				break;
			case eFairy.MUSA:
				path.Append( "Musa/Prefabs/" );
				break;
			case eFairy.TECNA:
				path.Append( "Tecna/Prefabs/" );
				break;
			case eFairy.AISHA:
				path.Append( "Aisha/Prefabs/" );
				break;
		}

		// Boss Room - use Bloom Butterflix prefab
		// path = "Fairies/Bloom/Prefabs/pfbBloomButterflix";
		if( GameManager.Instance != null && GameManager.Instance.CurrentLocation == eLocation.BOSS_ROOM )
		{
			switch( fairy )
			{
				case eFairy.BLOOM:
					path.Append( "pfbBloomButterflix" );
					break;
				case eFairy.STELLA:
					path.Append( "pfbStellaButterflix" );
					break;
				case eFairy.FLORA:
					path.Append( "pfbFloraButterflix" );
					break;
				case eFairy.MUSA:
					path.Append( "pfbMusaButterflix" );
					break;
				case eFairy.TECNA:
					path.Append( "pfbTecnaButterflix" );
					break;
				case eFairy.AISHA:
					path.Append( "pfbAishaButterflix" );
					break;
			}

			// Load prefab
			prefab = Resources.Load( path.ToString() ) as GameObject;

			if( prefab != null ) return prefab;

			Debug.Log( "Butterflix prefab not found in resources" );
			return null;
		}

		// Fairy owned?
		var curFairy = GetFairyData( fairy );
		if( curFairy == null )
		{
			// Not owned, show default outfit
			var defaultItem = ClothingItemsManager.GetClothingDefaultItem( fairy );

			if( !String.IsNullOrEmpty( defaultItem ) )
			{
				if( isMenuPrefab )
					path.Append( ClothingItemsManager.GetClothingItem( defaultItem ).prefabName );
				else
					path.Append( ClothingItemsManager.GetClothingItem( defaultItem ).gamePrefabName );
			}
		}
		else
		{
			// Owned, show current outfit
			var currentOutfit = curFairy.Outfit;

			if(currentOutfit == "OUTFIT_0000")
			{
				Debug.Log("Kostyl Method works now");
				currentOutfit = "BloomOutfit00-S6";
				curFairy.Outfit = currentOutfit;
			}
			Debug.Log("Current Outfit: " + currentOutfit);
			Debug.Log("Path NOW: " + path);

			if(!String.IsNullOrEmpty(currentOutfit))
			{
				if(isMenuPrefab)
				{
					ClothingItemData itemData = ClothingItemsManager.GetClothingItem(currentOutfit);
					if(itemData != null)
					{
						path.Append(itemData.prefabName );
					}
					else
					{
						var defaultItem = ClothingItemsManager.GetClothingDefaultItem( fairy );

						if(!String.IsNullOrEmpty(defaultItem))
						{
							path.Append( ClothingItemsManager.GetClothingItem( defaultItem ).prefabName );
						}
					}
				}
				else
				{
					ClothingItemData itemData = ClothingItemsManager.GetClothingItem(currentOutfit);
					if(itemData != null)
					{
						path.Append(itemData.gamePrefabName);
					}
					else
					{
						// Not owned, show default outfit
						var defaultItem = ClothingItemsManager.GetClothingDefaultItem( fairy );
						path.Append( ClothingItemsManager.GetClothingItem( defaultItem ).gamePrefabName );
					}
				}
			}

		}

		// Load prefab
		prefab = Resources.Load( path.ToString() ) as GameObject;

		if( prefab != null ) return prefab;

		if(fairy == eFairy.BLOOM)
		{
			string bloomDefaultOutfit = "Fairies/Bloom/Prefabs/pfbBloomOutfit00-S6";

			prefab = Resources.Load(bloomDefaultOutfit) as GameObject;

			return prefab;
		}
		Debug.Log( "Fairy prefab not found in resources" );
		return null;
	}

	#endregion

	//=====================================================

	#region Save / Load Player Data

	//=============================================================================

	private void SavePlayer()
	{
		var docPath = PreHelpers.GetFileFolderPath();

		var filePath = docPath + "Player" + ".txt";
		Debug.Log( "Saving player: " + filePath );

		SaveJSON( filePath );
	}

	//=============================================================================

	private void SaveJSON( string filename )
	{
#if UNITY_WEBPLAYER
		
#else
		// Open/create file
		var curFile = File.CreateText( filename );

		// Write avatar info
		WriteJSON( curFile );

		// Close file
		curFile.Close();
#endif
	}

	//=====================================================

	private void WriteJSON( StreamWriter curFile )
	{
		if( curFile == null ) return;

		// Write JSON
		var encoder = GetJSON();

		curFile.Write( encoder.serialized );
	}

	//=====================================================

	public JSON GetJSON()
	{
		// Create JSON object
		var encoder = new JSON();

		if( null == PlayerData )
			PlayerData = new PlayerData();

		// Write player data
		encoder["player"] = (JSON)PlayerData;

		// Write options
		encoder["musicOn"] = _musicOn;
		encoder["fxOn"] = _fxOn;
		encoder["qualityLevel"] = _qualityLevel;

		if( null == PuzzleRoomDataList )
		{
			PuzzleRoomDataList = new List<PuzzleRoomData>( (int)eLocation.NUM_PUZZLE_LOCATIONS );

			for( var i = 0; i < (int)eLocation.NUM_PUZZLE_LOCATIONS; i++ )
			{
				PuzzleRoomDataList.Add( new PuzzleRoomData( (eLocation)i,
															Convert.ToInt32( SettingsManager.GetSettingsItem(
																			"NUM_PUZZLE_ROOM_KEYS", i ) ) ) );
			}
		}

		if( null == TradingCardDataList )
		{
			TradingCardDataList = new List<TradingCardHeldItem>();
		}

		if( null == AchievementDataList )
		{
			AchievementDataList = new List<AchievementItem>();
		}

		// Write puzzle-room data
		for( var i = 0; i < (int)eLocation.NUM_PUZZLE_LOCATIONS; i++ )
		{
			if(PuzzleRoomDataList.Count > i)
			{
				encoder["puzzleroomdata" + i.ToString( "00" )] = (JSON)PuzzleRoomDataList[i];
			}
		}

		// Write trading card data
		{
			var tradingCardDataListJson = new List<JSON>();

			foreach( var item in TradingCardDataList )
			{
				var objJson = (JSON)(item);
				tradingCardDataListJson.Add( objJson );
			}

			encoder["tradingCardDataList"] = tradingCardDataListJson;
		}

		// Write achievement card data
		{
			var achievementDataListJson = new List<JSON>();

			foreach( var item in AchievementDataList )
			{
				var objJson = (JSON)(item);
				achievementDataListJson.Add( objJson );
			}

			encoder["achievementDataList"] = achievementDataListJson;
		}

		return (encoder);
	}

	//=============================================================================

	public void LoadPlayer()	//, bool bIsLocalAvatar )
	{
		var docPath = PreHelpers.GetFileFolderPath();
		const string fileName = "Player";

		//if( Application.isEditor || (Application.platform == RuntimePlatform.WindowsPlayer) )

		// In the editor, load as a standard file to prevent caching by Unity
		var filePath = docPath + fileName + ".txt";
		//Debug.Log("Loading uncached player data: "  + filePath);

		// *** Player Data Exists ***
		if( File.Exists( filePath ) ) { LoadJSON( filePath ); return; }

		// *** Player Data Not Found - create new player / game data ***
		Debug.Log( "Player data not found! Creating default player data." );	//" Retrying for default avatar in Resources." );

		// Set player 'new game' defaults
		PlayerData = new PlayerData
		{
			Lives = _maxPlayerLives,
			Health = Convert.ToInt32( SettingsManager.GetSettingsItem( "PLAYER_HEALTH", -1 ) ),
			Population = Convert.ToInt32( SettingsManager.GetSettingsItem( "POPN_NEW_GAME", -1 ) ),
			LowestPopulationCap = 0
		};

		// Set default fairy available
		BuyFairy( eFairy.BLOOM );

		// Set puzzle-room 'new game' defaults
		PuzzleRoomDataList = new List<PuzzleRoomData>( (int)eLocation.NUM_PUZZLE_LOCATIONS );

		for( var i = 0; i < (int)eLocation.NUM_PUZZLE_LOCATIONS; i++ )
		{
			var numKeys = SettingsManager.GetSettingsItem( "NUM_PUZZLE_ROOM_KEYS", i );
			// ToDo: 11-Dec-15: I've added this 'continue' to bypass receiving null values from SettingsManager
			//if( string.IsNullOrEmpty( numKeys ) ) continue;

			var roomData = new PuzzleRoomData( (eLocation)i, Convert.ToInt32( numKeys ) );

			PuzzleRoomDataList.Add( roomData );
		}

		// Reset trading cards
		TradingCardDataList = new List<TradingCardHeldItem>();

		// Reset achievements
		AchievementDataList = new List<AchievementItem>();

		SaveGameData();

		//DocPath = GetFileFolderPath( false );
		//FilePath = DocPath + "DefaultAvatar" + ".txt";

		//if( System.IO.File.Exists( FilePath ) )
		//	LoadJSON( FilePath, destFairyAvatar, false );

		//return false;

		//}
		//else
		//{
		//string FilePath = DocPath + fileName;

		//if( bIsLocalAvatar )
		//{
		//	FilePath += ".txt";
		//	Debug.Log( "Loading avatar locally: " + FilePath );
		//	if( System.IO.File.Exists( FilePath ) )
		//	{
		//		Debug.Log( "Exists..." );
		//		LoadJSON( FilePath, destFairyAvatar, bIsLocalAvatar );
		//	}
		//	else
		//	{
		//		DocPath = GetFileFolderPath( false );
		//		FilePath = DocPath + "DefaultAvatar"; // + ".txt";
		//		Debug.Log( "Doesn't exist - Loading avatar from resources: " + FilePath );
		//		LoadJSON( FilePath, destFairyAvatar, false );
		//	}
		//}
		//else

		//Debug.Log( "Loading avatar from resources: " + FilePath );
		//LoadJSON( FilePath );

		//}

		//return true;
	}

	//=============================================================================

	private void LoadJSON( string filename )	//, bool bIsLocalLevel )
	{
		// Open/create file
		//if( Application.isEditor || (Application.platform == RuntimePlatform.WindowsPlayer) ) //|| bIsLocalLevel )
		{
			// Read as a normal file
			var curFile = new StreamReader( filename );
			var contents = curFile.ReadToEnd();

			ParseJSON( contents );

			curFile.Close();
		}
		//else
		//{
		//	// Read as a unity resource
		//	TextAsset textAsset = (TextAsset)Resources.Load( Filename, typeof( TextAsset ) );

		//	if( textAsset != null )
		//	{
		//		Stream fs = new MemoryStream( textAsset.bytes );
		//		StreamReader CurFile = new StreamReader( fs );

		//		string Contents = CurFile.ReadToEnd();
		//		ParseJSON( Contents );

		//		CurFile.Close();
		//		fs.Close();
		//	}
		//	else
		//	{
		//		Debug.Log( "Failed to open text asset JSON for reading avatar (" + Filename + ")" );
		//	}
		//}
	}

	//=============================================================================

	private void ParseJSON( string inputText )
	{
		// Parse JSON from current web request
		ReadJSON( inputText );
	}

	//=====================================================

	private void ReadJSON( string inputText )
	{
		// Read JSON data
		var decoder = new JSON { serialized = inputText };

		// Player data
		var playerJSON = decoder.ToJSON( "player" );

		if( null == PlayerData )
			PlayerData = new PlayerData();

		PlayerData = (PlayerData)playerJSON;

		// Read options
		if( decoder.ContainsJSON( "musicOn" ) )
			_musicOn = decoder.ToBoolean( "musicOn" );
		else
			_musicOn = true;

		if( decoder.ContainsJSON( "fxOn" ) )
			_fxOn = decoder.ToBoolean( "fxOn" );
		else
			_fxOn = true;

		UpdateAudio();

		if( decoder.ContainsJSON( "qualityLevel" ) )
			_qualityLevel = decoder.ToInt( "qualityLevel" );
		else
			_qualityLevel = 0;

		UpdateQuality();

		// Puzzle-room data
		if( null == PuzzleRoomDataList )
			PuzzleRoomDataList = new List<PuzzleRoomData>( (int)eLocation.NUM_PUZZLE_LOCATIONS );

		PuzzleRoomDataList.Clear();
		for( var i = 0; i < (int)eLocation.NUM_PUZZLE_LOCATIONS; i++ )
		{
			if( decoder.ContainsJSON( "puzzleroomdata" + i.ToString( "00" ) ) )
			{
				var roomJSON = decoder.ToJSON( "puzzleroomdata" + i.ToString( "00" ) );

				PuzzleRoomDataList.Add( (PuzzleRoomData)roomJSON );

				// Check that the number of keys in the spreadsheet matches
				string puzzleRoomKeys =  SettingsManager.GetSettingsItem("NUM_PUZZLE_ROOM_KEYS", i);
				if(puzzleRoomKeys == "")
				{
					puzzleRoomKeys = "0";
				}
				if( PuzzleRoomDataList[i].Keys.Length != Convert.ToInt32(puzzleRoomKeys))
				{
					PuzzleRoomDataList[i].ResetNumKey(Convert.ToInt32(puzzleRoomKeys));
				}
			}
			else
			{
				string puzzleRoomKeys = SettingsManager.GetSettingsItem("NUM_PUZZLE_ROOM_KEYS", i);
				if(puzzleRoomKeys == "")
				{
					puzzleRoomKeys = "0";
				}
				var prData = new PuzzleRoomData( (eLocation)i,
						Convert.ToInt32(puzzleRoomKeys) );
				PuzzleRoomDataList.Add( prData );
			}
		}

		// Trading card data
		if( null == TradingCardDataList )
			TradingCardDataList = new List<TradingCardHeldItem>();

		TradingCardDataList.Clear();
		if( decoder.ContainsJSON( "tradingCardDataList" ) )
		{
			var array = decoder.ToArray<JSON>( "tradingCardDataList" );

			for( var idx = 0; idx < array.Length; idx++ )
			{
				TradingCardDataList.Add( (TradingCardHeldItem)array[idx] );
			}
		}

		// Achievement Data
		if( null == AchievementDataList )
			AchievementDataList = new List<AchievementItem>();

		AchievementDataList.Clear();
		if( decoder.ContainsJSON( "achievementDataList" ) )
		{
			var array = decoder.ToArray<JSON>( "achievementDataList" );

			for( var idx = 0; idx < array.Length; idx++ )
			{
				AchievementDataList.Add( (AchievementItem)array[idx] );
			}
		}
	}

	#endregion

	//=====================================================
}
