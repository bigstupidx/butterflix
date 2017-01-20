using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SoundFXManager : MonoBehaviour
{
	// Static instance
	public 	static SoundFXManager	    	instance;

	[System.Serializable]
	public class SoundEntry
	{
		public	string				Name;
		public	AudioClip			Clip;
	};

	public float					curVolume				= 1.0f;
	public AudioSource				audioSource01			= null;
	//public AudioSource				audioSource02			= null;
	public SoundEntry[]				SoundList;

	//public AudioClip[]				animalAngry;
	//public AudioClip[]				obstacleHit;

	private AudioSource				characterAudioSource	= null;
	//private eCharacter				currentCharacter		= eCharacter.NULL;
	//private AudioClip[]				characterHits			= null;
	//private AudioClip[]				characterTricks			= null;
	//private AudioClip[]				characterAbilities		= null;
	//private AudioClip[]				characterFall			= null;
	//private AudioClip[]				characterCelebrate		= null;
	//private AudioClip[]				characterLose			= null;
	//private AudioClip[]				characterSlide			= null;

	//private bool					isInTransition = false;
	//private	int						transitionDir = 0;
	//private	float					transitionVolume = 0.0f;


	//=============================================================================

	//public void PlayCharacterHit()
	//{
	//	if( characterHits != null && characterHits.Length > 0 )
	//	{
	//		int i = Random.Range( 0, characterHits.Length );
	//		characterAudioSource.PlayOneShot( characterHits[i] );
	//	}
	//}

	//=============================================================================

	//public void PlayCharacterSpecialAbility()
	//{
	//	// Only one sound per character
	//	if( characterAbilities != null && characterAbilities.Length > 0 )
	//	{
	//		characterAudioSource.PlayOneShot( characterAbilities[0] );
	//	}
	//}

	//=============================================================================

	//public void PlayCharacterSlide()
	//{
	//	// Only one sound per character
	//	if( characterSlide != null && characterSlide.Length > 0 )
	//	{
	//		characterAudioSource.PlayOneShot( characterSlide[0] );
	//	}
	//}

	//=============================================================================

	//public void PlayObstacleHit()
	//{
	//	if( obstacleHit != null && obstacleHit.Length > 0 )
	//	{
	//		int i = Random.Range( 0, obstacleHit.Length );
	//		audioSource01.pitch = 1.0f;
	//		audioSource01.PlayOneShot( obstacleHit[i] );
	//	}
	//}

	//=============================================================================

	//public void PlayAnimalAngry( eAnimal animal )
	//{
	//	audioSource01.pitch = 1.0f;

	//	switch( animal )
	//	{
	//		case eAnimal.SQUIRREL:
	//			audioSource01.PlayOneShot( animalAngry[0] );
	//			break;

	//		case eAnimal.RACCOON:
	//			audioSource01.PlayOneShot( animalAngry[1] );
	//			break;

	//		case eAnimal.BEAR:
	//			audioSource01.PlayOneShot( animalAngry[2] );
	//			break;

	//		case eAnimal.OWL:
	//			audioSource01.PlayOneShot( animalAngry[3] );
	//			break;
	//	}
	//}

	//=============================================================================

	//public void PlaySoundPitched( string Name, float pitch )
	//{
	//	if( PlayerManager.SoundOn == false )
	//		return;

	//	// Find sound fx
	//	int FoundIdx = -1;
	//	for( int Idx = 0; Idx < SoundList.Length; Idx++ )
	//	{
	//		if( SoundList[Idx].Name == Name )
	//		{
	//			FoundIdx = Idx;
	//			break;
	//		}
	//	}

	//	if( FoundIdx == -1 )
	//	{
	//		Debug.Log( "Unknown sound: " + Name );
	//		return;
	//	}

	//	if( SoundList[FoundIdx].Clip == null )
	//	{
	//		Debug.Log( "Null sound prefab for: " + Name );
	//		return;
	//	}

	//	//Debug.Log( FoundIdx + " : sounds: " + SoundList.Length );

	//	audioSource01.clip = SoundList[FoundIdx].Clip;
	//	audioSource01.pitch = pitch;
	//	audioSource01.Play();
	//}

	//=============================================================================

	public void PlaySound( string Name )
	{
		//if( GameManager.Instance.IsSoundOn() == false )
		//	return;

		// Find sound fx
		var FoundIdx = -1;
		for( int Idx = 0; Idx < SoundList.Length; Idx++ )
		{
			if( SoundList[Idx].Name == Name )
			{
				FoundIdx = Idx;
				break;
			}
		}

		if( FoundIdx == -1 )
		{
			Debug.Log( "Unknown sound: " + Name );
			return;
		}

		if( SoundList[FoundIdx].Clip == null )
		{
			Debug.Log( "Null sound prefab for: " + Name );
			return;
		}

		//Debug.Log( FoundIdx + " : sounds: " + SoundList.Length );

		//audioSource01.pitch = 1.0f;
		audioSource01.PlayOneShot( SoundList[FoundIdx].Clip );
	}

	//=============================================================================

	//public void PlaySoundLooped( string Name )
	//{
	//	if( PlayerManager.SoundOn == false )
	//		return;

	//	// Find sound fx
	//	int FoundIdx = -1;
	//	for( int Idx = 0; Idx < SoundList.Length; Idx++ )
	//	{
	//		if( SoundList[Idx].Name == Name )
	//		{
	//			FoundIdx = Idx;
	//			break;
	//		}
	//	}

	//	if( FoundIdx == -1 )
	//	{
	//		Debug.Log( "Unknown sound: " + Name );
	//		return;
	//	}

	//	if( SoundList[FoundIdx].Clip == null )
	//	{
	//		Debug.Log( "Null sound prefab for: " + Name );
	//		return;
	//	}

	//	//Debug.Log( FoundIdx + " : sounds: " + SoundList.Length );

	//	//audioSource02.pitch = 1.0f;
	//	audioSource02.clip = SoundList[FoundIdx].Clip;
	//	audioSource02.loop = true;
	//	audioSource02.Play();
	//}

	//=============================================================================

	//public void StopSoundLooped()
	//{
	//	if( audioSource01.loop )
	//	{
	//		audioSource01.loop = false;
	//		audioSource01.Stop();
	//	}

	//	if( audioSource02.loop )
	//	{
	//		audioSource02.loop = false;
	//		audioSource02.Stop();
	//	}
	//}

	//=============================================================================

	public void PlayCharacterSound( string Name )
	{
		// Find sound fx
		int FoundIdx = -1;
		for( int Idx = 0; Idx < SoundList.Length; Idx++ )
		{
			if( SoundList[Idx].Name == Name )
			{
				FoundIdx = Idx;
				break;
			}
		}

		if( FoundIdx == -1 )
		{
			Debug.Log( "Unknown sound: " + Name );
			return;
		}

		if( SoundList[FoundIdx].Clip == null )
		{
			Debug.Log( "Null sound prefab for: " + Name );
			return;
		}

		//Debug.Log( FoundIdx + " : sounds: " + SoundList.Length );

		characterAudioSource.PlayOneShot( SoundList[FoundIdx].Clip );
	}

	//=============================================================================
	// Fade out sfx and stops any looped clips
	//public void FadeSoundOut()
	//{
	//	if( PlayerManager.SoundOn )
	//	{
	//		isInTransition = true;
	//		transitionDir = -1;
	//		transitionVolume = 1.0f;
	//	}
	//}

	//=============================================================================

	//public void FadeSoundIn()
	//{
	//	if( PlayerManager.SoundOn )
	//	{
	//		isInTransition = true;
	//		transitionDir = 1;
	//		transitionVolume = 0.0f;
	//	}
	//}

	//=============================================================================

	void Awake()
	{
		instance = this;
	}

	//=============================================================================

	//void OnEnable()
	//{
	//	// Set volume
	//	if( PlayerManager.SoundOn )
	//	{
	//		audioSource01.volume = curVolume;
	//		audioSource02.volume = curVolume;
	//	}
	//	else
	//	{
	//		audioSource01.volume = 0.0f;
	//		audioSource02.volume = 0.0f;
	//	}

	//	//if( PlayerPrefsWrapper.GetInt( "Option_SoundFXOn" ) > 0 )

	//	// Get character
	//	if( CharacterManager.instance != null )
	//	{
	//		GetCharacterResources();
	//	}
	//	else
	//	{
	//		Invoke( "GetCharacterResources", 0.25f );
	//	}
	//}

	//=============================================================================

	//public void Update()
	//{
	//	// Fade in / out
	//	if( isInTransition )
	//	{
	//		switch( transitionDir )
	//		{
	//			case -1:
	//				transitionVolume -= Time.deltaTime;
	//				if( transitionVolume <= 0.0f )
	//				{
	//					isInTransition = false;
	//					transitionVolume = 0.0f;

	//					// After fade out, stop any looped sfx
	//					StopSoundLooped();
	//				}
	//				break;

	//			case 1:
	//				transitionVolume += Time.deltaTime;
	//				if( transitionVolume >= 1.0f )
	//				{
	//					isInTransition = false;
	//					transitionVolume = 1.0f;
	//				}
	//				break;
	//		}

	//		audioSource01.volume = curVolume * transitionVolume;
	//		audioSource02.volume = curVolume * transitionVolume;
	//	}
	//}

	//=============================================================================

	//private void GetCharacterResources()
	//{
	//	if( PlayerController_EMS.instance == null )
	//		return;

	//	// Grab second audio source in character prefab
	//	characterAudioSource = PlayerController_EMS.instance.CharacterAudioSource;

	//	// Set volume
	//	if( PlayerManager.SoundOn )
	//		characterAudioSource.volume = curVolume;
	//	else
	//		characterAudioSource.volume = 0.0f;

	//	if( CharacterManager.instance == null )
	//	{
	//		Invoke( "GetCharacterResources", 0.2f );
	//	}
	//	else
	//	{
	//		currentCharacter = CharacterManager.instance.GetCharacterSelectedID();

	//		switch( currentCharacter )
	//		{
	//			case eCharacter.BOY:
	//				characterHits = Resources.LoadAll<AudioClip>( "Audio/Hits/Boy" );
	//				characterTricks = Resources.LoadAll<AudioClip>( "Audio/Tricks/Boy" );
	//				characterAbilities = Resources.LoadAll<AudioClip>( "Audio/Abilities/Boy" );
	//				characterFall = Resources.LoadAll<AudioClip>( "Audio/Fall/Boy" );
	//				characterCelebrate = Resources.LoadAll<AudioClip>( "Audio/Celebrate/Boy" );
	//				characterLose = Resources.LoadAll<AudioClip>( "Audio/Lose/Boy" );
	//				characterSlide = Resources.LoadAll<AudioClip>( "Audio/Slide/Boy" );
	//				break;

	//			case eCharacter.GIRL:
	//				characterHits = Resources.LoadAll<AudioClip>( "Audio/Hits/Girl" );
	//				characterTricks = Resources.LoadAll<AudioClip>( "Audio/Tricks/Girl" );
	//				characterAbilities = Resources.LoadAll<AudioClip>( "Audio/Abilities/Girl" );
	//				characterFall = Resources.LoadAll<AudioClip>( "Audio/Fall/Girl" );
	//				characterCelebrate = Resources.LoadAll<AudioClip>( "Audio/Celebrate/Girl" );
	//				characterLose = Resources.LoadAll<AudioClip>( "Audio/Lose/Girl" );
	//				characterSlide = Resources.LoadAll<AudioClip>( "Audio/Slide/Girl" );
	//				break;

	//			case eCharacter.DOG:
	//				characterHits = Resources.LoadAll<AudioClip>( "Audio/Hits/Dog" );
	//				characterTricks = Resources.LoadAll<AudioClip>( "Audio/Tricks/Dog" );
	//				characterAbilities = Resources.LoadAll<AudioClip>( "Audio/Abilities/Dog" );
	//				characterFall = Resources.LoadAll<AudioClip>( "Audio/Fall/Dog" );
	//				characterCelebrate = Resources.LoadAll<AudioClip>( "Audio/Celebrate/Dog" );
	//				characterLose = Resources.LoadAll<AudioClip>( "Audio/Lose/Dog" );
	//				characterSlide = Resources.LoadAll<AudioClip>( "Audio/Slide/Dog" );
	//				break;

	//			case eCharacter.PENGZ:
	//				characterHits = Resources.LoadAll<AudioClip>( "Audio/Hits/Penguin" );
	//				characterTricks = Resources.LoadAll<AudioClip>( "Audio/Tricks/Penguin" );
	//				characterAbilities = Resources.LoadAll<AudioClip>( "Audio/Abilities/Penguin" );
	//				characterFall = Resources.LoadAll<AudioClip>( "Audio/Fall/Penguin" );
	//				characterCelebrate = Resources.LoadAll<AudioClip>( "Audio/Celebrate/Penguin" );
	//				characterLose = Resources.LoadAll<AudioClip>( "Audio/Lose/Penguin" );
	//				characterSlide = Resources.LoadAll<AudioClip>( "Audio/Slide/Penguin" );
	//				break;

	//			case eCharacter.SNOWMAN:
	//				characterHits = Resources.LoadAll<AudioClip>( "Audio/Hits/Snowman" );
	//				characterTricks = Resources.LoadAll<AudioClip>( "Audio/Tricks/Snowman" );
	//				characterAbilities = Resources.LoadAll<AudioClip>( "Audio/Abilities/Snowman" );
	//				characterFall = Resources.LoadAll<AudioClip>( "Audio/Fall/Snowman" );
	//				characterCelebrate = Resources.LoadAll<AudioClip>( "Audio/Celebrate/Snowman" );
	//				characterLose = Resources.LoadAll<AudioClip>( "Audio/Lose/Snowman" );
	//				characterSlide = Resources.LoadAll<AudioClip>( "Audio/Slide/Snowman" );
	//				break;

	//			case eCharacter.ROCKET:
	//				characterHits = Resources.LoadAll<AudioClip>( "Audio/Hits/Rocket" );
	//				characterTricks = Resources.LoadAll<AudioClip>( "Audio/Tricks/Rocket" );
	//				characterAbilities = Resources.LoadAll<AudioClip>( "Audio/Abilities/Rocket" );
	//				characterFall = Resources.LoadAll<AudioClip>( "Audio/Fall/Rocket" );
	//				characterCelebrate = Resources.LoadAll<AudioClip>( "Audio/Celebrate/Rocket" );
	//				characterLose = Resources.LoadAll<AudioClip>( "Audio/Lose/Rocket" );
	//				characterSlide = Resources.LoadAll<AudioClip>( "Audio/Slide/Rocket" );
	//				break;
	//		}
	//	}
	//}

	//=============================================================================

}
