using UnityEngine;
using System.Collections.Generic;

public class ResourcesSpellsAndFX
{
	//=====================================================

	private class FairyAudioClip
	{
		public eFairy FairyName { get; set; }
		public AudioClip AudioClip { get; set; }
	}

	//=====================================================

	private static readonly List<string> _spellsBloom = new List<string>();
	//private static readonly List<string> _spellsStella = new List<string>();
	//private static readonly List<string> _spellsFlora = new List<string>();
	//private static readonly List<string> _spellsMusa = new List<string>();
	//private static readonly List<string> _spellsTecna = new List<string>();
	//private static readonly List<string> _spellsAisha = new List<string>();

	private static bool _isInitialised = false;

	private static List<FairyAudioClip> _audioClips;

	//=====================================================

	public static string[] SpellsBloom { get { return _spellsBloom.ToArray(); } }

	//=====================================================

	//public static AudioClip GetAudioClip( string audioClipName )
	//{
	//	if( _audioClips == null )
	//		_audioClips = new List<AudioClip>();

	//	foreach( var fairy in _audioClips )
	//	{
	//		if( fairy.FairyName == eFairy.NULL && fairy.AudioClip.name == audioClipName )
	//			return fairy.AudioClip;
	//	}

	//	var audioClip = Resources.Load<AudioClip>( "Audio/Fairies/" + audioClipName );

	//	if( audioClip != null )
	//	{
	//		_audioClips.Add( audioClip );
	//		return audioClip;
	//	}

	//	Debug.Log( "AudioClip not found in resources" );
	//	return null;
	//}

	//=====================================================

	public static AudioClip GetAudioClip( eFairy fairy, string audioClipName )
	{
		if( _audioClips == null )
			_audioClips = new List<FairyAudioClip>();

		foreach( var clip in _audioClips )
		{
			if( clip.FairyName == fairy && clip.AudioClip.name == audioClipName )
				return clip.AudioClip;
		}

		var path = "Audio/Fairies/";

		switch( fairy )
		{
			case eFairy.AISHA:
				path += "Aisha/";
				break;
			case eFairy.BLOOM:
				path += "Bloom/";
				break;
			case eFairy.FLORA:
				path += "Flora/";
				break;
			case eFairy.MUSA:
				path += "Musa/";
				break;
			case eFairy.STELLA:
				path += "Stella/";
				break;
			case eFairy.TECNA:
				path += "Tecna/";
				break;
		}

		var audioClip = Resources.Load<AudioClip>( path + audioClipName );

		if( audioClip != null )
		{
			var clip = new FairyAudioClip { FairyName = fairy, AudioClip = audioClip };
			
			_audioClips.Add( clip );
			return audioClip;
		}

		Debug.Log( "AudioClip not found in resources" );
		return null;
	}

	//=====================================================

	public static Object GetPrefabSpell( eFairy fairy, int level )
	{
		Init();

		// Set resources path for spell prefabs
		var path = "Prefabs/Spells/";

		switch( fairy )
		{
			case eFairy.AISHA:
				path += "Aisha/";
				break;
			case eFairy.BLOOM:
				path += "Bloom/";
				break;
			case eFairy.FLORA:
				path += "Flora/";
				break;
			case eFairy.MUSA:
				path += "Musa/";
				break;
			case eFairy.STELLA:
				path += "Stella/";
				break;
			case eFairy.TECNA:
				path += "Tecna/";
				break;
		}

		var projectile = "pfbSpell" + fairy + level.ToString( "00" );

		var prefab = Resources.Load( path + projectile );

		if( prefab != null ) return prefab;

		Debug.Log( "Spell prefab not found in resources" );
		return null;
	}

	//=====================================================

	public static Object GetPrefabFx( eFairy fairy )
	{
		Init();

		var fx = "pfbChangeFairyFx" + fairy;

		var prefab = Resources.Load( "Prefabs/Particles/" + fx );

		if( prefab != null )
			return prefab;

		Debug.Log( "Particle FX prefab not found in resources" );
		return null;
	}

	//=====================================================

	private static void Init()
	{
		if( _isInitialised == true ) return;

		Debug.Log( "Initialising resources (spells)" );

		var objects = Resources.LoadAll( "Prefabs/Spells/Bloom" );
		InitListWithObjectNames( _spellsBloom, objects );

		_isInitialised = true;
	}

	//=====================================================

	private static void InitListWithObjectNames( List<string> list, Object[] objects )
	{
		list.Clear();

		if( objects == null ) return;

		for( var i = 0; i < objects.Length; i++ )
		{
			if( objects[i].name.Contains( "pfbSpell" ) )
			{
				//Debug.Log( "Add model to list: " + objects[i].name );
				list.Add( objects[i].name );
			}
		}
	}

	//=====================================================
}
