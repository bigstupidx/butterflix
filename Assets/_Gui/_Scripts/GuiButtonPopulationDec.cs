using UnityEngine;

public class GuiButtonPopulationDec : MonoBehaviour
{
	//=====================================================

	public void OnButtonClick()
	{
		GameDataManager.Instance.AddPlayerPopulation( -50000 );
	}

	//=====================================================
}
