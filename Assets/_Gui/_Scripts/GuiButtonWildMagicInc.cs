using UnityEngine;

public class GuiButtonWildMagicInc : MonoBehaviour
{
	//=====================================================

	public void OnButtonClick()
	{
		GameDataManager.Instance.AddWildMagicRate( 5 );
	}

	//=====================================================
}
