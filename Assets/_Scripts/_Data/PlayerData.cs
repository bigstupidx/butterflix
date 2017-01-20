using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
	public List<FairyData> FairyData { get; set; }

	public int CurrentFairy { get; set; }

	public int Lives { get; set; }

	public int Health { get; set; }

	public int Gems { get; set; }

	public int RedGems { get; set; }

	public int Diamonds { get; set; }

	private int _population;

	public int Population
	{
		get { return _population; }
		set
		{
			_population = value;
			if( value > HighestEverPopulation )
				HighestEverPopulation = value;
		}
	}

	public int HighestEverPopulation { get; set; }

	public int LowestPopulationCap { get; set; }

	public float WildMagicRate { get; set; }

	public int BossLevel { get; set; }

	public int[] NPCs { get; set; }

	//=====================================================

	public PlayerData()
	{
		FairyData = new List<FairyData>( (int)eFairy.NUM_FAIRIES );

		for( var i = 0; i < (int)eFairy.NUM_FAIRIES; i++ )
			FairyData.Add( new FairyData() );

		CurrentFairy = 0;
		Lives = 0;
		Health = 0;
		Gems = 0;
		RedGems = 0;
		Diamonds = 0;
		Population = 0;
		HighestEverPopulation = 0;
		LowestPopulationCap = 0;
		WildMagicRate = 0.0f;
		BossLevel = 1;

		// NPCs: NPC_STUDENT (index: 0) is always unlocked (value: 1)
		NPCs = new[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
	}

	//=====================================================

	public int[] FairiesOwned()
	{
		var fairies = new int[FairyData.Count];

		for( var i = 0; i < FairyData.Count; i++ )
			fairies[i] = (FairyData[i].Fairy >= 0) ? 1 : 0;

		return fairies;
	}

	//=====================================================

	public PlayerData Copy()
	{
		var obj = new PlayerData
		{
			FairyData = FairyData,
			CurrentFairy = CurrentFairy,
			Lives = Lives,
			Health = Health,
			Gems = Gems,
			RedGems = RedGems,
			Diamonds = Diamonds,
			Population = Population,
			HighestEverPopulation = HighestEverPopulation,
			LowestPopulationCap = LowestPopulationCap,
			WildMagicRate = WildMagicRate,
			BossLevel = BossLevel,
			NPCs = NPCs
		};


		return obj;
	}

	//=====================================================

	public void Reset( bool buyDefaultFairy = true )
	{
		if( FairyData == null )
			FairyData = new List<FairyData>( (int)eFairy.NUM_FAIRIES );
		else
			FairyData.Clear();

		for( var i = 0; i < (int)eFairy.NUM_FAIRIES; i++ )
			FairyData.Add( new FairyData() );

		// Set default fairy (BLOOM) owned
		CurrentFairy = (int)eFairy.BLOOM;
		Lives = 0;
		Health = 0;
		Gems = 0;
		RedGems = 0;
		Diamonds = 0;
		Population = 0;
		HighestEverPopulation = 0;
		LowestPopulationCap = 0;
		WildMagicRate = 0.0f;
		BossLevel = 1;

		// NPCs: NPC_STUDENT (index: 0) is always unlocked (value: 1)
		NPCs = new[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

		if( buyDefaultFairy == true )
			GameDataManager.Instance.BuyFairy( eFairy.BLOOM, true );
	}

	//=====================================================

	public static implicit operator JSON( PlayerData data )
	{
		// Write JSON 
		var js = new JSON();

		if( data.FairyData != null && data.FairyData.Count > 0 )
		{
			var fairyDataArray = new JSON[data.FairyData.Count];
			for( var i = 0; i < data.FairyData.Count; i++ )
				fairyDataArray[i] = (JSON)data.FairyData[i];

			js["fairydataarray"] = fairyDataArray;
		}

		js["currentfairy"] = data.CurrentFairy;
		js["lives"] = data.Lives;
		js["health"] = data.Health;
		js["gems"] = data.Gems;
		js["redgems"] = data.RedGems;
		js["diamonds"] = data.Diamonds;
		js["_population"] = data.Population;
		js["highesteverpopulation"] = data.HighestEverPopulation;
		js["lowestPopulationCap"] = data.LowestPopulationCap;
		js["wildmagicrate"] = data.WildMagicRate;
		js["bosslevel"] = data.BossLevel;

		if( data.NPCs != null && data.NPCs.Length > 0 )
			js["npcsdataarray"] = data.NPCs;

		//if( data.m_Rotation != 0 ) js["rotation"] = data.m_Rotation;
		//if( ((int)data.m_SelectionType) != 0 ) js["selectionType"] = (int)data.m_SelectionType;

		return (js);
	}

	// //=====================================================

	public static explicit operator PlayerData( JSON obj )
	{
		// Read JSON
		var newObj = new PlayerData();
		//const bool bVerbose = false;

		var fairyDataArray = obj.ToArray<JSON>( "fairydataarray" );

		newObj.FairyData.Clear();
		foreach( var jsFairy in fairyDataArray )
		{
			newObj.FairyData.Add( (FairyData)jsFairy );
			//if( bVerbose ) Debug.Log( "fairydata: " + newObj.FairyData );
		}

		newObj.CurrentFairy = obj.ToInt( "currentfairy" );
		//if( bVerbose ) Debug.Log( "currentfairy: " + newObj.CurrentFairy );

		newObj.Lives = obj.ToInt( "lives" );
		//if( bVerbose ) Debug.Log( "lives: " + newObj.Lives );

		newObj.Health = obj.ToInt( "health" );
		//if( bVerbose ) Debug.Log( "health: " + newObj.Health );

		newObj.Gems = obj.ToInt( "gems" );
		//if( bVerbose ) Debug.Log( "gems: " + newObj.Gems );

		newObj.RedGems = obj.ToInt( "redgems" );
		//if( bVerbose ) Debug.Log( "redgems: " + newObj.RedGems );

		newObj.Diamonds = obj.ToInt( "diamonds" );
		//if( bVerbose ) Debug.Log( "diamonds: " + newObj.Diamonds );

		newObj.Population = obj.ToInt( "_population" );
		//if( bVerbose ) Debug.Log( "_population: " + newObj.Population );

		newObj.HighestEverPopulation = obj.ToInt( "highesteverpopulation" );
		//if( bVerbose ) Debug.Log( "highesteverpopulation: " + newObj.HighestEverPopulation );

		newObj.LowestPopulationCap = obj.ToInt( "lowestPopulationCap" );
		//if( bVerbose ) Debug.Log( "lowestPopulationCap: " + newObj.LowestPopulationCap );

		newObj.WildMagicRate = obj.ToFloat( "wildmagicrate" );
		//if( bVerbose ) Debug.Log( "wildmagicrate: " + newObj.WildMagicRate );

		newObj.BossLevel = obj.ToInt( "bosslevel" );
		//if( bVerbose ) Debug.Log( "bosslevel: " + newObj.BossLevel );

		newObj.NPCs = obj.ToArray<int>( "npcsdataarray" );

		//if( obj["unlockStage"] != null )
		//{
		//	newObj.m_UnlockStage = obj.ToInt( "unlockStage" );
		//	if( bVerbose ) Debug.Log( "unlockStage: " + newObj.m_UnlockStage );
		//}
		//else
		//{
		//	newObj.m_UnlockStage = 0;
		//}

		return (newObj);
	}

	//=====================================================
}
