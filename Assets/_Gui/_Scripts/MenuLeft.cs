using UnityEngine;
using System.Collections;

public class MenuLeft : Menu
{
	public static MenuLeft Instance;

	[SerializeField]
	private BtnController _btnCentreMenu;
	[SerializeField]
	private BtnController _btnAnotherMenu;

	protected override void Awake()
	{
		base.Awake();

		Instance = this;
	}

	void OnEnable()
	{
		_btnCentreMenu.ButtonClickedEvent += OnOpenLeftMenu;
		_btnAnotherMenu.ButtonClickedEvent += OnOpenAnotherMenu;
	}

	void OnDisable()
	{
		_btnCentreMenu.ButtonClickedEvent -= OnOpenLeftMenu;
		_btnAnotherMenu.ButtonClickedEvent -= OnOpenAnotherMenu;
	}

	protected override void Update()
	{
		base.Update();
	}

	private void OnOpenLeftMenu()
	{
		MenuManager.ShowMenu( MenuCentre.Instance );
	}

	private void OnOpenAnotherMenu()
	{
		MenuManager.ShowMenu( MenuCentre.Instance );
	}
}
