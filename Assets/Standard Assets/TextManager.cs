using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;


//===============================================================================================

public class TextManager
{
	private static Dictionary< string , string > 		textTable;
	private static bool									m_bLanguageLoaded = false;

	//=============================================================================
	
	public static void Reload()
	{
		m_bLanguageLoaded = false;
		LoadLanguage();
	}

	//=============================================================================

	static bool LoadLanguage()
	{
		string language_type = PreHelpers.GetLanguageCode();
		
		Debug.Log( "Loaded localisations for language code: '" + language_type + "'" );
		
		if( m_bLanguageLoaded == true )
			return true;
		
		m_bLanguageLoaded = true;
		
		StringReader reader = null;
		
		if(PlayerPrefs.HasKey(PreHelpers.GetLiveDataVersionString()) && false)
		{
			// Load locally
			string DocPath = PreHelpers.GetFileFolderPath();
			string FilePath = DocPath + "Localisations_" + language_type.ToUpper() + ".txt";

			// Read file
			string Contents = null;
			try
			{
				StreamReader CurFile = new StreamReader( FilePath );
				Contents = CurFile.ReadToEnd();

				// Close file
				CurFile.Dispose();
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
			string fullpath = "Languages/Localisations_" + language_type.ToUpper();
			TextAsset textAsset = (TextAsset) Resources.Load(fullpath,typeof(TextAsset));
			if (textAsset == null) 
			{
				Debug.Log(fullpath + " file not found.");
				return false;
			}
			reader = new StringReader(textAsset.text);
		}

		// create the text hash table if one doesn't exist
		if (textTable == null) 
		{
			textTable = new Dictionary<string , string>();
		}
			
		// clear the dictionary
		textTable.Clear();
		
		string key = string.Empty;
		string val = string.Empty;
		string multival = string.Empty;
		string intext = string.Empty;

		// First parse through rows until we find the starting one
		// The 'localisations.txt' file has loads of info at the start that we can ignore as it's all excel settings
		int RowCount = 0;
		while(true)
		{
			intext = reader.ReadLine();

			int Idx = intext.IndexOf("<Row");
			if(Idx != -1)
				RowCount++;
			
			if(RowCount == 7)
				break;
		}
		
		// Here's an example of a 'row' from the localisations.txt file containing the various cells
		/*
		<Row>
			<Cell ss:StyleID="s32"><Data ss:Type="String">POPUP_CONFIRMATION</Data></Cell>
			<Cell><Data ss:Type="Number">7</Data></Cell>
			<Cell ss:StyleID="s32"><Data ss:Type="String">Confirm</Data></Cell>
			<Cell ss:StyleID="s32"><Data ss:Type="Number">0</Data></Cell>
			<Cell ss:StyleID="s32"><Data ss:Type="String">Conferma</Data></Cell>
			<Cell ss:StyleID="s31"/>
			<Cell ss:StyleID="s31"/>
			<Cell ss:StyleID="s31"/>
		</Row>
		*/
		
		// Start of text data, begin parsing the rows
		int Line = 0;
		while(true)
		{
			// Read cell containing text code
			intext = reader.ReadLine();
			
			// intext might look something like this now:
			// <Cell ss:StyleID="s32"><Data ss:Type="String">POPUP_CONFIRMATION</Data></Cell>

			// Find the data in the cell - in this case it's "POPUP_CONFIRMATION"
			bool bValidKey = false;
			int Idx4 = intext.IndexOf( "><" );
			if( Idx4 != -1 )
			{
				int Idx2 = intext.IndexOf( ">" , Idx4 + 2 );
				if( Idx2 != -1 )
				{
					int Idx3 = intext.IndexOf( "<" , Idx2 );
					if( Idx3 != -1 )
					{
						// String is between Idx2 and Idx3 - this is the text 'key' - "POPUP_CONFIRMATION"
						key = intext.Substring( Idx2 + 1 , Idx3 - Idx2 - 1 );
						bValidKey = true;
					}
				}
			}

			// If we've found a key continue reading the translation text for this key
			if( bValidKey )
			{
				// Skip until correct language column is reached - each column is a line in the file
				int ColumnCount = 2;
				do
				{
					multival = reader.ReadLine();
					ColumnCount++;
				} while( ColumnCount <= 2 );
						
				intext = reader.ReadLine();
				
				bool bValidVal = false;
				
				int Idx5 = intext.IndexOf( "><" );
				if( Idx5 != -1 )
				{
					int Idx6 = intext.IndexOf( ">" , Idx5 + 2 );
					if( Idx6 != -1 )
					{
						int Idx7 = intext.IndexOf( "<" , Idx6 );
						if( Idx7 != -1 )
						{
							// String is between Idx2 and Idx3 - this is the text 'val'
							val = intext.Substring( Idx6 + 1 , Idx7 - Idx6 - 1 );

							val = val.Replace( "(newline)" , "\n" );
							
							/* Below replacement is now done on server
							// Convert newline characters and other special codes
							val = val.Replace( "&#10;" , "\n" );
							val = val.Replace( "&#39;" , "'" );
							val = val.Replace( "&quot;" , "\"" );
							val = val.Replace( "&amp;" , "&" );
							
							// Replace microsoft word special chars
							val = val.Replace( "’" , "'" );
							val = val.Replace( "–" , "-" );
							val = val.Replace( "‘" , "'" );
							val = val.Replace( "“" , "\"" );
							val = val.Replace( "”" , "\"" );
							*/

							bValidVal = true;
						}
					}
				}
	
				if( bValidVal )
				{
					//Debug.Log( "Valid Key: " + key + " (" + val + ")" );
					try
					{
						textTable.Add(key, val);
					}
					catch
					{
						Debug.LogError("Duplicate key in localisations sheet: " + key + " " + val );
					}
				}
				else
				{
					Debug.Log( "Invalid Value for key [" + key + "]" );
				}
				
				// Find the end of this row by looking for the start of the next row
				while(true)
				{
					multival = reader.ReadLine();

					int Idx = multival.IndexOf( "<Row" );
					if( Idx != -1 )
					{
						// Found next row, break
						break;
					}
					
					// End of excel sheet?
					Idx = multival.IndexOf( "Workbook>" );
					if( Idx != -1 )
					{
						// Found end of workbook
						reader.Dispose();
						return true;
					}
				}
			}
			else
			{
				Debug.Log( "Invalid Key! " + key);
				#if UNITY_EDITOR
				UnityEditor.EditorUtility.DisplayDialog("Error", "Key missing in localisations after:" + key + " - line:" + Line, "OK" );
				#endif
				break;
			}
			
			Line++;
		}

		reader.Dispose();
				
		return true;
	}

	//=============================================================================
	
	public static void Touch()
	{
		if( m_bLanguageLoaded == false )
		{
			LoadLanguage();
		}
	}
	
	//=============================================================================

	public static string GetText(string key)
	{
		if( m_bLanguageLoaded == false )
		{
			//double StartTime = Time.realtimeSinceStartup;
			LoadLanguage();
			//double DeltaTime = Time.realtimeSinceStartup - StartTime;
			
			//Debug.Log( "Parsed localisations in " + DeltaTime + "s" );
		}
		
		if(null == textTable)
		{
			Debug.Log("ERROR: null reference for localised text table!");
			return key;
		}
		
		if(null == key || "" == key)
		{
			Debug.Log("WARNING: submitted string param was empty!");
			return key;
		}
		
		if( textTable.ContainsKey( key ) )
		{
			string value = (string)textTable[key];
			
			if(null == value || "" == value)
			{
				Debug.Log("WARNING: key for localisation table was not found: " + key);
				return key;
			}
			
			//Debug.Log("key: " + key + "   val: " + value );
			return value;
		}
		else
		{
			Debug.Log("Table doesn't contain key: " + key);
			return key;
		}
	}
}