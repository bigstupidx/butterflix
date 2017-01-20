using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// JobManager is just a proxy object so we have a launcher for the coroutines
public class JobManager : MonoBehaviour
{
	// Only one JobManager can exist. We use a singleton pattern to enforce this.
	static JobManager _instance = null;

	//=====================================================

	public static JobManager Instance
	{
		get
		{
			if( !_instance )
			{
				// Check if an JobManager is already available in the scene graph
				_instance = FindObjectOfType( typeof( JobManager ) ) as JobManager;

				// Nope, create a new one
				if( !_instance )
				{
					var obj = new GameObject( "JobManager" );
					_instance = obj.AddComponent<JobManager>();

					DontDestroyOnLoad( obj );
				}
			}

			return _instance;
		}
	}

	//=====================================================

	void OnApplicationQuit()
	{
		// Release reference on exit
		_instance = null;
	}

	//=====================================================
}

public class Job
{
	public event System.Action<bool> jobComplete;

	public bool Running { get; private set; }
	public bool Paused { get; private set; }

	private IEnumerator _coroutine;
	private bool _jobWasKilled;
	private Stack<Job> _childJobStack;

	//=====================================================

	#region Constructors

	public Job( IEnumerator coroutine ) : this( coroutine, true ) { }

	//=====================================================

	public Job( IEnumerator coroutine, bool shouldStart )
	{
		_coroutine = coroutine;

		if( shouldStart )
			Start();
	}

	#endregion

	//=====================================================

	#region Static Job makers

	public static Job Make( IEnumerator coroutine )
	{
		return new Job( coroutine );
	}

	//=====================================================

	public static Job Make( IEnumerator coroutine, bool shouldStart )
	{
		return new Job( coroutine, shouldStart );
	}

	#endregion

	//=====================================================

	private IEnumerator DoWork()
	{
		// null out the first run through in case we start paused
		yield return null;

		while( Running )
		{
			if( Paused )
			{
				yield return null;
			}
			else
			{
				// run the next iteration and stop if we are done
				if( _coroutine.MoveNext() )
				{
					yield return _coroutine.Current;
				}
				else
				{
					// run our child jobs if we have any
					if( _childJobStack != null && _childJobStack.Count > 0 )
					{
						var childJob = _childJobStack.Pop();
						_coroutine = childJob._coroutine;
					}
					else
					{
						Running = false;
					}
				}
			}
		}

		// fire off a complete event
		if( jobComplete != null )
			jobComplete( _jobWasKilled );
	}

	//=====================================================

	#region public API

	public Job CreateAndAddChildJob( IEnumerator coroutine )
	{
		var j = new Job( coroutine, false );
		AddChildJob( j );

		return j;
	}

	//=====================================================

	public void AddChildJob( Job childJob )
	{
		if( _childJobStack == null )
			_childJobStack = new Stack<Job>();

		_childJobStack.Push( childJob );
	}

	//=====================================================

	public void RemoveChildJob( Job childJob )
	{
		if( _childJobStack.Contains( childJob ) )
		{
			var childStack = new Stack<Job>( _childJobStack.Count - 1 );
			var allCurrentChildren = _childJobStack.ToArray();
			System.Array.Reverse( allCurrentChildren );

			for( var i = 0; i < allCurrentChildren.Length; i++ )
			{
				var j = allCurrentChildren[i];
				if( j != childJob )
					childStack.Push( j );
			}

			// assign the new stack
			_childJobStack = childStack;
		}
	}

	//=====================================================

	public void Start()
	{
		Running = true;
		JobManager.Instance.StartCoroutine( DoWork() );
	}

	//=====================================================

	public IEnumerator StartAsCoroutine()
	{
		Running = true;
		yield return JobManager.Instance.StartCoroutine( DoWork() );
	}

	//=====================================================

	public void Pause()
	{
		Paused = true;
	}

	//=====================================================

	public void Unpause()
	{
		Paused = false;
	}

	//=====================================================

	public void Kill()
	{
		_jobWasKilled = true;
		Running = false;
		Paused = false;
	}

	//=====================================================

	public void Kill( float delayInSeconds )
	{
		var delay = (int)(delayInSeconds * 1000);

		new System.Threading.Timer( obj =>
		{
			lock( this )
			{
				Kill();
			}
		}, null, delay, System.Threading.Timeout.Infinite );
	}

	#endregion

	//=====================================================
}