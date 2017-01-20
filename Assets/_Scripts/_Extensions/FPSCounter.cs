using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FPSCounter : MonoBehaviour 
{
	//private	SpriteText						m_txtFPS;
	//private float 							m_LastTime;
	//private float 							m_LastFPS;

	////=============================================================================

	//void Awake()
	//{
	//	m_LastTime = Time.realtimeSinceStartup;
	//	m_LastFPS = 60.0f;
	//}
	
	////=============================================================================
	
	//public void SetText( SpriteText txt )
	//{
	//	m_txtFPS = txt;
	//}

	////=============================================================================

	//void OnPostRender()
	//{
	//	if( m_txtFPS == null )
	//		return;
		
	//	float deltaTime = Time.realtimeSinceStartup - m_LastTime;
	//	m_LastTime = Time.realtimeSinceStartup;
	//	float FPS = ( 1.0f / deltaTime );
		
	//	m_LastFPS = Mathf.Lerp( m_LastFPS , FPS , 0.01f );
	//	m_txtFPS.Text = "" + (int)m_LastFPS + " fps";
		
	///*	float fScreenRatio = Screen.width / Screen.height;
	//	float yOffset = 320.0f;
	//	float xOffset = yOffset * fScreenRatio;
		
	//	yOffset -= 135.0f;
	//	xOffset += 75.0f;
		
	//	m_txtFPS.transform.position = new Vector3( xOffset , yOffset , transform.position.z );*/
	//}

	//=============================================================================
}
