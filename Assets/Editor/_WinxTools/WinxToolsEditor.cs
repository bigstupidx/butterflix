using UnityEngine;
using UnityEditor;

public class WinxToolsEditor
{
	//=====================================================

	[MenuItem( "Winx/Add Door/Basic", false, 1 )]
	public static void AddBasicDoor()
	{
		AddDoorOfType( eDoorType.BASIC );
	}

	//=====================================================

	[MenuItem( "Winx/Add Door/Basic-Double", false, 1 )]
	public static void AddBasicDoubleDoor()
	{
		AddDoorOfType( eDoorType.BASIC_DOUBLE );
	}

	//=====================================================

	[MenuItem( "Winx/Add Door/Crawl", false, 1 )]
	public static void AddCrawlDoor()
	{
		AddDoorOfType( eDoorType.CRAWL );
	}

	//=====================================================

	[MenuItem( "Winx/Add Door/Oblivion Portal", false, 1 )]
	public static void AddOblivionPortal()
	{
		AddDoorOfType( eDoorType.OBLIVION_PORTAL );
	}

	//=====================================================

	[MenuItem( "Winx/Add Door/Puzzle Locked", false, 1 )]
	public static void AddPuzzleLockedDoor()
	{
		AddDoorOfType( eDoorType.PUZZLE_LOCKED );
	}

	//=====================================================

	[MenuItem( "Winx/Add Door/Puzzle Entrance-Exit", false, 1 )]
	public static void AddPuzzleEntranceDoor()
	{
		AddDoorOfType( eDoorType.PUZZLE_ENTRANCE );
	}

	//=====================================================

	[MenuItem( "Winx/Add Door/Player Hub Entrance-Exit", false, 1 )]
	public static void AddPlayerHubDoor()
	{
		AddDoorOfType( eDoorType.PLAYER_HUB );
	}

	//=====================================================

	[MenuItem( "Winx/Add Door/Boss Entrance-Exit", false, 1 )]
	public static void AddBossEntranceDoor()
	{
		AddDoorOfType( eDoorType.BOSS );
	}

	//=====================================================

	[MenuItem( "Winx/Add Switch/Floor", false, 2 )]
	public static void AddFloorSwitch()
	{
		AddSwitchOfType( eSwitchType.FLOOR_LEVER );
	}

	//=====================================================

	[MenuItem( "Winx/Add Switch/Wall", false, 2 )]
	public static void AddWallSwitch()
	{
		AddSwitchOfType( eSwitchType.WALL_SWITCH );
	}

	//=====================================================

	[MenuItem( "Winx/Add Switch/Pressure", false, 2 )]
	public static void AddPressureSwitch()
	{
		AddSwitchOfType( eSwitchType.PRESSURE );
	}

	//=====================================================

	[MenuItem( "Winx/Add Spawn Point/Scene-Start", false, 3 )]
	public static void AddSpawnStartPoint()
	{
		AddSpawnPointOfType( eSpawnType.SCENE_START );
	}

	//=====================================================

	[MenuItem( "Winx/Add Spawn Point/Respawn", false, 3 )]
	public static void AddRespawnPoint()
	{
		AddSpawnPointOfType( eSpawnType.RESPAWN );
	}

	//=====================================================

	[MenuItem( "Winx/Add Spawn Point/Crawl-Through", false, 3 )]
	public static void AddSpawnCrawlPoint()
	{
		AddSpawnPointOfType( eSpawnType.CRAWL_THROUGH );
	}

	//=====================================================

	[MenuItem( "Winx/Add Spawn Point/Oblivion-Portal", false, 3 )]
	public static void AddSpawnPortalPoint()
	{
		AddSpawnPointOfType( eSpawnType.OBLIVION_PORTAL );
	}

	//=====================================================

	[MenuItem( "Winx/Add Platform/On Path", false, 4 )]
	public static void AddPlatformOnPath()
	{
		AddPlatformOfType( ePlatformType.ON_PATH );
	}

	//=====================================================

	[MenuItem( "Winx/Add Platform/On Spline", false, 4 )]
	public static void AddPlatformOnSpline()
	{
		AddPlatformOfType( ePlatformType.ON_SPLINE );
	}

