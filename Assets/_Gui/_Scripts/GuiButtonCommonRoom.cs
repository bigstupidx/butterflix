using UnityEngine;

public class GuiButtonCommonRoom : MonoBehaviour
{
	//=====================================================

	public void OnButtonClick()
	{
		GameManager.Instance.OnGoToCommonRoom();
	}

	//=====================================================
}
