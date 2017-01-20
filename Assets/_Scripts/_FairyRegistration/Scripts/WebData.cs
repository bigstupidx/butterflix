#define NO_GZIP

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;


#if !NO_GZIP
using ICSharpCode.SharpZipLib.GZip;
#endif

#if USE_MINIJSON
  using MiniJSON
#endif
	
namespace Tsumanga
{
	/// <summary>
	/// Data structures and data conversions for Tsumanga.WebServices
	/// </summary>
	public static class WebData
	{
		/// <summary>
		/// hexadecimal representation of an HMAC SHA256 digest of UTF8 encoding of a string
		/// </summary>
		/// <returns>
		/// A string of hex digits
		/// </returns>
		/// <param name='key'>
		/// Key for the HMAC digest
		/// </param>
		/// <param name='json'>
		/// A string (usually a JSON representation of something) to be digested.
		/// </param>
		public static string HexDigest(byte[] key, byte[] data)
		{
			HMACSHA256 hasher = new HMACSHA256(key);
			hasher.ComputeHash(data);
			return BytesToHex(hasher.Hash);
		}
        /// <summary>
        /// hexadecimal representation of an HMAC SHA256 digest of UTF8 encoding of a string
        /// </summary>
        /// <returns>
        /// A string of hex digits
        /// </returns>
        /// <param name='key'>
        /// Key for the HMAC digest
        /// </param>
        /// <param name='json'>
        /// A string (usually a JSON representation of something) to be digested.
        /// </param>
        public static string HexDigest(byte[] key, string json)
        {
            HMACSHA256 hasher = new HMACSHA256(key);
            hasher.ComputeHash(Encoding.UTF8.GetBytes(json));
            return BytesToHex(hasher.Hash);
        }
		/// <summary>
		/// convert a byte array to a hexadecimal string
		/// </summary>
		/// <returns>
		/// hexadecimal string. No '0x' prefix, just hex digits.
		/// </returns>
		/// <param name='bytes'>
		/// bytes to convert
		/// </param>
		public static string BytesToHex(byte[] bytes)
		{
			StringBuilder hex = new StringBuilder(bytes.GetLength(0) * 2);
			foreach (byte b in bytes) hex.AppendFormat("{0:x2}", b);
			return hex.ToString();
		}
		
		/// <summary>
		/// Convenience factory for IDictionary
		/// </summary>
		/// <returns>
		/// An IDictionary with string keys
		/// </returns>
		/// <param name='keysNvalues'>
		/// An even number of parameters, alternating keys and values for the dict.
		/// </param>
		public static IDictionary<string, object> QuickDict(params object[] keysNvalues)
		{
			Dictionary<string, object> d = new Dictionary<string, object>();
			for (int i = 0; i < keysNvalues.GetLength(0) - 1; i+=2)
			{
				string key = keysNvalues[i] as string;
				if (key != null)
				{
					object val = keysNvalues[i+1];
					d[key] = val;
				}
			}
			return d;
		}

		/// <summary>
		/// Compress bytes using the gzip algorithm
		/// </summary>
		/// <returns>An array of compressed bytes</returns>
		/// <param name="input">An array of uncompressed bytes</param>
		/// <remarks>From http://www.dotnetperls.com/compress</remarks>
		public static byte[] Gzip(byte[] input)
		{
#if NO_GZIP
			return input;
#else
			using (MemoryStream memory = new MemoryStream())
			{
				using (GZipOutputStream gzip = new GZipOutputStream(memory))
				{
					gzip.Write(input, 0, input.Length);
				}
				return memory.ToArray();
			}
#endif
		}
		
		/// <summary>
		/// A Dictionary from JSON in a UTF8-coded bytestring
		/// </summary>
		/// <returns>
		/// an IDictionary<string, object>
		/// </returns>
		/// <param name='bytes'>
		/// A JSON-serialised object in a UTF8-coded bytestring
		/// </param>
		public static IDictionary<string, object> DictFromBytes(byte[] bytes)
		{
			if (bytes == null || bytes.Length == 0) return new Dictionary<string, object>();
#if USE_MINIJSON
			return Json.Deserialize(Encoding.UTF8.GetString(bytes)) as IDictionary<string, object>;
#else
			JSON Decoder = new JSON();
			Decoder.serialized = System.Text.Encoding.UTF8.GetString(bytes);
			return Decoder.fields;
#endif
		}

		/// <summary>
		/// Serialise an object as JSON
		/// </summary>
		/// <returns>JSON string</returns>
		/// <param name="data">Some object that the current JSON implementation can convert</param>
		public static string JsonSerialise(object data)
		{
#if USE_MINIJSON
			return Json.Serialize(data);
#else
			return JSON.serializeObject(data);
#endif		
		}

		/// <summary>
		/// Convenience extension for IDictionary<string, object> like a type-safe Python dict.get()
		/// </summary>
		/// <param name="dict">Dictionary</param>
		/// <param name="field">Key name</param>
		/// <param name="defaultValue">Default value if key missing</param>
		/// <typeparam name="T">Entry in dict (if present) must be castable to this type</typeparam>
		public static T Get<T>(this IDictionary<string, object> dict, string field, T defaultValue)
		{
			if (!dict.ContainsKey(field)) return defaultValue;
			return (T) (dict[field]);
		}
		
	}
}
