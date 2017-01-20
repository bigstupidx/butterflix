using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class ClothingItemsManager 
{
	private static List<ClothingItemData> 	itemList			= new List<ClothingItemData>(64);
	private static bool						isItemListLoaded 	= false;
	
	//==================================================================
	
	public static List< ClothingItemData > GetClothingItems( eFairy fairy )
	{
		List< ClothingItemData > CurClothingList = new List< ClothingItemData >();

		if(! isItemListLoaded)
		{
			LoadItemsList();
		}
		
		foreach(ClothingItemData item in itemList)
		{
			if(fairy == item.fairy)
			{
				if(item.state != eClothingState.HIDDEN )
				{
					CurClothingList.Add( item );
				}
			}
		}
		
		return CurClothingList;
	}

	//==================================================================

	public static ClothingItemData GetClothingItem(string itemId)
	{
		if(! isItemListLoaded)
		{
			LoadItemsList();
		}
		
		foreach(ClothingItemData item in itemList)
		{
			if(itemId == item.id)
				return item;
		}
		
		return null;
	}
	
	//==================================================================
	
	public static string GetClothingDefaultItem(eFairy fairy)
	{
		if(! isItemListLoaded)
		{
			LoadItemsList();
		}
		
		foreach(ClothingItemData item in itemList)
		{
			if(fairy == item.fairy)
			{
				if(item.isDefault)
				{
					return item.id;
				}
			}
		}
		
		return null;
	}
	
	//==================================================================

	public static ClothingItemData GetClothingItem(int Idx)
	{
		if(! isItemListLoaded)
		{
			LoadItemsList();
		}
		
		if( ( Idx >= 0 ) && ( Idx < itemList.Count ) )
			return( itemList[ Idx ] );
		else
			return( null );
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
			string FilePath = DocPath + "Clothing.txt";

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
			string fullpath = "Clothing/Clothing";
			TextAsset textAsset = (TextAsset) Resources.Load(fullpath,typeof(TextAsset));
			if (textAsset == null) 
			{
				Debug.Log(fullpath + " file not found.");
				return false;
			}

			reader = new StringReader(textAsset.text);
		}

		// Create the clothing item list if one doesn't exist
		if(null == itemList) 
			itemList = new List<ClothingItemData>(64);
			
		// Clear the dictionary
		itemList.Clear();
		
		string dataVal = string.Empty;
		string intext = string.Empty;
		
		// [ ItemID | Fairy | Default | Cost | GUI - 2D | PrefabName | GamePrefabName | State ]
		int maxColumns = 8;
		int columnCount = 0;
		int itemCount = 0;

		// First parse through rows until we find the starting one
		// The 'clothingitems.txt' file has loads of info at the start that we can ignore as it's all excel settings
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
						
						itemList.Add( new ClothingItemData() );
						
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
						//Debug.Log("Finished loading clothing items data list.");
						reader.Dispose();
						
						// Order list by priority
						//itemList.Sort((x,y) => x.GetOrderPriority() - y.GetOrderPriority());
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
			try	{ itemList[itemIndex].fairy = (eFairy)Enum.Parse(typeof(eFairy), data); }
			catch 
			{
				Debug.Log("Warning: fairy type not recognised!");
				itemList[itemIndex].fairy = eFairy.BLOOM; 
			}
			break;
			
		case 2:
			itemList[itemIndex].isDefault = ( data == "YES" ? true : false );
			break;
		
		case 3:
			itemList[itemIndex].cost = int.Parse( data );
			break;

		case 4:
			itemList[itemIndex].guiTexture2D = data;
			break;

		case 5:
			itemList[itemIndex].prefabName = data;
			break;

		case 6:
			itemList[itemIndex].gamePrefabName = data;
			break;

		case 7:
			try	{ itemList[itemIndex].state = (eClothingState)Enum.Parse(typeof(eClothingState), data); }
			catch 
			{
				Debug.Log("Warning: eClothingState type not recognised!");
				itemList[itemIndex].state = eClothingState.AVAILABLE; 
			}
			break;
		}
	}
	
	//==================================================================
}
