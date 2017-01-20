using UnityEngine;

[RequireComponent( typeof( Animation ) )]
public class EnemyAnimation : MonoBehaviour
{
	private Animation _animation;
	private string _currentAnimation;

	//=====================================================

	public void Stop()
	{
		_animation.Stop();
	}

	//=====================================================

	public void Pause()
	{
		Stop();
	}

	//=====================================================

	public void Resume()
	{
		_animation.Play( _currentAnimation );
	}

	//=====================================================

	public void Spawn()
	{
		_currentAnimation = "Spawn";
		_animation.CrossFade( _currentAnimation );
	}

	//=====================================================

	public void Idle()
	{
		_currentAnimation = "Idle";
		_animation.CrossFade( _currentAnimation );
	}

	//=====================================================

	public void IdleFidget()
	{
		_currentAnimation = "IdleFidget";
		_animation.CrossFade( _currentAnimation );
	}

	//=====================================================

	public void Walk()
	{
		_currentAnimation = "Walk";
		_animation.CrossFade( _currentAnimation );
	}

	//=====================================================

	public void Run()
	{
		_currentAnimation = "Run";
		_animation.CrossFade( _currentAnimation );
	}

	//=====================================================

	public void Explode()
	{
		_currentAnimation = "Explode_DEATH";
		_animation.CrossFade( _currentAnimation );
	}

	//=====================================================

	public void Hit( bool hitFront )
	{
		if(hitFront == true)
		{
			_currentAnimation = "GetHitFront";
			_animation.CrossFade( _currentAnimation );
		}
		else
		{
			_currentAnimation = "GetHitBack";
			_animation.CrossFade( _currentAnimation );
		}
	}

	//=====================================================

	void Awake()
	{
		_animation = GetComponent<Animation>();
	}

	//=====================================================

	void OnEnable()
	{
		_currentAnimation = "Idle";
		_animation.Play( _currentAnimation );
	}

	//=====================================================
}
