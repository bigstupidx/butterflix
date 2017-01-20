using UnityEngine;
using System;
using UnityEditor;

// CanEditMultipleObjects
[CustomEditor( typeof( Door ) )]
public class DoorEditor : Editor
{
	private Door _myTarget;

	private SerializedObject _serializedTarget;
	private SerializedProperty _hingePosition;
	private SerializedProperty _switches;
	private SerializedProperty _locks;
	private SerializedProperty _spawnPoint;
	private SerializedProperty _targetScene;
	private SerializedProperty _fairyRequired;
	private SerializedProperty _clipOpen;
	private SerializedProperty _clipPuzzleUnlocked;
	private SerializedProperty _bossTorches;

	private string[] _models;

	//=====================================================

	void OnEnable()
	{
		//Debug.Log("OnEnable");
		_serializedTarget = new SerializedObject( target );
		_myTarget = (Door)target;

		_hingePosition = _serializedTarget.FindProperty( "_hingePosition" );
		_switches = _serializedTarget.FindProperty( "_switches" );
		_locks = _serializedTarget.FindProperty( "_locks" );
		_spawnPoint = _serializedTarget.FindProperty( "_spawnPoint" );
		_targetScene = _serializedTarget.FindProperty( "_targetScene" );
		_fairyRequired = _serializedTarget.FindProperty( "_fairyRequired" );
		_clipOpen = _serializedTarget.FindProperty( "_clipOpen" );
		_clipPuzzleUnlocked = _serializedTarget.FindProperty( "_clipPuzzleUnlocked" );
		_bossTorches = _serializedTarget.FindProperty( "_bossTorches" );

		// Get list of models for this object-type
		GetModels();
	}

	//=====================================================

