using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FairyData
{
	public int							Fairy { get; set; }
	public string						Outfit { get; set; }
	public int							Level { get; set; }
	public List< FairyOutfit >			OutfitsOwned;

	//=====================================================

	public FairyData()
	{
		Fairy = -1;									// eFairy.NULL == -1
		Outfit = string.Empty;
		Level = -1;
		OutfitsOwned = new List< FairyOutfit >();
	}

	//=====================================================

	public FairyData Copy()
	{
		var obj = new FairyData
		{
			Fairy = Fairy,
			Outfit = Outfit,
			Level = Level,
			OutfitsOwned = OutfitsOwned
		};

		return obj;
	}

	//=====================================================

	public void BuyOutfit( string id )
	{
		foreach( FairyOutfit CurOutfit in OutfitsOwned )
		{
			if( CurOutfit.id == id )
				return;
		}
		
		OutfitsOwned.Add( new FairyOutfit( id ) );
	}

	//=====================================================

	public bool OutfitOwned( string id )
	{
		foreach( FairyOutfit CurOutfit in OutfitsOwned )
		{
			if( CurOutfit.id == id )
				return( true );
		}
		
		return( false );
	}

	//=====================================================

	public static implicit operator JSON( FairyData data )
	{
		// Write JSON 
		var js = new JSON();
		js["fairy"] = data.Fairy;
		js["outfit"] = data.Outfit;
		js["level"] = data.Level;

		{
			var outfitsOwnedDataListJson = new List<JSON>();

			foreach( var item in data.OutfitsOwned )
			{
				var objJson = (JSON)(item);
				outfitsOwnedDataListJson.Add( objJson );
			}

			js["outfitsOwnedDataList"] = outfitsOwnedDataListJson;
		}

		//if( data.m_Rotation != 0 ) js["rotation"] = data.m_Rotation;
		//if( ((int)data.m_SelectionType) != 0 ) js["selectionType"] = (int)data.m_SelectionType;

		return (js);
	}

	// //=====================================================

	public static explicit operator FairyData( JSON obj )
	{
		// Read JSON
		var newObj = new FairyData();
		var bVerbose = false;

		newObj.Fairy = obj.ToInt( "fairy" );
		if( bVerbose ) Debug.Log( "fairy: " + (eFairy)newObj.Fairy );

		newObj.Outfit = obj.ToString( "outfit" );
		if( bVerbose ) Debug.Log( "outfit: " + newObj.Outfit );

		newObj.Level = obj.ToInt( "level" );
		if( bVerbose ) Debug.Log( "level: " + newObj.Level );

		// Outfits owned data
		if( null == newObj.OutfitsOwned )
			newObj.OutfitsOwned = new List<FairyOutfit>();

		newObj.OutfitsOwned.Clear();
		if( obj.ContainsJSON( "outfitsOwnedDataList" ) )
		{
			JSON[] array = obj.ToArray<JSON>( "outfitsOwnedDataList" );

			for( int Idx = 0; Idx < array.Length; Idx++ )
			{
				newObj.OutfitsOwned.Add( (FairyOutfit)array[Idx] );
			}
		}

		//if( obj["unlockStage"] != null )
		//{
		//	newObj.m_UnlockStage = obj.ToInt( "unlockStage" );
		//	if( bVerbose ) Debug.Log( "unlockStage: " + newObj.m_UnlockStage );
		//}
		//else
		//{
		//	newObj.m_UnlockStage = 0;
		//}

		return (newObj);
	}

	//=====================================================
}
