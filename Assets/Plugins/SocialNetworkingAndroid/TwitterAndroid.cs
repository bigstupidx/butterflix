using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Prime31;


#if UNITY_ANDROID

namespace Prime31
{
	public class TwitterAndroid
	{
		private static AndroidJavaObject _plugin;


		static TwitterAndroid()
		{
			if( Application.platform != RuntimePlatform.Android )
				return;

			// find the plugin instance
			using( var pluginClass = new AndroidJavaClass( "com.prime31.TwitterPlugin" ) )
				_plugin = pluginClass.CallStatic<AndroidJavaObject>( "instance" );
		}


		// Sets up the Twitter Plugin for use.  When it is ready for use the twitterInitializedEvent will fire. Note that if you will be using an external browser for authentication you should
		// pass in your own custom callbackUrlScheme. Additionally you will need to manually add the proper intent-filter to your AndroidManifest.xml file. See the documentation for more
		// information.
		public static void init( string consumerKey, string consumerSecret, string callbackUrlScheme = "twitterplugin" )
		{
			if( Application.platform != RuntimePlatform.Android )
				return;

			_plugin.Call( "init", consumerKey, consumerSecret, callbackUrlScheme );
		}


		// Checks to see if a user is logged in
		public static bool isLoggedIn()
		{
			if( Application.platform != RuntimePlatform.Android )
				return false;

			return _plugin.Call<bool>( "isLoggedIn" );
		}


		// Fetches the users access token
		public static string getAccessToken()
		{
			if( Application.platform != RuntimePlatform.Android )
				return string.Empty;

			return _plugin.Call<string>( "getAccessToken" );
		}


		// Fetches the users token secret
		public static string getTokenSecret()
		{
			if( Application.platform != RuntimePlatform.Android )
				return string.Empty;

			return _plugin.Call<string>( "getTokenSecret" );
		}


		// Shows the login dialog optionally using an external brwoser instead of the in-app WebView
		public static void showLoginDialog( bool useExternalBrowserForAuthentication = false )
		{
			if( Application.platform != RuntimePlatform.Android )
				return;

			_plugin.Call( "showLoginDialog", useExternalBrowserForAuthentication );
		}


		// Logs the user out
		public static void logout()
		{
			if( Application.platform != RuntimePlatform.Android )
				return;

			_plugin.Call( "logout" );
		}


		// Posts an update to the users timeline
		public static void postStatusUpdate( string status )
		{
			var param = new Dictionary<string, string>()
			{ { "status", status } };

			performRequest( "post", "/1.1/statuses/update.json", param );
		}


		// Posts an update with an image. Please note that the image will add ~22 characters to the status update
		public static void postStatusUpdate( string update, byte[] image )
		{
			if( Application.platform != RuntimePlatform.Android )
				return;

			_plugin.Call( "postUpdateWithImage", update, image );

			// alternative method
			//var postImageMethod = AndroidJNI.GetMethodID( _plugin.GetRawClass(), "postUpdateWithImage", "([Ljava/lang/String;B)V" );
			//AndroidJNI.CallObjectMethod( _plugin.GetRawObject(), postImageMethod, AndroidJNIHelper.CreateJNIArgArray( new object[] { update, image } ) );
		}


		// Gets the users home timeline
		public static void getHomeTimeline()
		{
			performRequest( "get", "/1.1/statuses/home_timeline.json", null );
		}


		// Gets a list of the users followers
		public static void getFollowers()
		{
			performRequest( "get", "/1.1/followers/ids.json", null );
		}


		// Performs a request for any available Twitter API methods.  methodType must be either "get" or "post".  path is the
		// url fragment from the API docs (excluding https://api.twitter.com) and parameters is a dictionary of key/value pairs
		// for the given method.  See Twitter's API docs for all available methods
		public static void performRequest( string methodType, string path, Dictionary<string,string> parameters )
		{
			if( Application.platform != RuntimePlatform.Android )
				return;

			string param = parameters != null ? parameters.toJson() : string.Empty;
			_plugin.Call( "performRequest", methodType, path, param );
		}

	}

}
#endif
