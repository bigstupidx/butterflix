using UnityEngine;
using System.Collections;

public class AchievementsTest : MonoBehaviour 
{
    //=============================================================================

	string		m_LastMessage;
	
	void Start() 
	{
		AchievementsManager.LoginSuccessEvent += OnLoginSuccessEvent;
		AchievementsManager.LoginFailEvent += OnLoginFailEvent;
                                   
		AchievementsManager.LogoutSuccessEvent += OnLogoutSuccessEvent;
		AchievementsManager.LogoutFailEvent += OnLogoutFailEvent;
                                   
		AchievementsManager.AddAchievementSuccessEvent += OnAddAchievementSuccessEvent;
		AchievementsManager.AddAchievementFailEvent += OnAddAchievementFailEvent;

		m_LastMessage = "Null";
	}
	
    //=============================================================================
	
	void Update() 
	{
	
	}
	
    //=============================================================================
	
	void OnGUI()
	{
		if( GUI.Button( new Rect(10,10,180,70) , "GC/GP Login" ) )
		{
			AchievementsManager.m_Instance.Login();
		}
	
		if( GUI.Button( new Rect(10,90,180,70) , "GC/GP Logout" ) )
		{
			AchievementsManager.m_Instance.Logout();
		}

		if( GUI.Button( new Rect(10,170,180,70) , "GC/GP IsLoggedIn" ) )
		{
			m_LastMessage = "IsLoggedIn: " + AchievementsManager.m_Instance.IsLoggedIn().ToString();
		}

		if( GUI.Button( new Rect(10,250,180,70) , "GC/GP Add achievement" ) )
		{
			//AchievementsManager.m_Instance.AddAchievement( "CgkI_I3jsK8YEAIQAQ" );
		}
		
		if( GUI.Button( new Rect(10,330,180,70) , "GC/GP Display achievements" ) )
		{
			AchievementsManager.m_Instance.DisplayAchievements();
		}

		GUI.Label( new Rect(10,410,350,20) , m_LastMessage );
	}
	
    //=============================================================================

	void OnLoginSuccessEvent()
	{
		m_LastMessage = "OnLoginSuccessEvent";
	}
	
    //=============================================================================

	void OnLoginFailEvent( string Error )
	{
		m_LastMessage = "OnLoginFailEvent " + Error;
	}
	
    //=============================================================================

	void OnLogoutSuccessEvent()
	{
		m_LastMessage = "OnLogoutSuccessEvent";
	}
	
    //=============================================================================

	void OnLogoutFailEvent( string Error )
	{
		m_LastMessage = "OnLogoutFailEvent " + Error;
	}
	
    //=============================================================================

	void OnAddAchievementSuccessEvent()
	{
		m_LastMessage = "OnAddAchievementSuccessEvent";
	}
	
    //=============================================================================

	void OnAddAchievementFailEvent( string Error )
	{
		m_LastMessage = "OnAddAchievementFailEvent " + Error;
	}
	
    //=============================================================================
}
