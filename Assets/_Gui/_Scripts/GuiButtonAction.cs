using System;

public class GuiButtonAction : MonoBehaviourEMS, IPauseListener
{
	public static event Action	PerformActionEvent;

	private bool _isPaused;

	//=====================================================

	public void OnButtonClick()
	{
		if( _isPaused == true ) return;

		if( PerformActionEvent != null )
			PerformActionEvent();
	}

	//=====================================================

	public void OnPauseEvent( bool isPaused )
	{
		_isPaused = isPaused;
	}

	//=====================================================

	void OnEnable()
	{
		GameManager.Instance.PauseEvent += OnPauseEvent;
	}

	//=====================================================

	private void OnDisable()
	{
		if( _isAppQuiting == false )
			GameManager.Instance.PauseEvent -= OnPauseEvent;
	}

	//=====================================================
}
