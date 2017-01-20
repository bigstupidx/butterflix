using UnityEngine;
using System;


#if UNITY_ANDROID

namespace Prime31
{
	public partial class TwitterManager : MonoBehaviour
	{
		// Android only. Fired after the Twitter service is initialized and ready for use.
		public static event Action twitterInitializedEvent;
	
	
		public void twitterInitialized()
		{
			if( twitterInitializedEvent != null )
				twitterInitializedEvent();
		}
	}

}
#endif
