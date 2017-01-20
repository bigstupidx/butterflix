using UnityEngine;
using System.Collections;

public class TriggerActivateBossShield : MonoBehaviour
{
	[SerializeField] private BossShield[] _shields;
	[Range( 0.5f, 5.0f )]
	[SerializeField] float _delayActivation;

	//=====================================================

	public void OnTriggerEnter( Collider other )
	{
		if( other.tag != UnityTags.Player ) return;

		if( _shields.Length == 0 ) return;

		StartCoroutine( ActivateShields() );
	}

	//=====================================================

	private IEnumerator ActivateShields()
	{
		yield return new WaitForSeconds( _delayActivation );

		// Activate shields
		foreach( var shield in _shields )
			shield.OnActivate();
	}

	//=====================================================
}
