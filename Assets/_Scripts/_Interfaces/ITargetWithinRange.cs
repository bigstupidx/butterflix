using UnityEngine;

public interface ITargetWithinRange {

	void OnTargetWithinRange(Transform target, bool isPlayer = false);
	void OnTargetLost();
}
