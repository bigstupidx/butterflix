using UnityEngine;
using UnityEngine.UI;

public class GuiBubbleBossHealthBar : MonoBehaviour
{
	[SerializeField] private CameraFacingBillboard _billboardController;

	private Transform _thisTransform;
	private Canvas _canvas;
	private RectTransform _healthBar;
	private Animator _animator;

	private bool _isEnabled;

	//=====================================================

	#region Public Interface

	public void OnButtonClick()
	{
		HideBubble();
	}

	//=====================================================

	public void ShowBubble()
	{
		if( _isEnabled == true ) return;
		
		_isEnabled = true;

		if( _animator != null && _animator.GetBool( HashIDs.IsEnabled ) == false )
			_animator.SetBool( HashIDs.IsEnabled, true );

		// Face bubble towards player-camera
		if( _billboardController != null )
			_billboardController.enabled = true;
	}

	//=====================================================

	public void HideBubble()
	{
		_isEnabled = false;

		if( _animator != null && _animator.GetBool( HashIDs.IsEnabled ) == true )
			_animator.SetBool( HashIDs.IsEnabled, false );

		// Stop updating bubble rotations (towards player-camera)
		if( _billboardController != null )
			_billboardController.enabled = false;
	}

	//=====================================================
	
	public void ResetHealthBar()
	{
		_healthBar.localScale = Vector3.one;
	}

	//=====================================================
	// Set health bar scale on x-axis in range 0.0 -> 1.0
	public void SetHealthBar( float health )
	{
		health = Mathf.Clamp( health, 0.0f, 1.0f );

		_healthBar.localScale = new Vector3( health, 1.0f, 1.0f );
	}

	#endregion

	//=====================================================

	#region Private Methods

	void Awake()
	{
		_thisTransform = this.transform;

		_canvas = _thisTransform.FindChild( "Canvas" ).GetComponent<Canvas>();

		if( _canvas != null )
		{
			var background = _canvas.transform.FindChild( "Background" );
			_animator = background.GetComponent<Animator>();
			_healthBar = background.FindChild( "HealthBar" ).GetComponent<RectTransform>();
		}

		_isEnabled = false;
	}

	//=====================================================

	//private IEnumerator AutoHideBubble()
	//{
	//	yield return new WaitForSeconds( 4.0f );

	//	HideBubble();
	//}

	#endregion

	//=====================================================
}
