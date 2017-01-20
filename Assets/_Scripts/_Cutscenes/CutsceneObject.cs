using UnityEngine;

public class CutsceneObject : MonoBehaviour, ICutsceneObject
{
	[SerializeField] private Animator _animator;
	[SerializeField] private string _animationTrigger;
	[SerializeField] private ParticleSystem _particleSystem;

	//=====================================================

	#region ICutsceneObject

	public void OnPlayCutsceneAnimation( int animationIndex = 0 )
	{
		if( _animator != null )
			_animator.SetTrigger( _animationTrigger );

		if( _particleSystem != null )
			_particleSystem.Play();
	}

	public LTSpline CameraPath()
	{
		// Do nothing
		return null;
	}

	public Transform[] CutsceneCameraPoints()
	{
		// Do nothing
		return null;
	}

	public Transform CutsceneCameraLookAt()
	{
		// Do nothing
		return null;
	}

	public Transform CutscenePlayerPoint()
	{
		// Do nothing
		return null;
	}

	public float CutsceneDuration()
	{
		// Do nothing
		return 0.0f;
	}

	public bool OrientCameraToPath()
	{
		// Do nothing
		return false;
	}

	public bool IsFlyThruAvailable()
	{
		// Do nothing
		return false;
	}

	public CameraPathAnimator GetFlyThruAnimator()
	{
		// Do nothing
		return null;
	}

	#endregion

	//=====================================================

	#region Private Methods

	//void Awake()
	//{
	//	_animator = transform.GetComponent<Animator>();
	//	_particleSystem = transform.GetComponent<ParticleSystem>();
	//}

	#endregion

	//=====================================================
}
