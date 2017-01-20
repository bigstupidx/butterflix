using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GuiBouncingText : MonoBehaviour
{
	private Text _text;

	//=====================================================

	public string Text { set { _text.text = value; } }

	//=====================================================

	void Awake()
	{
		var obj = this.transform.FindChild( "Canvas" );
		
		if( obj != null )
			_text = obj.GetComponentInChildren<Text>();
	}

	//=====================================================

	void Start()
	{
		StartCoroutine( Kill() );
	}

	//=====================================================

	private IEnumerator Kill()
	{
		yield return new WaitForSeconds( 2.0f );

		Destroy( this.gameObject );
	}

	//=====================================================
}
