using UnityEngine;
using UnityEditor;
using System;

[CustomEditor( typeof( SwingingObstacle ) )]
public class SwingingObstacleEditor : Editor
{
	private SwingingObstacle _myTarget;

	private SerializedObject _serializedTarget;

	private string[] _modelsArm;
	private string[] _modelsBody;


	//=====================================================

	void OnEnable()
	{
		_serializedTarget = new SerializedObject( target );
		_myTarget = (SwingingObstacle)target;

		// Get list of models for this object-type
		_modelsArm = ResourcesObstacles.ObstaclesSwingingArm;
		_modelsBody = ResourcesObstacles.ObstaclesSwingingBody;
	}

	//=====================================================

	public override void OnInspectorGUI()
	{
		_serializedTarget.Update();
		// Set / display all coded defaults
		//base.OnInspectorGUI();

		EditorGUILayout.Space();
		EditorGUILayout.LabelField( "Obstacle Type: ", "Swinging" );

		// Switch Model Arms
		EditorGUILayout.Space();
		_myTarget.Model = EditorGUILayout.Popup( "Swinging Arm", _myTarget.Model, _modelsArm );

		// Switch Model Bodies
		EditorGUILayout.Space();
		_myTarget.ModelBody = EditorGUILayout.Popup( "Swinging Body", _myTarget.ModelBody, _modelsBody );

		// Maximum Rotation
		EditorGUILayout.Space();
		_myTarget.RotationMax = (float)EditorGUILayout.IntSlider( "Maximum Rotation", (int)_myTarget.RotationMax, 10, 60 );

		// Rotation Duration
		EditorGUILayout.Space();
		_myTarget.RotationDuration = EditorGUILayout.Slider( "Rotation Duration", _myTarget.RotationDuration,  0.5f, 5.0f );

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
