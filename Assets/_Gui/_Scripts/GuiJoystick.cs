using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

[RequireComponent( typeof( Image ) )]
public class GuiJoystick : MonoBehaviourEMS, IBeginDragHandler, IDragHandler, IEndDragHandler, IPauseListener
{
	public event Action<Vector2> JoystickEvent;

	[SerializeField] private Camera _guiCamera;
	[SerializeField] private RectTransform _boundaryRectTransform;
	[SerializeField] private float _boundaryOffset = 50.0f;
	[SerializeField] private bool _isLeftAligned = false;
	[SerializeField] private bool _snapStickPosition = false;

	private RectTransform _thisRectTransform;
	private float _boundaryRadius;
	private float _boundaryAdjusted;
	private float _stickMag;
	private bool _isStickActive;
	private bool _isPaused;

	//=====================================================

	private float StickHorizontal
	{
		get
		{
			return (_thisRectTransform.anchoredPosition.x / _boundaryAdjusted) * 1.1f;
		}
	}

	private float StickVertical
	{
		get
		{
			return (_thisRectTransform.anchoredPosition.y / _boundaryAdjusted) * 1.1f;
		}
	}

	//=====================================================

	public void OnBeginDrag( PointerEventData eventData )
	{
		if( _isPaused == true ) return;

		_isStickActive = true;
		_stickMag = 0.0f;
	}

	//=====================================================

	public void OnDrag( PointerEventData eventData )
	{
		if( _isPaused == true ) return;

		// Drag stick while touch is inside parent rectTransform
		//if( RectTransformUtility.RectangleContainsScreenPoint( _boundaryRectTransform, eventData.position, _guiCamera ) )

		var touchPos = Vector2.zero;

		if( RectTransformUtility.ScreenPointToLocalPointInRectangle( _boundaryRectTransform,
																		eventData.position,
																		_guiCamera,
																		out touchPos ) == false ) return;
		
		var stickPos = _isLeftAligned == true ? new Vector2( touchPos.x - _boundaryRadius, touchPos.y - _boundaryRadius ) :
												new Vector2( touchPos.x + _boundaryRadius, touchPos.y - _boundaryRadius );
		
		_stickMag = stickPos.magnitude;

		if( _stickMag > _boundaryAdjusted )
		{
			_stickMag = _boundaryAdjusted;
			stickPos.Normalize();
			stickPos *= _stickMag;
		}

		// Snap stick position?
		if( _snapStickPosition == true )
		{
			var snappedStickPos = SnapStickPosition( ref stickPos, 30.0f ); // 6.0f || 45.0f
			_thisRectTransform.anchoredPosition = snappedStickPos;
		}
		else
		{
			_thisRectTransform.anchoredPosition = stickPos;
		}
	}

	//=====================================================

	public void OnEndDrag( PointerEventData eventData )
	{
		//if( _isPaused == true ) return;

		if( JoystickEvent != null )
			JoystickEvent( Vector2.zero );

		_thisRectTransform.anchoredPosition = Vector2.zero;
		_stickMag = 0.0f;
		_isStickActive = false;
	}

	//=====================================================

	public void OnPauseEvent( bool isPaused )
	{
		_isPaused = isPaused;

		OnEndDrag( null );
	}

	//=====================================================

	private static Vector2 SnapStickPosition( ref Vector2 stickPos, float snapAngle )
	{
		var angleMagnitude = stickPos.magnitude;
		var angle = Mathf.Atan2( stickPos.normalized.y, -stickPos.normalized.x );
		angle *= Mathf.Rad2Deg;
		angle += 270.0f;
		angle = angle % 360.0f;

		var newAngle = ((int)(angle + (snapAngle * 0.5f)) / (int)snapAngle) * snapAngle;
		//Debug.Log( angle + " " + newAngle );

		newAngle *= Mathf.Deg2Rad;
		var newStickPos = new Vector2( Mathf.Sin( newAngle ), Mathf.Cos( newAngle ) ) * angleMagnitude;

		//Debug.Log( ( newAngle * Mathf.Rad2Deg ) + " " + newStickPos );

		return newStickPos;
	}

	//=====================================================

	void Awake()
	{
		_thisRectTransform = GetComponent<RectTransform>();
		_boundaryRadius = _boundaryRectTransform.rect.width * 0.5f;
		_boundaryAdjusted = _boundaryRadius - _boundaryOffset;
		_isPaused = false;
	}

	//=====================================================

	void OnEnable()
	{
		GameManager.Instance.PauseEvent += OnPauseEvent;

		// Reset stick position
		_thisRectTransform.anchoredPosition = Vector2.zero;
	}

	//=====================================================

	private void OnDisable()
	{
		if( _isAppQuiting == false )
			GameManager.Instance.PauseEvent -= OnPauseEvent;
	}

	//=====================================================

	void Update()
	{
		if( _isStickActive == false ) return;
		
		if( JoystickEvent != null )
			JoystickEvent( new Vector2( Mathf.Clamp( StickHorizontal, -1.0f, 1.0f ),
										Mathf.Clamp( StickVertical, -1.0f, 1.0f ) ) );
	}

	//=====================================================
}