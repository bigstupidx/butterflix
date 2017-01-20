using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( MagicalTrap ) )]
public class MagicalTrapEditor : Editor
{
	private MagicalTrap _myTarget;

	private SerializedObject _serializedTarget;
	private SerializedProperty _damageType;
	private SerializedProperty _clipActivate;
	private SerializedProperty _clipBubbleBurst;
	private SerializedProperty _clipDestroy;

	//=====================================================

	void OnEnable()
	{
		_serializedTarget = new SerializedObject( target );
		_myTarget = (MagicalTrap)target;

		_damageType = _serializedTarget.FindProperty( "_damageType" );
		_clipActivate = _serializedTarget.FindProperty( "_clipActivate" );
		_clipBubbleBurst = _serializedTarget.FindProperty( "_clipBubbleBurst" );
		_clipDestroy = _serializedTarget.FindProperty( "_clipDestroy" );
	}

	//=====================================================

	public override void OnInspectorGUI()
	{
		_serializedTarget.Update();

		EditorGUILayout.Space();
		switch( _myTarget.Type )
		{
			case eObstacleType.MAGICAL_TRAP_ICE:
				EditorGUILayout.LabelField( "Obstacle Type: ", "Magical Trap - Ice" );
				break;
			case eObstacleType.MAGICAL_TRAP_WIND:
				EditorGUILayout.LabelField( "Obstacle Type: ", "Magical Trap - Wind" );
				break;
		}

		// AudioClips
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField( _clipActivate, new GUIContent( "Activate Clip" ), false );
		EditorGUILayout.PropertyField( _clipBubbleBurst, new GUIContent( "Bubble Burst Clip" ), false );
		EditorGUILayout.PropertyField( _clipDestroy, new GUIContent( "Destroy Clip" ), false );

		EditorGUILayout.Space();
		// Damage Level
		_damageType.intValue = (int)(eDamageType)EditorGUILayout.EnumPopup( "Damage Type", (eDamageType)_damageType.intValue );

		// Difficulty Level - Determines how many globes appear around trap and time-penalty on duration
		EditorGUILayout.Space();
		_myTarget.DifficultyLevel = EditorGUILayout.IntSlider( "Diffculty Level", _myTarget.DifficultyLevel, 0, 7 );

		// Active Duration - Duration spell traps player before exploding
		EditorGUILayout.Space();
		_myTarget.ActiveDuration = EditorGUILayout.IntSlider( "Active Duration", (int)_myTarget.ActiveDuration, 5, 15 );

		EditorGUILayout.Space();
		EditorGUILayout.LabelField( "Spinning Globes" );

		// Radius X - Determines distance between globes and trap-centre
		EditorGUILayout.Space();
		_myTarget.RadiusX = EditorGUILayout.Slider( "Radius X-Axis", _myTarget.RadiusX, 1.0f, 3.0f );

		// Radius Z - Determines distance between globes and trap-centre
		EditorGUILayout.Space();
		_myTarget.RadiusZ = EditorGUILayout.Slider( "Radius Z-Axis", _myTarget.RadiusZ, 1.0f, 3.0f );

		EditorGUILayout.Space();
		_myTarget.IsCircular = EditorGUILayout.Toggle( "Force Circular", _myTarget.IsCircular );

		EditorGUILayout.Space();
		_myTarget.IsElipse = EditorGUILayout.Toggle( "Force Elipse", _myTarget.IsElipse );

		//var autoRefresh = false;
		if( GUI.changed )
		{
			EditorUtility.SetDirty( target );
			EditorUtility.SetDirty( _myTarget );

			//autoRefresh = true;
		}

		// Refresh serialized parameters
		_serializedTarget.ApplyModifiedProperties();

		//if( autoRefresh )
		//	_myTarget.Refresh();

		// Refresh trap
		//EditorGUILayout.Space();
		//if( GUILayout.Button( "Refresh Obstacle" ) )
		//	_myTarget.Refresh();
	}

	//=====================================================
}
