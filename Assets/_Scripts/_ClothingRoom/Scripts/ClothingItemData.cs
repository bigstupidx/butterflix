using UnityEngine;
using System.Collections;

[System.Serializable]
public enum eClothingState
{
	HIDDEN,
	AVAILABLE,
	OWNED
}

[System.Serializable]
public class ClothingItemData 
{
	public string				id;
	public eFairy				fairy;
	public bool					isDefault;
	public int					cost;
	public string				guiTexture2D;
	public string				prefabName;
	public string				gamePrefabName;
	public eClothingState		state;


	public float				currentRotation;
	
	public ClothingItemData()
	{
		id						= string.Empty;
		fairy					= eFairy.BLOOM;
		isDefault				= false;
		cost					= 0;
		guiTexture2D			= string.Empty;
		prefabName				= string.Empty;
		gamePrefabName			= string.Empty;
		state					= eClothingState.AVAILABLE;
		
		currentRotation			= UnityEngine.Random.Range( 0.0f , 100.0f );
	}
	
	public ClothingItemData Copy()
	{
		ClothingItemData obj 	= new ClothingItemData();
		
		obj.id					= id;
		obj.fairy				= fairy;
		obj.isDefault			= isDefault;
		obj.cost				= cost;
		obj.guiTexture2D		= guiTexture2D;
		obj.prefabName			= prefabName;
		obj.gamePrefabName		= gamePrefabName;
		obj.state				= state;
		
		return obj;
	}
}
