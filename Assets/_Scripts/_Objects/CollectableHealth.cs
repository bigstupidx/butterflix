using UnityEngine;

[ExecuteInEditMode]
public class CollectableHealth : Collectable {

	//=====================================================

	protected override void OnEnable()
	{
		_type = eCollectable.HEALTH_GEM;

		base.OnEnable();
	}

	//=====================================================
}
