using UnityEngine;

public class GuiButtonDiamondInc : MonoBehaviour
{
	//=====================================================

	public void OnButtonClick()
	{
		GameDataManager.Instance.AddPlayerDiamonds( 25, true );
		GameDataManager.Instance.BroadcastGuiData();
	}

	//=====================================================
}
