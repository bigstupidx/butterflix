using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Tsumanga;

public class AchievementManagerCreator : MonoBehaviour 
{
	public GameObject			pfbAchievementsManager;
	
	//=============================================================================
	
	void Awake()
	{
		#if UNITY_EDITOR
		// If achievements manager doesnt exist then create it
		if( AchievementsManager.m_Instance == null )
		{
			Instantiate( pfbAchievementsManager );
		}
		#endif
	}
	
	//=============================================================================
}
