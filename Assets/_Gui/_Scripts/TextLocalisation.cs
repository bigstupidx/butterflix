using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//=====================================================

public class TextLocalisation : MonoBehaviour
{
	public string						textID;

	public void Awake()
	{
		Text TextCmp = GetComponent( typeof( Text ) ) as Text;
		if( TextCmp != null )
		{
			TextCmp.text = TextManager.GetText( textID );
		}
	}
}


//=====================================================
