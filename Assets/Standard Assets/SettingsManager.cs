using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class SettingsManager {
	
	#region Fields
	
	private static List<SettingsDataItem> 		settingsList		= new List<SettingsDataItem>(16);
	private static bool							isSettingsLoaded 	= false;

	private	static string						curItemGameSetting;
	private	static int							curItemFairyLevel;
	private	static string						curItemValue;
	
	#endregion
	
	#region Public Interface
	
	//==================================================================
	
	public static string GetSettingsItem(string gameSetting,int fairyLevel,string defaultValue=null)
	{
		if(! isSettingsLoaded)
		{
			LoadSettings();
		}
		
		foreach(SettingsDataItem item in settingsList)
		{
			if(gameSetting == item.gameSetting)
			{
				SettingsDataItemValuePair CurPair = item.GetValuePair( fairyLevel );
				if( CurPair != null )
				{
					return( CurPair.value );
				}
				else
				{
					if( defaultValue == null )
						return( string.Empty );
					else
						return( defaultValue );
				}
			}
		}
		
		Debug.Log( "Setting missing: " + gameSetting );
		return( string.Empty );
	}

	//==================================================================
	
	public static void Touch()
	{
		if(! isSettingsLoaded)
		{
			LoadSettings();
		}
	}

	#endregion
	
	#region Private Methods
	
	//==================================================================

	public static void Reload()
	{
		isSettingsLoaded = false;
		LoadSettings();
	}

	//=============================================================================

	private static bool LoadSettings()
	{
		if(isSettingsLoaded)
			return true;
		
		isSettingsLoaded = true;
		
		StringReader reader = null;
		if(PlayerPrefs.HasKey( PreHelpers.GetLiveDataVersionString()) && false)
		{
			// Load locally
			string DocPath = PreHelpers.GetFileFolderPath();
			string FilePath = DocPath + "Settings" + ".txt";

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
				PlayerPrefs.DeleteKey( PreHelpers.GetLiveDataVersionString() );
			}
			
			reader = new StringReader(Contents);
		}
		else
		{
			// Load from resources
			string fullpath = "Settings/Settings";
			TextAsset textAsset = (TextAsset) Resources.Load(fullpath,typeof(TextAsset));
			if(null == textAsset) 
			{
				Debug.Log( fullpath + " file not found." );
				return false;
			}

			reader = new StringReader(textAsset.text);
		}
		
		// Create the Settings list if one doesn't exist
		if(null == settingsList)
			settingsList = new List<SettingsDataItem>(16);
			
		// Clear the dictionary
		settingsList.Clear();
		
		string dataVal = string.Empty;
		string intext = string.Empty;
		
		int maxColumns = 3;
		int columnCount = 0;
		int itemCount = 0;

		// First parse through rows until we find the starting one
		// The 'Settings.txt' file has loads of info at the start that we can ignore as it's all excel settings
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
						
						settingsList.Add( new SettingsDataItem() );
						
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
						//Utils.DebugLog("Finished loading Settings items.");
						reader.Close();
						return true;
					}
				}
			}
		}

		// ToDo: DEBUG - REMOVE THIS
		//Debug.Log( "Num settings items: " + settingsList.Count );
	}
	
	//==================================================================
	
	private static void UpdateItemData(int itemIndex, int dataindex, string data)
	{	
		switch(dataindex)
		{
		case 0:
			curItemGameSetting = data;
			break;
			
		case 1:
			curItemFairyLevel = int.Parse( data );
			break;
			
		case 2:
			data = data.Replace( "(newline)" , "\n" );
			curItemValue = data;
		
			// Add setting to list
			foreach( SettingsDataItem CurItem in settingsList )
			{
				if( CurItem.gameSetting == curItemGameSetting )
				{
					CurItem.AddValue( curItemFairyLevel , curItemValue );
					return;
				}
			}
			
			SettingsDataItem NewItem = new SettingsDataItem();
			NewItem.gameSetting = curItemGameSetting;
			NewItem.AddValue( curItemFairyLevel , curItemValue );
			settingsList.Add( NewItem );
			break;
			
		}
	}
	
	//==================================================================
	
	#endregion
}
