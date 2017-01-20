using UnityEngine;

[RequireComponent( typeof( Collider ) )]
public class TriggerAudioZone : MonoBehaviour
{
	[SerializeField] private eAudioZoneType _zoneType;

	//=====================================================

	public void OnTriggerEnter( Collider other )
	{
		if( other.tag != UnityTags.Player ) return;
		
		if( AudioZonesManager.Instance != null )
			AudioZonesManager.Instance.BlendSnapshot( _zoneType );
	}

	//=====================================================

	void OnEnable()
	{
		transform.GetComponent<Collider>().isTrigger = true;
	}

	//=====================================================

	void OnDrawGizmos()
	{
		switch( _zoneType )
		{
			case eAudioZoneType.SMALL_ROOM:
				Gizmos.color = new Color( Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.25f );
				break;
			case eAudioZoneType.LARGE_ROOM:
				Gizmos.color = new Color( Color.blue.r, Color.blue.g, Color.blue.b, 0.25f );
				break;
			case eAudioZoneType.HALL:
				Gizmos.color = new Color( Color.green.r, Color.green.g, Color.green.b, 0.25f );
				break;
		}

		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawCube( Vector3.zero, Vector3.one );
	}

	//=====================================================
}
