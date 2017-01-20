using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//=====================================================

public class IAPDescription : MonoBehaviour
{
	public string						iapID;

	public void Start()
	{
		Text TextCmp = GetComponent( typeof( Text ) ) as Text;
		if( TextCmp != null )
		{
			if( ( IAPManager.instance != null ) && IAPManager.instance.IsStoreAvailable() )
			{
				IAPItem CurItem = IAPManager.instance.GetIAPItem( iapID );
				TextCmp.text = /*CurItem.ItemTitle + " - "*/"" + CurItem.ItemPrice;
			}
			else
			{
				TextCmp.text = "Store unavailable";
			}
		}
	}
}


//=====================================================
