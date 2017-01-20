using UnityEngine;

public class ResourcesEnemies
{
	//private static List<string> _enemies = new List<string>();

	//private static bool _isInitialised = false;

	//=====================================================

	//public static string[] Enemies { get { return _enemies.ToArray(); } }

	//=====================================================

	public static Object GetManagerPrefab()
	{
		var prefab = Resources.Load( "Prefabs/Enemies/pfbEnemyManager" );

		if( prefab != null )
			return prefab;

		Debug.Log( "EnemyManager prefab not found in resources" );
		return null;
	}

	//=====================================================

	public static Object GetDoorwayPrefab()
	{
		var prefab = Resources.Load( "Prefabs/Enemies/pfbEnemyDoorway" );

		if( prefab != null )
			return prefab;

		Debug.Log( "EnemyDoorway prefab not found in resources" );
		return null;
	}

	//=====================================================

	public static Object GetPrefab()
	{
		//Init();

		var prefab = Resources.Load( "Prefabs/Enemies/pfbEnemy" );

		if( prefab != null )
			return prefab;

		Debug.Log( "Enemy prefab not found in resources" );
		return null;
	}

	//=====================================================

//	public static UnityEngine.Object GetModel( eCollectable type, int index )
//	{
//		Init();

//		string path = GetModelPath( type, index );

//		if( string.IsNullOrEmpty( path ) == false )
//		{
//			//Debug.Log( "Model path: " + path );
//			UnityEngine.Object model = Resources.Load( path );

//			if( model != null )
//				return model;
//		}

//		Debug.Log( "Collectable not found in resources: " + " [" + index + "]" );
//		return null;
//	}

//	//=====================================================

//	private static string GetModelPath( eCollectable type, int index )
//	{
//		StringBuilder path = new StringBuilder();
//		path.Append( "Prefabs/Collectables/" );

//		switch( type )
//		{
//			case eCollectable.GEM:
//				SetModelPathLocation( ref path, _gems, "Gem", index );
//				break;
//		}

//		return (string.IsNullOrEmpty( path.ToString() ) == false ? path.ToString() : string.Empty);
//	}

//	//=====================================================

//	private static void SetModelPathLocation( ref StringBuilder path, List<string> collectables, string folder, int index )
//	{
//		if( index < 0 || index >= collectables.Count )
//			index = 0;
//		path.Append( folder + "/" );
//		path.Append( collectables[index] );
//	}

//	//=====================================================

//	private static void Init()
//	{
//#if UNITY_EDITOR

//		if( _isInitialised == false )
//		{
//			Debug.Log( "Initialising resources (collectables)" );

//			UnityEngine.Object[] objects = Resources.LoadAll( "Prefabs/Collectables/Gem" );
//			InitListWithObjectNames( _gems, objects );

//			_isInitialised = true;
//		}
//#endif
//	}

	//=====================================================

	//private static void InitListWithObjectNames( List<string> list, UnityEngine.Object[] objects )
	//{
	//	list.Clear();

	//	for( var i = 0; i < objects.Length; i++ )
	//	{
	//		if( objects[i].name.Contains( "mdl" ) )
	//		{
	//			//Debug.Log( "Add model to list: " + objects[i].name );
	//			list.Add( objects[i].name );
	//		}
	//	}
	//}

	//=====================================================
}
