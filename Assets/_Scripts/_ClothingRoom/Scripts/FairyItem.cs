using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//=====================================================

[System.Serializable]
public class FairyOutfit
{
	public string						id;
	
	public FairyOutfit()
	{
		id = "NULL";
	}

	//=====================================================

	public FairyOutfit( string a_id )
	{
		id = a_id;
	}

	//=====================================================

	public static implicit operator JSON( FairyOutfit data )
	{
		// Write JSON 
		var js = new JSON();

		js["id"] = data.id;

		return (js);
	}

	// //=====================================================

	public static explicit operator FairyOutfit( JSON obj )
	{
		// Read JSON
		FairyOutfit newObj = new FairyOutfit();
		bool bVerbose = false;

		newObj.id = obj.ToString( "id" );
		if( bVerbose ) Debug.Log( "id: " + newObj.id );
		
		return (newObj);
	}
}

//=====================================================
/*
[System.Serializable]
public class FairyItem
{
	public eFairy						type;
	public int							fairyLevel;
	public List< FairyOutfit >			outfitsOwned;
	
	public FairyItem()
	{
		type = eFairy.BLOOM;
		fairyLevel = 0;
		outfitsOwned = new List< FairyOutfit >();
	}

	//=====================================================

	public FairyItem( eFairy a_type , int a_fairyLevel , string outfitID )
	{
		type = a_type;
		fairyLevel = a_fairyLevel;
		outfitsOwned.Add( outfitID );
	}

	//=====================================================

	public static implicit operator JSON( FairyItem data )
	{
		// Write JSON 
		var js = new JSON();

		js["type"] = (int)data.type;
		js["fairyLevel"] = data.fairyLevel;

		{
			var outfitsOwnedDataListJson = new List<JSON>();

			foreach( var item in data.outfitsOwned )
			{
				var objJson = (JSON)(item);
				outfitsOwnedDataListJson.Add( objJson );
			}

			js["outfitsOwnedDataList"] = outfitsOwnedDataListJson;
		}

		return (js);
	}

	// //=====================================================

	public static explicit operator FairyItem( JSON obj )
	{
		// Read JSON
		FairyItem newObj = new FairyItem();
		bool bVerbose = false;

		newObj.type = (eFairy)obj.ToInt( "type" );
		if( bVerbose ) Debug.Log( "type: " + newObj.type );

		newObj.fairyLevel = obj.ToInt( "fairyLevel" );
		if( bVerbose ) Debug.Log( "fairyLevel: " + newObj.fairyLevel );

		// Outfits owned data
		if( null == newObj.outfitsOwned )
			newObj.outfitsOwned = new List<FairyOutfit>();

		newObj.outfitsOwned.Clear();
		if( obj.ContainsJSON( "outfitsOwnedDataList" ) )
		{
			JSON[] array = obj.ToArray<JSON>( "outfitsOwnedDataList" );

			for( int Idx = 0; Idx < array.Length; Idx++ )
			{
				newObj.outfitsOwned.Add( (FairyOutfit)array[Idx] );
			}
		}
		
		return (newObj);
	}
}
*/

//=====================================================