	//=====================================================

	[MenuItem( "Winx/Add Path Node", false, 4 )]
	public static void AddPathNode()
	{
		var pfb = ResourcesPlatforms.GetPathNodePrefab();
		if( pfb == null ) return;
		
		var prefab = PrefabUtility.InstantiatePrefab( pfb ) as GameObject;
		if( prefab == null ) return;

		PositionObjectAndSelect( prefab );
	}

	//=====================================================

	[MenuItem( "Winx/Add Obstacle/Swinging", false, 5 )]
	public static void AddSwingingObstacle()
	{
		AddObstacleOfType( eObstacleType.SWINGING );
	}

	//=====================================================

	[MenuItem( "Winx/Add Obstacle/Pushable Box", false, 5 )]
	public static void AddPushableBox()
	{
		AddObstacleOfType( eObstacleType.PUSHABLE_BOX );
	}

	//=====================================================

	[MenuItem( "Winx/Add Obstacle/Magical Trap/Ice", false, 5 )]
	public static void AddMagicalTrapIce()
	{
		AddObstacleOfType( eObstacleType.MAGICAL_TRAP_ICE );
	}

	//=====================================================

	[MenuItem( "Winx/Add Obstacle/Magical Trap/Wind", false, 5 )]
	public static void AddMagicalTrapWind()
	{
		AddObstacleOfType( eObstacleType.MAGICAL_TRAP_WIND );
	}

	//=====================================================

	[MenuItem( "Winx/Add Collectable/Gem", false, 6 )]
	public static void AddCollectableGem()
	{
		AddCollectableOfType( eCollectable.GEM );
	}

	//=====================================================

	[MenuItem( "Winx/Add Collectable/Red Gem", false, 6 )]
	public static void AddCollectableRedGem()
	{
		AddCollectableOfType( eCollectable.RED_GEM );
	}

	//=====================================================

	[MenuItem( "Winx/Add Collectable/Key Normal", false, 6 )]
	public static void AddCollectableKey()
	{
		AddCollectableOfType( eCollectable.KEY, ePuzzleKeyType.KEY_001 );
	}

	//=====================================================

	[MenuItem( "Winx/Add Collectable/Key Red Gem", false, 6 )]
	public static void AddCollectableKeyRedGem()
	{
		AddCollectableOfType( eCollectable.KEY, ePuzzleKeyType.KEY_GEM_RED );
	}

	//=====================================================

	[MenuItem( "Winx/Add Collectable/Key 100 Gem", false, 6 )]
	public static void AddCollectableKey100Gem()
	{
		AddCollectableOfType( eCollectable.KEY, ePuzzleKeyType.KEY_GEM_100 );
	}

	//=====================================================

	[MenuItem( "Winx/Add Chest/Small", false, 7 )]
	public static void AddSmallChest()
	{
		AddChestOfType( eChestType.SMALL );
	}

	//=====================================================

	[MenuItem( "Winx/Add Chest/Medium", false, 7 )]
	public static void AddMediumChest()
	{
		AddChestOfType( eChestType.MEDIUM );
	}

	//=====================================================

	[MenuItem( "Winx/Add Chest/Large", false, 7 )]
	public static void AddLargeChest()
	{
		AddChestOfType( eChestType.LARGE );
	}

	//=====================================================

	[MenuItem( "Winx/Add Enemy/Manager", false, 8 )]
	public static void AddEnemyManager()
	{
		var pfb = ResourcesEnemies.GetManagerPrefab();
		if( pfb == null ) return;
		
		var prefab = PrefabUtility.InstantiatePrefab( pfb ) as GameObject;
		if( prefab == null ) return;

		PositionObjectAndSelect( prefab );

		// AddEnemyOfType();
	}

	//=====================================================

	[MenuItem( "Winx/Add Enemy/Doorway", false, 8 )]
	public static void AddEnemyDooway()
	{
		var pfb = ResourcesEnemies.GetDoorwayPrefab();
		if( pfb == null ) return;

		var prefab = PrefabUtility.InstantiatePrefab( pfb ) as GameObject;
		if( prefab == null ) return;

		PositionObjectAndSelect( prefab );
	}

	//=====================================================

