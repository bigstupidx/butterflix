using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( Collectable ) )]
public class CollectablesEditor : Editor
{
	private Collectable _myTarget;

	private SerializedObject _serializedTarget;
	private SerializedProperty _clipGem;
	private SerializedProperty _clipRedGem;
	private SerializedProperty _clipKey;
	private SerializedProperty _keyId;
	private SerializedProperty _matKeyIsCollected;

	//private string[] _models;

	//=====================================================

	void OnEnable()
	{
		//Debug.Log("OnEnable");
		_serializedTarget = new SerializedObject( target );
		_myTarget = (Collectable)target;

		_clipGem = _serializedTarget.FindProperty( "_clipGem" );
		_clipRedGem = _serializedTarget.FindProperty( "_clipRedGem" );
		_clipKey = _serializedTarget.FindProperty( "_clipKey" );
		_keyId = _serializedTarget.FindProperty( "_keyId" );
		_matKeyIsCollected = _serializedTarget.FindProperty( "_matKeyIsCollected" );
		//_switches = _serializedTarget.FindProperty( "_switches" );

		// Get list of models for this object-type
		//GetModels();
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
			case eCollectable.GEM:
				EditorGUILayout.LabelField( "Collectables Type: ", "Gem" );
				break;
			case eCollectable.RED_GEM:
				EditorGUILayout.LabelField( "Collectables Type: ", "Red Gem" );
				break;
			case eCollectable.KEY:
				EditorGUILayout.LabelField( "Collectables Type: ", "Key" );
				break;
			case eCollectable.ANIMAL:
				EditorGUILayout.LabelField( "Collectables Type: ", "Animal" );
				break;
			case eCollectable.HEALTH_GEM:
				EditorGUILayout.LabelField( "Collectables Type: ", "Health" );
				break;
		}

		
		// Collectables Model
		//EditorGUILayout.Space();
		//_myTarget.Model = EditorGUILayout.Popup( "Collectable Model", _myTarget.Model, _models );

		// AudioClips
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField( _clipGem, new GUIContent( "Gem Clip" ), false );
		EditorGUILayout.PropertyField( _clipRedGem, new GUIContent( "Red Gem Clip" ), false );
		EditorGUILayout.PropertyField( _clipKey, new GUIContent( "Key Clip" ), false );

		// Keys only
		switch( _myTarget.Type )
		{
			case eCollectable.KEY:
				EditorGUILayout.Space();
				_keyId.intValue = (int)(ePuzzleKeyType)EditorGUILayout.EnumPopup( "Key Id", (ePuzzleKeyType)_keyId.intValue );

				break;
		}

		// Key material - switch to this for keys already collected
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField( _matKeyIsCollected, new GUIContent( "Material Collected Key" ), true );

		// Refresh collectable
		//EditorGUILayout.Space();
		//if( GUILayout.Button( "Refresh Collectable" ) )
		//	_myTarget.Refresh();

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

	//private void GetModels()
	//{
	//	// Get list of models for this object-type
	//	switch( _myTarget.Type )
	//	{
	//		case eCollectable.GEM:
	//			_models = ResourcesCollectables.Gems;
	//			break;
	//		case eCollectable.RED_GEM:
	//			_models = ResourcesCollectables.RedGems;
	//			break;
	//		case eCollectable.KEY:
	//			_models = ResourcesCollectables.Keys;
	//			break;
	//		//case eCollectable.ANIMAL:
	//		//	_models = ResourcesCollectables.Animals;
	//		//	break;
	//	}
	//}

	//=====================================================
}


//DrawDefaultInspector();
