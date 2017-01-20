using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( NPCManager ) )]
public class NPCManagerEditor : Editor
{
	private NPCManager _myTarget;

	private SerializedObject _serializedTarget;
	//private SerializedProperty _pfbNPCs;
	private SerializedProperty _wayPoints;
	private SerializedProperty _emoticons;

	//=====================================================

	void OnEnable()
	{
		_serializedTarget = new SerializedObject( target );
		_myTarget = (NPCManager)target;

		//_pfbNPCs = _serializedTarget.FindProperty( "_pfbNPCs" );
		_wayPoints = _serializedTarget.FindProperty( "_wayPoints" );
		_emoticons = _serializedTarget.FindProperty( "_emoticons" );
	}

	//=====================================================

	public override void OnInspectorGUI()
	{
		_serializedTarget.Update();
		// Set / display all coded defaults
		//base.OnInspectorGUI();

		//EditorGUILayout.Space();
		//EditorGUILayout.LabelField( "Enemy Manager", "" );

		// NPC Prefabs
		//EditorGUILayout.Space();
		//EditorGUILayout.PropertyField( _pfbNPCs, new GUIContent( "NPC Prefabs" ), true );

		// Way / Spawn Points
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField( _wayPoints, new GUIContent( "Way / Spawn Points" ), true );

		// Emoticon Sprites
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField( _emoticons, new GUIContent( "Emoticons Sprites" ), true );

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
}
