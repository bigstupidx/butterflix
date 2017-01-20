using UnityEngine;

[RequireComponent( typeof( Collider ) )]
public class TriggerEnemyFall : MonoBehaviour
{
	//=====================================================

	void OnTriggerEnter( Collider other )
	{
		if( other.tag == UnityTags.Enemy )
			other.GetComponent<EnemyAI>().OnFall();
	}

	//=====================================================

	void OnDrawGizmos()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = Color.white;
		Gizmos.DrawCube( Vector3.zero, Vector3.one );
	}

	//=====================================================
}
