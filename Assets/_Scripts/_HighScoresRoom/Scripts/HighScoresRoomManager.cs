using UnityEngine;
using System;

public class HighScoresRoomManager : MonoBehaviour
{
	public GameObject						pfbHighScoresPopup;
	
	private	bool 		m_bFadeInComplete = false;
	private	bool 		m_bFadingOut = false;
	private	float		m_Timer = 0.0f;

	//=====================================================

	public void OnButtonPressed_CommonRoom()
	{
		Application.LoadLevel( "CommonRoom" );
	}

	//=====================================================
	
	public void Update()
	{
		/*
		if (Input.GetKeyDown(KeyCode.Keypad1))
		{
			if( AchievementsManager.m_Instance != null )
			{
				AchievementsManager.m_Instance.GetScoreboard( AchievementsManager.eScoreTime.AllTime , 1 , 10 );
			}
		}
		if (Input.GetKeyDown(KeyCode.Keypad3))
		{
			// Create high scores popup
			GameObject HS = Instantiate( pfbHighScoresPopup ) as GameObject;
			HighScoresManager.instance.ShowPanel( true );
		}
		if (Input.GetKeyDown(KeyCode.Y))
		{
			if( AchievementsManager.m_Instance != null )
			{
				//AchievementsManager.m_Instance.RandomiseName();
				AchievementsManager.m_Instance.ReportScore( 212941 );
			}
		}
		*/
	}
	
	//=====================================================
	
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

		// Load the CommonRoom
		Application.LoadLevel( "CommonRoom" );
	}
	
	//=====================================================
}
