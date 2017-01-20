using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Tsumanga;

public class ServerManagerCreator : MonoBehaviour 
{
	public GameObject			pfbServerManager;
	
	//=============================================================================
	
	void Awake()
	{
		#if UNITY_EDITOR
		// If server manager doesnt exist then create it
		if( ServerManager.instance == null )
		{
			Instantiate( pfbServerManager );
		}
		#endif
	}
	
	//=============================================================================
}
