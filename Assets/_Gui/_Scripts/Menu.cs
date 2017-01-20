using UnityEngine;

public class Menu : MonoBehaviour
{
	protected Animator _animator;
	protected CanvasGroup _canvasGroup;

	protected int _isOpenHash = Animator.StringToHash( "IsOpen" );
	protected int _stateOpenHash = Animator.StringToHash( "Base Layer.Open" );

	public bool IsOpen
	{
		get { return _animator.GetBool( _isOpenHash ); }
		set { _animator.SetBool( _isOpenHash, value ); }
	}

	protected virtual void Awake()
	{
		_animator = GetComponent<Animator>();
		_canvasGroup = GetComponent<CanvasGroup>();

		var rect = GetComponent<RectTransform>();
		rect.anchoredPosition = Vector2.zero;
	}

	protected virtual void Update()
	{
		if( _animator.GetCurrentAnimatorStateInfo( 0 ).nameHash != _stateOpenHash )
			_canvasGroup.blocksRaycasts = _canvasGroup.interactable = false;
		else
			_canvasGroup.blocksRaycasts = _canvasGroup.interactable = true;
	}
}
