using UnityEngine;

public class RefectoryRoomManager : MonoBehaviour
{
	//=====================================================
	
	private	bool 		m_bFadeInComplete = false;
	private	bool 		m_bFadingOut = false;
	private	float		m_Timer = 0.0f;

	void Awake()
	{
		m_bFadeInComplete = false;
		m_bFadingOut = false;
		m_Timer = 0.0f;
	}

	//=====================================================
	
	void Start()
	{
		ScreenManager.FadeInCompleteEvent += OnFadeInCompleteEvent;
		ScreenManager.FadeOutCompleteEvent += OnFadeOutCompleteEvent;

		//ScreenManager.FadeIn();
	}

	//=====================================================
	
	void OnFadeInCompleteEvent()
	{
		m_bFadeInComplete = true;
	}
	
	//=====================================================

	void OnFadeOutCompleteEvent()
	{
		ScreenManager.FadeInCompleteEvent -= OnFadeInCompleteEvent;
		ScreenManager.FadeOutCompleteEvent -= OnFadeOutCompleteEvent;

		// Load the tutorial or main hall scene
		if( PlayerPrefsWrapper.HasKey( "IsTutorialCompleted" ) && PlayerPrefsWrapper.GetInt( "IsTutorialCompleted" ) != 0 )
			Application.LoadLevel( "MainHall" );
		else
			Application.LoadLevel( "Tutorial" );
	}
	
	//=====================================================
}