	private static void AddDoorOfType( eDoorType type )
	{
		var pfb = ResourcesDoors.GetPrefab();
		var mdl = ResourcesDoors.GetModel( type, 0 );

		if( pfb == null ) return;
		
		var prefab = PrefabUtility.InstantiatePrefab( pfb ) as GameObject;
		if( prefab == null ) return;
		
		var script = prefab.GetComponent<Door>();

		switch( type )
		{
			case eDoorType.BASIC:
				prefab.name = "BasicDoor";
				break;
			case eDoorType.BASIC_DOUBLE:
				prefab.name = "DoubleDoor";
				break;
			case eDoorType.CRAWL:
				prefab.name = "CrawlDoor";
				break;
			case eDoorType.OBLIVION_PORTAL:
				prefab.name = "OblivionPortal";
				break;
			case eDoorType.PUZZLE_LOCKED:
				prefab.name = "LockedPuzzleDoor";
				break;
			case eDoorType.PUZZLE_ENTRANCE:
				prefab.name = "PuzzleRoomDoor";
				break;
			case eDoorType.PLAYER_HUB:
				prefab.name = "MainHallDoor";
				break;
			case eDoorType.BOSS:
				prefab.name = "BossRoomDoor";
				break;
		}

		if( script != null )
		{
			script.Type = type;

			if( mdl != null )
			{
				//var model = PrefabUtility.InstantiatePrefab( mdl ) as GameObject;
				//script.Init( model );
				script.Init();
			}
		}

		PositionObjectAndSelect( prefab );
	}

	//=====================================================

	private static void AddSwitchOfType( eSwitchType type )
	{
		Object pfb = null;

		switch( type )
		{
			default:
				pfb = ResourcesSwitches.GetPrefab();
				break;
			case eSwitchType.PRESSURE:
				pfb = ResourcesSwitches.GetPrefab( eSwitchType.PRESSURE );
				break;
		}

		if( pfb == null ) return;

		var mdl = ResourcesSwitches.GetModel( type, 0 );

		var prefab = PrefabUtility.InstantiatePrefab( pfb ) as GameObject;
		if( prefab == null ) return;
		
		var script = prefab.GetComponent<Switch>();

		switch( type )
		{
			case eSwitchType.FLOOR_LEVER:
				prefab.name = "Switch-Floor-0";
				break;
			case eSwitchType.WALL_SWITCH:
				prefab.name = "Switch-Wall-0";
				break;
			case eSwitchType.PRESSURE:
				prefab.name = "Switch-Pressure-0";
				break;
		}

		if( script != null )
		{
			script.Type = type;

			if( mdl != null )
			{
				var model = PrefabUtility.InstantiatePrefab( mdl ) as GameObject;

				script.Init( model );
			}
		}

		PositionObjectAndSelect( prefab );
	}

	//=====================================================

	private static void AddPlatformOfType( ePlatformType type )
	{
		var pfb = ResourcesPlatforms.GetPrefab();
		var mdl = ResourcesPlatforms.GetModel( 0 );

		if( pfb == null ) return;
		
		var prefab = PrefabUtility.InstantiatePrefab( pfb ) as GameObject;
		if( prefab == null ) return;
		
		var script = prefab.GetComponent<Platform>();

		switch( type )
		{
			case ePlatformType.ON_PATH:
				prefab.name = "PlatformOnPath";
				break;
			case ePlatformType.ON_SPLINE:
				prefab.name = "PlatformOnSpline";
				break;
		}

		if( script != null )
		{
			script.Type = type;

			if( mdl != null )
			{
				var model = PrefabUtility.InstantiatePrefab( mdl ) as GameObject;

				script.Init( model );
			}
		}

		PositionObjectAndSelect( prefab );
	}

	//=====================================================

	private static void AddObstacleOfType( eObstacleType type )
	{
		switch( type )
		{
			case eObstacleType.SWINGING:
				AddObstacleSwinging();
				break;
			case eObstacleType.PUSHABLE_BOX:
				AddPushableObject();
				break;
			case eObstacleType.MAGICAL_TRAP_ICE:
			case eObstacleType.MAGICAL_TRAP_WIND:
				AddMagicalTrap( type );
				break;
		}
	}

	//=====================================================

