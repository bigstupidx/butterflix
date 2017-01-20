using UnityEngine;
using System.Collections.Generic;

public class GemManager : MonoBehaviour
{

	[SerializeField]
	private GameObject _pfbGem;

	private Vector3[] _locations;
	private List<int> _activeGemIndexes;

	#region Public Interface

	public Vector3[] Locations { set { _locations = value; } }

	//=====================================================

	public void BossSpawnsGems( int numGems )
	{
		if( _locations == null ) return;

		if( numGems - _activeGemIndexes.Count <= 0 ) return;

		do
		{
			// Select next gem index (grid position)
			var index = GetNewGemIndex();

			_activeGemIndexes.Add( index );

			// Create gem
			var gem = Instantiate( _pfbGem, _locations[index] + Vector3.up, Quaternion.identity ) as GameObject;
			if( gem == null ) continue;

			gem.GetComponent<Collectable>().GridIndex = index;
			gem.GetComponent<Collectable>().ConsumedEvent += OnConsumedEvent;

		} while( _activeGemIndexes.Count < numGems );
	}

	#endregion

	//=====================================================

	#region Private Methods

	void Awake()
	{
		_activeGemIndexes = new List<int>();
	}

	//=====================================================

	private void OnConsumedEvent( int gridIndex )
	{
		if( _activeGemIndexes == null || _activeGemIndexes.Count == 0 ) return;

		_activeGemIndexes.Remove( gridIndex );
		_activeGemIndexes.TrimExcess();
	}

	//=====================================================

	private int GetNewGemIndex()
	{
		if( _locations == null || _locations.Length == 0 ) return 0;

		int index;
		bool isIndexOk;

		do
		{
			isIndexOk = true;
			index = Random.Range( 0, _locations.Length );

			// Check against active gems
			foreach( var i in _activeGemIndexes )
			{
				if( index != i ) continue;

				isIndexOk = false;
				break;
			}

		} while( isIndexOk == false );

		return index;
	}

	#endregion

	//=====================================================
}
