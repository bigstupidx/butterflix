using System.Collections;
using UnityEngine;

[RequireComponent( typeof( Collider ) )]
public class CutsceneTrigger : MonoBehaviourEMS, ICutsceneObject
{
	[SerializeField] private CutsceneContainer _cutsceneContainer;
	[SerializeField] protected bool _playOnStart;
	[Range( 0.0f, 2.0f )]
	[SerializeField] protected float _startDelay;
	[SerializeField] protected bool _isTriggeredExternally;
	protected bool _isTriggered;

	//=====================================================

	#region Public Interface

	public void OnTriggerEnter( Collider other )
	{
		if( _isTriggeredExternally ) return;

		if( other.tag != UnityTags.Player ) return;

		if( _playOnStart == false && _isTriggered == false )
			StartCoroutine( PlayCutscene() );
	}

	#endregion

	//=====================================================

	#region ICutsceneObject

	public void OnPlayCutsceneAnimation( int animationIndex = 0 )
	{
		// Do nothing
	}

	//=====================================================

	public LTSpline CameraPath()
	{
		return (_cutsceneContainer != null) ? _cutsceneContainer.CameraPath : null;
	}

	//=====================================================

	public Transform[] CutsceneCameraPoints()
	{
		return (_cutsceneContainer != null) ? _cutsceneContainer.CameraPoints : null;
	}

	//=====================================================

	public Transform CutsceneCameraLookAt()
	{
		return (_cutsceneContainer != null) ? _cutsceneContainer.CameraLookAt : null;
	}

	//=====================================================

	public Transform CutscenePlayerPoint()
	{
		return (_cutsceneContainer != null) ? _cutsceneContainer.PlayerPoint : null;
	}

	//=====================================================

	public float CutsceneDuration()
	{
		return (_cutsceneContainer != null) ? _cutsceneContainer.Duration : 2.5f;
	}

	//=====================================================

	public bool OrientCameraToPath()
	{
		return (_cutsceneContainer != null) && _cutsceneContainer.OrientCameraToPath;
	}

	//=====================================================

	public bool IsFlyThruAvailable()
	{
		return (_cutsceneContainer != null) && _cutsceneContainer.IsFlyThruAvailable;
	}

	//=====================================================

	public CameraPathAnimator GetFlyThruAnimator()
	{
		return (_cutsceneContainer != null) ? _cutsceneContainer.FlyThruAnimator : null;
	}

	#endregion

	//=====================================================

	#region Private Methods

	protected virtual void Awake()
	{
		_isTriggered = false;
	}

	//=====================================================

	protected virtual void OnEnable()
	{
		if( Application.isPlaying == false ) return;

		// Disable collider if trigger is being manually triggered
		if( _isTriggeredExternally )
			transform.GetComponent<Collider>().enabled = false;

		// Get cutscene container
		_cutsceneContainer = transform.GetComponentInChildren<CutsceneContainer>();

		if( _cutsceneContainer == null )
			Debug.LogWarning( "CutsceneTrigger: cutscene object is missing." );
	}

	//=====================================================

	protected void Start()
	{
		if( _playOnStart == true && _isTriggered == false )
		{
			StartCoroutine( PlayCutscene( _startDelay ) );
		}
	}

	//=====================================================

	void OnDrawGizmos()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = new Color( Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.5f );
		Gizmos.DrawCube( Vector3.zero, Vector3.one );
	}

	//=====================================================

	private IEnumerator PlayCutscene( float delay = 0.0f )
	{
		_isTriggered = true;

		yield return new WaitForSeconds( delay );

		CutsceneManager.Instance.OnStartCutscene( eCutsceneType.FLY_THRU, this );
	}

	#endregion

	//=====================================================
}
