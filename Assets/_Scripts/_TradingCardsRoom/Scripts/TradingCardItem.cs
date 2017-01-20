using UnityEngine;
using System.Collections;

//=====================================================

public enum eTradingCardClassification
{
	NULL = 0,
	WINX,
	WILD,
	STORY,
	STANDARD,
	NUM_ITEMS
}

//=====================================================

public enum eTradingCardRarity
{
	NULL,
	UNIQUE,
	VERYRARE,
	RARE,
	COMMON,
	VERYCOMMON,
	TEACHER,
	NUM_ITEMS
}

//=====================================================

public enum eTradingCardCondition
{
	NULL,
	MINT,
	SCUFFED,
	NUM_ITEMS
}

//=====================================================

// Item read in from the spreadsheet - holds information about each trading card

[System.Serializable]
public class TradingCardSpreadsheetItem
{
	public string						id;
	public eTradingCardClassification	classification;
	public int							value;
	public int							valueScuffed;
	public eTradingCardRarity			rarity;
	public int							waitTime;
	public int							revealPrice;
	public int							page;
	public int							position;
	public int							globalPosition;
	public string						smallGuiTexture2D;
	public string						largeGuiTexture2D;
	
	public TradingCardSpreadsheetItem()
	{
		id						= string.Empty;
		classification			= eTradingCardClassification.NULL;
		value					= 0;
		valueScuffed			= 0;
		rarity					= eTradingCardRarity.NULL;
		waitTime				= 0;
		revealPrice				= 0;
		page					= 0;
		position				= 0;
		globalPosition			= 0;
		smallGuiTexture2D		= string.Empty;
		largeGuiTexture2D		= string.Empty;
	}
	
	public TradingCardSpreadsheetItem  Copy()
	{
		TradingCardSpreadsheetItem obj = new TradingCardSpreadsheetItem();
		
		obj.id					= id;
		obj.classification		= classification;
		obj.value				= value;
		obj.valueScuffed		= valueScuffed;
		obj.rarity				= rarity;
		obj.waitTime			= waitTime;
		obj.revealPrice			= revealPrice;
		obj.page				= page;
		obj.position			= position;
		obj.smallGuiTexture2D	= smallGuiTexture2D;
		obj.largeGuiTexture2D	= largeGuiTexture2D;
		
		return obj;
	}
}

//=====================================================

// Item read/written locally

[System.Serializable]
public class TradingCardHeldItem
{
	public string						id;
	public eTradingCardCondition		condition;
	public float						notifyTimer;
	
	public TradingCardHeldItem()
	{
		id = string.Empty;
		condition = eTradingCardCondition.NULL;
		notifyTimer = 0.0f;
	}

	//=====================================================

	public TradingCardHeldItem( string a_id , eTradingCardCondition a_condition )
	{
		id = a_id;
		condition = a_condition;
	}

	//=====================================================

	public TradingCardHeldItem Copy()
	{
		TradingCardHeldItem obj = new TradingCardHeldItem
		{
			id = id,
			condition = condition
		};

		return obj;
	}

	//=====================================================

	public static implicit operator JSON( TradingCardHeldItem data )
	{
		// Write JSON 
		var js = new JSON();

		js["id"] = data.id;
		js["condition"] = (int)data.condition;

		return (js);
	}

	// //=====================================================

	public static explicit operator TradingCardHeldItem( JSON obj )
	{
		// Read JSON
		TradingCardHeldItem newObj = new TradingCardHeldItem();
		bool bVerbose = false;

		newObj.id = obj.ToString( "id" );
		if( bVerbose ) Debug.Log( "id: " + newObj.id );

		newObj.condition = (eTradingCardCondition)obj.ToInt( "condition" );
		if( bVerbose ) Debug.Log( "condition: " + newObj.condition );

		return (newObj);
	}
}


//=====================================================
