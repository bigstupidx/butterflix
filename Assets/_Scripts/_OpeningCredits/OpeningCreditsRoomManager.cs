using UnityEngine;
using System;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

public class OpeningCreditsRoomManager : MonoBehaviour
{
	//=====================================================
	
	public	GameObject	m_Screen1;
	public	GameObject	m_Screen2;
	public	GameObject	m_Screen3;

	public	GameObject	m_SceneSettingEN;
	public	GameObject	m_SceneSettingIT;
	public	GameObject	m_SceneSettingRU;
	public	GameObject	m_SceneSettingDE;

	private	bool 		m_bFadeInComplete = false;
	private	bool 		m_bFadingOut = false;
	private	float		m_Timer = 0.0f;
	private	int			m_ScreenIndex = 0;

	void Awake()
	{
		m_bFadeInComplete = false;
		m_bFadingOut = false;
		m_Timer = 0.0f;
		m_ScreenIndex = 0;
		m_Screen1.SetActive( true );
		m_Screen2.SetActive( false );
		m_Screen3.SetActive( false );

		#if UNITY_ANDROID
		this.SetupAndroidTrust();
		#endif
	}

	void SetupAndroidTrust()
	{
		//content of your *.crt file
		string cert1 =
			@"-----BEGIN CERTIFICATE-----
MIIFUzCCBDugAwIBAgIRANb8oaG6yEz/3uOXUFMDv4wwDQYJKoZIhvcNAQELBQAw
gZAxCzAJBgNVBAYTAkdCMRswGQYDVQQIExJHcmVhdGVyIE1hbmNoZXN0ZXIxEDAO
BgNVBAcTB1NhbGZvcmQxGjAYBgNVBAoTEUNPTU9ETyBDQSBMaW1pdGVkMTYwNAYD
VQQDEy1DT01PRE8gUlNBIERvbWFpbiBWYWxpZGF0aW9uIFNlY3VyZSBTZXJ2ZXIg
Q0EwHhcNMTYwNDIxMDAwMDAwWhcNMTkwNDMwMjM1OTU5WjBcMSEwHwYDVQQLExhE
b21haW4gQ29udHJvbCBWYWxpZGF0ZWQxHjAcBgNVBAsTFUVzc2VudGlhbFNTTCBX
aWxkY2FyZDEXMBUGA1UEAwwOKi50c3VtYW5nYS5uZXQwggEiMA0GCSqGSIb3DQEB
AQUAA4IBDwAwggEKAoIBAQC6/WNbbmEV3EFmxTTfracW4xFMKLZcTzypQen/CXqE
BNADjMvGp5SzDQocMtcblOgckcqo5wzCUqj6TvcgiPLpOZYcScefmzE8vTRYpxzs
NARtyttV5T5er47xN4J7Ljl3Awxyo/8/XcjkLM5rl0oluq00czeHbZT/CWniuk8E
RuZVLMxgadrhMEnMif1belyPiKVEo0l9hI3Zw7+Z2B86DPiw6N9AYAyQrl+mjs0W
mU1/q4x1GBMFG6FNaw+CRtWm3d+n0Ke5ubboHB1lLbVJYAwSKuouJorS/sReZB/j
U5chuUuZoT717jAvzbHtOeMuFJBtDVwV3DaioJqVxIDTAgMBAAGjggHZMIIB1TAf
BgNVHSMEGDAWgBSQr2o6lFoL2JDqElZz30O0Oija5zAdBgNVHQ4EFgQUYw9HgovH
qxEiPMyeklstNA9t3t0wDgYDVR0PAQH/BAQDAgWgMAwGA1UdEwEB/wQCMAAwHQYD
VR0lBBYwFAYIKwYBBQUHAwEGCCsGAQUFBwMCME8GA1UdIARIMEYwOgYLKwYBBAGy
MQECAgcwKzApBggrBgEFBQcCARYdaHR0cHM6Ly9zZWN1cmUuY29tb2RvLmNvbS9D
UFMwCAYGZ4EMAQIBMFQGA1UdHwRNMEswSaBHoEWGQ2h0dHA6Ly9jcmwuY29tb2Rv
Y2EuY29tL0NPTU9ET1JTQURvbWFpblZhbGlkYXRpb25TZWN1cmVTZXJ2ZXJDQS5j
cmwwgYUGCCsGAQUFBwEBBHkwdzBPBggrBgEFBQcwAoZDaHR0cDovL2NydC5jb21v
ZG9jYS5jb20vQ09NT0RPUlNBRG9tYWluVmFsaWRhdGlvblNlY3VyZVNlcnZlckNB
LmNydDAkBggrBgEFBQcwAYYYaHR0cDovL29jc3AuY29tb2RvY2EuY29tMCcGA1Ud
EQQgMB6CDioudHN1bWFuZ2EubmV0ggx0c3VtYW5nYS5uZXQwDQYJKoZIhvcNAQEL
BQADggEBAA2lIVSBFJD3jHHp6jomcyT86W8ohiV9cO0Ti/xO/SUJ73FJ+AE6MPK4
vlgHarnIn7AG2Wd2vOWDO1Gcd87mBebAp99a0l8mnAd/6xKVOZXde/k816LuYUZb
5vCDNw52HyIIkiiR0J61xwShRzKfUQDlCQCuEc6lqsM4HmFzj+500Eh5Hc+qUag6
YSsNQ+EZMjFg63FSS+RrC4mgMMH/PSGVTC99Z09Uxfm55G661ZK/AcY9ujEFQk5G
QdG1SDo8vaelmxAtYTEKvLx1apT8c/w0fRVPrPVXCR2mNLab2brus27ceKDE3g9A
ap/S3Q2nFdhqrjNjwALexkO/MEpjdgM=
-----END CERTIFICATE-----";

		AndroidHttpsHelper.AddCertificate(cert1);
		AndroidHttpsHelper.IgnoreCertificates();
	}

