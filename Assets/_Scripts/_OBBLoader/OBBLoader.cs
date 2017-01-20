using UnityEngine;
using System;
using System.IO;
using System.Collections;

public class OBBLoader : MonoBehaviour 
{
	//private bool isGooglePlayDownloaderOnAndroid	= true;
	#if UNITY_ANDROID && ANDROID_GOOGLE
	private bool isFetchingOBBs						= false;
	#endif
	
	private bool isOkToLoadLevel					= false;
	private bool isLoadingLevel						= false;

	//=============================================================================
	void Awake()
	{
	}
	void OnAwake()
	{
		#if UNITY_ANDROID && ANDROID_GOOGLE
		isFetchingOBBs = false;
		#endif

		isOkToLoadLevel = false;
		isLoadingLevel = false;
	}

	//=============================================================================

	void OnGUI()
	{
#if UNITY_ANDROID
	#if ANDROID_GOOGLE
		if(! isOkToLoadLevel)
		{
			if (!GooglePlayDownloader.RunningOnAndroid())
			{
				GUI.Label(new Rect(10, 10, Screen.width-10, 20), "Use GooglePlayDownloader only on Android device!");
				return;
			}
			
			string expPath = GooglePlayDownloader.GetExpansionFilePath();
			if (expPath == null)
			{
				GUI.Label(new Rect(10, 10, Screen.width-10, 20), "External storage is not available!");
			}
			else
			{
				string mainPath = GooglePlayDownloader.GetMainOBBPath(expPath);
				//string patchPath = GooglePlayDownloader.GetPatchOBBPath(expPath);

				if(mainPath == null)	// || patchPath == null)
				{
					if(!isFetchingOBBs)
					{
						Debug.Log("Main = ..."  + (mainPath == null ? " NOT AVAILABLE" : mainPath.Substring(expPath.Length)));
						//Debug.Log("Patch = ..." + (patchPath == null ? " NOT AVAILABLE" : patchPath.Substring(expPath.Length)));
						Debug.Log("Fetching OBB ...");

						//Camera.main.backgroundColor = new Color( 0.0f , 0.0f , 0.0f , 1.0f );
						//Camera.main.clearFlags = CameraClearFlags.SolidColor;
						
						GooglePlayDownloader.FetchOBB();
						isFetchingOBBs = true;
					}
				}
				else
				{
					Debug.Log("Main = ..."  + (mainPath == null ? " NOT AVAILABLE" : mainPath.Substring(expPath.Length)));
					//Debug.Log("Patch = ..." + (patchPath == null ? " NOT AVAILABLE" : patchPath.Substring(expPath.Length)));

					isOkToLoadLevel = true;
				}
			}
		}
	#else
		isOkToLoadLevel = true;
	#endif
#else
		isOkToLoadLevel = true;
#endif
		if(isOkToLoadLevel && !isLoadingLevel)
		{
			if( (string.Empty != Application.dataPath) && ("" != Application.dataPath) )
			{
				Debug.Log(Application.dataPath);
				Debug.Log("Loading level: OpeningCreditsRoom");

				Application.LoadLevel( "OpeningCreditsRoom" );
				//isLoadingLevel = true;

				//DummyActivity();
			}
			else
			{
				Debug.Log("App datapath is empty!");
			}

			isLoadingLevel = true;
		}
	}

	//=============================================================================

/*	void OnApplicationFocus(bool focusStatus)
	{
		Debug.Log("OnApplicationFocus: " + ((focusStatus) ? "TRUE" : "FALSE" ));
		if(focusStatus)
		{
			Application.LoadLevel( "Intro" );
			//isLoadingLevel = true;
		}
	}
*/
	//=============================================================================

/*	private void DummyActivity()
	{
		using (AndroidJavaClass unity_player = new AndroidJavaClass("com.unity3d.player.UnityPlayer") )
		{
			AndroidJavaObject current_activity = unity_player.GetStatic<AndroidJavaObject>("currentActivity");
			
			AndroidJavaObject intent = current_activity.Call<AndroidJavaObject>("getIntent");
			int Intent_FLAG_ACTIVITY_NO_ANIMATION = 0x10000;
			intent.Call<AndroidJavaObject>("addFlags", Intent_FLAG_ACTIVITY_NO_ANIMATION);
			
			//current_activity.Call("finish");
			
			current_activity.Call("startActivity", intent);
			
			if (AndroidJNI.ExceptionOccurred() != System.IntPtr.Zero)
			{
				Debug.LogError("Exception occurred while attempting to start activity - is the AndroidManifest.xml incorrect?");
				AndroidJNI.ExceptionDescribe();
				AndroidJNI.ExceptionClear();
			}
		}
	}
*/
	//=============================================================================

/*	void OnGUI()
	{
		if (!GooglePlayDownloader.RunningOnAndroid())
		{
			GUI.Label(new Rect(10, 10, Screen.width-10, 20), "Use GooglePlayDownloader only on Android device!");
			return;
		}
		
		string expPath = GooglePlayDownloader.GetExpansionFilePath();
		if (expPath == null)
		{
			GUI.Label(new Rect(10, 10, Screen.width-10, 20), "External storage is not available!");
		}
		else
		{
			string mainPath = GooglePlayDownloader.GetMainOBBPath(expPath);
			string patchPath = GooglePlayDownloader.GetPatchOBBPath(expPath);
			
			GUI.Label(new Rect(10, 10, Screen.width-10, 20), "Main = ..."  + ( mainPath == null ? " NOT AVAILABLE" :  mainPath.Substring(expPath.Length)));
			GUI.Label(new Rect(10, 25, Screen.width-10, 20), "Patch = ..." + (patchPath == null ? " NOT AVAILABLE" : patchPath.Substring(expPath.Length)));
			if (mainPath == null || patchPath == null)
				if (GUI.Button(new Rect(10, 100, 100, 100), "Fetch OBBs"))
					GooglePlayDownloader.FetchOBB();
		}	
	}
	*/
}