	public override void OnInspectorGUI()
	{
		_serializedTarget.Update();
		// Set / display all coded defaults
		//base.OnInspectorGUI();

		//EditorGUILayout.Space();
		//_doorType.enumValueIndex = (int)(ObstacleDoor.eDoorType)EditorGUILayout.EnumPopup( "Door Type",
		//									(ObstacleDoor.eDoorType)Enum.ToObject( typeof( ObstacleDoor.eDoorType ), _doorType.enumValueIndex ) );

		EditorGUILayout.Space();
		switch( _myTarget.Type )
		{
			case eDoorType.BASIC:
				EditorGUILayout.LabelField( "Door Type: ", "Basic" );
				break;
			case eDoorType.BASIC_DOUBLE:
				EditorGUILayout.LabelField( "Door Type: ", "Basic Double" );
				break;
			case eDoorType.CRAWL:
				EditorGUILayout.LabelField( "Door Type: ", "Crawl" );
				break;
			case eDoorType.OBLIVION_PORTAL:
				EditorGUILayout.LabelField( "Door Type: ", "Oblivian Portal" );
				break;
			case eDoorType.PUZZLE_LOCKED:
				EditorGUILayout.LabelField( "Door Type: ", "Puzzle Locked" );
				break;
			case eDoorType.PUZZLE_ENTRANCE:
				EditorGUILayout.LabelField( "Door Type: ", "Puzzle Entrance-Exit" );
				break;
			case eDoorType.PLAYER_HUB:
				EditorGUILayout.LabelField( "Door Type: ", "Player Hub Entrance-Exit" );
				break;
			case eDoorType.BOSS:
				EditorGUILayout.LabelField( "Door Type: ", "Boss Entrance-Exit" );
				break;
		}

		if( _myTarget.IsDoubleDoor == false )
		{
			// Door Model
			EditorGUILayout.Space();
			_myTarget.Model = EditorGUILayout.Popup( "Door Model", _myTarget.Model, _models );

			// AudioClips
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField( _clipOpen, new GUIContent( "Open Door Clip" ), false );
			EditorGUILayout.PropertyField( _clipPuzzleUnlocked, new GUIContent( "Puzzle Door Unlocked" ), false );

			if( _myTarget.Type != eDoorType.CRAWL && _myTarget.Type != eDoorType.OBLIVION_PORTAL )
			{
				// Door Hinge
				if( _myTarget.HasDoubleDoor == false )
				{
					EditorGUILayout.Space();
					_hingePosition.enumValueIndex = (int)(Door.eHingePosition)EditorGUILayout.EnumPopup( "Hinge Position",
														 (Door.eHingePosition)Enum.ToObject( typeof( Door.eHingePosition ), _hingePosition.enumValueIndex ) );
				}

				// Door Rotation
				EditorGUILayout.Space();
				_myTarget.RotateOpenBy = (float)EditorGUILayout.IntSlider( "Rotate Open: Degrees", (int)_myTarget.RotateOpenBy, 45, 180 );

				// Door Rotation Duration
				EditorGUILayout.Space();
				_myTarget.RotateDuration = (float)EditorGUILayout.IntSlider( "Rotate Open: Duration", (int)_myTarget.RotateDuration, 1, 5 );

				// Door Trigger Size
				//EditorGUILayout.Space();
				//_triggerSize.vector3Value = EditorGUILayout.Vector3Field( "Trigger Size", _triggerSize.vector3Value );
			}

			// Door-Type Parameters (Auto-Close, Delay Closing Door, Opening Directions, Has Double Door)
			EditorGUILayout.Space();
			switch( _myTarget.Type )
			{
				case eDoorType.BASIC:
					EditorGUILayout.Space();
					_myTarget.AutoClose = EditorGUILayout.Toggle( "Auto Close Door", _myTarget.AutoClose );
					EditorGUILayout.Space();
					_myTarget.AutoCloseDelay = (float)EditorGUILayout.IntSlider( "Delay Close Door", (int)_myTarget.AutoCloseDelay, 1, 10 );
					EditorGUILayout.Space();
					_myTarget.OpenInAndOut = EditorGUILayout.Toggle( "Open Inwards and Outwards", _myTarget.OpenInAndOut );
					// Interactive Level - Player needs to have collected 'x' keys in current scene in order to interact with door
					EditorGUILayout.Space();
					_myTarget.KeyLevel = EditorGUILayout.IntSlider( "Min Keys Collected", _myTarget.KeyLevel, 0, 10 );
					EditorGUILayout.Space();
					_fairyRequired.intValue = (int)(eFairy)EditorGUILayout.EnumPopup( "Fairy Required", (eFairy)_fairyRequired.intValue );
					break;

				case eDoorType.BASIC_DOUBLE:
					EditorGUILayout.Space();
					_myTarget.AutoClose = EditorGUILayout.Toggle( "Auto Close Door", _myTarget.AutoClose );
					EditorGUILayout.Space();
					_myTarget.AutoCloseDelay = (float)EditorGUILayout.IntSlider( "Delay Close Door", (int)_myTarget.AutoCloseDelay, 1, 10 );
					EditorGUILayout.Space();
					_myTarget.OpenInAndOut = EditorGUILayout.Toggle( "Open Inwards and Outwards", _myTarget.OpenInAndOut );
					EditorGUILayout.Space();
					_myTarget.HasDoubleDoor = EditorGUILayout.Toggle( "Has Double Door", _myTarget.HasDoubleDoor );
					// Interactive Level - Player needs to have collected 'x' keys in current scene in order to interact with door
					EditorGUILayout.Space();
					_myTarget.KeyLevel = EditorGUILayout.IntSlider( "Min Keys Collected", _myTarget.KeyLevel, 0, 10 );
					EditorGUILayout.Space();
					_fairyRequired.intValue = (int)(eFairy)EditorGUILayout.EnumPopup( "Fairy Required", (eFairy)_fairyRequired.intValue );
					break;

				case eDoorType.CRAWL:
				case eDoorType.OBLIVION_PORTAL:
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField( _spawnPoint, new GUIContent( "Spawn Point" ), true );
					// Interactive Level - Player needs to have collected 'x' keys in current scene in order to interact with door
					EditorGUILayout.Space();
					_myTarget.KeyLevel = EditorGUILayout.IntSlider( "Min Keys Collected", _myTarget.KeyLevel, 0, 10 );
					break;

				case eDoorType.PUZZLE_LOCKED:
					EditorGUILayout.Space();
					_myTarget.HasDoubleDoor = EditorGUILayout.Toggle( "Has Double Door", _myTarget.HasDoubleDoor );
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField( _switches, new GUIContent( "Door Switches" ), true );
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField( _locks, new GUIContent( "Door Locks" ), true );
					break;

				case eDoorType.PUZZLE_ENTRANCE:
				case eDoorType.PLAYER_HUB:
				case eDoorType.BOSS:
					EditorGUILayout.Space();
					_myTarget.AutoClose = EditorGUILayout.Toggle( "Auto Close Door", _myTarget.AutoClose );
					EditorGUILayout.Space();
					_myTarget.AutoCloseDelay = (float)EditorGUILayout.IntSlider( "Delay Close Door", (int)_myTarget.AutoCloseDelay, 1, 10 );
					EditorGUILayout.Space();
					_myTarget.OpenInAndOut = EditorGUILayout.Toggle( "Open Inwards and Outwards", _myTarget.OpenInAndOut );
					EditorGUILayout.Space();
					_myTarget.HasDoubleDoor = EditorGUILayout.Toggle( "Has Double Door", _myTarget.HasDoubleDoor );
					EditorGUILayout.Space();
					// This version avoids enum values being treated as an array (so I can use out-of-order and out-of-range values in the enum declaration)
					_targetScene.intValue = (int)(eLocation)EditorGUILayout.EnumPopup( "Target Scene", (eLocation)_targetScene.intValue );
					//_targetScene.enumValueIndex = (int)(eLocation)EditorGUILayout.EnumPopup( "Target Scene",
					//								   (eLocation)Enum.ToObject( typeof( eLocation ), _targetScene.enumValueIndex ) );
					break;
			}

			// Boss Door Torches
			if( _myTarget.Type == eDoorType.BOSS && _targetScene.intValue == (int)eLocation.BOSS_ROOM )
			{
				EditorGUILayout.Space();
				EditorGUILayout.PropertyField( _bossTorches, new GUIContent( "Boss Torches" ), true );
			}

			// Refresh door(s)
			//EditorGUILayout.Space();
			//if( GUILayout.Button( "Refresh Door" ) )
			//	_myTarget.Refresh();
		}

		var autoRefresh = false;
		if( GUI.changed )
		{
			EditorUtility.SetDirty( target );
			EditorUtility.SetDirty( _myTarget );

			autoRefresh = true;
		}

		// Refresh serialized parameters
		_serializedTarget.ApplyModifiedProperties();

		if( autoRefresh )
			_myTarget.Refresh();
	}

	//=====================================================

	private void GetModels()
	{
		// Get list of models for this object-type
		switch( _myTarget.Type )
		{
			case eDoorType.BASIC:
				_models = ResourcesDoors.DoorsBasic;
				break;
			case eDoorType.BASIC_DOUBLE:
				_models = ResourcesDoors.DoorsBasicDouble;
				break;
			case eDoorType.PUZZLE_LOCKED:
				_models = ResourcesDoors.DoorsPuzzleLocked;
				break;
			case eDoorType.CRAWL:
				_models = ResourcesDoors.DoorsCrawl;
				break;
			case eDoorType.OBLIVION_PORTAL:
				_models = ResourcesDoors.DoorsOblivionPortal;
				break;
			case eDoorType.PUZZLE_ENTRANCE:
				_models = ResourcesDoors.DoorsPuzzleEntrance;
				break;
			case eDoorType.PLAYER_HUB:
				_models = ResourcesDoors.DoorsPlayerHub;
				break;
			case eDoorType.BOSS:
				_models = ResourcesDoors.DoorsBoss;
				break;
		}
	}

	//=====================================================
}


//DrawDefaultInspector();
