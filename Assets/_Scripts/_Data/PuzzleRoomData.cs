using UnityEngine;

[System.Serializable]
public class PuzzleRoomData
{
	public eLocation Location { get; set; }
	public int[] Keys { get; set; }

	//=====================================================

	public PuzzleRoomData()
	{
		Location = eLocation.NULL;
		Keys = null;
	}

	//=====================================================

	public PuzzleRoomData( eLocation location, int numKeys )
	{
		Location = location;
		Keys = new int[numKeys];
	}

	//=====================================================

	public void ResetNumKey( int numKeys )
	{
		Keys = new int[numKeys];
	}

	//=====================================================

	public PuzzleRoomData Copy()
	{
		var obj = new PuzzleRoomData
		{
			Location = Location,
			Keys = Keys
		};

		return obj;
	}

	//=====================================================

	public void ResetKeys()
	{
		for( var i = 0; i < Keys.Length; i++ )
			Keys[i] = 0;
	}

	//=====================================================

	public static implicit operator JSON( PuzzleRoomData data )
	{
		// Write JSON 
		var js = new JSON();

		js["location"] = (int)data.Location;

		if( data.Keys != null && data.Keys.Length > 0 )
			js["keys"] = data.Keys;

		//if( data.m_Rotation != 0 ) js["rotation"] = data.m_Rotation;
		//if( ((int)data.m_SelectionType) != 0 ) js["selectionType"] = (int)data.m_SelectionType;

		return (js);
	}

	// //=====================================================

	public static explicit operator PuzzleRoomData( JSON obj )
	{
		// Read JSON
		var newObj = new PuzzleRoomData();
		var bVerbose = false;

		newObj.Location = (eLocation)obj.ToInt( "location" );
		if( bVerbose ) Debug.Log( "location: " + newObj.Location );

		newObj.Keys = obj.ToArray<int>( "keys" );
		if( bVerbose ) Debug.Log( "keys: " + newObj.Keys );

		return (newObj);
	}

	//=====================================================
}
