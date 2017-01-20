using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class TradingCardItemsManager 
{
	private static List< TradingCardSpreadsheetItem > 	itemList			= new List< TradingCardSpreadsheetItem >(64);
	private static bool									isItemListLoaded 	= false;
	private static int[]								pageTypeOffsets		= new int[ 16 ];
	
	//==================================================================
	
	public static int GetPageOffset( eTradingCardClassification Classification )
	{
		return( pageTypeOffsets[ (int)Classification ] + 1 );
	}

	//==================================================================

	public static eTradingCardClassification GetPageClassification( int Page )
	{
		if( Page < pageTypeOffsets[ (int)eTradingCardClassification.WILD ] )
			return eTradingCardClassification.WINX;
		if( Page < pageTypeOffsets[ (int)eTradingCardClassification.STORY ] )
			return eTradingCardClassification.WILD;
		if( Page < pageTypeOffsets[ (int)eTradingCardClassification.STANDARD ] )
			return eTradingCardClassification.STORY;

		return eTradingCardClassification.STANDARD;
	}

	//==================================================================

	static int GetPageCount( eTradingCardClassification Classification )
	{
		int MaxPage = 0;

		if(! isItemListLoaded)
		{
			LoadItemsList();
		}
		
		foreach( TradingCardSpreadsheetItem item in itemList )
		{
			if(Classification == item.classification)
				MaxPage = Mathf.Max( MaxPage , item.page + 1 );
		}
		
		return( MaxPage );
	}
	
	//==================================================================
	
	static void OffsetPageCount( eTradingCardClassification Classification , int Offset )
	{
		if(! isItemListLoaded)
		{
			LoadItemsList();
		}
		
		foreach( TradingCardSpreadsheetItem item in itemList )
		{
			if(Classification == item.classification)
				item.page += Offset;
		}
	}

	//==================================================================

	public static TradingCardSpreadsheetItem GetCard( eChestType type, eTradingCardClassification cardClassification, eTradingCardRarity cardRarity , eTradingCardCondition cardCondition )
	{
		TradingCardSpreadsheetItem card = new TradingCardSpreadsheetItem();
		
		//if( _forceRareCardReward )
		//	type = eChestType.LARGE;
		
		if( ( cardClassification != eTradingCardClassification.NULL ) || ( cardRarity != eTradingCardRarity.NULL ) )
		{
			// Use a specific classification/type rather than a random one
			eNPC UnlockedNPC = eNPC.NULL;
			card = GetSpecificCardType( cardClassification , cardRarity , cardCondition , ref UnlockedNPC );
			return card;
		}
		
		switch( type )
		{
			case eChestType.SMALL:
				// Return common card
				//card = TradingCardItemsManager.GetRandomCard( eTradingCardRarity.COMMON , 30.0f , eTradingCardRarity.VERYCOMMON , 70.0f );
				card = GetRandomCard( eTradingCardRarity.COMMON , 30.0f , eTradingCardRarity.COMMON , 70.0f );
				break;
			case eChestType.MEDIUM:
				// Return common or uncommon card
				card = GetRandomCard( eTradingCardRarity.RARE , 100.0f , eTradingCardRarity.NULL , 0.0f );
				break;
			case eChestType.LARGE:
				// Return uncmmon or rare card
				//card = TradingCardItemsManager.GetRandomCard( eTradingCardRarity.VERYRARE , 80.0f , eTradingCardRarity.UNIQUE , 20.0f );
				card = GetRandomCard( eTradingCardRarity.VERYRARE , 80.0f , eTradingCardRarity.VERYRARE , 20.0f );
				break;
		}

		return card;
	}
	
	//==================================================================
	
	public static int GetNumCards( eTradingCardClassification Classification )
	{
		if(! isItemListLoaded)
		{
			LoadItemsList();
		}
		
		int Count = 0;
		
		foreach( TradingCardSpreadsheetItem item in itemList )
		{
			if( Classification == eTradingCardClassification.NULL )
			{
				if( item.classification == eTradingCardClassification.WINX )
					Count++;
				if( item.classification == eTradingCardClassification.WILD )
					Count++;
				if( item.classification == eTradingCardClassification.STANDARD )
					Count++;
			}
			else
			{
				if( Classification == item.classification )
					Count++;
			}
		}
		
		return( Count );
	}

	//==================================================================

	public static TradingCardSpreadsheetItem GetTradingCardItem( string itemId )
	{
		if(! isItemListLoaded)
		{
			LoadItemsList();
		}
		
		foreach( TradingCardSpreadsheetItem item in itemList )
		{
			if(itemId == item.id)
				return item;
		}
		
		return null;
	}
	
	//==================================================================
	
	public static TradingCardSpreadsheetItem GetTradingCardItem( int Page , int Position )
	{
		if(! isItemListLoaded)
		{
			LoadItemsList();
		}
		
		foreach( TradingCardSpreadsheetItem item in itemList )
		{
			if( ( Page == item.page ) && ( Position == item.position ) )
				return item;
		}
		
		return null;
	}
	
	//==================================================================

	public static TradingCardSpreadsheetItem GetTradingCardItem( int RawIndex )
	{
		if(! isItemListLoaded)
		{
			LoadItemsList();
		}
		
		return( itemList[ RawIndex ] );
	}
	
	//==================================================================

	// Use a specific classification/type rather than a random one
	public static TradingCardSpreadsheetItem GetSpecificCardType( eTradingCardClassification CardClassification , eTradingCardRarity CardRarity , eTradingCardCondition CardCondition , ref eNPC UnlockedNPC )
	{
		UnlockedNPC = eNPC.NULL;
		
		if(! isItemListLoaded)
		{
			LoadItemsList();
		}

		if( CardRarity == eTradingCardRarity.TEACHER )
		{
			// If this is a 'teacher' card then unlock the next NPC in the list
			// If we can't unlock any switch it to a random 'rare' card instead
			
			// Get current population and find the first teacher above it
			
			// If teacher isn't owned then give the card
			int PlayerPopulation = GameDataManager.Instance.PlayerPopulation;
			int NumNPCs = NPCItemsManager.GetNumItems();
			NPCItemData FoundNPCItem = null;
			bool bNPCFound = false;
			
			Debug.Log( "Looking for TEACHER card - current population: " + PlayerPopulation );
			
			for( int Idx = 0 ; Idx < NumNPCs ; Idx++ )
			{
				NPCItemData CurNPCItem = NPCItemsManager.GetNPCItem( Idx );
				
				// Ignore CurNPCItem.PopulationUnlock of zero (default)
				if( CurNPCItem.PopulationUnlock == 0 )
					continue;
				
				if( bNPCFound == false )
				{
					if( PlayerPopulation >= CurNPCItem.PopulationUnlock )
					{
						// Do we have this card already?
						int NumMint = 0;
						int NumScuffed = 0;
						TradingCardHeldItem CurHeldCard = GameDataManager.Instance.GetHeldTradingCard( CurNPCItem.CardId , ref NumMint , ref NumScuffed );
						
						if( ( NumMint > 0 ) || ( NumScuffed > 0 ) )
						{
							// Has card,keep searching
						}
						else
						{
							// Doesn't have card - add it
							bNPCFound = true;
							FoundNPCItem = CurNPCItem;
							Debug.Log( "Found TEACHER card: " + FoundNPCItem.Id + " " + FoundNPCItem.CardId );
						}
					}
				}
				
				//Debug.Log( CurNPCItem.PopulationUnlock );
			}
			
		
			// If no teacher card was given then use a random 'rare' card instead
			if( bNPCFound == false )
			{
				Debug.Log( "No TEACHER card found - giving very rare card instead" );
				CardClassification = eTradingCardClassification.NULL;
				CardRarity = eTradingCardRarity.VERYRARE;
			}
			else
			{
				eNPC FoundNPCId = eNPC.NULL;
				try	{ FoundNPCId = (eNPC)Enum.Parse(typeof(eNPC), FoundNPCItem.Id); }
				catch 
				{
					Debug.Log("Warning: FoundNPCId state not recognised!");
				}
				
				UnlockedNPC = FoundNPCId; //GameDataManager.Instance.UnlockPlayerNPC( FoundNPCId );
				return( GetTradingCardItem( FoundNPCItem.CardId ) );
			}
		}
		
		if( CardClassification == eTradingCardClassification.NULL )
		{
			// Pick a random classification
			switch( UnityEngine.Random.Range( 0 , 3 ) )
			{
				case 0:
					CardClassification = eTradingCardClassification.WINX;
					break;
				case 1:
					CardClassification = eTradingCardClassification.WILD;
					break;
				case 2:
					CardClassification = eTradingCardClassification.STORY;
					break;
			}
		}
		if( CardRarity == eTradingCardRarity.NULL )
		{
			// Pick a random rarity (exclude teacher cards)
			CardRarity = (eTradingCardRarity)( UnityEngine.Random.Range( (int)eTradingCardRarity.UNIQUE , (int)eTradingCardRarity.TEACHER ) );
		}
		
		// Find all cards with this specification and pick a random one
		List< TradingCardSpreadsheetItem > CardList = new List< TradingCardSpreadsheetItem >();
		
		foreach( TradingCardSpreadsheetItem Item in itemList )
		{
			if( ( Item.classification == CardClassification ) && ( Item.rarity == CardRarity ) )
				CardList.Add( Item );
		}

		// Pick from card list
		int CardIdx = UnityEngine.Random.Range( 0 , CardList.Count );
		if( CardList.Count == 0 )
		{
			Debug.LogWarning( "No cards to reward player!" );
			Debug.LogWarning( "Classification: " + CardClassification + " Rarity: " + CardRarity + " Condition: " + CardCondition );
			return( itemList[ 0 ] );
		}
		else
		{
			return( CardList[ CardIdx ] );
		}
	}

	//==================================================================

	public static TradingCardSpreadsheetItem GetRandomCard( eTradingCardRarity Rarity1 , float Chance1 , eTradingCardRarity Rarity2 , float Chance2 )
	{
		if(! isItemListLoaded)
		{
			LoadItemsList();
		}

		// Get list of cards with the passed rarity values
		List< TradingCardSpreadsheetItem > CardList1 = new List< TradingCardSpreadsheetItem >();
		List< TradingCardSpreadsheetItem > CardList2 = new List< TradingCardSpreadsheetItem >();
		
		foreach( TradingCardSpreadsheetItem Item in itemList )
		{
			if( Item.rarity == Rarity1 )
				CardList1.Add( Item );
			if( Item.rarity == Rarity2 )
				CardList2.Add( Item );
		}
		
		// Find which group of cards to pick from
		float CurChance = UnityEngine.Random.Range( 0.0f , 100.0f );
		if( CurChance < Chance1 )
		{
			// Pick from card list 1
			int CardIdx = UnityEngine.Random.Range( 0 , CardList1.Count );
			if( CardList1.Count == 0 )
			{
				Debug.LogError( "No cards to reward player!" );
				Debug.LogError( "Rarity: " + Rarity1 );
				return( itemList[ 0 ] );
			}
			else
			{
				return( CardList1[ CardIdx ] );
			}
		}
		else
		{
			// Pick from card list 2
			int CardIdx = UnityEngine.Random.Range( 0 , CardList2.Count );
			
			if( CardList2.Count == 0 )
			{
				Debug.LogError( "No cards to reward player!" );
				Debug.LogError( "Rarity: " + Rarity2 );
				return( itemList[ 0 ] );
			}
			else
			{
				return( CardList2[ CardIdx ] );
			}
		}
	}

	//==================================================================

	public static int GetNumItems()
	{
		if(! isItemListLoaded)
		{
			LoadItemsList();
		}
		
		return itemList.Count;
	}
	
	//==================================================================
	
	public static void Reload()
	{
		isItemListLoaded = false;
		LoadItemsList();
	}

	//=============================================================================

	private static bool LoadItemsList()
	{
		if(isItemListLoaded)
			return true;
		
		isItemListLoaded = true;

		StringReader reader = null;
		if(PlayerPrefs.HasKey(PreHelpers.GetLiveDataVersionString()) && false)
		{
			// Load locally
			string DocPath = PreHelpers.GetFileFolderPath();
			string FilePath = DocPath + "Cards.txt";

			// Read file
			string Contents = null;
			try
			{
				StreamReader CurFile = new StreamReader( FilePath );
				Contents = CurFile.ReadToEnd();

				// Close file
				CurFile.Close();
			}
			catch
			{
				PlayerPrefsWrapper.DeleteKey( PreHelpers.GetLiveDataVersionString() );
			}
			
			reader = new StringReader(Contents);
		}
		else
		{
			// Load from resources
			string fullpath = "Cards/Cards";
			TextAsset textAsset = (TextAsset) Resources.Load(fullpath,typeof(TextAsset));
			if (textAsset == null) 
			{
				Debug.Log(fullpath + " file not found.");
				return false;
			}

			reader = new StringReader(textAsset.text);
		}

		// Create the TradingCard item list if one doesn't exist
		if(null == itemList) 
			itemList = new List< TradingCardSpreadsheetItem >(64);
			
		// Clear the dictionary
		itemList.Clear();
		
		string dataVal = string.Empty;
		string intext = string.Empty;
		
		// [ ItemID | Classification | Value | ValueScuffed | Rarity | Wait Time | Reveal Price | Page | Position | SmallGUI - 2D | LargeGUI - 2D ]
		int maxColumns = 11;
		int columnCount = 0;
		int itemCount = 0;

		// First parse through rows until we find the starting one
		// The 'TradingCarditems.txt' file has loads of info at the start that we can ignore as it's all excel settings
		int RowCount = 0;
		while(true)
		{
			intext = reader.ReadLine();

			int Idx = intext.IndexOf( "<Row" );
			if( Idx != -1 )
				RowCount++;
			
			if( RowCount == 6 )
				break;
		}
		
		// Start of text data, begin parsing the rows
		while(true)
		{
			int Idx, Idx2, Idx3, Idx4;
			bool isIdValid = false;
			columnCount = 0;
			
			// Read cell containing text code
			intext = reader.ReadLine();
			
			// intext might look something like this now:
			// <Cell ss:StyleID="s32"><Data ss:Type="String">0001</Data></Cell>

			// Find the data in the cell - in this case it's "Item ID"			
			Idx4 = intext.IndexOf( "><" );
			if( Idx4 != -1 )
			{
				Idx2 = intext.IndexOf( ">" , Idx4 + 2 );
				if( Idx2 != -1 )
				{
					Idx3 = intext.IndexOf( "<" , Idx2 );
					if( Idx3 != -1 )
					{
						// String is between Idx2 and Idx3 - this is the text 'ItemId' - "0001"
						dataVal = intext.Substring( Idx2 + 1 , Idx3 - Idx2 - 1 );
						
						itemList.Add( new TradingCardSpreadsheetItem() );
						
						UpdateItemData(itemCount, columnCount, dataVal);
						
						isIdValid = true;
					}
				}
				
				++columnCount;
			}
			
			// If we've found an itemId continue reading the item data for this item
			if(isIdValid)
			{				
				while(columnCount < maxColumns)
				{			
					// Read cell containing text code
					intext = reader.ReadLine();
					
					Idx4 = intext.IndexOf( "><" );
					if( Idx4 != -1 )
					{
						Idx2 = intext.IndexOf( ">" , Idx4 + 2 );
						if( Idx2 != -1 )
						{
							Idx3 = intext.IndexOf( "<" , Idx2 );
							if( Idx3 != -1 )
							{
								// String is between Idx2 and Idx3 - this is the text 'ItemId' - "0001"
								dataVal = intext.Substring( Idx2 + 1 , Idx3 - Idx2 - 1 );
								
								UpdateItemData(itemCount, columnCount, dataVal);
							}
						}
					}
					
					++columnCount;
				}
				
				++itemCount;
			}
			
			// Find the end of this row by looking for the start of the next row
			while(true)
			{
				intext = reader.ReadLine();

				Idx = intext.IndexOf( "<Row" );
				if(Idx != -1)
				{						
					// Found end of row
					break;
				}
				else
				{
					Idx = intext.IndexOf( "Table>" );
					if(Idx != -1)
					{
						// Found end of table
						//Debug.Log("Finished loading TradingCard items data list.");
						reader.Dispose();
						
						// Order list by priority
						//itemList.Sort((x,y) => x.GetOrderPriority() - y.GetOrderPriority());
						
						
						// Expand each item type into individual page sections
						int NumWinxPages = GetPageCount( eTradingCardClassification.WINX );
						int NumWildPages = GetPageCount( eTradingCardClassification.WILD );
						int NumStoryPages = GetPageCount( eTradingCardClassification.STORY );
						int NumStandardPages = GetPageCount( eTradingCardClassification.STANDARD );
						
						pageTypeOffsets[ (int)eTradingCardClassification.WINX ] = 0;
						pageTypeOffsets[ (int)eTradingCardClassification.WILD ] = NumWinxPages;
						pageTypeOffsets[ (int)eTradingCardClassification.STORY ] = NumWinxPages + NumWildPages;
						pageTypeOffsets[ (int)eTradingCardClassification.STANDARD ] = NumWinxPages + NumWildPages + NumStoryPages;
						
						OffsetPageCount( eTradingCardClassification.WINX , pageTypeOffsets[ (int)eTradingCardClassification.WINX ] );
						OffsetPageCount( eTradingCardClassification.WILD , pageTypeOffsets[ (int)eTradingCardClassification.WILD ] );
						OffsetPageCount( eTradingCardClassification.STORY , pageTypeOffsets[ (int)eTradingCardClassification.STORY ] );
						OffsetPageCount( eTradingCardClassification.STANDARD , pageTypeOffsets[ (int)eTradingCardClassification.STANDARD ] );
						
						return true;
					}
				}
			}
		}
	}
	
	//==================================================================
	
	private static void UpdateItemData(int itemIndex, int dataindex, string data)
	{		
		switch(dataindex)
		{
		case 0:
			itemList[itemIndex].id = data;
			break;
			
		case 1:
			try	{ itemList[itemIndex].classification = (eTradingCardClassification)Enum.Parse(typeof(eTradingCardClassification), data); }
			catch 
			{
				Debug.Log("Warning: eTradingCardClassification state not recognised!");
				itemList[itemIndex].classification = eTradingCardClassification.NULL; 
			}
			break;

		case 2:
			itemList[itemIndex].value = int.Parse( data );
			break;
			
		case 3:
			itemList[itemIndex].valueScuffed = int.Parse( data );
			break;

		case 4:
			try	{ itemList[itemIndex].rarity = (eTradingCardRarity)Enum.Parse(typeof(eTradingCardRarity), data); }
			catch 
			{
				Debug.Log("Warning: eTradingCardRarity state not recognised!");
				itemList[itemIndex].rarity = eTradingCardRarity.NULL; 
			}
			break;

		case 5:
			itemList[itemIndex].waitTime = int.Parse( data );
			break;

		case 6:
			itemList[itemIndex].revealPrice = int.Parse( data );
			break;

		case 7:
			itemList[itemIndex].page = int.Parse( data ) - 1;
			break;

		case 8:
			itemList[itemIndex].position = int.Parse( data ) - 1;
			break;

		case 9:
			itemList[itemIndex].smallGuiTexture2D = data;
			break;

		case 10:
			itemList[itemIndex].largeGuiTexture2D = data;
			break;
		}
		
		itemList[itemIndex].globalPosition = itemIndex;
	}
	
	//==================================================================
}
