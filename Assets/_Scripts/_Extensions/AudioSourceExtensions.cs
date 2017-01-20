using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class AudioSourceExtensions {

	//=============================================================================
	
	public static void playClip(this AudioSource audioSource, AudioClip audioclip)
	{
		audioSource.Stop();
		audioSource.clip = audioclip;
		audioSource.Play();
	}
	
	//=============================================================================
	
	public static IEnumerator playClip(this AudioSource audioSource, AudioClip audioclip, Action onComplete)
	{
		audioSource.playClip(audioclip);
		
		while(audioSource.isPlaying)
		{
			yield return null;
		}
		
		onComplete();
	}
	
	//=============================================================================
	
	public static void playRandomClip(this AudioSource audioSource, AudioClip[] clips)
	{
		int clipIndex = UnityEngine.Random.Range(0, clips.Length);
		audioSource.playClip(clips[clipIndex]);
	}
	
	//=============================================================================
	
	public static IEnumerator fadeOut(this AudioSource audioSource, AudioClip audioclip, float duration = 1.0f, Action onComplete = null)
	{
		// Avoid divisions by zero
		if(duration <= 0)
			duration = 1.0f;
		
		float startingVolume = audioSource.volume;
		
		audioSource.playClip(audioclip);
		
		// Fade out over time
		while(audioSource.volume > 0)
		{
			audioSource.volume -= Time.deltaTime * startingVolume / duration;
			yield return null;
		}
		
		// Reset volume to match previous value
		audioSource.volume = startingVolume;
		
		// Fade complete
		if(onComplete != null)
			onComplete();
	}
	
	//=============================================================================
	
	public static IEnumerator fadeIn(this AudioSource audioSource, AudioClip audioclip, float duration = 1.0f, Action onComplete = null)
	{
		// Avoid divisions by zero
		if(duration <= 0)
			duration = 1.0f;
		
		float endVolume = audioSource.volume;
		audioSource.volume = 0;
		
		audioSource.playClip(audioclip);
		
		// Fade out over time
		while(audioSource.volume < endVolume)
		{
			audioSource.volume += Time.deltaTime * endVolume / duration;
			yield return null;
		}
		
		// Reset volume to match previous value
		audioSource.volume = endVolume;
		
		// Fade complete
		if(onComplete != null)
			onComplete();
	}
	
	//=============================================================================
}
