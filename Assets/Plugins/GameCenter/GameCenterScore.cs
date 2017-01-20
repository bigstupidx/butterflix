using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Prime31;


#if UNITY_IPHONE || UNITY_STANDALONE_OSX

namespace Prime31
{
	public class GameCenterScore
	{
		public string category;
		public string formattedValue;
		public long value;
		public UInt64 context;
		public double rawDate;
		public DateTime date
		{
			get
			{
				var intermediate = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );
				return intermediate.AddSeconds( rawDate );
			}
		}
		public string playerId;
		public int rank;
		public bool isFriend;
		public string alias;
		public int maxRange; // this is only properly set when retrieving all scores without limiting by playerId
	
	
		public GameCenterScore()
		{}
	
	
		public override string ToString()
		{
			 return string.Format( "<Score> category: {0}, formattedValue: {1}, date: {2}, rank: {3}, alias: {4}, maxRange: {5}, value: {6}, context: {7}",
				category, formattedValue, date, rank, alias, maxRange, value, context );
		}
	
	}

}
#endif
