// =====================================================
// MonoBehaviourEMS - MonoBehaviour Wrapper
//
// Monitors OnApplicationQuit events
// - useful if object registers with other object-events (particularly objects that are singletons)
//   then attempt to unregister as the app is closing
// =====================================================

using UnityEngine;

public class MonoBehaviourEMS : MonoBehaviour {

	protected bool _isAppQuiting = false;

	//=====================================================
	
	void OnApplicationQuit()
	{
		_isAppQuiting = true;
	}

	//=====================================================
}
