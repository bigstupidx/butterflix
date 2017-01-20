using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent( typeof( Button ) )]
public class BtnController : MonoBehaviour
{
	public event Action	ButtonClickedEvent;

	private Button _button;

	void Awake()
	{
		_button = GetComponent<Button>();

		_button.onClick.AddListener( () => OnButtonClicked() );
	}

	private void OnButtonClicked()
	{
		if( ButtonClickedEvent != null )
			ButtonClickedEvent();
	}
}
