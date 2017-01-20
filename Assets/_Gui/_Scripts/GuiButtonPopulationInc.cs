using UnityEngine;

public class GuiButtonPopulationInc : MonoBehaviour
{
	//=====================================================

	public void OnButtonClick()
	{
		GameDataManager.Instance.AddPlayerPopulation( 50000 );
	}

	//=====================================================
}
