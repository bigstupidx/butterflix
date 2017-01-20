using UnityEngine;

public class GuiButtonChangeScene : MonoBehaviour
{
	private int _levelIndex = 0;
	[SerializeField]
	private int _maxLevels = 3;

	//=====================================================

	public void Start()
	{
		_levelIndex = PlayerPrefs.GetInt( "NextSceneIndex" );
	}

	//=====================================================

	public void OnButtonClick()
	{
		++_levelIndex;
		if(_levelIndex >= _maxLevels)
			_levelIndex = 0;

		PlayerPrefs.SetInt( "NextSceneIndex", _levelIndex );

		Application.LoadLevel(_levelIndex);
	}

	//=====================================================
}
