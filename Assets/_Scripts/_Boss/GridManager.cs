using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviourEMS, IPauseListener
{
	//=====================================================

	[Serializable]
	public sealed class GridLayout
	{
		[SerializeField] private string[] _rows;

		public string[] Rows { get { return _rows; } set { _rows = value; } }
	}

	//=====================================================

	[SerializeField] private GridBox[] _gridBoxes;
	[SerializeField] private GridLayout[] _gridLayouts;

	private List<GridBox> _activeBoxes;

	//=====================================================

	#region Public Inteface

	public void OnBossDeadEvent()
	{
		if( _gridBoxes.Length == 0 ) return;

		foreach( var box in _gridBoxes )
			box.OnDeath();
	}

	//=====================================================

	public void OnAttack( eBossAttack attackType, int repeatAttacks = 1 )
	{
		if( attackType == eBossAttack.NULL || (int)attackType > (int)eBossAttack.NUM_ATTACKS )
			attackType = eBossAttack.ATTACK_01;

		repeatAttacks = Mathf.Clamp( repeatAttacks, 0, 3 );

		// Select grid layout for attack
		if( _gridLayouts.Length > 0 )
		{
			var layout = Random.Range( 0, _gridLayouts.Length );

			ParseGridLayout( _gridLayouts[layout] );
		}
		else
		{
			Debug.LogError( "Boss Grid Manager: missing layouts for attack grid!" );
			return;
		}

		// Activate grid boxes for current layout
		if( _activeBoxes.Count == 0 ) return;

		foreach( var box in _activeBoxes )
			box.OnAttack( attackType, repeatAttacks );

		//Debug.Log( "GridManager: OnAttack" );
	}

	//=====================================================

	public Vector3[] GetGridLocations()
	{
		var locations = new List<Vector3>( _gridBoxes.Length );

		foreach( var box in _gridBoxes )
		{
			if( box != null )
				locations.Add( box.transform.position );
		}

		return locations.ToArray();
	}

	#endregion

	//=====================================================

	#region IPauseListener

	public void OnPauseEvent( bool isPaused )
	{
		if( isPaused == false ) return;

		if( _gridBoxes.Length == 0 ) return;

		foreach( var box in _gridBoxes )
			box.OnPause();
	}

	#endregion

	//=====================================================

	#region Private Methods

	void Awake()
	{
		_activeBoxes = new List<GridBox>( 16 );
	}

	//=====================================================

	void OnEnable()
	{
		if( Application.isPlaying == false ) return;

		GameManager.Instance.PauseEvent += OnPauseEvent;
		GameManager.Instance.PlayerDeathEvent += OnPlayerDeathEvent;
	}

	//=====================================================

	void OnDisable()
	{
		if( Application.isPlaying == false ) return;

		if( _isAppQuiting == true ) return;

		GameManager.Instance.PauseEvent -= OnPauseEvent;
		GameManager.Instance.PlayerDeathEvent -= OnPlayerDeathEvent;
	}

	//=====================================================

	private void OnPlayerDeathEvent( bool _isPlayerOkToRespawn )
	{
		if( _gridBoxes.Length == 0 ) return;

		foreach( var box in _gridBoxes )
			box.OnDeath();
	}

	//=====================================================

	private void ParseGridLayout( GridLayout gridLayout )
	{
		// Clear active box list
		if( _activeBoxes == null )
			_activeBoxes = new List<GridBox>( 16 );
		else
			_activeBoxes.Clear();

		// Parse array of strings into boxes to be activated
		for( var row = 0; row < gridLayout.Rows.Length; row++ )
		{
			var gridRow = gridLayout.Rows[row];

			if( string.IsNullOrEmpty( gridRow ) ) continue;

			// Rows are a string of comma-separated 1's and 0's e.g. [0,0,1,0,0]
			var columns = gridRow.Split( ',' );

			// Write grid boxes to active box list
			for( var column = 0; column < columns.Length; column++ )
			{
				if( columns[column] == "1" )
				{
					//Debug.Log( "1" );
					_activeBoxes.Add( _gridBoxes[(row * columns.Length) + column] );
				}
			}
			//Debug.Log( "-------" );
		}
	}

	#endregion

	//=====================================================
}
