using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Text;

public class UpdateSpreadsheets: ScriptableWizard
{
	//=============================================================================

	[UnityEditor.MenuItem("Tools/EM Studios/Update All Google Spreadsheets &x",false,10)]
	static void GoogleSpreadsheetsUpdate()
	{
		UpdateXML( "Localisations_EN" , "Languages" , 2 );
		UpdateXML( "Localisations_FR" , "Languages" , 4 );
		UpdateXML( "Localisations_IT" , "Languages" , 5 );
		UpdateXML( "Localisations_DE" , "Languages" , 6 );
		UpdateXML( "Localisations_ES" , "Languages" , 7 );
		UpdateXML( "Localisations_RU" , "Languages" , 4 );
		UpdateXML( "Localisations_PT" , "Languages" , 9 );
		UpdateXML( "Settings" , "Settings" , 0 );
		UpdateXML( "Cards" , "Cards" , 0 );
		UpdateXML( "Clothing" , "Clothing" , 0 );
		UpdateXML( "Fairies" , "Fairies" , 0 );
		UpdateXML( "WildMagic" , "WildMagic" , 0 );
		UpdateXML( "NPC" , "NPC" , 0 );
		
		AssetDatabase.Refresh();
	}
	
	//=============================================================================

	
	[UnityEditor.MenuItem("Tools/EM Studios/Get used font characters",false,15)]
	static void GetUsedFontCharacters()
	{
		// Add currency symbols as they don't get pulled through as extended ASCII
		string AllChars = "£¢¥©®";
		
		// Get a list of files to include 
		FileInfo[] FileList1 = new DirectoryInfo( Application.dataPath + "/" + "/Resources/Languages/" ).GetFiles();

		// Add each file to the global string
		foreach( FileInfo FileName in FileList1 )
			AllChars += GetFileAsString( FileName.ToString() );
		
		// Find the distinct characters in the global string and sort them for output
		AllChars = new string( AllChars.Distinct().ToArray() );
		char[] CharArray = AllChars.ToCharArray();
		Array.Sort( CharArray );
		AllChars = new string( CharArray );
		
		string savePath = EditorUtility.SaveFilePanel(
				"Save used font characters",
				"",
				"UsedFontChars.txt",
				"txt");

		if( savePath.Length != 0) 
		{			
			// Save file (as UNICODE so the font program can load it correctly)
			File.WriteAllBytes( savePath , System.Text.Encoding.Unicode.GetBytes( AllChars ) );
		}
	}

	//=============================================================================
	
	static string GetFileAsString( string FileName )
	{
		if( FileName.EndsWith( ".txt" ) && FileName.Contains( "_" ) )
		{
			UnityEngine.Debug.Log( FileName );
			StreamReader FileInput = new StreamReader( FileName );
			string FileString = FileInput.ReadToEnd();
			FileInput.Close();	
			
			return( FileString );
		}
		else
		{
			return( "" );
		}
	}

	//=============================================================================

	[UnityEditor.MenuItem("Tools/EM Studios/Update Localisations")]
	static void GoogleSpreadsheetsUpdateLocalisations()
	{
		UpdateXML( "Localisations_EN" , "Languages" , 2 );
		UpdateXML( "Localisations_FR" , "Languages" , 4 );
		UpdateXML( "Localisations_IT" , "Languages" , 5 );
		UpdateXML( "Localisations_DE" , "Languages" , 6 );
		UpdateXML( "Localisations_ES" , "Languages" , 7 );
		UpdateXML( "Localisations_RU" , "Languages" , 4 );
		UpdateXML( "Localisations_PT" , "Languages" , 9 );
		AssetDatabase.Refresh();
	}

	[UnityEditor.MenuItem("Tools/EM Studios/Update Settings")]
	static void GoogleSpreadsheetsUpdateSettings()
	{
		UpdateXML( "Settings" , "Settings" , 0 );
		AssetDatabase.Refresh();
	}

	[UnityEditor.MenuItem("Tools/EM Studios/Update Cards")]
	static void GoogleSpreadsheetsUpdateCards()
	{
		UpdateXML( "Cards" , "Cards" , 0 );
		AssetDatabase.Refresh();
	}

	[UnityEditor.MenuItem("Tools/EM Studios/Update Clothing")]
	static void GoogleSpreadsheetsUpdateClothing()
	{
		UpdateXML( "Clothing" , "Clothing" , 0 );
		AssetDatabase.Refresh();
	}

	[UnityEditor.MenuItem("Tools/EM Studios/Update Fairies")]
	static void GoogleSpreadsheetsUpdateFairies()
	{
		UpdateXML( "Fairies" , "Fairies" , 0 );
		AssetDatabase.Refresh();
	}

	[UnityEditor.MenuItem("Tools/EM Studios/Update WildMagic")]
	static void GoogleSpreadsheetsUpdateWildMagic()
	{
		UpdateXML( "WildMagic" , "WildMagic" , 0 );
		AssetDatabase.Refresh();
	}

	[UnityEditor.MenuItem("Tools/EM Studios/Update NPC")]
	static void GoogleSpreadsheetsUpdateNPC()
	{
		UpdateXML( "NPC" , "NPC" , 0 );
		AssetDatabase.Refresh();
	}

