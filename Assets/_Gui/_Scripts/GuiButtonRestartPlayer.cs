using UnityEngine;

public class GuiButtonRestartPlayer : MonoBehaviour
{
	//=====================================================

	public void OnButtonClick()
	{
		GameManager.Instance.OnReturnToStart();
	}

	//=====================================================
}
