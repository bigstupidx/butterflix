using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class ResourcesChests
{
	private static readonly List<string> _smallChests = new List<string>();
	private static readonly List<string> _mediumChests = new List<string>();
	private static readonly List<string> _largeChests = new List<string>();

	private static bool _isInitialised = false;

	//=====================================================

	public static string[] SmallChests { get { return _smallChests.ToArray(); } }
	public static string[] MediumChests { get { return _mediumChests.ToArray(); } }
	public static string[] LargeChests { get { return _largeChests.ToArray(); } }

	//=====================================================

	public static Object GetPrefab()
	{
		Init();

		var	prefab = Resources.Load( "Prefabs/Chests/pfbChest" );

		if( prefab != null )
			return prefab;

		Debug.Log( "Chest prefab not found in resources" );
		return null;
	}

	//=====================================================

	public static Object GetModel( eChestType type, int index )
	{
		Init();

		var path = GetModelPath( type, index );

		if( string.IsNullOrEmpty( path ) == false )
		{
			var model = Resources.Load( path );

			if( model != null )
				return model;
		}

		Debug.Log( "Chest not found in resources: " + type + " [" + index + "]" );
		return null;
	}

	//=====================================================

	private static string GetModelPath( eChestType type, int index )
	{
		var path = new StringBuilder();
		path.Append( "Prefabs/Chests/" );

		switch( type )
		{
			case eChestType.SMALL:
				SetModelPathLocation( ref path, _smallChests, "Small", index );
				break;

			case eChestType.MEDIUM:
				SetModelPathLocation( ref path, _mediumChests, "Medium", index );
				break;

			case eChestType.LARGE:
				SetModelPathLocation( ref path, _largeChests, "Large", index );
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

		if( _isInitialised == true ) return;
		
		Debug.Log( "Initialising resources (chests)" );

		var objects = Resources.LoadAll( "Prefabs/Chests/Small" );
		InitListWithObjectNames( _smallChests, objects );

		objects = Resources.LoadAll( "Prefabs/Chests/Medium" );
		InitListWithObjectNames( _mediumChests, objects );

		objects = Resources.LoadAll( "Prefabs/Chests/Large" );
		InitListWithObjectNames( _largeChests, objects );

		_isInitialised = true;
#endif
	}

	//=====================================================

	private static void InitListWithObjectNames( List<string> list, Object[] objects )
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
