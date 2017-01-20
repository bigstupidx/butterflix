using UnityEngine;
using System.Collections;

[RequireComponent( typeof( Animator ) )]
public class TutorialBrafilius : MonoBehaviourEMS
{
	[SerializeField] private AudioClip _clipFootstep;
	//[SerializeField] private AudioClip _clipTaunt;

	private Animator _animator;
	private AudioSource _audioSource;

	//=====================================================

	#region Public Interface

	public void OnPlayFootstep()
	{
		if( _audioSource.isPlaying == false )
			_audioSource.Play();
	}

	#endregion

	//=====================================================

	#region Private Methods

	void Awake()
	{
		_animator = transform.GetComponent<Animator>();
		_audioSource = transform.GetComponent<AudioSource>();
		_audioSource.clip = _clipFootstep;
	}

	//=====================================================

	void OnEnable()
	{
		PopupTutorial.PopupStartTutorial += OnPopupStartTutorial;
	}

	//=====================================================

	void OnPopupStartTutorial()
	{
		PopupTutorial.PopupStartTutorial -= OnPopupStartTutorial;

		StartCoroutine( PlayTutorial( HashIDs.Tutorial01 ) );
	}

	//=====================================================

	private IEnumerator PlayTutorial( int tutorial )
	{
		yield return new WaitForSeconds( 0.35f );

		_animator.SetTrigger( tutorial );
	}

	#endregion

	//=====================================================
}
