using UnityEngine;
using System.Collections;

//=====================================================

[System.Serializable]
public class AchievementItem
{
	public int							id;
	public bool							bHasUploadedToGCGP;
	
	public AchievementItem()
	{
		id						= -1;
		bHasUploadedToGCGP		= false;
	}
	
	//=====================================================

	public static implicit operator JSON( AchievementItem data )
	{
		// Write JSON 
		var js = new JSON();

		js["id"] = data.id;
		js["bHasUploadedToGCGP"] = data.bHasUploadedToGCGP;

		return (js);
	}

	//=====================================================

	public static explicit operator AchievementItem( JSON obj )
	{
		// Read JSON
		AchievementItem newObj = new AchievementItem();
		bool bVerbose = false;

		newObj.id = obj.ToInt( "id" );
		if( bVerbose ) Debug.Log( "id: " + newObj.id );

		newObj.bHasUploadedToGCGP = obj.ToBoolean( "bHasUploadedToGCGP" );
		if( bVerbose ) Debug.Log( "bHasUploadedToGCGP: " + newObj.bHasUploadedToGCGP );

		return (newObj);
	}
}

//=====================================================

