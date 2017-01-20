using UnityEngine;
using UnityEditor;

// CanEditMultipleObjects
[CustomEditor( typeof( Switch ) )]
public class SwitchEditor : Editor
{
	private Switch _myTarget;

	private SerializedObject _serializedTarget;

	private string[] _models;

	//=====================================================

	void OnEnable()
	{
		_serializedTarget = new SerializedObject( target );
		_myTarget = (Switch)target;

		// Get list of models for this object-type
		GetModels();
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
			case eSwitchType.FLOOR_LEVER:
				EditorGUILayout.LabelField( "Switch Type: ", "Floor" );
				break;
			case eSwitchType.WALL_SWITCH:
				EditorGUILayout.LabelField( "Switch Type: ", "Wall" );
				break;
			case eSwitchType.PRESSURE:
				EditorGUILayout.LabelField( "Switch Type: ", "Pressure" );
				break;
		}

		// Switch Model
		EditorGUILayout.Space();
		_myTarget.Model = EditorGUILayout.Popup( "Switch Model", _myTarget.Model, _models );

		// Interactive Level - Player's fairy needs to be at min-level in order to interact with the obstacle
		EditorGUILayout.Space();
		_myTarget.InteractiveLevel = EditorGUILayout.IntSlider( "Min Interactive Level", _myTarget.InteractiveLevel, 0, 7 );

		// Pressure switch radius
		//if( _myTarget.Type == eSwitchType.PRESSURE )
		//	_myTarget.Radius = EditorGUILayout.Slider( "Pressure Radius", _myTarget.Radius, 0.1f, 2.5f );
 
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
			case eSwitchType.FLOOR_LEVER:
				_models = ResourcesSwitches.FloorSwitches;
				break;
			case eSwitchType.WALL_SWITCH:
				_models = ResourcesSwitches.WallSwitches;
				break;
			case eSwitchType.PRESSURE:
				_models = ResourcesSwitches.PressureSwitches;
				break;
		}
	}

	//=====================================================
}
