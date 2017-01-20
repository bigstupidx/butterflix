using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class ResourcesSwitches
{
	private static readonly List<string> _floorSwitches = new List<string>();
	private static readonly List<string> _wallSwitches = new List<string>();
	private static readonly List<string> _pressureSwitches = new List<string>();

	private static bool _isInitialised = false;

	//=====================================================

	public static string[] FloorSwitches { get { return _floorSwitches.ToArray(); } }
	public static string[] WallSwitches { get { return _wallSwitches.ToArray(); } }
	public static string[] PressureSwitches { get { return _pressureSwitches.ToArray(); } }

	//=====================================================

	public static Object GetPrefab(eSwitchType type = eSwitchType.FLOOR_LEVER)
	{
		Init();

		Object prefab = null;

		switch(type)
		{
			default:
				prefab = Resources.Load( "Prefabs/Switches/pfbSwitch" );
				break;
			case eSwitchType.PRESSURE:
				prefab = Resources.Load( "Prefabs/Switches/pfbPressureSwitch" );
				break;
		}

		if( prefab != null )
			return prefab;

		Debug.Log( "Switch prefab not found in resources" );
		return null;
	}

	//=====================================================

	public static Object GetModel( eSwitchType type, int index )
	{
		Init();

		var path = GetModelPath( type, index );

		if( string.IsNullOrEmpty( path ) == false )
		{
			var model = Resources.Load( path );

			if( model != null )
				return model;
		}

		Debug.Log( "Switch not found in resources: " + type + " [" + index + "]" );
		return null;
	}

	//=====================================================

	private static string GetModelPath( eSwitchType type, int index )
	{
		var path = new StringBuilder();
		path.Append( "Prefabs/Switches/" );

		switch( type )
		{
			case eSwitchType.FLOOR_LEVER:
				SetModelPathLocation( ref path, _floorSwitches, "Floor", index );
				break;

			case eSwitchType.WALL_SWITCH:
				SetModelPathLocation( ref path, _wallSwitches, "Wall", index );
				break;

			case eSwitchType.PRESSURE:
				SetModelPathLocation( ref path, _pressureSwitches, "Pressure", index );
				break;
		}

		return (string.IsNullOrEmpty( path.ToString() ) == false ? path.ToString() : string.Empty);
	}

	//=====================================================

	private static void SetModelPathLocation( ref StringBuilder path, List<string> collectables, string folder, int index )
	{
		if( index < 0 || index >= collectables.Count )
			index = 0;
		path.Append( folder + "/" );
		path.Append( collectables[index] );
	}

	//=====================================================

	private static void Init()
	{
#if UNITY_EDITOR

		if( _isInitialised == false )
		{
			Debug.Log( "Initialising resources (switches)" );

			var objects = Resources.LoadAll( "Prefabs/Switches/Floor" );
			InitListWithObjectNames( _floorSwitches, objects );

			objects = Resources.LoadAll( "Prefabs/Switches/Wall" );
			InitListWithObjectNames( _wallSwitches, objects );

			objects = Resources.LoadAll( "Prefabs/Switches/Pressure" );
			InitListWithObjectNames( _pressureSwitches, objects );

			_isInitialised = true;
		}
#endif
	}

	//=====================================================

	private static void InitListWithObjectNames( List<string> list, UnityEngine.Object[] objects )
	{
		list.Clear();

		for( var i = 0; i < objects.Length; i++ )
		{
			if( objects[i].name.Contains( "mdl" ) )
				list.Add( objects[i].name );
		}
	}

	//=====================================================
}
