using UnityEditor;

[CustomEditor( typeof( SpawnPoint ) )]
public class SpawnPointEditor : Editor
{
	private SpawnPoint _myTarget;

	//private SerializedObject _serializedSPoint;
	//private SerializedProperty _switchType;

	//=====================================================

	void OnEnable()
	{
		//_serializedSPoint = new SerializedObject( target );
		_myTarget = (SpawnPoint)target;
		//_switchType		= _serializedSPoint.FindProperty( "_switchType" );
	}

	//=====================================================

	public override void OnInspectorGUI()
	{
		//_serializedSwitch.Update();
		
		// Set / display all coded defaults
		//base.OnInspectorGUI();
		
		EditorGUILayout.Space();
		switch( _myTarget.Type )
		{
			case eSpawnType.SCENE_START:
				EditorGUILayout.LabelField( "Spawn Type: ", "Scene-Start" );
				break;
			case eSpawnType.RESPAWN:
				EditorGUILayout.LabelField( "Spawn Type: ", "Respawn" );
				break;
			case eSpawnType.CRAWL_THROUGH:
				EditorGUILayout.LabelField( "Spawn Type: ", "Crawl-Through" );
				break;
			case eSpawnType.OBLIVION_PORTAL:
				EditorGUILayout.LabelField( "Spawn Type: ", "Oblivion-Portal" );
				break;
		}

		// Refresh switch
		//EditorGUILayout.Space();
		//if( GUILayout.Button( "Refresh SpawnPoint" ) )
		//	_myTarget.Refresh();

		// Refresh serialized switch parameters
		//_serializedSwitch.ApplyModifiedProperties();
	}

	//=====================================================
}


//DrawDefaultInspector();
