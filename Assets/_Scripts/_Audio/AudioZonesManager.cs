using UnityEngine;
using UnityEngine.Audio;

public class AudioZonesManager : MonoBehaviour
{
	public static AudioZonesManager Instance;

	[SerializeField] private AudioMixer _mixer;
	[SerializeField] private AudioMixerSnapshot[] _snapshots;

	private eAudioZoneType _lastZoneType;

	//=====================================================

	public void BlendSnapshot( eAudioZoneType zoneType )
	{
		if( zoneType == _lastZoneType ) return;

		Debug.Log( "Blending to: " + zoneType );

		switch( zoneType )
		{
			case eAudioZoneType.SMALL_ROOM:
			{
				var weights = new[] { 1.0f, 0.0f };
				_mixer.TransitionToSnapshots( _snapshots, weights, 1.0f );
				break;
			}
			case eAudioZoneType.LARGE_ROOM:
			{
				var weights = new[] { 0.6f, 0.4f };
				_mixer.TransitionToSnapshots( _snapshots, weights, 1.0f );
				break;
			}
			case eAudioZoneType.HALL:
			{
				var weights = new[] { 0.0f, 1.0f };
				_mixer.TransitionToSnapshots( _snapshots, weights, 1.0f );
				break;
			}
		}

		_lastZoneType = zoneType;
	}

	//=====================================================

	void Awake()
	{
		Instance = this;

		_lastZoneType = eAudioZoneType.SMALL_ROOM;
	}

	//=====================================================
}
