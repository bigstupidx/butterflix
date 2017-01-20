using UnityEngine;
using UnityEngine.UI;

public class GuiBubbleEmoticon : MonoBehaviour
{
	[SerializeField]
	private CameraFacingBillboard _billboardController;
	[SerializeField]
	private Sprite _spriteGem;								// Reward: Gem
	[SerializeField]
	private Sprite _spriteCard;							// Reward: Card
	[SerializeField]
	private Sprite _spriteEmoticonDefault;

	private Transform _thisTransform;
	private Image _emoticon;
	private Animator _animator;
	private CameraFacingBillboard _cameraFacingBillboard;

	private NPC _parentNPC;
	private bool _isRewardAvailable;

	private Canvas _canvas;
	private Image _image;
	private Camera _camera;

	private Renderer _parentRenderer;

	private bool _isEnabled;
	private LayerMask _layerCollidable;
	private int _counter;
	private int _updateDelay;

	//=====================================================

	#region Public Interface

	public void OnButtonClick()
	{
		if( _isRewardAvailable == true && _parentNPC != null )
			_parentNPC.OnCollectReward();

		_isRewardAvailable = false;

		HideBubble();
	}

	//=====================================================

	public void ShowBubble( Sprite image )
	{
		if( _isEnabled == true ) return;

		_isEnabled = true;

		if( _isRewardAvailable == false )
			SetEmoticon( image );

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

	public void SetReward( bool isRewardAvailable, bool isGems )
	{
		if( _emoticon == null ) return;

		_isRewardAvailable = isRewardAvailable;

		if( _isRewardAvailable == true )
		{
			_emoticon.sprite = isGems == true ? _spriteGem : _spriteCard;
		}
		else
		{
			SetDefaultEmoticonIcon();
		}
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
			_cameraFacingBillboard = _canvas.GetComponent<CameraFacingBillboard>();

			var button = _canvas.GetComponentInChildren<Button>();

			if( button != null )
			{
				_animator = button.GetComponent<Animator>();

				var image = button.transform.FindChild( "Image" );
				_emoticon = image.GetComponent<Image>();
			}
		}

		_parentNPC = _thisTransform.parent.GetComponent<NPC>();
		if( _parentNPC != null )
		{
			if( _parentNPC.Type == eNPC.NPC_STUDENT )
			{
				var skinRenderers = _parentNPC.GetComponentsInChildren<Renderer>();

				if( skinRenderers.Length > 0 )
				{
					foreach( var skinRenderer in skinRenderers )
					{
						if( skinRenderer.name.Contains( "Skin" ) == false ) continue;

						_parentRenderer = skinRenderer;
					}
				}
			}
			else
			{
				var teachers = _parentNPC.transform.FindChild( "Teachers" );
				if( teachers != null )
					_parentRenderer = teachers.GetComponentInChildren<Renderer>();
			}

			if( _parentRenderer == null )
				Debug.Log( "Parent renderer not found" );
		}

		_isEnabled = false;
		_isRewardAvailable = false;
		_layerCollidable = 1 << LayerMask.NameToLayer( "Collidable" );
	}

	//=====================================================

	void OnEnable()
	{
		if( _canvas == null ) return;

		var camObj = GameObject.FindGameObjectWithTag( UnityTags.GuiCameraInGame );

		if( camObj != null )
		{
			_camera = camObj.GetComponent<Camera>();
			_canvas.worldCamera = _camera;
		}
		else
			Debug.LogWarning( "In-game gui camera not found!" );

		_counter = 0;
		_updateDelay = Random.Range( 5, 10 );
	}

	//=====================================================

	private void SetEmoticon( Sprite image )
	{
		// ToDo: pass in sprite from NPC via NPCManager

		if( _emoticon == null ) return;

		if( image != null )
			_emoticon.sprite = image;
		else
			SetDefaultEmoticonIcon();
	}

	//=====================================================

	private void SetDefaultEmoticonIcon()
	{
		// ToDo: select emoticon from available icons (allow for localisation)
		_emoticon.sprite = _spriteEmoticonDefault;
	}

	//=====================================================

	void Update()
	{
		if( _isEnabled == false ) return;

		// Reducing per frame updates
		if( ++_counter < _updateDelay ) return;

		_counter = 0;

		var planes = GeometryUtility.CalculateFrustumPlanes( _camera );
		var parentPosition = _parentNPC.transform.position;

		// Is bounds of rectTransform within frustrum of camera
		var isInFrustrum = GeometryUtility.TestPlanesAABB( planes, _parentRenderer.bounds );
		var isInLineOfSight = false;

		if( isInFrustrum )
		{
			// Test line-of-sight
			RaycastHit hit;
			if( Physics.Raycast( _camera.transform.position, (parentPosition + Vector3.up) - _camera.transform.position, out hit, 200, _layerCollidable ) )
			{
				if( hit.collider.tag == UnityTags.NPC )
					isInLineOfSight = true;
			}
		}

		if( isInLineOfSight == true )
		{
			if( _animator != null && _animator.GetBool( HashIDs.IsEnabled ) == false )
				_animator.SetBool( HashIDs.IsEnabled, true );

			if( _cameraFacingBillboard != null && _cameraFacingBillboard.enabled == false )
				_cameraFacingBillboard.enabled = true;
		}
		else
		{
			if( _animator != null && _animator.GetBool( HashIDs.IsEnabled ) == true )
				_animator.SetBool( HashIDs.IsEnabled, false );

			if( _cameraFacingBillboard != null && _cameraFacingBillboard.enabled == true )
				_cameraFacingBillboard.enabled = false;
		}
	}

	#endregion

	//=====================================================
}
