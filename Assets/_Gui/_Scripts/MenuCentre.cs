using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuCentre : Menu
{
	public static MenuCentre Instance;

	[SerializeField]
	private BtnController _btnTest;

	//public static Menu Instance { get; private set; }

	protected override void Awake()
	{
		base.Awake();

		Instance = this;
	}

	void OnEnable()
	{
		_btnTest.ButtonClickedEvent += OnTestButtonClicked;
	}

	void OnDisable()
	{
		_btnTest.ButtonClickedEvent -= OnTestButtonClicked;
	}

	protected override void Update()
	{
		base.Update();
	}

	private void OnTestButtonClicked()
	{
		MenuManager.ShowMenu( MenuLeft.Instance );
	}
}
