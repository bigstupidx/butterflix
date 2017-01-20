using UnityEngine;

public interface IPlayerInteraction : ICutsceneObject
{
	bool IsInteractionOk();
	Transform OnPlayerInteraction();
}
