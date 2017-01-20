using System;
using System.Collections;
using System.Collections.Generic;

namespace Tsumanga
{
	/// <summary>
	/// An IEnumerator representing a coroutine that can end with an Error or a Result.
	/// </summary>
	/// <description>
	/// After <code>yield return StartCoroutine(request)</code>
	/// the request will be complete.
	/// Its Status property will indicate whether it succeeded, and
	/// it will either have a Result dictionary or an Error string.
	/// </description>
	public interface IRequest : IEnumerator
	{
		/// <summary>
		/// A dictionary of result data.
		/// </summary>
		/// <value>
		/// An IDictionary, or null if the request failed.
		/// </value>
		IDictionary<string, object> Result { get; }
		
		/// <summary>
		/// Error string
		/// </summary>
		/// <value>
		/// A string, or null if there was no error.
		/// </value>
		string Error { get; }
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="Tsumanga.IRequest"/> is done.
		/// </summary>
		/// <value>
		/// <c>true</c> if the request is complete; otherwise, <c>false</c>.
		/// </value>
		bool isDone { get; }
		
		/// <summary>
		/// Status of the request.
		/// </summary>
		/// <value>
		/// Usually PENDING until the request is complete, then STATUS_OK or a failure code.
		/// </value>
		RequestStatus Status { get; }
	}
	
	/// <summary>
	/// Status codes for classes that implement IRequest.
	/// </summary>
	public enum RequestStatus
	{
		PENDING = 0,
		NOT_REGISTERED = 1,
		STATUS_OK = 200,
		FAIL_BAD = 400,
		FAIL_AUTH = 401,
		FAIL_FORBIDDEN = 403,
		FAIL_NOT_FOUND = 404,
		FAIL_CONFLICT = 409,
		FAIL_UNKNOWN = 500
	}
	
}

