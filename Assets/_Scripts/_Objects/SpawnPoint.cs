using UnityEngine;
using System;

[ExecuteInEditMode]
[Serializable]
public class SpawnPoint : MonoBehaviour
{
	[SerializeField] private eSpawnType _type = eSpawnType.SCENE_START;

	// Non-serialized
	private Transform _thisTransform;
	private AudioSource _audioSource;
	private bool _isActivated;

	//=====================================================

	public eSpawnType Type { get { return _type; } set { _type = value; } }

	public bool IsActivated { get { return _isActivated; } }

	//=====================================================

	public void SetActivated( bool isActivated )
	{
		//if( _isActivated == isActivated ) return;
		if( _type != eSpawnType.RESPAWN ) return;

		_isActivated = isActivated;

		var ps = this.GetComponentInChildren<ParticleSystem>();
		var childHighlight = transform.Find( "Respawn_Highlight" );

		if( ps == null )
			Debug.LogError( "Spawn point has no particle system: " + transform.name );
		if( childHighlight == null )
			Debug.LogError( "Spawn point has no child object called 'Respawn_Highlight': " + transform.name );

		if( ps == null || childHighlight == null ) return;

		// When activated start particle emission and highlight game objects
		if( isActivated )
		{
			ps.enableEmission = true;
			childHighlight.GetComponent<Renderer>().enabled = true;

			// Play sfx
			if( _audioSource != null )
				_audioSource.Play();
		}
		else
		{
			ps.enableEmission = false;
			childHighlight.GetComponent<Renderer>().enabled = false;
		}
	}

	//=====================================================

	//public void Refresh()
	//{
	//	switch( _type )
	//	{
	//		case eSpawnType.SCENE_START:
	//			_thisTransform.name = "SpawnStart";
	//			break;
	//		case eSpawnType.RESPAWN:
	//			_thisTransform.name = "Respawn";
	//			break;
	//		case eSpawnType.CRAWL_THROUGH:
	//			_thisTransform.name = "SpawnCrawl";
	//			break;
	//		case eSpawnType.OBLIVION_PORTAL:
	//			_thisTransform.name = "SpawnPortal";
	//			break;
	//	}
	//}

	//=====================================================

	void Awake()
	{
		_thisTransform = this.transform;
		_audioSource = _thisTransform.GetComponent<AudioSource>();
		_isActivated = false;
	}

	//=====================================================

	void OnEnable()
	{
		//Refresh();
	}

	//=====================================================

	void OnDrawGizmos()
	{
		// Allow for rotations
		Gizmos.DrawIcon( _thisTransform.position + new Vector3( 0.0f, 0.3f, 0.0f ), "TickIcon.png", false );

		Gizmos.color = GetGizmoColor();
		Gizmos.DrawSphere( _thisTransform.position, 0.1f );

		DrawArrow.ForGizmo( _thisTransform.position + new Vector3( 0.0f, 0.05f, 0.0f ), _thisTransform.forward, 0.4f );
	}

	//=====================================================

	private Color GetGizmoColor()
	{
		switch( _type )
		{
			case eSpawnType.CRAWL_THROUGH:
				return Color.magenta;
			case eSpawnType.OBLIVION_PORTAL:
				return Color.red;
			default:
				return Color.white;
		}
	}

	//=====================================================
}
