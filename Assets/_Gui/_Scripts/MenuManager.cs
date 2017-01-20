using UnityEngine;

public class MenuManager : MonoBehaviour
{
	[SerializeField]
	private Menu _defaultMenu;
	
	private static Menu CurrentMenu;

	void Start()
	{
		CurrentMenu = _defaultMenu;

		ShowMenu( CurrentMenu );
	}

	public static void ShowMenu( Menu menu )
	{
		if( CurrentMenu != null )
			CurrentMenu.IsOpen = false;

		CurrentMenu = menu;
		CurrentMenu.IsOpen = true;
	}
}
