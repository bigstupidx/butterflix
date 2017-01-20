using UnityEngine;
using System;

[ExecuteInEditMode]
[Serializable]
public class SpawnManager : MonoBehaviour
{
	[Serializable]
	private sealed class Location
	{
		public eLocation _sceneOrigin;
		public Transform _spawnPoint;
	}

	private static SpawnManager _instance;

	[SerializeField] private Location[] _spawnPoints;
	//[SerializeField] private GameObject _spawnPointContainer;
	[SerializeField] private Transform[] _respawnPoints;
	//[SerializeField] private GameObject _respawnPointContainer;

	// Non-serialized
	private Transform _lastRespawnPoint;
	private int _debugRespawnPoint;

	//=====================================================
	// Check for and return an instance of the scene manager
	public static SpawnManager Instance
	{
		get
		{
			if( _instance != null ) return _instance;

			// Look for existing GameManager object in scene
			var sm = GameObject.FindGameObjectWithTag( UnityTags.SceneManager );
			if( sm != null )
			{
				var script = sm.GetComponent<SpawnManager>();
				if( script != null )
					_instance = script;
			}
			// Otherwise, inform dev about missing manager instance
			else
			{
				Debug.LogError( "SpawnManager: instance not found. Please add managers prefab to the scene." );
			}
			return _instance;
		}
	}

	//=====================================================
	// Cycles through existing
	public Transform DebugGetNextRespawnPoint()
	{
		if( _respawnPoints.Length <= 0 || _respawnPoints[0] == null )
		{
			Debug.LogError( "SpawnManager(Debug): No respawn points found. Please add a respawnpoints to the scene." );
			return null;
		}

		// Return next spawnpoint transform
		if( ++_debugRespawnPoint >= _respawnPoints.Length )
			_debugRespawnPoint = 0;

		return _respawnPoints[_debugRespawnPoint];
	}

	//=====================================================

	public Transform GetSpawnPoint()
	{
		if( _spawnPoints.Length <= 0 || _spawnPoints[0] == null )
		{
			Debug.LogError( "SpawnManager: No spawn points found. Please add a spawnpoint to the scene." );
			return null;
		}

		// Find spawnPoint matching the gameManager's lastLocation
		//Debug.Log( "Loc: " + GameManager.Instance.LastLocation );
		foreach( var sp in _spawnPoints )
		{
			if( sp._sceneOrigin == GameManager.Instance.LastLocation )
				return sp._spawnPoint;
		}

		// Otherwise, return the default spawnPoint for this scene
		return _spawnPoints[0]._spawnPoint;
	}

	//=====================================================

	public Transform GetRespawnPoint()
	{
		if( _respawnPoints.Length <= 0 || _respawnPoints[0] == null )
		{
			Debug.LogError( "SpawnManager: No respawn points found. Please add a respawn point to the scene." );
			return null;
		}

		// Find spawnPoint matching the gameManager's lastLocation
		//Debug.Log( "Loc: " + GameManager.Instance.LastLocation );
		//foreach( var sp in _spawnPoints )
		//{
		//	if( sp._sceneOrigin == GameManager.Instance.LastLocation )
		//		return sp._spawnPoint;
		//}

		// ToDo: Return correct spawnPoint for this scene
		foreach( var rsp in _respawnPoints )
		{
			if( rsp == _lastRespawnPoint )
				return rsp;
		}

		// If respawn point not found then returning default scene spawn point for now
		if( _spawnPoints.Length <= 0 || _spawnPoints[0] == null )
		{
			Debug.LogError( "SpawnManager: No spawn points found. Please add a spawn point to the scene." );
			return null;
		}

		return _spawnPoints[0]._spawnPoint;
	}

	//=====================================================

	public void ResetRespawnPoints()
	{
		if( _respawnPoints.Length <= 0 || _respawnPoints[0] == null )
		{
			Debug.LogError( "SpawnManager: No respawn points found. Please add a respawn point to the scene." );
			return;
		}

		foreach( var spt in _respawnPoints )
		{
			var sp = spt.GetComponent<SpawnPoint>();
			sp.SetActivated( false );
		}
	}

	//=====================================================

	public void SetCurrentRespawnPoint( Transform spt, bool setActive = false )
	{
		_lastRespawnPoint = spt;

		if( setActive == true )
		{
			var sp = spt.GetComponent<SpawnPoint>();
			sp.SetActivated( true );
		}
	}

	//=====================================================

	public void Refresh()
	{
		if( _spawnPoints.Length <= 0 || _spawnPoints[0] == null )
			return;

		//if( _spawnPointContainer == null )
		//{
		//	_spawnPointContainer = new GameObject( "SpawnPoints" );
		//	_spawnPointContainer.transform.position = Vector3.zero;
		//}

		// Update spawn point names
		for( var i = 0; i < _spawnPoints.Length; i++ )
		{
			if( _spawnPoints[i] != null && _spawnPoints[i]._spawnPoint != null )
			{
				_spawnPoints[i]._spawnPoint.name = "SpawnStart" + i.ToString( "00" );
				// Move spawn point into container
				//_spawnPoints[i]._spawnPoint.parent = _spawnPointContainer.transform;
			}
		}

		if( _respawnPoints.Length <= 0 || _respawnPoints[0] == null )
			return;

		// Update respawn point names
		for( var i = 0; i < _respawnPoints.Length; i++ )
		{
			if( _respawnPoints[i] != null && _respawnPoints[i] != null )
				_respawnPoints[i].name = "Respawn" + i.ToString( "00" );
		}
	}

	//=====================================================

	void OnEnable()
	{
		if( Application.isPlaying == false ) return;

		_debugRespawnPoint = 0;
	}

	//=====================================================
}
