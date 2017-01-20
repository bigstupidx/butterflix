using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class ResourcesPlatforms
{
	private static readonly List<string> _platforms = new List<string>();

	private static bool _isInitialised = false;

	//=====================================================

	public static string[] Platforms { get { return _platforms.ToArray(); } }

	//=====================================================

	public static Object GetPrefab()
	{
		Init();

		var prefab = Resources.Load( "Prefabs/Platforms/pfbPlatform" );

		if( prefab != null )
			return prefab;

		Debug.Log( "Platform prefab not found in resources" );
		return null;
	}

	//=====================================================

	public static Object GetModel( int index )
	{
		Init();

		var path = GetModelPath( index );

		if( string.IsNullOrEmpty( path ) == false )
		{
			var model = Resources.Load( path );

			if( model != null )
				return model;
		}

		Debug.Log( "Platform not found in resources: " + " [" + index + "]" );
		return null;
	}

	//=====================================================

	private static string GetModelPath( int index )
	{
		var path = new StringBuilder();
		path.Append( "Prefabs/Platforms/" );

		if( index < 0 || index >= _platforms.Count )
			index = 0;

		path.Append( _platforms[index] );

		return (string.IsNullOrEmpty( path.ToString() ) == false ? path.ToString() : string.Empty);
	}

	//=====================================================

	public static Object GetPathNodePrefab()
	{
		var pfbPathNode = Resources.Load( "Prefabs/PathNode/pfbPathNode" );

		if( pfbPathNode != null )
			return pfbPathNode;

		Debug.Log( "Path Node prefab not found in resources" );
		return null;
	}

	//=====================================================

	private static void Init()
	{
#if UNITY_EDITOR

		if( _isInitialised == false )
		{
			Debug.Log( "Initialising resources (platforms)" );

			var objects = Resources.LoadAll( "Prefabs/Platforms" );
			InitListWithObjectNames( _platforms, objects );

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
