using UnityEngine;
using System;

[ExecuteInEditMode]
[Serializable]
public class PlatformWithCutsceneObjects : Platform
{
	// Editable in inspector
	[SerializeField]
	private CutsceneObject[] _cutsceneObjects;

	//=====================================================

	#region ICutsceneObject

	public override void OnPlayCutsceneAnimation( int animationIndex = 0 )
	{
		SwitchActivatesPlatform();

		if( _cutsceneObjects.Length == 0 ) return;

		foreach( var cutsceneObject in _cutsceneObjects )
		{
			if( cutsceneObject != null )
				cutsceneObject.OnPlayCutsceneAnimation( animationIndex );
		}
	}

	#endregion

	//=====================================================
}
