using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class EveryplayWrapper
{
	//=============================================================================

	public static bool IsRecordingSupported()
	{
		#if UNITY_IPHONE || UNITY_ANDROID
		return( Everyplay.IsRecordingSupported() );
		#else
		return( false );
		#endif
	}
	
	//=============================================================================

	public static void StartRecording()
	{
		#if UNITY_IPHONE || UNITY_ANDROID
		if( Everyplay.IsRecordingSupported() )
		{
			if( Everyplay.IsRecording() == true )
				Everyplay.StopRecording();

			//if( PlayerManager.EveryplayOn )
			{
				Everyplay.SetMaxRecordingMinutesLength( 1 );
				Everyplay.StartRecording();
			}
		}
		else
		{
			Debug.Log( "Everyplay recording not supported" );
		}
		#endif
	}
	
	//=============================================================================

	public static void StopRecording()
	{
		#if UNITY_IPHONE || UNITY_ANDROID
		if( Everyplay.IsRecordingSupported() )
		{
			if( Everyplay.IsRecording() == true )
				Everyplay.StopRecording();
		}
		else
		{
			Debug.Log( "Everyplay recording not supported" );
		}
		#endif
	}
	
	//=============================================================================

	public static void PauseRecording()
	{
		#if UNITY_IPHONE || UNITY_ANDROID
		if( Everyplay.IsRecordingSupported() )
		{
			if( Everyplay.IsRecording() == true )
				Everyplay.PauseRecording();
		}
		else
		{
			Debug.Log( "Everyplay recording not supported" );
		}
		#endif
	}
	
	//=============================================================================

	public static void ResumeRecording()
	{
		#if UNITY_IPHONE || UNITY_ANDROID
		if( Everyplay.IsRecordingSupported() )
		{
			Everyplay.ResumeRecording();
		}
		else
		{
			Debug.Log( "Everyplay recording not supported" );
		}
		#endif
	}
	
	//=============================================================================

	public static void ShareRecording( string CharacterName )
	{
		#if UNITY_IPHONE || UNITY_ANDROID
		if( Everyplay.IsRecordingSupported() )
		{
			//if( PlayerManager.EveryplayOn )
			{
				// Set distance metadata
				Dictionary<string,object> dict = new Dictionary<string,object>();

				dict[ "Character" ] = CharacterName;
				Everyplay.SetMetadata( dict );
				Everyplay.ShowSharingModal();
			}
		}
		else
		{
			Debug.Log( "Everyplay recording not supported" );
		}
		#endif
    }

	//=============================================================================

	public static void ShowHomepage()
	{
		#if UNITY_IPHONE || UNITY_ANDROID
		if( Everyplay.IsRecordingSupported() )
		{
			Everyplay.Show();
		}
		else
		{
			Debug.Log( "Everyplay recording not supported" );
		}
		#endif
	}

	//=============================================================================

	public static void ShowHomepageWithPath( string CurPath )
	{
		#if UNITY_IPHONE || UNITY_ANDROID
		if( Everyplay.IsRecordingSupported() )
		{
			Everyplay.ShowWithPath( CurPath );
		}
		else
		{
			Debug.Log( "Everyplay recording not supported" );
		}
		#endif
	}

	//=============================================================================
}
