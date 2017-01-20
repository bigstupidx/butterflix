using UnityEngine;
using System.Collections.Generic;


#if UNITY_IPHONE || UNITY_STANDALONE_OSX

namespace Prime31
{
	public class GameCenterRetrieveScoresResult
	{
		public List<GameCenterScore> scores;
		public string category;
	}

}
#endif
