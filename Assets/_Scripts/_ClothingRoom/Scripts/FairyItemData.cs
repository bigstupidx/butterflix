using UnityEngine;
using System.Collections;


[System.Serializable]
public class FairyItemData 
{
	public eFairy				fairy;
	public int[]				KeysRequired = new int[ 4 ];
	public int[]				GemsRequired = new int[ 4 ];
	public int[]				PopulationRequired = new int[ 4 ];

	public FairyItemData()
	{
		fairy					= eFairy.BLOOM;
	}
}
