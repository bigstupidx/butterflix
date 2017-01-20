using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public class WildMagicItemsManager
{
	private static List<WildMagicItemData> 	_itemList			= new List<WildMagicItemData>( 16 );
	private static bool						_isItemListLoaded 	= false;

	//==================================================================

	public static float GetWildMagicItemValue( string itemId )
	{
		if( !_isItemListLoaded )
		{
			LoadItemsList();
		}

		foreach( var item in _itemList )
		{
			if( itemId == item.Id )
				return item.Value;
		}

		return -1;
	}

	//==================================================================

	public static WildMagicItemData GetWildMagicItem( string itemId )
	{
		if( !_isItemListLoaded )
		{
			LoadItemsList();
		}

		foreach( var item in _itemList )
		{
			if( itemId == item.Id )
				return item;
		}

		return null;
	}

	//==================================================================

	public static int GetNumItems()
	{
		if( !_isItemListLoaded )
		{
			LoadItemsList();
		}

		return _itemList.Count;
	}

	//==================================================================

	public static void Reload()
	{
		_isItemListLoaded = false;
		LoadItemsList();
	}

	//=============================================================================

	private static bool LoadItemsList()
	{
		if( _isItemListLoaded )
			return true;

		_isItemListLoaded = true;

		StringReader reader = null;
		if(PlayerPrefs.HasKey(PreHelpers.GetLiveDataVersionString()) && false)
		{
			// Load locally
			string DocPath = PreHelpers.GetFileFolderPath();
			string FilePath = DocPath + "WildMagic.txt";

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

			reader = new StringReader( Contents );
		}
		else
		{
			// Load from resources
			const string fullpath = "WildMagic/WildMagic";
			var textAsset = (TextAsset)Resources.Load( fullpath, typeof( TextAsset ) );
			if( textAsset == null )
			{
				Debug.Log( fullpath + " file not found." );
				return false;
			}

			reader = new StringReader( textAsset.text );
		}

		// Create the wild magic item list if one doesn't exist
		if( null == _itemList )
			_itemList = new List<WildMagicItemData>( 16 );

		// Clear the list
		_itemList.Clear();

		var dataVal = string.Empty;
		var intext = string.Empty;

		// [ ItemID | Value | Population Bonus ]
		const int maxColumns = 3;
		var columnCount = 0;
		var itemCount = 0;

		// First parse through rows until we find the starting one
		// The 'WildNagic.txt' file has loads of info at the start that we can ignore as it's all excel settings
		var rowCount = 0;
		while( true )
		{
			intext = reader.ReadLine();

			var idx = intext.IndexOf( "<Row" );
			if( idx != -1 )
				rowCount++;

			if( rowCount == 6 )
				break;
		}

		// Start of text data, begin parsing the rows
		while( true )
		{
			int Idx, Idx2, Idx3, Idx4;
			var isIdValid = false;
			columnCount = 0;

			// Read cell containing text code
			intext = reader.ReadLine();

			// intext might look something like this now:
			// <Cell ss:StyleID="s32"><Data ss:Type="String">0001</Data></Cell>

			// Find the data in the cell - in this case it's "Item ID"			
			Idx4 = intext.IndexOf( "><" );
			if( Idx4 != -1 )
			{
				Idx2 = intext.IndexOf( ">", Idx4 + 2 );
				if( Idx2 != -1 )
				{
					Idx3 = intext.IndexOf( "<", Idx2 );
					if( Idx3 != -1 )
					{
						// String is between Idx2 and Idx3 - this is the text 'ItemId' - "0001"
						dataVal = intext.Substring( Idx2 + 1, Idx3 - Idx2 - 1 );

						_itemList.Add( new WildMagicItemData() );

						UpdateItemData( itemCount, columnCount, dataVal );

						isIdValid = true;
					}
				}

				++columnCount;
			}

			// If we've found an itemId continue reading the item data for this item
			if( isIdValid )
			{
				while( columnCount < maxColumns )
				{
					// Read cell containing text code
					intext = reader.ReadLine();

					Idx4 = intext.IndexOf( "><" );
					if( Idx4 != -1 )
					{
						Idx2 = intext.IndexOf( ">", Idx4 + 2 );
						if( Idx2 != -1 )
						{
							Idx3 = intext.IndexOf( "<", Idx2 );
							if( Idx3 != -1 )
							{
								// String is between Idx2 and Idx3 - this is the text 'ItemId' - "0001"
								dataVal = intext.Substring( Idx2 + 1, Idx3 - Idx2 - 1 );

								UpdateItemData( itemCount, columnCount, dataVal );
							}
						}
					}

					++columnCount;
				}

				++itemCount;
			}

			// Find the end of this row by looking for the start of the next row
			while( true )
			{
				intext = reader.ReadLine();

				Idx = intext.IndexOf( "<Row" );
				if( Idx != -1 )
				{
					// Found end of row
					break;
				}
				else
				{
					Idx = intext.IndexOf( "Table>" );
					if( Idx != -1 )
					{
						// Found end of table
						//Debug.Log("Finished loading wild magic items data list.");
						reader.Dispose();

						// Order list by priority
						//_itemList.Sort((x,y) => x.GetOrderPriority() - y.GetOrderPriority());
						return true;
					}
				}
			}
		}
	}

	//==================================================================

	private static void UpdateItemData( int itemIndex, int dataindex, string data )
	{
		switch( dataindex )
		{
			case 0:
				_itemList[itemIndex].Id = data;
				break;

			case 1:
				_itemList[itemIndex].Value = Convert.ToSingle( data );
				break;

			case 2:
				_itemList[itemIndex].PopulationBonus = Convert.ToInt32( data );
				break;
		}
	}

	//==================================================================
}
