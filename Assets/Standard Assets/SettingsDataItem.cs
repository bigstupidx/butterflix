using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SettingsDataItemValuePair
{
	public int					fairyLevel;
	public string				value;
	
	public SettingsDataItemValuePair( int _fairyLevel , string _value )
	{
		fairyLevel = _fairyLevel;
		value = _value;
	}
}

//==================================================================

public class SettingsDataItem 
{
	public string								gameSetting;
	public List< SettingsDataItemValuePair >	values = new List< SettingsDataItemValuePair >();
	
	public SettingsDataItem()
	{
		gameSetting				= string.Empty;
		values.Clear();
	}
	
	//==================================================================

	public SettingsDataItemValuePair GetValuePair( int _fairyLevel )
	{
		foreach( SettingsDataItemValuePair Pair in values )
		{
			if( Pair.fairyLevel == _fairyLevel )
				return Pair;
		}
		return( null );
	}

	//==================================================================
	
	public void AddValue( int _fairyLevel , string _value )
	{
		foreach( SettingsDataItemValuePair Pair in values )
		{
			if( Pair.fairyLevel == _fairyLevel )
				return;
		}
		
		values.Add( new SettingsDataItemValuePair( _fairyLevel , _value ) );
	}
}
