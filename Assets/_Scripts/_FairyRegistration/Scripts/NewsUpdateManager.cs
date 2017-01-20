using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tsumanga;


public class NewsUpdateManager : MonoBehaviour 
{
	public static NewsUpdateManager	instance				= null;
	
	
	private	float	m_UpdateTimer = 5.0f; //( 60.0f * 3.0f );
	private	WWW		m_NewsRequestWWW = null;
	
	//=============================================================================
	
	void Start()
	{
		
	}
	
	//=============================================================================

	void Awake()
	{
		instance = this;
		
        Init();
		DontDestroyOnLoad( transform.gameObject );
	}
	
	//=============================================================================

	public void Init()
	{
	}

	//=============================================================================
	
	void Update()
	{
		m_UpdateTimer -= Time.deltaTime;
		if( m_UpdateTimer < 0.0f )
		{
			// Attempt to get update info on live data
			m_UpdateTimer = ( 60.0f * 10.0f );
			m_NewsRequestWWW = new WWW( "https://s3-eu-west-1.amazonaws.com/butterflix-liveupdates/newsinfo.txt?r=" + UnityEngine.Random.Range( 1000,999999 ) );
		}
		
		// Check if web request for news info has completed
		if( m_NewsRequestWWW != null )
		{
			if( m_NewsRequestWWW.isDone == true )
			{
				if( String.IsNullOrEmpty( m_NewsRequestWWW.error ) )
				{
					UnityEngine.Debug.Log( "Update news flash OK" );
					
					SetupNewsFlash( m_NewsRequestWWW.text );
				}
				else
				{
					UnityEngine.Debug.Log( "Update news flash failed" );
				}
				m_NewsRequestWWW.Dispose();
				m_NewsRequestWWW = null;
			}
		}
	}
	
	//=============================================================================
	
	void SetupNewsFlash( string InputText )
	{
		JSON Decoder = new JSON();
		Decoder.serialized = InputText;
		
		JSON NewsDataJSON = Decoder.ToJSON("newsData");
		
		int PopupVersion = NewsDataJSON.ToInt( "version" );
		int LiveDataVersion = NewsDataJSON.ToInt( "liveDataVersion" );
		int CouponCodesActive = NewsDataJSON.ToInt( "couponCodesActive" );
		PlayerPrefs.SetInt( "couponCodesActive" , CouponCodesActive );
		
		// Do we need to download live data?
		if( LiveDataVersion > PlayerPrefs.GetInt( "LiveDataVersion" , PreHelpers.GetLiveDataVersion() ) )
		{
			Debug.Log( "Updating live data!" );
			//PlayerPrefsWrapper.SetInt( "LiveDataVersion" , LiveDataVersion );
			StartCoroutine( GetLiveData( LiveDataVersion , NewsDataJSON.ToString("liveDataURL") ) );
		}
		else
		{
			Debug.Log( "Don't need to update live data - our version:" + PlayerPrefs.GetInt( "LiveDataVersion" , PreHelpers.GetLiveDataVersion() ) + " remote version:" + LiveDataVersion );
		}
	}
	
	//=============================================================================

	IEnumerator GetLiveData( int Version , string URL )
	{
		WWW GameDataWWW = new WWW( URL );
		yield return GameDataWWW;
		
		uint NewGameDataChecksum = 0;
		
		// Find checksum
		int ChecksumIndex = URL.IndexOf( "wfs2_" );
		if( ChecksumIndex != -1 )
		{
			try
			{
				NewGameDataChecksum = uint.Parse( URL.Substring( ChecksumIndex + 5 , 8 ) , System.Globalization.NumberStyles.HexNumber );
			}
			catch
			{
				NewGameDataChecksum = 0;
			}
		}
		
		if( String.IsNullOrEmpty( GameDataWWW.error ) && ( GameDataWWW.bytes != null ) )
		{
			UnityEngine.Debug.Log( "Updated Game Data OK" );
			
			// Make sure checksum matches
			uint Checksum = HelperChecksum.GetChecksum( GameDataWWW.bytes );
			if( Checksum == NewGameDataChecksum )
			{
				// Save file
				string DocPath = PreHelpers.GetFileFolderPath();
				string FilePath = DocPath + "wfs2.zip";

				// Write file
				FileStream fs = null;
				try
				{
					fs = new FileStream( FilePath , FileMode.Create );
				}
				catch
				{
					Debug.Log( "GameData file creation exception: " + FilePath );
				}
				
				if( fs != null )
				{
					PlayerPrefs.SetInt( "LiveDataVersion" , Version );

					BinaryWriter CurFile = new BinaryWriter(fs);
					CurFile.Write( GameDataWWW.bytes );

					// Close file
					CurFile.Close();
					fs.Close();
					
					// Unzip into local data files
					//PreHelpers.LoadGameDataUpdate();
				}
			}
			else
			{
				UnityEngine.Debug.Log( "Game Data update checksum mismatch: " + Checksum.ToString("X") + " " + NewGameDataChecksum.ToString("X") );
			}
		}
		else
		{
			UnityEngine.Debug.Log( "Game Data updated error: " + GameDataWWW.error );
		}
	}

	//=============================================================================
}