	private static Transform CreateObstacleContainers( eObstacleType type )
	{
		// Create containers for obstacles if they don't already exist
		var container = GameObject.Find( "Obstacles" );
		var swinging = GameObject.Find( "Swinging" );
		var pushable = GameObject.Find( "Pushable" );
		var magicalTrap = GameObject.Find( "MagicalTraps" );

		if( container == null )
		{
			container = CreateContainer( "Obstacles" );

			if( swinging == null )
				swinging = CreateContainer( "Swinging", container );

			if( pushable == null )
				pushable = CreateContainer( "Pushable", container );

			if( magicalTrap == null )
				magicalTrap = CreateContainer( "MagicalTraps", container );
		}
		else
		{
			Debug.Log( "Instance of obstacle container found." );
		}

		// Return appropriate container
		switch( type )
		{
			case eObstacleType.SWINGING:
				return swinging.transform;
			case eObstacleType.PUSHABLE_BOX:
				return pushable.transform;
			case eObstacleType.MAGICAL_TRAP_ICE:
			case eObstacleType.MAGICAL_TRAP_WIND:
				return magicalTrap.transform;
		}

		return container.transform;
	}

	//=====================================================

	private static void AddObstacleSwinging()
	{
		const eObstacleType type = eObstacleType.SWINGING;
		var pfb = ResourcesObstacles.GetPrefab( type );
		var mdl01 = ResourcesObstacles.GetModel( eObstacleType.SWINGING_ARM, 0 );
		var mdl02 = ResourcesObstacles.GetModel( eObstacleType.SWINGING_BODY, 0 );

		if( pfb == null ) return;

		// Create containers for obstacles if they don't already exist
		var container = CreateObstacleContainers( type );

		var prefab = PrefabUtility.InstantiatePrefab( pfb ) as GameObject;
		if( prefab == null ) return;
		
		prefab.name = "SwingingObstacle";

		// Add to traps gameObject in hierarchy
		prefab.transform.parent = container;

		var script = prefab.GetComponent<SwingingObstacle>();

		if( script != null )
		{
			script.Type = type;

			if( mdl01 != null && mdl02 != null )
			{
				var model01 = PrefabUtility.InstantiatePrefab( mdl01 ) as GameObject;
				var model02 = PrefabUtility.InstantiatePrefab( mdl02 ) as GameObject;

				script.Init( model01, model02 );
			}
		}

		PositionObjectAndSelect( prefab );
	}

	//=====================================================

	private static void AddPushableObject()
	{
		const eObstacleType type = eObstacleType.PUSHABLE_BOX;
		var pfb = ResourcesObstacles.GetPrefab( type );
		var mdl = ResourcesObstacles.GetModel( type, 0 );

		if( pfb == null ) return;
		
		// Create containers for obstacles if they don't already exist
		var container = CreateObstacleContainers( type );

		var prefab = PrefabUtility.InstantiatePrefab( pfb ) as GameObject;
		if( prefab == null ) return;
		
		switch( type )
		{
			case eObstacleType.PUSHABLE_BOX:
				prefab.name = "PushableBox";
				break;
		}

		// Add to traps gameObject in hierarchy
		prefab.transform.parent = container;

		var script = prefab.GetComponent<Obstacle>();

		if( script != null )
		{
			script.Type = type;

			if( mdl != null )
			{
				var model = PrefabUtility.InstantiatePrefab( mdl ) as GameObject;

				script.Init( model );
			}
		}

		PositionObjectAndSelect( prefab );
	}

	//=====================================================

	private static void AddMagicalTrap( eObstacleType type )
	{
		var pfb = ResourcesObstacles.GetPrefab( type );
		if( pfb == null ) return;
		
		// Create containers for obstacles if they don't already exist
		var container = CreateObstacleContainers( type );

		var prefab = PrefabUtility.InstantiatePrefab( pfb ) as GameObject;
		if( prefab == null ) return;
		
		switch( type )
		{
			case eObstacleType.MAGICAL_TRAP_ICE:
				prefab.name = "MagicalTrapIce";
				break;
			case eObstacleType.MAGICAL_TRAP_WIND:
				prefab.name = "MagicalTrapWind";
				break;
		}

		// Add to traps gameObject in hierarchy
		prefab.transform.parent = container;

		var script = prefab.GetComponent<MagicalTrap>();

		if( script != null )
			script.Type = type;

		PositionObjectAndSelect( prefab );
	}

