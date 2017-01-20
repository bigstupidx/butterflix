using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( CollectableHealth ) )]
public class CollectableHealthEditor : Editor
{
	private Collectable _myTarget;

	private SerializedObject _serializedTarget;
	private SerializedProperty _clipGem;

	//=====================================================

	void OnEnable()
	{
		//Debug.Log("OnEnable");
		_serializedTarget = new SerializedObject( target );
		_myTarget = (Collectable)target;
		_clipGem = _serializedTarget.FindProperty( "_clipGem" );
	}

	//=====================================================

	public override void OnInspectorGUI()
	{
		_serializedTarget.Update();

		EditorGUILayout.Space();
		EditorGUILayout.LabelField( "Collectables Type: ", "Health" );

		// AudioClips
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField( _clipGem, new GUIContent( "Gem Clip" ), false );

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
