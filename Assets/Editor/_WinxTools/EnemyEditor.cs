using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( Enemy ) )]
public class EnemyEditor : Editor
{
	private Enemy _myTarget;

	private SerializedObject _serializedTarget;

	//private SerializedProperty _model;

	//=====================================================

	void OnEnable()
	{
		_serializedTarget = new SerializedObject( target );
		_myTarget = (Enemy)target;

		// Get list of models for this object-type
		//switch( _myTarget.Type )
		//{
		//	case eObstacleType.PUSHABLE_BOX:
		//		_modelsArm = ResourcesObstacles.PushableBoxes;
		//		break;
		//}
	}

	//=====================================================

	public override void OnInspectorGUI()
	{
		_serializedTarget.Update();

		EditorGUILayout.Space();
		EditorGUILayout.LabelField( "Enemy Type: ", "Shadow Creature" );
		
		//EditorGUILayout.Space();
		//switch( _myTarget.Type )
		//{
		//	case eSpawnType.SCENE_START:
		//		EditorGUILayout.LabelField( "Spawn Type: ", "Scene-Start" );
		//		break;
		//	case eSpawnType.RESPAWN:
		//		EditorGUILayout.LabelField( "Spawn Type: ", "Respawn" );
		//		break;
		//}

		// Tigger Target Radius
		EditorGUILayout.Space();
		_myTarget.RadiusTriggerForTarget = EditorGUILayout.Slider( "Tigger Target Radius", _myTarget.RadiusTriggerForTarget, 4.0f, 10.0f );

		// Trigger Interactive Radius
		//EditorGUILayout.Space();
		//_myTarget.RadiusTriggerInteractive = EditorGUILayout.Slider( "Trigger Interactive Radius", _myTarget.RadiusTriggerInteractive, 2.0f, 4.0f );

		// Recovery Duration
		EditorGUILayout.Space();
		_myTarget.PreAttackDelay = EditorGUILayout.Slider( "Pre EXPLODE Delay", _myTarget.PreAttackDelay, 1.0f, 5.0f );

		// Refresh switch
		//EditorGUILayout.Space();
		//if( GUILayout.Button( "Refresh SpawnPoint" ) )
		//	_myTarget.Refresh();

		var autoRefresh = false;
		if( GUI.changed )
		{
			EditorUtility.SetDirty( target );
			EditorUtility.SetDirty( _myTarget );

			autoRefresh = true;
		}

		if( autoRefresh )
			_myTarget.Refresh();

		// Refresh serialized switch parameters
		_serializedTarget.ApplyModifiedProperties();
	}

	//=====================================================
}


//DrawDefaultInspector();
