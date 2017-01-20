using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( NPC ) )]
public class NPCEditor : Editor
{
	private NPC _myTarget;

	private SerializedObject _serializedTarget;

	private SerializedProperty _npcType;
	private SerializedProperty _clipFootstep;

	//=====================================================

	void OnEnable()
	{
		_serializedTarget = new SerializedObject( target );
		_myTarget = (NPC)target;

		_npcType = _serializedTarget.FindProperty( "_type" );
		_clipFootstep = _serializedTarget.FindProperty( "_clipFootstep" );

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
		_npcType.intValue = (int)(eNPC)EditorGUILayout.EnumPopup( "NPC Type", (eNPC)_npcType.intValue );

		// AudioClips
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField( _clipFootstep, new GUIContent( "Footstep Clip" ), false );

		// Tigger Target Radius
		EditorGUILayout.Space();
		_myTarget.TriggerTargetRadius = EditorGUILayout.Slider( "Tigger Target Radius", _myTarget.TriggerTargetRadius, 3.0f, 8.0f );

		// Trigger Interactive Radius
		//EditorGUILayout.Space();
		//_myTarget.RadiusTriggerInteractive = EditorGUILayout.Slider( "Trigger Interactive Radius", _myTarget.RadiusTriggerInteractive, 2.0f, 4.0f );

		var autoRefresh = false;
		if( GUI.changed )
		{
			EditorUtility.SetDirty( target );
			EditorUtility.SetDirty( _myTarget );

			autoRefresh = true;
		}

		// Refresh serialized switch parameters
		_serializedTarget.ApplyModifiedProperties();

		if( autoRefresh )
			_myTarget.Refresh();
	}

	//=====================================================
}


//DrawDefaultInspector();