	//=====================================================

	private static void AddCollectableOfType( eCollectable type, ePuzzleKeyType keyType = ePuzzleKeyType.NULL )
	{
		var pfb = ResourcesCollectables.GetPrefab();
		Object mdl = null;

		mdl = ResourcesCollectables.GetModel( type, ( type == eCollectable.KEY ) ? (int)keyType : 0 );

		if( pfb == null ) return;

		// Create containers for collectables if they don't already exist
		var container = GameObject.Find( "Collectables" );
		var gems = GameObject.Find( "Gems" );
		var redGems = GameObject.Find( "RedGems" );
		var keys = GameObject.Find( "Keys" );

		if( container == null )
		{
			container = CreateContainer( "Collectables" );

			if( gems == null )
				gems = CreateContainer( "Gems", container );

			if( redGems == null )
				redGems = CreateContainer( "RedGems", container );

			if( keys == null )
				keys = CreateContainer( "Keys", container );
		}

		var prefab = PrefabUtility.InstantiatePrefab( pfb ) as GameObject;
		if( prefab == null ) return;
		
		var script = prefab.GetComponent<Collectable>();

		switch( type )
		{
			case eCollectable.GEM:
				prefab.name = "Gem";
				prefab.transform.parent = gems.transform;
				break;
			case eCollectable.RED_GEM:
				prefab.name = "RedGem";
				prefab.transform.parent = redGems.transform;
				break;
			case eCollectable.KEY:
				switch( keyType )
				{
					default:
						prefab.name = "Key";
						break;
					case ePuzzleKeyType.KEY_GEM_RED:
						prefab.name = "Red Gem Key";
						break;
					case ePuzzleKeyType.KEY_GEM_100:
						prefab.name = "100 Gem Key";
						break;
				}

				prefab.transform.parent = keys.transform;
				break;
		}

		if( script != null )
		{
			script.Type = type;

			if( mdl != null )
			{
				var model = PrefabUtility.InstantiatePrefab( mdl ) as GameObject;

				script.Init( model );

				if( type == eCollectable.KEY )
				{
					switch( keyType )
					{
						default:
							script.KeyId = ePuzzleKeyType.KEY_001;
							break;
						case ePuzzleKeyType.KEY_GEM_RED:
							script.KeyId = ePuzzleKeyType.KEY_GEM_RED;
							break;
						case ePuzzleKeyType.KEY_GEM_100:
							script.KeyId = ePuzzleKeyType.KEY_GEM_100;
							break;
					}
				}

				script.Refresh();
			}
		}

		PositionObjectAndSelect( prefab );
	}

	//=====================================================

	private static void AddSpawnPointOfType( eSpawnType type )
	{
		Object pfb;

		if( type != eSpawnType.RESPAWN )
			pfb = ResourcesDoors.GetSpawnPointPrefab();
		else
			pfb = ResourcesDoors.GetRespawnPointPrefab();

		if( pfb == null ) return;

		// Create containers for collectables if they don't already exist
		var container = GameObject.Find( "SpawnPoints" );
		var sceneStart = GameObject.Find( "SceneStarts" );
		var respawn = GameObject.Find( "Respawns" );
		var crawlThrough = GameObject.Find( "CrawlTroughs" );
		var oblivionPortal = GameObject.Find( "OblivionPortals" );

		if( container == null )
		{
			container = CreateContainer( "SpawnPoints" );

			if( sceneStart == null )
				sceneStart = CreateContainer( "SceneStarts", container );

			if( respawn == null )
				respawn = CreateContainer( "Respawns", container );

			if( crawlThrough == null )
				crawlThrough = CreateContainer( "CrawlTroughs", container );

			if( oblivionPortal == null )
				oblivionPortal = CreateContainer( "OblivionPortals", container );
		}

		var prefab = PrefabUtility.InstantiatePrefab( pfb ) as GameObject;
		if( prefab == null ) return;

		var script = prefab.GetComponent<SpawnPoint>();

		switch( type )
		{
			case eSpawnType.SCENE_START:
				prefab.name = "SpawnStart";
				prefab.transform.parent = sceneStart.transform;
				break;
			case eSpawnType.RESPAWN:
				prefab.name = "Respawn";
				prefab.transform.parent = respawn.transform;
				break;
			case eSpawnType.CRAWL_THROUGH:
				prefab.name = "SpawnCrawl";
				prefab.transform.parent = crawlThrough.transform;
				break;
			case eSpawnType.OBLIVION_PORTAL:
				prefab.name = "SpawnPortal";
				prefab.transform.parent = oblivionPortal.transform;
				break;
		}

		if( script != null )
			script.Type = type;

		PositionObjectAndSelect( prefab );
	}

