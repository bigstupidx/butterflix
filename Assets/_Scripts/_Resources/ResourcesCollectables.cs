using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class ResourcesCollectables
{
	private static readonly List<string> _gems = new List<string>();
	private static readonly List<string> _redGems = new List<string>();
	private static readonly List<string> _keys = new List<string>();

	private static bool _isInitialised = false;

	//=====================================================

	public static string[] Gems { get { return _gems.ToArray(); } }
	public static string[] RedGems { get { return _redGems.ToArray(); } }
	public static string[] Keys { get { return _keys.ToArray(); } }

	//=====================================================

	public static Object GetPrefab()
	{
		Init();

		var prefab = Resources.Load( "Prefabs/Collectables/pfbCollectable" );

		if( prefab != null )
			return prefab;

		Debug.Log( "Collectable prefab not found in resources" );
		return null;
	}

	//=====================================================

	public static Object GetModel( eCollectable type, int index )
	{
		Init();

		var path = GetModelPath( type, index );

		if( string.IsNullOrEmpty( path ) == false )
		{
			//Debug.Log( "Model path: " + path );
			var model = Resources.Load( path );

			if( model != null )
				return model;
		}

		Debug.Log( "Collectable not found in resources: " + " [" + index + "]" );
		return null;
	}

	//=====================================================

	private static string GetModelPath( eCollectable type, int index )
	{
		var path = new StringBuilder();
		path.Append( "Prefabs/Collectables/" );

		switch( type )
		{
			case eCollectable.GEM:
				SetModelPathLocation( ref path, _gems, "Gem", index );
				break;
			case eCollectable.RED_GEM:
				SetModelPathLocation( ref path, _redGems, "RedGem", index );
				break;
			case eCollectable.KEY:
				SetModelPathLocation( ref path, _keys, "Key", index );
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
			Debug.Log( "Initialising resources (collectables)" );

			var objects = Resources.LoadAll( "Prefabs/Collectables/Gem" );
			InitListWithObjectNames( _gems, objects );

			objects = Resources.LoadAll( "Prefabs/Collectables/RedGem" );
			InitListWithObjectNames( _redGems, objects );

			objects = Resources.LoadAll( "Prefabs/Collectables/Key" );
			InitListWithObjectNames( _keys, objects );

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
			{
				//Debug.Log( "Add model to list: " + objects[i].name );
				list.Add( objects[i].name );
			}
		}
	}

	//=====================================================
}
