using UnityEngine;
using System.Collections;

public class ServerBase {
	
	private static bool						isInitialised			= false;
	private static Tsumanga.WebServices		webServices				= null;
	
	#region Getters / Setters
	
	//=============================================================================
	
	public static Tsumanga.WebServices WebServices
	{
		get { return webServices; }
	}
	
	//=============================================================================
	
	#endregion
	
	#region Private Methods
	
	//=============================================================================
	
	//static ServerBase()
	//{
		//Init();
	//}
	
	//=============================================================================

	public static Tsumanga.WebServices Init(string urlBase)
	{
		if( (string.Empty != urlBase) && (! isInitialised) )
		{
			isInitialised = true;
			
			webServices = new Tsumanga.WebServices(urlBase);
			
			return webServices;
		}
		
		return null;
	}
	
	//=============================================================================
	
	#endregion
}
