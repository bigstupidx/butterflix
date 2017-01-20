using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class PlayerPrefsWrapper
{
#if UNITY_XBOXONE
	static Dictionary<string,float> cachedFloatProps = new Dictionary<string, float>();
	static Dictionary<string,string> cachedStringProps = new Dictionary<string, string>();
	static Dictionary<string,int> cachedIntProps = new Dictionary<string, int>();
	
	//==================================================================

	public static float GetFloat(string prefName, float defaultValue=0.0f)
	{
		if (!cachedFloatProps.ContainsKey(prefName))
		{
			cachedFloatProps.Add(prefName, PlayerPrefs.GetFloat(prefName, defaultValue));
		}
		return cachedFloatProps[prefName];
	}

	//==================================================================

	public static void SetFloat(string prefName, float newValue)
	{
		if (!cachedFloatProps.ContainsKey(prefName))
		{
			cachedFloatProps.Add(prefName, newValue);
		}
		else
		{
			cachedFloatProps[prefName] = newValue;
		}
	}

	//==================================================================

	public static string GetString(string prefName, string defaultValue="")
	{
		if (!cachedStringProps.ContainsKey(prefName))
		{
			cachedStringProps.Add(prefName, PlayerPrefs.GetString(prefName, defaultValue));
		}
		return cachedStringProps[prefName];
	}

	//==================================================================

	public static void SetString(string prefName, string newValue)
	{
		if (!cachedStringProps.ContainsKey(prefName))
		{
			cachedStringProps.Add(prefName, newValue);
		}
		else
		{
			cachedStringProps[prefName] = newValue;
		}
	}

	//==================================================================

	public static int GetInt(string prefName, int defaultValue=0)
	{
		if (!cachedIntProps.ContainsKey(prefName))
		{
			cachedIntProps.Add(prefName, PlayerPrefs.GetInt(prefName, defaultValue));
		}
		return cachedIntProps[prefName];
	}

	//==================================================================

	public static void SetInt(string prefName, int newValue)
	{
		if (!cachedIntProps.ContainsKey(prefName))
		{
			cachedIntProps.Add(prefName, newValue);
		}
		else
		{
			cachedIntProps[prefName] = newValue;
		}
	}

	//==================================================================

	public static bool HasKey(string prefName)
	{
		if (cachedIntProps.ContainsKey(prefName))
			return( true );
		if (cachedFloatProps.ContainsKey(prefName))
			return( true );
		if (cachedStringProps.ContainsKey(prefName))
			return( true );
		
		return( false );
	}

	//==================================================================

	public static void DeleteKey(string prefName)
	{
		if (cachedIntProps.ContainsKey(prefName))
			cachedIntProps.Remove(prefName);
		if (cachedFloatProps.ContainsKey(prefName))
			cachedFloatProps.Remove(prefName);
		if (cachedStringProps.ContainsKey(prefName))
			cachedStringProps.Remove(prefName);
	}

	//==================================================================
#else
	//==================================================================

	public static float GetFloat( string prefName, float defaultValue = 0.0f )
	{
		return (PlayerPrefs.GetFloat( prefName, defaultValue ));
	}

	//==================================================================

	public static void SetFloat( string prefName, float newValue )
	{
		PlayerPrefs.SetFloat( prefName, newValue );
	}

	//==================================================================

	public static string GetString( string prefName, string defaultValue = "" )
	{
		return (PlayerPrefs.GetString( prefName, defaultValue ));
	}

	//==================================================================

	public static void SetString( string prefName, string newValue )
	{
		PlayerPrefs.SetString( prefName, newValue );
	}

	//==================================================================

	public static int GetInt( string prefName, int defaultValue = 0 )
	{
		return (PlayerPrefs.GetInt( prefName, defaultValue ));
	}

	//==================================================================

	public static void SetInt( string prefName, int newValue )
	{
		PlayerPrefs.SetInt( prefName, newValue );
	}

	//==================================================================

	public static bool HasKey( string prefName )
	{
		return (PlayerPrefs.HasKey( prefName ));
	}

	//==================================================================

	public static void DeleteKey( string prefName )
	{
		PlayerPrefs.DeleteKey( prefName );
	}

	//==================================================================

	public static double GetDouble( string prefName, double defaultValue = 0d )
	{
		var defaultVal = DoubleToString( defaultValue );

		return StringToDouble( PlayerPrefs.GetString( prefName, defaultVal ) );
	}

	//==================================================================

	public static void SetDouble( string prefName, double newValue )
	{
		PlayerPrefs.SetString( prefName, DoubleToString( newValue ) );
	}

	//==================================================================

	private static string DoubleToString( double target )
	{
		return target.ToString( "R" );
	}

	//==================================================================

	private static double StringToDouble( string target )
	{
		return string.IsNullOrEmpty( target ) ? 0d : double.Parse( target );
	}

	//==================================================================
#endif
}