using UnityEngine;

[RequireComponent( typeof( Collider ) )]
public class TriggerBrafiliusMessage : MonoBehaviour
{
	[SerializeField] private eTutorial _nextTutorial;
	[SerializeField] private bool _isSphereCollider;

	private Collider _collider;

	//=====================================================

	void Awake()
	{
		_collider = transform.GetComponent<Collider>();
	}

	//=====================================================

	void OnTriggerExit( Collider other )
	{
		if( other.tag != UnityTags.Player ) return;

		// Player exits trigger - show tutorial popup then start first Brafilius animation
		if( PopupTutorial.Instance != null )
			PopupTutorial.Instance.Show( _nextTutorial );

		_collider.enabled = false;
	}

	//=====================================================

	void OnDrawGizmos()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = new Color( Color.green.r, Color.green.g, Color.green.b, 0.5f );

		if( _isSphereCollider == true )
			Gizmos.DrawSphere( Vector3.zero, 0.5f );
		else
			Gizmos.DrawCube( Vector3.zero, Vector3.one );
	}

	//=====================================================
}
