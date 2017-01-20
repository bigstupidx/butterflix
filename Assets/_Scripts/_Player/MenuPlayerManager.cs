using UnityEngine;
using System;
using System.Collections;

public class MenuPlayerManager : MonoBehaviour
{
	//=====================================================

	public void OnPlayFootstep()
	{
		AudioSource CurSource = GetComponent( typeof( AudioSource ) ) as AudioSource;
		if( ( CurSource != null ) && ( CurSource.isPlaying == false ) )
			CurSource.Play();
	}

	//=====================================================
}
