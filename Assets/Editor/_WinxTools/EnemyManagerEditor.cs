using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( EnemyManager ) )]
public class EnemyManagerEditor : Editor
{
	private EnemyManager _myTarget;

	private SerializedObject _serializedTarget;
	private SerializedProperty _doorwayTriggers;
	private SerializedProperty _wayPoints;

	//=====================================================

	void OnEnable()
	{
		_serializedTarget = new SerializedObject( target );
		_myTarget = (EnemyManager)target;

		_doorwayTriggers = _serializedTarget.FindProperty( "_doorwayTriggers" );
		_wayPoints = _serializedTarget.FindProperty( "_wayPoints" );
	}

	//=====================================================

	public override void OnInspectorGUI()
	{
		_serializedTarget.Update();
		// Set / display all coded defaults
		//base.OnInspectorGUI();

		//EditorGUILayout.Space();
		//EditorGUILayout.LabelField( "Enemy Manager", "" );

		// Doorway Triggers
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField( _doorwayTriggers, new GUIContent( "Doorway Triggers" ), true );

		// Way / Spawn Points
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField( _wayPoints, new GUIContent( "Way / Spawn Points" ), true );

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
