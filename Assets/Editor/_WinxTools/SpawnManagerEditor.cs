using UnityEngine;
using UnityEditor;
using System;

//using Rotorz.ReorderableList;

[CustomEditor( typeof( SpawnManager ) )]
public class SpawnManagerEditor : Editor
{
	private SpawnManager _myTarget;

	private SerializedObject _serializedTarget;
	private SerializedProperty _spawnPoints;
	private SerializedProperty _respawnPoints;

	//=====================================================

	void OnEnable()
	{
		_serializedTarget = new SerializedObject( target );
		_myTarget = (SpawnManager)target;

		_spawnPoints = _serializedTarget.FindProperty( "_spawnPoints" );
		_respawnPoints = _serializedTarget.FindProperty( "_respawnPoints" );
	}

	//=====================================================

	public override void OnInspectorGUI()
	{
		_serializedTarget.Update();

		// Set / display all coded defaults
		//base.OnInspectorGUI();

		// Spawn Points
		EditorGUILayout.PropertyField( _spawnPoints, true );
		//ReorderableListGUI.Title( "Spawn Points" );
		//ReorderableListGUI.ListField( _spawnPoints, ReorderableListFlags.ShowIndices );

		// RespawnPoints
		EditorGUILayout.PropertyField( _respawnPoints, true );

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
