using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Prime31;


#if UNITY_IPHONE || UNITY_STANDALONE_OSX

namespace Prime31
{
	public class GKPlayer
	{
		public string playerId;
		public string displayName;
		public string alias;
	
	
		public GKPlayer()
		{}
	
	
		public GKPlayer( Dictionary<string,object> dict )
		{
			if( dict.ContainsKey( "playerId" ) )
				playerId = dict["playerId"] as string;
	
			if( dict.ContainsKey( "displayName" ) )
				displayName = dict["displayName"] as string;
	
			if( dict.ContainsKey( "alias" ) )
				alias = dict["alias"] as string;
		}
	
	
		public override string ToString()
		{
			 return string.Format( "<GKPlayer> playerId: {0}, displayName: {1}, alias: {2}",
				playerId, displayName, alias );
		}
	
	}

}
#endif
