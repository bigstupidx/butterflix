using UnityEngine;
using System;
using System.Collections;

[Serializable]
[RequireComponent( typeof( Animator ) )]
public class ChestItem : MonoBehaviour
{
	private Transform _thisTransform;
	private Animator _animator;
	private GameObject _container;
	private GameObject _gem;
	private GameObject _card;
	private GameObject _key;
	private GameObject _psCollectRewardFx;

	private GameObject _currentReward;

	//=====================================================

	public void Init( ChestReward reward )
	{
		// Hack to fix chest that don't find the reward items during first Awake()
		if( _card == null )
			Awake();

		// Store current reward item
		if( string.IsNullOrEmpty( reward.Card.id ) == false )
			_currentReward = _card;
		else if( reward.SwitchItem != eSwitchItem.NULL )
			_currentReward = _key;
		else
			_currentReward = _gem;
	}

	//=====================================================

	public void OnChestOpen()
	{
		if( _container == null )
			Debug.Log( "Container not found" );

		if( _currentReward == null )
			Debug.Log( "CurrentReward not found" );

		// Activate reward
		_container.SetActive( true );
		_currentReward.SetActive( true );

		// Animate reward
		_animator.SetTrigger( HashIDs.IsChestOpen );
	}

	//=====================================================

	public void OnCollectReward()
	{
		// Play particleFx, hide reward then disable container
		StartCoroutine( CollectReward() );
	}

	//=====================================================

	public void OnPause( bool isPaused )
	{
		_animator.speed = (isPaused) ? 0.0f : 1.0f;
	}

	//=====================================================

	void Awake()
	{
		_thisTransform = this.transform;
		_animator = _thisTransform.GetComponent<Animator>();

		_container = _thisTransform.FindChild( "Reward" ).gameObject;
		_container.tag = UnityTags.ChestReward;
		_gem = _container.transform.FindChild( "Gem" ).gameObject;
		_card = _container.transform.FindChild( "Card" ).gameObject;
		_key = _container.transform.FindChild( "Key" ).gameObject;
		_psCollectRewardFx = _container.transform.FindChild( "psCollectRewardFx" ).gameObject;

		// Hide reward items
		if( _container != null ) _container.SetActive( false );
		if( _gem != null ) _gem.SetActive( false );
		if( _card != null ) _card.SetActive( false );
		if( _key != null ) _key.SetActive( false );
		if( _psCollectRewardFx != null ) _psCollectRewardFx.gameObject.SetActive( false );
	}

	//=====================================================

	private IEnumerator CollectReward()
	{
		_psCollectRewardFx.SetActive( true );
		_currentReward.SetActive( false );

		var parent =_thisTransform.GetComponentInParent<Chest>();
		if( parent != null )
		{
			parent.OnPlayCollectRewardSfx();

			yield return new WaitForSeconds( 0.5f );

			parent.OnCollectReward();
		}

		yield return new WaitForSeconds( 0.5f );

		_container.SetActive( false );
	}

	//=====================================================
}
