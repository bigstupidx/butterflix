using UnityEngine;
using System;

[ExecuteInEditMode]
[Serializable]
public class SwingingObstacle : Obstacle
{
	// Editable in inspector
	[SerializeField] private int _modelBody = 0;
	[SerializeField] private float _rotateMax = 30;		// Degrees
	[SerializeField] private float _rotateDuration = 2.0f;

	// Other vars
	[SerializeField] private Transform _obstacleBody;

	// Non-serialized
	private int _tweenId = -1;
	private float _rotateTarget = 0;

	//=====================================================

	#region Public Interface

	public int ModelBody { get { return _modelBody; } set { _modelBody = value; } }
	public float RotationMax { get { return _rotateMax; } set { _rotateMax = value; } }
	public float RotationDuration { get { return _rotateDuration; } set { _rotateDuration = value; } }

	//=====================================================

	public void Init( GameObject armModel, GameObject bodyModel )
	{
		// Remove previous obstacle instances
		if( _obstacle != null )
			DestroyImmediate( _obstacle.gameObject );

		if( _obstacleBody != null )
			DestroyImmediate( _obstacleBody.gameObject );

		Debug.Log( "Arm: " + armModel.name + " Body: " + bodyModel );

		_obstacle = armModel.transform;
		_obstacle.parent = _thisTransform;
		_obstacle.name = "Arm";

		// Parent under _obstacleArm's Container gameObject
		_obstacleBody = bodyModel.transform;
		_obstacleBody.parent = _obstacle.FindChild( "Container" );
		_obstacleBody.name = "Body";

		//CheckReferences();

		// Store this switch-prefab's rotation then zero it out before updating obstacle
		Quaternion rot;
		rot = _thisTransform.rotation;
		_thisTransform.rotation = Quaternion.identity;

		// Position obstacle
		_obstacle.localPosition = Vector3.zero;
		_obstacle.localRotation = Quaternion.Euler( Vector3.zero );
		_obstacleBody.localPosition = Vector3.zero;
		_obstacleBody.localRotation = Quaternion.Euler( Vector3.zero );

		// Reset the door prefab rotation
		_thisTransform.rotation = rot;

		// Position above / on floor (world origin)
		//_thisTransform.position = new Vector3(	_thisTransform.position.x,
		//										_obstacle.GetComponent<Collider>().bounds.size.y + _obstacleBody.GetComponent<Collider>().bounds.size.y * 0.5f,
		//										_thisTransform.position.z );
	}

	//=====================================================

	public override void Refresh()
	{
		CheckReferences();

		// Update obstacle-models
		var mdlArm = ResourcesObstacles.GetModel( eObstacleType.SWINGING_ARM, _model );
		var mdlBody = ResourcesObstacles.GetModel( eObstacleType.SWINGING_BODY, _modelBody );

		var modelArm = Instantiate( mdlArm ) as GameObject;
		var modelBody = Instantiate( mdlBody ) as GameObject;

		Init( modelArm, modelBody );
	}

	#endregion

	//=====================================================

	#region IPauseListener

	public override void OnPauseEvent( bool isPaused )
	{
		// Manage active tween
		if( isPaused == true && _tweenId != -1 )
		{
			LeanTween.pause( _tweenId );
		}
		else if( isPaused == false && _tweenId != -1 )
		{
			LeanTween.resume( _tweenId );
		}
	}

	#endregion

	//=====================================================

	#region Private Methods

	void OnEnable()
	{
		if( Application.isPlaying )
		{
			GameManager.Instance.PauseEvent += OnPauseEvent;

			SwingObstacle();
		}
	}

	//=====================================================

	void OnDisable()
	{
		if( Application.isPlaying )
		{
			if( _isAppQuiting == false )
				GameManager.Instance.PauseEvent -= OnPauseEvent;
		}
	}

	//=====================================================
	// Having problems with prefabs instantiated from Resources losing their private references
	protected override void CheckReferences()
	{
		base.CheckReferences();
		
		if( _obstacleBody == null )
			_obstacleBody = _obstacleBody.parent = _obstacle.FindChild( "Container" ).FindChild( "Body" );

		if( _obstacleBody == null )
			Debug.Log( "CheckReferences: Obstacle not found" );
	}

	//=====================================================

	private void SwingObstacle()
	{
		if( _tweenId != -1 )
			LeanTween.cancel( _obstacle.gameObject, _tweenId );

		// Determine rotation
		_rotateTarget = (_rotateTarget <= 0) ? _rotateMax : -_rotateMax;

		// Tween door open around hinge - on completion tween door to closed position if required
		_tweenId = LeanTween.rotateZ( _obstacle.gameObject, _rotateTarget, _rotateDuration )
			 .setEase( LeanTweenType.easeInOutQuad )
			 .setOnComplete( () =>
			 {
				
				 SwingObstacle();
			 } )
			 .id;
	}

	//=====================================================

	void OnDrawGizmos()
	{
		if(_obstacleBody == null) return;

		// Allow for rotations
		Gizmos.color = Color.white;
		DrawArrow.ForGizmo( _obstacleBody.position, _thisTransform.right, 0.4f );
		DrawArrow.ForGizmo( _obstacleBody.position, -_thisTransform.right, 0.4f );
	}

	#endregion

	//=====================================================
}
