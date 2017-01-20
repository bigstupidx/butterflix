using UnityEngine;

public class ResourcesNPCs
{
	private const int _maxStudents = 12;

	//=====================================================

	public static Object GetSpecialsPrefab( int npcIndex )
	{
		var path = "Prefabs/NPCs/Specials/";

		switch( npcIndex )
		{
			case 1:
				path += "pfbEldora";
				break;
			case 2:
				path += "pfbDufour";
				break;
			case 3:
				path += "pfbAvalon";
				break;
			case 4:
				path += "pfbPalladium";
				break;
			case 5:
				path += "pfbWizgiz";
				break;
			case 6:
				path += "pfbGriselda";
				break;
			case 7:
				path += "pfbFaragonda";
				break;
			case 8:
				path += "pfbRoxy";
				break;
			case 9:
				path += "pfbDaphne";
				break;
			default:
				return GetStudentPrefab();
		}

		var prefab = Resources.Load( path );

		if( prefab != null )
			return prefab;

		Debug.Log( "NPC specials prefab not found in resources" );
		return null;
	}

	//=====================================================

	public static Object GetStudentPrefab()
	{
		const string path = "Prefabs/NPCs/Students/pfbStudent";

		var randStudent = Random.Range( 0, _maxStudents - 1 );

		var prefab = Resources.Load( path + randStudent.ToString("00") );

		//Debug.Log( "Student: " + path + randStudent.ToString( "00" ) );

		if( prefab != null )
			return prefab;

		Debug.Log( "NPC student prefab not found in resources" );
		return null;
	}

	//=====================================================
}
