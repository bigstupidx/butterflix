using UnityEngine;

public class GuiButtonRespawnToNext : MonoBehaviour
{
	//=====================================================

	public void OnButtonClick()
	{
		GameManager.Instance.DebugRespawnFairyNextLocation();
	}

	//=====================================================
}
