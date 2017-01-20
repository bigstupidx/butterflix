using UnityEngine;

public class GuiButtonWildMagicDec : MonoBehaviour
{
	//=====================================================

	public void OnButtonClick()
	{
		GameDataManager.Instance.AddWildMagicRate( -5 );
	}

	//=====================================================
}
