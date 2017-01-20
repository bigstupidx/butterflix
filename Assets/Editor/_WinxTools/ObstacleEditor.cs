using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( Obstacle ) )]
public class ObstacleEditor : Editor
{
	private Obstacle _myTarget;

	private SerializedObject _serializedTarget;

	private string[] _modelsArm;


	//=====================================================

	void OnEnable()
	{
		_serializedTarget = new SerializedObject( target );
		_myTarget = (Obstacle)target;

		// Get list of models for this object-type
		switch( _myTarget.Type )
		{
			case eObstacleType.PUSHABLE_BOX:
				_modelsArm = ResourcesObstacles.PushableBoxes;
				break;
		}
	}

	//=====================================================

	public override void OnInspectorGUI()
	{
		_serializedTarget.Update();

		EditorGUILayout.Space();
		switch( _myTarget.Type )
		{
			case eObstacleType.PUSHABLE_BOX:
				EditorGUILayout.LabelField( "Obstacle Type: ", "Pushable Box" );
				break;
		}

		// Switch Model Arms
		EditorGUILayout.Space();
		_myTarget.Model = EditorGUILayout.Popup( "Obstacle", _myTarget.Model, _modelsArm );

		switch( _myTarget.Type )
		{
			case eObstacleType.PUSHABLE_BOX:
				// Interactive Level - Player's fairy needs to be at min-level in order to interact with the obstacle
				EditorGUILayout.Space();
				_myTarget.InteractiveLevel = EditorGUILayout.IntSlider( "Min Interactive Level", _myTarget.InteractiveLevel, 0, 7 );
				break;
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

		// Refresh door(s)
		EditorGUILayout.Space();
		if( GUILayout.Button( "Refresh Obstacle" ) )
			_myTarget.Refresh();
	}

	//=====================================================
}
