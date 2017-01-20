using UnityEngine;
using UnityEditor;
using System;

using Rotorz.ReorderableList;

[CustomEditor( typeof( Platform ) )]
public class PlatformEditor : Editor
{
	private Platform _myTarget;

	private SerializedObject _serializedTarget;
	private SerializedProperty _pathParameter;
	private SerializedProperty _pathNodes;
	private SerializedProperty _switches;

	private string[] _models;

	//=====================================================

	void OnEnable()
	{
		_serializedTarget = new SerializedObject( target );
		_myTarget = (Platform)target;

		_pathParameter = _serializedTarget.FindProperty( "_pathParameter" );
		_pathNodes = _serializedTarget.FindProperty( "_pathNodes" );
		_switches = _serializedTarget.FindProperty( "_switches" );

		// Get list of models for this object-type
		_models = ResourcesPlatforms.Platforms;
	}

	//=====================================================

	public override void OnInspectorGUI()
	{
		_serializedTarget.Update();
		// Set / display all coded defaults
		//base.OnInspectorGUI();

		EditorGUILayout.Space();
		switch( _myTarget.Type )
		{
			case ePlatformType.ON_PATH:
				EditorGUILayout.LabelField( "Platform Type: ", "On Path" );
				break;
			case ePlatformType.ON_SPLINE:
				EditorGUILayout.LabelField( "Platform Type: ", "On Spline" );
				break;
		}

		// Switch Model
		EditorGUILayout.Space();
		_myTarget.Model = EditorGUILayout.Popup( "Platform Model", _myTarget.Model, _models );

		// Path Parameter - one_shot, ping_pong, loop
		EditorGUILayout.Space();
		_pathParameter.enumValueIndex = (int)(Platform.ePathParameter)EditorGUILayout.EnumPopup( "Path Parameter",
											 (Platform.ePathParameter)Enum.ToObject( typeof( Platform.ePathParameter ), _pathParameter.enumValueIndex ) );

		// Platform Speed
		EditorGUILayout.Space();
		switch( _myTarget.Type )
		{
			case ePlatformType.ON_PATH:
				_myTarget.DurationNodeToNode = (float)EditorGUILayout.IntSlider( "Time: Between Nodes", (int)_myTarget.DurationNodeToNode, 1, 20 );
				break;
			case ePlatformType.ON_SPLINE:
				_myTarget.DurationStartToEnd = (float)EditorGUILayout.IntSlider( "Time: Start To End", (int)_myTarget.DurationStartToEnd, 1, 20 );
				break;
		}

		// Path Nodes
		ReorderableListGUI.Title( "Path Nodes" );
		ReorderableListGUI.ListField( _pathNodes, ReorderableListFlags.ShowIndices );

		// Platform Switches
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField( _switches, new GUIContent( "Platform Switches" ), true );

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