	//=============================================================================

	static void UpdateXML( string Name , string Dir , int LocaleColumn )
	{
		double fProgress = 0.0f;

		double startVal = EditorApplication.timeSinceStartup;
		WWW XMLUpdate = new WWW( "http://app.doodletales.co.uk/www/include/getxmlparsed_locale_wfs2.php?name=" + Name + "&localecolumn=" + LocaleColumn );
		
		double fLastProgress = fProgress;
		while( XMLUpdate.isDone == false )
		{
			fProgress = ( EditorApplication.timeSinceStartup - startVal );
			
			if( ( fProgress - fLastProgress ) > 0.0125f )
			{
				fLastProgress = fProgress;
			
				EditorUtility.DisplayProgressBar(
							"XML Update from Google (" + Name + ")",
							"Downloading...",
							(float)fProgress % 1.0f );
			}
		};
		EditorUtility.ClearProgressBar();
		
		if( String.IsNullOrEmpty( XMLUpdate.error ) )
		{
			UnityEngine.Debug.Log( "Updated XML OK: " + Name );
			
			//UnityEngine.Debug.Log( Encoding.UTF8.GetString( XMLUpdate.bytes ) );
			// Save file
			string OutputText = XMLUpdate.text;
			OutputText = OutputText.Replace( "\n" , "\r\n" );
			
			File.WriteAllBytes( Application.dataPath + "/" + "/Resources/" + Dir + "/" + Name + ".txt" , System.Text.Encoding.UTF8.GetBytes( OutputText ) ); //Encoding.UTF8.GetString( XMLUpdate.bytes ) );
		}
		else
		{
			UnityEngine.Debug.Log( "XML Updated with error: " + XMLUpdate.error );
		}
	}
	
	//=============================================================================

	[UnityEditor.MenuItem("Tools/EM Studios/Get game update ZIP file",false,11)]
	static void GetGameUpdateZIP()
	{
		double fProgress = 0.0f;

		double startVal = EditorApplication.timeSinceStartup;
		WWW XMLUpdate = new WWW( "http://app.doodletales.co.uk/www/include/getxmlparsed_zip_wfs2.php" );
		
		double fLastProgress = fProgress;
		while( XMLUpdate.isDone == false )
		{
			fProgress = ( EditorApplication.timeSinceStartup - startVal ) * 30.0f;
			//UnityEngine.Debug.Log( fProgress % 1.0f );
			
			if( ( fProgress - fLastProgress ) > 0.1f )
			{
				fLastProgress = fProgress;
			
				EditorUtility.DisplayProgressBar(
							"Get Game Update ZIP",
							"Downloading... (can be slow!!)",
							(float)fProgress % 17.0f );
			}
		};
		EditorUtility.ClearProgressBar();
		
		if( String.IsNullOrEmpty(XMLUpdate.error) )
		{
			UnityEngine.Debug.Log( "Updated WFS2.zip OK" );
			
			// Find checksum of file (simple, just for checking validity later)
			uint Checksum = HelperChecksum.GetChecksum( XMLUpdate.bytes );
			
			string savePath = EditorUtility.SaveFilePanel(
					"Save game ZIP",
					"",
					"wfs2_" + Checksum.ToString("X") + ".zip",
					"zip");

			if( savePath.Length != 0) 
			{			
				// Save file
				File.WriteAllBytes( savePath , XMLUpdate.bytes );
			}
		}
		else
		{
			UnityEngine.Debug.Log( "ZIP Downloaded with error: " + XMLUpdate.error );
		}
	}

	//=============================================================================

	[UnityEditor.MenuItem("Tools/EM Studios/Update ZIP checksum",false,11)]
	static void UpdateZIPChecksum()
	{
		// Open zip file
		string loadPath = EditorUtility.OpenFilePanel(
				"Load game ZIP",
				"",
				"zip");

		if( loadPath.Length != 0) 
		{			
			// Load zip
			if( System.IO.File.Exists( loadPath ) )
			{
				FileStream fs = new FileStream( loadPath , FileMode.Open );
				BinaryReader r = new BinaryReader( fs );

				int ZIPSize = (int)fs.Length;
				
				byte[] SourceDataPtr;

				SourceDataPtr = r.ReadBytes( ZIPSize );
				uint Checksum = HelperChecksum.GetChecksum( SourceDataPtr );
				
				EditorUtility.DisplayDialog( "ZIP Checksum" , "New ZIP file checksum is wfs2_" + Checksum.ToString("X") + ".zip" , "OK" , "Cancel" ); 
				//UnityEngine.Debug.Log( Checksum.ToString("X") );

				string savePath = EditorUtility.SaveFilePanel(
						"Save updated game ZIP",
						"",
						"wfs2_" + Checksum.ToString("X") + ".zip",
						"zip");

				if( savePath.Length != 0) 
				{			
					// Save file
					File.WriteAllBytes( savePath , SourceDataPtr );
				}
				
				r.Close();
				fs.Close();		
			}
		}
	}

	//=============================================================================
	
}