	//=====================================================
	
	void Start()
	{
		// Unpack any live data updates present in the local files folder
		LoadLiveUpdate();
		
		// Set opening scene language
		switch( PreHelpers.GetLanguageCode() )
		{
			default:
				m_SceneSettingEN.SetActive( true );
				break;
			case "it":
				m_SceneSettingIT.SetActive( true );
				break;
			case "ru":
				m_SceneSettingRU.SetActive( true );
				break;
			case "de":
				m_SceneSettingDE.SetActive( true );
				break;
		}
		
		ScreenManager.FadeInCompleteEvent += OnFadeInCompleteEvent;
		ScreenManager.FadeOutCompleteEvent += OnFadeOutCompleteEvent;
		ScreenManager.FadeIn();
	}

	//=====================================================
	
	void OnFadeInCompleteEvent()
	{
		m_bFadeInComplete = true;
	}
	
	//=====================================================

	void OnFadeOutCompleteEvent()
	{
		m_ScreenIndex++;
		switch( m_ScreenIndex )
		{
			case 1:
				m_Screen1.SetActive( false );
				m_Screen2.SetActive( true );
				m_Screen3.SetActive( false );
				m_bFadingOut = false;
				m_Timer = 0.0f;
				ScreenManager.FadeIn();
				m_bFadeInComplete = false;
				break;
			
			case 2:
				m_Screen1.SetActive( false );
				m_Screen2.SetActive( false );
				m_Screen3.SetActive( true );
				m_bFadingOut = false;
				m_Timer = 0.0f;
				ScreenManager.FadeIn();
				m_bFadeInComplete = false;
				break;
			
			case 3:
				ScreenManager.FadeInCompleteEvent -= OnFadeInCompleteEvent;
				ScreenManager.FadeOutCompleteEvent -= OnFadeOutCompleteEvent;

				// If not registered then load the registration scene
				if( ServerManager.Registered == false )
				{
					Application.LoadLevel( "RefectoryRoom" );
				}
				else
				{
					// Load the tutorial or main hall scene
					if( PlayerPrefsWrapper.HasKey( "IsTutorialCompleted" ) && PlayerPrefsWrapper.GetInt( "IsTutorialCompleted" ) != 0 )
						Application.LoadLevel( "MainHall" );
					else
						Application.LoadLevel( "Tutorial" );
				}
				break;
		}
	}
	
	//=====================================================

	void Update()
	{
		m_Timer += Time.deltaTime;
		
		if( m_bFadeInComplete )
		{
			if( m_bFadingOut == false )
			{
				if( Input.GetMouseButtonDown( 0 ) || ( m_Timer > 3.5f ) )
				{
					ScreenManager.FadeOut();
					m_bFadingOut = true;
				}
			}
		}
	}
	
	//=====================================================
	
	void LoadLiveUpdate()
	{
		string DocPath = PreHelpers.GetFileFolderPath();
		string FilePath = DocPath + "wfs2.zip";

		if( System.IO.File.Exists( FilePath ) == false )
			return;

		// Read file
		FileStream fs = null;
		try
		{
			fs = new FileStream( FilePath , FileMode.Open );
		}
		catch
		{
			Debug.Log( "GameData file open exception: " + FilePath );
		}
		
		if( fs != null )
		{
			try
			{
				// Read zip file
				ZipFile zf = new ZipFile(fs);
				int numFiles = 0;
				
				if( zf.TestArchive( true ) == false )
				{
					Debug.Log( "Zip file failed integrity check!" );
					zf.IsStreamOwner = false;
					zf.Close();
					fs.Close();
				}
				else
				{
					foreach( ZipEntry zipEntry in zf ) 
					{
						// Ignore directories
						if( !zipEntry.IsFile ) 
							continue;           
						
						String entryFileName = zipEntry.Name;
						
						// Skip .DS_Store files
						if( entryFileName.Contains( "DS_Store" ) )
							continue;
						
						Debug.Log( "Unpacking zip file entry: " + entryFileName );
						
						byte[] buffer = new byte[ 4096 ];     // 4K is optimum
						Stream zipStream = zf.GetInputStream(zipEntry);

						// Manipulate the output filename here as desired.
						string fullZipToPath = PreHelpers.GetFileFolderPath() + Path.GetFileName( entryFileName );

						// Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
						// of the file, but does not waste memory.
						// The "using" will close the stream even if an exception occurs.
						using (FileStream streamWriter = System.IO.File.Create(fullZipToPath)) 
						{
							StreamUtils.Copy(zipStream, streamWriter, buffer);
						}
						numFiles++;
					}

					zf.IsStreamOwner = false;
					zf.Close();
					fs.Close();
					
					// If we've unpacked the local files (at least 13) then start using local spreadsheets data
					Debug.Log( "Zip updated with " + numFiles + " files (needs 13)" );
					if( numFiles >= 13 )
					{
						PlayerPrefs.SetInt(PreHelpers.GetLiveDataVersionString(), 1);
					}
					
					// Reload managers
					TextManager.Reload();
					SettingsManager.Reload();
					TradingCardItemsManager.Reload();
					ClothingItemsManager.Reload();
					FairyItemsManager.Reload();
					WildMagicItemsManager.Reload();
					NPCItemsManager.Reload();
				}
			}
			catch
			{
				Debug.Log( "Zip file error!" );
			}

			// Remove zip file 
			Debug.Log( "Removing zip file" );
			System.IO.File.Delete( FilePath );
		}
	}
}
