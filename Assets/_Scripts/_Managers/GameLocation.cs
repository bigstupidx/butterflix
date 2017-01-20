using UnityEngine;

public class GameLocation : MonoBehaviour
{
	[SerializeField] private eLocation _location = eLocation.NULL;

	public eLocation Location { get { return _location; } }
}