	//=====================================================

	private static void AddChestOfType( eChestType type )
	{
		var pfb = ResourcesChests.GetPrefab();
		var mdl = ResourcesChests.GetModel( type, 0 );

		if( pfb == null ) return;

		// Create containers for chests if they don't already exist
		var container = GameObject.Find( "Chests" );
		var small = GameObject.Find( "Small" );
		var medium = GameObject.Find( "Mediun" );
		var large = GameObject.Find( "Large" );

		if( container == null )
		{
			container = CreateContainer( "Chests" );

			if( small == null )
				small = CreateContainer( "Small", container );

			if( medium == null )
				medium = CreateContainer( "Mediun", container );

			if( large == null )
				large = CreateContainer( "Large", container );
		}

		var prefab = PrefabUtility.InstantiatePrefab( pfb ) as GameObject;
		if( prefab == null ) return;

		var script = prefab.GetComponent<Chest>();

		switch( type )
		{
			case eChestType.SMALL:
				prefab.name = "SmallChest";
				prefab.transform.parent = small.transform;
				break;
			case eChestType.MEDIUM:
				prefab.name = "MediumChest";
				prefab.transform.parent = medium.transform;
				break;
			case eChestType.LARGE:
				prefab.name = "LargeChest";
				prefab.transform.parent = large.transform;
				break;
		}

		if( script != null )
		{
			script.Type = type;

			if( mdl != null )
			{
				var model = PrefabUtility.InstantiatePrefab( mdl ) as GameObject;

				script.Init( model );
			}
		}

		PositionObjectAndSelect( prefab );
	}

	//=====================================================

	private static void AddEnemyOfType()	// eEnemyType type )
	{
		var pfb = ResourcesEnemies.GetPrefab();
		if( pfb == null ) return;

		// Create containers for enemies if they don't already exist
		var container = GameObject.Find( "Enemies" ) ?? CreateContainer( "Enemies" );

		var prefab = PrefabUtility.InstantiatePrefab( pfb ) as GameObject;
		if( prefab == null ) return;

		prefab.name = "Enemy";
		prefab.transform.parent = container.transform;

		PositionObjectAndSelect( prefab );
	}

	//=====================================================

	private static void PositionObjectAndSelect( GameObject prefab )
	{
		// Position object in relation to editor view or at world-zero
		prefab.transform.position = ScreenCentreToEditorView();

		Selection.activeGameObject = prefab;
	}

	//=====================================================

	private static Vector3 ScreenCentreToEditorView()
	{
		var camEditor = Camera.current;

		if( camEditor == null ) return Vector3.zero;

		var ray = camEditor.ScreenPointToRay( new Vector3( Screen.width * 0.5f, Screen.height * 0.5f, 0.0f ) );
		Debug.DrawRay( ray.origin, ray.direction * 100, Color.blue );
		RaycastHit hit;
		var mask = 1 << LayerMask.NameToLayer( "Collidable" ) | 1 << LayerMask.NameToLayer( "CollidableRaycast" );

		return Physics.Raycast( ray, out hit, 500.0f, mask ) ? hit.point : Vector3.zero;
	}

	//=====================================================

	private static GameObject CreateContainer( string name, GameObject parent = null )
	{
		var container = new GameObject( name );
		container.transform.position = Vector3.zero;
		container.transform.rotation = Quaternion.identity;

		if( parent != null )
			container.transform.parent = parent.transform;

		return container;
	}

	//=====================================================
}
