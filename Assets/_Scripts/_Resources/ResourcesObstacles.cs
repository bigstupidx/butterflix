using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class ResourcesObstacles
{
	private static readonly List<string> _obstaclesSwingingArm = new List<string>();
	private static readonly List<string> _obstaclesSwingingBody = new List<string>();
	private static readonly List<string> _pushableBoxes = new List<string>();

	private static bool _isInitialised = false;

	//=====================================================

	public static string[] ObstaclesSwingingArm { get { return _obstaclesSwingingArm.ToArray(); } }
	public static string[] ObstaclesSwingingBody { get { return _obstaclesSwingingBody.ToArray(); } }
	public static string[] PushableBoxes { get { return _pushableBoxes.ToArray(); } }

	//=====================================================

	public static Object GetPrefab( eObstacleType type )
	{
		Init();

		Object prefab = null;

		switch( type )
		{
			case eObstacleType.SWINGING:
				prefab = Resources.Load( "Prefabs/Obstacles/Swinging/pfbSwingingObstacle" );
				break;
			case eObstacleType.PUSHABLE_BOX:
				prefab = Resources.Load( "Prefabs/Obstacles/Pushable/pfbPushable" );
				break;
			case eObstacleType.MAGICAL_TRAP_ICE:
				prefab = Resources.Load( "Prefabs/Obstacles/MagicalTraps/pfbMagicalTrapIce" );
				break;
			case eObstacleType.MAGICAL_TRAP_WIND:
				prefab = Resources.Load( "Prefabs/Obstacles/MagicalTraps/pfbMagicalTrapWind" );
				break;
		}

		if( prefab != null )
			return prefab;

		Debug.Log( "Obstacle prefab not found in resources" );
		return null;
	}

	//=====================================================

	public static Object GetModel( eObstacleType type, int index )
	{
		Init();

		var path = GetModelPath( type, index );

		if( string.IsNullOrEmpty( path ) == false )
		{
			var model = Resources.Load( path );

			if( model != null )
				return model;
		}

		Debug.Log( "Obstacle not found in resources: " + type + " [" + index + "]" );
		return null;
	}

	//=====================================================

	private static string GetModelPath( eObstacleType type, int index )
	{
		var path = new StringBuilder();

		switch( type )
		{
			case eObstacleType.SWINGING_ARM:
				path.Append( "Prefabs/Obstacles/Swinging/Arm/" );
				index = Mathf.Clamp( index, 0, _obstaclesSwingingArm.Count - 1 );
				path.Append( _obstaclesSwingingArm[index] );
				break;
			case eObstacleType.SWINGING_BODY:
				path.Append( "Prefabs/Obstacles/Swinging/Body/" );
				index = Mathf.Clamp( index, 0, _obstaclesSwingingBody.Count - 1 );
				path.Append( _obstaclesSwingingBody[index] );
				break;
			case eObstacleType.PUSHABLE_BOX:
				path.Append( "Prefabs/Obstacles/Pushable/Box/" );
				index = Mathf.Clamp( index, 0, _pushableBoxes.Count - 1 );
				path.Append( _pushableBoxes[index] );
				break;
		}

		return (string.IsNullOrEmpty( path.ToString() ) == false ? path.ToString() : string.Empty);
	}

	//=====================================================

	private static void Init()
	{
#if UNITY_EDITOR

		if( _isInitialised == true ) return;
		
		Debug.Log( "Initialising resources (obstacles)" );

		var objects = Resources.LoadAll( "Prefabs/Obstacles/Swinging/Arm" );
		InitListWithObjectNames( _obstaclesSwingingArm, objects );

		objects = Resources.LoadAll( "Prefabs/Obstacles/Swinging/Body" );
		InitListWithObjectNames( _obstaclesSwingingBody, objects );

		objects = Resources.LoadAll( "Prefabs/Obstacles/Pushable/Box" );
		InitListWithObjectNames( _pushableBoxes, objects );

		_isInitialised = true;
#endif
	}

	//=====================================================

	private static void InitListWithObjectNames( List<string> list, UnityEngine.Object[] objects )
	{
		list.Clear();

		foreach( var o in objects )
		{
			if( o.name.Contains( "mdl" ) )
				list.Add( o.name );
		}
	}

	//=====================================================
}
