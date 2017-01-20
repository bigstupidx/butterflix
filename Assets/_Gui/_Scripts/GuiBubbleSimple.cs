using UnityEngine;
using UnityEngine.UI;

public class GuiBubbleSimple : MonoBehaviour
{
	[SerializeField] private CameraFacingBillboard _billboardController;
	[SerializeField] private Sprite _spriteNewMeeting;						// Exclamation mark
	[SerializeField] private Sprite _spriteAttention;						// Question mark

	private Transform _thisTransform;
	private Canvas _canvas;
	private Image _image;
	private Animator _animator;
	private Camera _camera;
	private CameraFacingBillboard _cameraFacingBillboard;

	private NPC _parentNPC;
	private Renderer _parentRenderer;
	//private Vector3 _parentPosition;

	private bool _isEnabled;
	//private Vector3 _plane0Normal;
	private LayerMask _layerCollidable;
	private int _counter;
	private int _updateDelay;

	//=====================================================

	#region Public Interface

	public void OnButtonClick()
	{
		if( _parentNPC != null )
			_parentNPC.OnShowReward();

		HideBubble();
	}

	//=====================================================

	public void ShowNewMeetingIcon()
	{
		//_canvas.enabled = true;

		if( _image != null )
			_image.sprite = _spriteNewMeeting;
	}

	//=====================================================

	public void ShowAttentionIcon()
	{
		if( _image != null )
			_image.sprite = _spriteAttention;
	}

	//=====================================================

	public void ShowBubble()
	{
		_isEnabled = true;

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
				_image = button.GetComponent<Image>();
				_animator = button.GetComponent<Animator>();
			}
		}

		// Get parent object's renderer
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

		// Set collidable layer reference
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

		//_parentPosition = _parentNPC.transform.position;
		//_plane0Normal = Vector3.zero;

		_counter = 0;
		_updateDelay = Random.Range( 5, 10 );
	}

	//=====================================================

	void Start()
	{
		// Displaying this speech bubble at Start
		_isEnabled = true;
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

		// ToDo: (These checks never block - think NPC or cam is always moving slightly) Check for change in camera position (plane.normal) and parent position
		//if( planes[0].normal == _plane0Normal && parentPosition == _parentPosition ) return;

		// Store data
		//_plane0Normal = planes[0].normal;
		//_parentPosition = parentPosition;

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
