using UnityEngine;
using System;
using System.Collections;

public class CutsceneTriggerSpecialKey : CutsceneTrigger {

	[SerializeField] private ePuzzleKeyType _keyType;
	[SerializeField] private Collectable _key;

	//=====================================================

	#region Private Methods

	protected override void Awake()
	{
		base.Awake();

		_isTriggeredExternally = true;
		_playOnStart = false;
	}

	//=====================================================

	protected override void OnEnable()
	{
		base.OnEnable();

		PopupKeyUnlocked.PopupSpecialKeyUnlocked += OnPopupSpecialKeyUnlocked;
	}

	//=====================================================

	void OnDisable()
	{
		if( _isAppQuiting == true ) return;

		PopupKeyUnlocked.PopupSpecialKeyUnlocked -= OnPopupSpecialKeyUnlocked;
	}

	//=====================================================

	private void OnPopupSpecialKeyUnlocked( ePuzzleKeyType keyType )
	{
		if( _keyType != keyType ) return;

		StartCoroutine( PlayCutscene( _startDelay ) );
	}

	//=====================================================

	private IEnumerator PlayCutscene( float delay = 0.0f )
	{
		_isTriggered = true;

		yield return new WaitForSeconds( delay );

		CutsceneManager.Instance.OnStartCutscene( eCutsceneType.FLY_THRU, this );

		yield return new WaitForSeconds( 3.0f );

		// Activate key
		if( _key != null )
			_key.OnUnlockSpecialKey();
	}

	#endregion

	//=====================================================
}
