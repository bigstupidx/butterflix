using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent( typeof( Collider ) )]
public class TriggerPressureSwitch : MonoBehaviour
{
	private Transform _thisTransform;
	private Collider _collider;
	private ParticleSystem _particleSystem;
	private Color _currentColor;
	private Color _particleColorStart;
	private bool _isActivated;
	private Collider _currentCollider;
	[Range( 0.25f, 3.0f )]
	[SerializeField]
	private float _radius;		// SqrMagnitude value from switch-centre to object on switch

	//=====================================================

	public float Radius { set { _radius = Mathf.Clamp( value, 0.25f, 5.0f ); } }

	//=====================================================

	void Awake()
	{
		_thisTransform = this.transform;
		_collider = _thisTransform.GetComponent<Collider>();
		_particleSystem = _thisTransform.parent.GetComponentInChildren<ParticleSystem>();
		_particleSystem.enableEmission = false;
		_isActivated = false;
		_currentCollider = null;
	}

	//=====================================================

	void OnEnable()
	{
		Radius = _radius;
	}

	//=====================================================

	void OnTriggerEnter( Collider other )
	{
		// Monitor first object entering trigger
		if( _currentCollider != null ) return;

		if( other.tag == UnityTags.Player || other.tag == UnityTags.Pushable )
			_currentCollider = other;
	}

	//=====================================================

	void OnTriggerStay( Collider other )
	{
		if( _currentCollider != other ) return;

		var diff = new Vector2( other.transform.position.x, other.transform.position.z )
					- new Vector2( _thisTransform.position.x, _thisTransform.position.z );

		if( _isActivated == false && diff.sqrMagnitude <= _radius )
		{
			_isActivated = true;
			_particleSystem.enableEmission = true;

			_thisTransform.parent.GetComponent<Switch>().OnPlayerInteraction();
		}
		else if( _isActivated == true && diff.sqrMagnitude > _radius )
		{
			_isActivated = false;
			_particleSystem.enableEmission = false;

			_thisTransform.parent.GetComponent<Switch>().OnDeactivation();
		}
	}

	//=====================================================

	void OnTriggerExit( Collider other )
	{
		// Clear current object being monitored
		if(_currentCollider == other)
			_currentCollider = null;
	}

	//=====================================================

	void OnDrawGizmos()
	{
		Gizmos.color = new Color( Color.blue.r, Color.blue.g, Color.blue.b, 0.5f );
		Gizmos.DrawCube( _collider.bounds.center, _collider.bounds.size );
	}

	//=====================================================
}
