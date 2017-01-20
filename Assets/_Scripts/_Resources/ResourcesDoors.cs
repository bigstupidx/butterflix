using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class ResourcesDoors
{
	private static readonly List<string> _doorsBasic = new List<string>();
	private static readonly List<string> _doorsBasicDouble = new List<string>();
	private static readonly List<string> _doorsPuzzleLocked = new List<string>();
	private static readonly List<string> _doorsCrawl = new List<string>();
	private static readonly List<string> _doorsOblivionPortal = new List<string>();
	private static readonly List<string> _doorsPuzzleEntrance = new List<string>();
	private static readonly List<string> _doorsPlayerHub = new List<string>();
	private static readonly List<string> _doorsBoss = new List<string>();

	private static bool _isInitialised = false;

	//=====================================================

	public static string[] DoorsBasic { get { return _doorsBasic.ToArray(); } }
	public static string[] DoorsBasicDouble { get { return _doorsBasicDouble.ToArray(); } }
	public static string[] DoorsCrawl { get { return _doorsCrawl.ToArray(); } }
	public static string[] DoorsOblivionPortal { get { return _doorsOblivionPortal.ToArray(); } }
	public static string[] DoorsPuzzleLocked { get { return _doorsPuzzleLocked.ToArray(); } }
	public static string[] DoorsPuzzleEntrance { get { return _doorsPuzzleEntrance.ToArray(); } }
	public static string[] DoorsPlayerHub { get { return _doorsPlayerHub.ToArray(); } }
	public static string[] DoorsBoss { get { return _doorsBoss.ToArray(); } }

	//=====================================================

	public static Object GetPrefab()
	{
		Init();

		var prefab = Resources.Load( "Prefabs/Doors/pfbDoor" );

		if( prefab != null )
			return prefab;

		Debug.Log( "Door prefab not found in resources" );
		return null;
	}

	//=====================================================

	public static Object GetModel( eDoorType type, int index )
	{
		Init();

		var path = GetModelPath( type, index );

		if( string.IsNullOrEmpty( path ) == false )
		{
			var model = Resources.Load( path );

			if( model != null )
				return model;
		}

		Debug.Log( "Door not found in resources: " + type + " [" + index + "]" );
		return null;
	}

	//=====================================================

	private static string GetModelPath( eDoorType type, int index )
	{
		var path = new StringBuilder();
		path.Append( "Prefabs/Doors/" );

		switch( type )
		{
			case eDoorType.BASIC:
				SetModelPathLocation( ref path, _doorsBasic, "Basic", index );
				break;

			case eDoorType.BASIC_DOUBLE:
				SetModelPathLocation( ref path, _doorsBasicDouble, "BasicDouble", index );
				break;

			case eDoorType.CRAWL:
				SetModelPathLocation( ref path, _doorsCrawl, "Crawl", index );
				break;

			case eDoorType.OBLIVION_PORTAL:
				SetModelPathLocation( ref path, _doorsOblivionPortal, "OblivionPortal", index );
				break;

			case eDoorType.PUZZLE_LOCKED:
				SetModelPathLocation( ref path, _doorsPuzzleLocked, "PuzzleLocked", index );
				break;

			case eDoorType.PUZZLE_ENTRANCE:
				SetModelPathLocation( ref path, _doorsPuzzleEntrance, "PuzzleEntrance", index );
				break;

			case eDoorType.PLAYER_HUB:
				SetModelPathLocation( ref path, _doorsPlayerHub, "PlayerHub", index );
				break;

			case eDoorType.BOSS:
				SetModelPathLocation( ref path, _doorsBoss, "Boss", index );
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

	public static Object GetSpawnPointPrefab()
	{
		var pfbSpawnPoint = Resources.Load( "Prefabs/SpawnPoint/pfbSpawnPoint" );

		if( pfbSpawnPoint != null )
			return pfbSpawnPoint;

		Debug.Log( "Spawn Point prefab not found in resources" );
		return null;
	}

	//=====================================================

	public static Object GetRespawnPointPrefab()
	{
		var pfbSpawnPoint = Resources.Load( "Prefabs/SpawnPoint/pfbRespawnPoint" );

		if( pfbSpawnPoint != null )
			return pfbSpawnPoint;

		Debug.Log( "Respawn Point prefab not found in resources" );
		return null;
	}

	//=====================================================

	private static void Init()
	{
#if UNITY_EDITOR

		if( _isInitialised == false )
		{
			Debug.Log( "Initialising resources (doors)" );

			UnityEngine.Object[] objects = Resources.LoadAll( "Prefabs/Doors/Basic" );
			InitListWithObjectNames( _doorsBasic, objects );

			objects = Resources.LoadAll( "Prefabs/Doors/BasicDouble" );
			InitListWithObjectNames( _doorsBasicDouble, objects );

			objects = Resources.LoadAll( "Prefabs/Doors/Crawl" );
			InitListWithObjectNames( _doorsCrawl, objects );

			objects = Resources.LoadAll( "Prefabs/Doors/OblivionPortal" );
			InitListWithObjectNames( _doorsOblivionPortal, objects );

			objects = Resources.LoadAll( "Prefabs/Doors/PuzzleEntrance" );
			InitListWithObjectNames( _doorsPuzzleEntrance, objects );

			objects = Resources.LoadAll( "Prefabs/Doors/PuzzleLocked" );
			InitListWithObjectNames( _doorsPuzzleLocked, objects );

			objects = Resources.LoadAll( "Prefabs/Doors/PuzzleEntrance" );
			InitListWithObjectNames( _doorsPuzzleEntrance, objects );

			objects = Resources.LoadAll( "Prefabs/Doors/PlayerHub" );
			InitListWithObjectNames( _doorsPlayerHub, objects );

			objects = Resources.LoadAll( "Prefabs/Doors/Boss" );
			InitListWithObjectNames( _doorsBoss, objects );

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
	// Helper method to write current door-model list to console - copy into string[] initialisations above
	// Currently called in GameManager->Start()
	//public static void WriteModelsToConsole()
	//{
	//	UnityEngine.Object[] objects = Resources.LoadAll( "Prefabs/Doors/" );

	//	StringBuilder doorPrefabs = new StringBuilder();

	//	for( var i = 0; i < objects.Length; i++ )
	//	{
	//		if( objects[i].name.Contains( "mdl" ) )
	//		{
	//			doorPrefabs.AppendLine();
	//			doorPrefabs.Append( objects[i].name );
	//		}
	//	}

	//	Debug.Log( doorPrefabs.ToString() );
	//}

	//=====================================================
}
