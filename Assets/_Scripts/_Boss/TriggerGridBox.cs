using UnityEngine;

[RequireComponent( typeof( Collider ) )]
public class TriggerGridBox : MonoBehaviour
{
	private ITargetWithinRange _parent;

	//=====================================================

	public void Init( ITargetWithinRange parent )
	{
		_parent = parent;
	}

	//=====================================================

	void OnTriggerEnter( Collider other )
	{
		if( other.tag == UnityTags.Player )
			_parent.OnTargetWithinRange( other.transform, true );
	}

	//=====================================================

	void OnTriggerExit( Collider other )
	{
		if( other.tag == UnityTags.Player )
			_parent.OnTargetLost();
	}

	//=====================================================

	void OnDrawGizmos()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = Color.yellow;
		Gizmos.DrawCube( Vector3.zero, Vector3.one );
	}

	//=====================================================
}
