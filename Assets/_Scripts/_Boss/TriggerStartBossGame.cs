using UnityEngine;

public class TriggerStartBossGame : MonoBehaviour
{
	[SerializeField]
	private BossManager _bossManager;
	private bool _isTriggered;

	//=====================================================

	public void Awake()
	{
		_isTriggered = false;
	}

	//=====================================================

	public void OnTriggerEnter( Collider collision )
	{
		if( _isTriggered == true ) return;
		
		if( collision.tag != UnityTags.Player) return;
		
		if( _bossManager == null ) return;

		_isTriggered = true;
		_bossManager.OnStartGame();
	}

	//=====================================================

	void OnDrawGizmos()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = Color.cyan;
		Gizmos.DrawCube( Vector3.zero, Vector3.one );
	}

	//=====================================================
}
