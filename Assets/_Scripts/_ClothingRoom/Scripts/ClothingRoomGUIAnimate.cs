using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Analytics;

public class ClothingRoomGUIAnimate : MonoBehaviour
{
	private	float		m_Timer;
	
	//=====================================================

	void Start()
	{
		m_Timer = 0.0f;
	}

	//=====================================================
	
	void Update()
	{
		float YPos = -684.0f;
		
		m_Timer += Time.deltaTime;
		if( m_Timer > 6.0f )
		{
			float fSlide = Mathf.Clamp( m_Timer - 6.0f , 0.0f , 1.0f );
			YPos += 300.0f * Mathf.Sin( fSlide * 1.57f );
		}
		
		//Debug.Log( YPos );
		transform.localPosition = new Vector3( 0.0f , YPos , 0.0f );
	}

	//=====================================================
}
