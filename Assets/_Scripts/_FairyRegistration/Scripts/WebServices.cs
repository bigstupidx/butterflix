using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Tsumanga;

namespace Tsumanga {

	#region IRequest Implementations
	/// <summary>
	/// an IRequest for one or more web requests.
	/// </summary>
	/// <description>
    /// It wraps another IEnumerator which may yield one or more WWW objects.
	/// The last WWW object yielded is used to generate Error and Result whenever it is finished.
	/// If the IEnumerator yields a RequestStatus, that becomes the status of the IRequest,
	/// else if the WWW has an error that decides the Status, otherwise it will be STATUS_OK.
	/// </description>
	internal class WWWRequest : IRequest
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Tsumanga.WWWRequest"/> class.
		/// </summary>
		/// <param name='requestActions'>
		/// Request actions: an IEnumerator carrying out actions to perform the web request. It
		/// should at some point yield a WWW object.
		/// </param>
		public WWWRequest(IEnumerator requestActions) {
			actions = requestActions;
		}
		
		public string Error { 
			get
			{
				if (www == null || !www.isDone || String.IsNullOrEmpty(www.error)) return null;
				if (www.responseHeaders != null)
				{
					try
					{
						
						IDictionary<string,string> hdrs = www.responseHeaders;
						string x_status = hdrs["X-STATUS"]; // All-caps even if server sent mixed case
						if (x_status != null) return x_status;
					}
					catch (KeyNotFoundException)
					{
						// Debug.LogError("No X-Status header and IDictionary broken");
					}
				}
				return www.error;
			}
		}
		
		public IDictionary<string, object> Result
		{
			get { 
				if (www == null || !www.isDone || !String.IsNullOrEmpty(www.error)) return null;
				return WebData.DictFromBytes(www.bytes);
			}
		}
		public bool isDone { get { return markedAsDone; }}
		public RequestStatus Status { get { return status; }}
			
		public object Current {	get { return actions.Current; }}
		
		public bool MoveNext() { 
			bool busy = actions.MoveNext();
			markedAsDone = !busy;
			if (actions.Current is RequestStatus)
				status = (RequestStatus) actions.Current;
			else if (actions.Current is WWW)
				www = actions.Current as WWW;
			if (markedAsDone && status == RequestStatus.PENDING)
				status = StatusFromWebError();
			return busy;
		}
		
		public void Reset() { return; }
			
		private WWW www;
		private IEnumerator actions;
		private bool markedAsDone = false;
		private RequestStatus status = RequestStatus.PENDING;
		
		private RequestStatus StatusFromWebError()
		{
			string error = Error;
			if (String.IsNullOrEmpty(error)) return RequestStatus.STATUS_OK;
			if (error.StartsWith("200")) return RequestStatus.STATUS_OK;
			if (error.StartsWith("400")) return RequestStatus.FAIL_BAD;
			if (error.StartsWith("401")) return RequestStatus.FAIL_AUTH;
			if (error.StartsWith("403")) return RequestStatus.FAIL_FORBIDDEN;
			if (error.StartsWith("404")) return RequestStatus.FAIL_NOT_FOUND;
			if (error.StartsWith("409")) return RequestStatus.FAIL_CONFLICT;
			return RequestStatus.FAIL_UNKNOWN;
		}
	}
	
	/// <summary>
	/// A pre-failed request, with only an Error string and no Result.
	/// </summary>
	internal class FailedRequest: IRequest
	{
		public FailedRequest(RequestStatus statuscode, string why) { status = statuscode; reason = why; }
		public string Error { get { return reason; }}
		public IDictionary<string, object> Result { get { return null; }}
		public bool isDone { get { return true; }}
		public RequestStatus Status { get { return status; }}
		
		public object Current { get { return null; }}
		public bool MoveNext() { return false; }
		public void Reset() { return; }
		private RequestStatus status;
		private string reason;
		
	}
	#endregion
	
	/// <summary>
	/// Web services for Tsumanga's multi-player Winx game.
	/// </summary>
	/// <description>
	/// How to use this class
	/// =====================
	/// 
	/// Create an instance of WebServices specifying the URL of the server
	/// providing the services.
	/// 
	/// If you already have the details of a registered player (uid, secret, nick)
	/// then use Initialise() with those details to set up the web services.
	/// 
	/// Otherwise (or to register a new player), use RegisterPlayer.
	/// 
	/// The class is designed to work with Unity's coroutines. Most methods return
	/// a <see cref="Tsumanga.IRequest"/>.
	/// Use Unity's StartCoroutine(request) to begin/wait for the IRequest to complete.
	/// After completion, the IRequest object will either have an Error or a Result.
	/// 
	/// It should be OK to have more than one request running concurrently.
	/// 
	/// </description>
	public class WebServices
	{
	
		/// <summary>
		/// Initializes a new instance of the <see cref="Tsumanga.WebServices"/> class.
		/// </summary>
		/// <param name='urlBase'>
		/// Base URL for the server providing the web services. Does not end in "/".
		/// </param>
		public WebServices(string urlBase="https://multi.tsumanga.net")
		{
			WebServiceHost = urlBase;
		}
		
		#region Public Properties
		/// <summary>
		/// Gets the player nick.
		/// </summary>
		/// <value>
		/// The player nick.
		/// </value>
		public string PlayerNick { get { return Nick; } set { Nick = value; }}
		
		/// <summary>
		/// Gets the player uid.
		/// </summary>
		/// <value>
		/// The player uid.
		/// </value>
		public string PlayerUid { get { return Uid; }}
		
		/// <summary>
		/// Gets the player secret login key.
		/// </summary>
		/// <value>
		/// The player secret login key.
		/// </value>
		public string PlayerSecret { get { return SecretKey; }}
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="Tsumanga.WebServices"/> player is banned.
		/// </summary>
		/// <value>
		/// <c>true</c> if player banned; otherwise, <c>false</c>.
		/// </value>
		/// <description>
		///  In rare circumstances, a player's name may be marked as banned. Their
		///  name will not appear to other players in any context, nor can it be
		///  found with the player search web service.
		///  This state can be cleared by changing the player's name.
		/// </description>
		public bool PlayerBanned { get { return Banned; }}
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="Tsumanga.WebServices"/> has the UID of a registered player.
		/// </summary>
		/// <value>
		/// <c>true</c> if registered; otherwise, <c>false</c>.
		/// </value>
		public bool Registered { get { return Uid != null & SecretKey != null; }}
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="Tsumanga.WebServices"/> is logged in.
		/// Mostly you don't need to know this, because requests that need login will attempt to log in
		/// on demand.
		/// </summary>
		/// <value>
		/// <c>true</c> if logged in; otherwise, <c>false</c>.
		/// </value>
		public bool LoggedIn { get { return SigningKey != null && LoginExpiry > DateTime.UtcNow; }}
		#endregion
		
		#region Public Methods
		/// <summary>
		/// Registers the player.
		/// </summary>
		/// <returns>
		/// An IRequest which when complete and successful will have a Result
		/// containing "uid" (player unique ID) and "ban" (player name rejected for profanity)
		/// </returns>
		/// <param name='nick'>
		/// Initial player nickname
		/// </param>
		/// <param name="locale">
		/// Locale code (e.g. en_GB) if available.
		/// </param>
		/// <description>
		/// After registration, you can read the PlayerUID and PlayerBanned properties
		/// instead of examining the Result data structure. This may be more
		/// convenient.
		/// Will fail with status FAIL_CONFLICT if the requested nickname is already
		/// taken.
		/// </description>
		public IRequest RegisterPlayer(string nick, string locale="")
		{
			return new WWWRequest(RegisterPlayerActions(nick, locale, false));
		}
		
		/// <summary>
		/// Re-registers the player, assuming they have registered from this device with same nickname
		/// </summary>
		/// <returns>
		/// An IRequest which when complete and successful will have a Result
		/// containing "uid" (player unique ID) and "ban" (player name rejected for profanity)
		/// </returns>
		/// <param name='nick'>
		/// Player nickname, as previously registered on this device.
		/// </param>
		/// <param name='locale'>
		/// Locale code (e.g. en_GB) if available.
		/// </param>
		public IRequest ReRegisterPlayer(string nick, string locale="")
		{
			return new WWWRequest(RegisterPlayerActions(nick, locale, true));			
		}
		
		/// <summary>
		/// Initialise the webservices with specified uid, secret and nickname of
		/// a player that has already been registered on this device.
		/// </summary>
		/// <param name='uid'>
		/// Unique ID of player
		/// </param>
		/// <param name='secret'>
		/// Secret key of player (hex digits)
		/// </param>
		/// <param name='nick'>
		/// Player's current nickname
		/// </param>
		public void Initialise(string uid, string secret, string nick)
		{
			Uid = uid;
			SecretKey = secret;
			Nick = nick;
			SigningKey = null;
		}
		
		/// <summary>
		/// Unsigned web request, for backwards compatibility
		/// with web services that take an app key header but
		/// don't use the newer signing mechanism (e.g. /app/sirenix/hearts )
		/// </summary>
		/// <returns>
		/// an IRequest
		/// </returns>
		/// <param name='path'>
		/// URL path for the web service
		/// </param>
		/// <param name='reqBody'>
		/// Object to POST as JSON
		/// </param>
		/// <param name='appKey'>
		/// Old-style application key
		/// </param>
		public IRequest UnsignedRequest(string path, object reqBody, string appKey)
		{
			return new WWWRequest(UnsignedRequestActions(path, reqBody, appKey));
		}
		
		/// <summary>
		/// Changes the nickname of registered player
		/// </summary>
		/// <returns>
		/// An IRequest whose status indicates success or failure.
		/// </returns>
		/// <description>
		/// Most likely failure statuses are FAIL_CONFLICT when the name is
		/// already taken, and FAIL_FORBIDDEN when the name is banned by
		/// profanity filtering.
		/// </description>
		public IRequest ChangeNickname(string newNick)
		{
			if (!Registered) return new FailedRequest(RequestStatus.NOT_REGISTERED, "not registered");
			return new WWWRequest(LoginAnd(ChangeNicknameActions(newNick)));
		}
		
		/// <summary>
		/// Stores some player data.
		/// </summary>
		/// <returns>
		/// An IRequest which, when complete, will have saved the data.
		/// </returns>
		/// <param name='dataKey'>
		/// Data key: specifies the kind of player data to store.
		/// Can contain alphanumeric chars, dots, hyphens and underscores.
		/// </param>
		/// <param name='playerData'>
		/// Player data: any JSON-serialisable object.
		/// </param>
		public IRequest StorePlayerData(string dataKey, object playerData)
		{
			if (!Registered) return new FailedRequest(RequestStatus.NOT_REGISTERED, "not registered");
			return new WWWRequest(LoginAnd(StorePlayerDataActions(dataKey, playerData)));
		}
		
		/// <summary>
		/// Stores some player data.
		/// </summary>
		/// <returns>
		/// An IRequest which, when complete, will have saved the data.
		/// </returns>
		/// <param name='dataKey'>
		/// Data key: specifies the kind of player data to store.
		/// Can contain alphanumeric chars, dots, hyphens and underscores.
		/// </param>
		/// <param name='serialisedData'>
		/// A string which must be valid JSON
		/// </param>
		public IRequest StorePlayerData(string dataKey, string serialisedData)
		{
			if (!Registered) return new FailedRequest(RequestStatus.NOT_REGISTERED, "not registered");
			return new WWWRequest(LoginAnd(StorePlayerDataActions(dataKey, serialisedData)));
		}
		
		/// <summary>
		/// Gets the specified player data.
		/// </summary>
		/// <returns>
		/// An IRequest which, when complete, will have a Result that is the requested player data
		/// as deserialised from JSON (probably an IDictionary<string, object>).
		/// </returns>
		/// <param name='dataKey'>
		/// Data key: identifies which kind of player data to retrieve.
		/// </param>
		/// <param name='uid'>
		/// Uid: identifies player whose data is to be retrieved (default is this player)
		/// </param>
		public IRequest GetPlayerData(string dataKey, string uid=null)
		{
			if (!Registered) return new FailedRequest(RequestStatus.NOT_REGISTERED, "not registered");
			if (uid == null) uid = Uid;
			return new WWWRequest(GetPlayerDataActions(uid, dataKey));
		}

		/// <summary>
		/// Restores an item of player data for this player, also signalling
		/// the server that it is replacing the data on the client, and can now
		/// be unlocked for further changes.
		/// </summary>
		/// <returns>The player data</returns>
		/// <param name="dataKey">Data key</param>
		public IRequest RestorePlayerData(string dataKey)
		{
			if (!Registered) return new FailedRequest(RequestStatus.NOT_REGISTERED, "not registered");
			return new WWWRequest(LoginAnd(RestorePlayerDataActions(dataKey)));
		}

		/// <summary>
		/// Gets the server time.
		/// After a successful call, subsequent calls to UnixUtcNow() will return an adjusted value
		/// close to the server time.
		/// </summary>
		/// <returns>
		/// An IRequest for the server time web service. If this succeeds, its Result will contain
		/// two keys: "iso" being the ISO-formatted time string, and "now" being a double representing
		/// UNIX time (seconds since 1970-01-01 00:00:00).
		/// </returns>
		public IRequest GetServerTime()
		{
			return new WWWRequest(GetServerTimeActions());
		}
		
		/// <summary>
		/// Search for a player whose nickname is exactly as specified.
		/// </summary>
		/// <returns>
		/// an IRequest whose result contains an array of matching players as "niks",
		/// each entry being a Dictionary with "nik" and "uid".
		/// </returns>
		/// <param name='nickname'>
		/// Nickname to search for.
		/// </param>
		public IRequest SearchPlayerExact(string nickname)
		{
			return new WWWRequest(SearchPlayerActions(nickname, true));
		}

		/// <summary>
		/// Search for players whose nicknames contain the given (POSIX regex) string.
		/// </summary>
		/// <returns>
		/// An IRequest whose result contains an array of matching players as "niks",
		/// each entry being a Dictionary with "nik" and "uid".
		/// Exact match and prefix matches first, then non-prefix matches.
		/// </returns>
		/// <param name='nickname'>
		/// Nickname pattern to search for.
		/// </param>
		public IRequest SearchPlayers(string nickname)
		{
			return new WWWRequest(SearchPlayerActions(nickname, false));
		}
		
		/// <summary>
		/// Searches for previous player accounts registered to this same device.
		/// </summary>
		/// <returns>
		/// An IRequest whose result contains an array of player nicknames as "niks".
		/// </returns>
		/// <description>
		/// Any of these can be re-registered with ReRegisterPlayer(nickname, locale)
		/// </description>
		public IRequest SearchPreviousPlayers()
		{
			return new WWWRequest(SearchPreviousPlayersActions());
		}
		
		/// <summary>
		/// Gets the UIDs and Nicknames of players linked to this player.
		/// </summary>
		/// <returns>
		/// IRequest whose Result contains an array of linked players as "link",
		/// each entry being a Dictionary with "nik" and "uid".
		/// </returns>
		public IRequest GetLinkedPlayers()
		{
			if (!Registered) return new FailedRequest(RequestStatus.NOT_REGISTERED, "not registered");
			return new WWWRequest(LoginAnd(LinkedPlayersActions()));
		}
		
		/// <summary>
		/// Gets the UIDs and Nicknames of players linked to this player and
		/// one item of player data for each.
		/// </summary>
		/// <returns>
		/// IRequest whose Result contains an array of linked players as "link",
		/// each entry being a Dictionary with "nik", "uid" and "dat".
		/// </returns>
		/// <param name='dataKey'>
		/// Key of the playerdata to fetch for all the linked players. Any who
		/// don't have this playerdata will have their "dat" entry == null.
		/// </param>
		/// <description>
		/// This might be useful for example to get a list of a player's
		/// friends together with the avatar structure of each, so that they
		/// can be graphically represented in the UI without having to make
		/// multiple calls.
		/// </description>
		public IRequest GetLinkedData(string dataKey)
		{
			if (!Registered) return new FailedRequest(RequestStatus.NOT_REGISTERED, "not registered");
			return new WWWRequest(LoginAnd(GetLinkedDataActions(dataKey)));
		}
		
		/// <summary>
		/// Links the player to another player (follower/friend relationship)
		/// </summary>
		/// <returns>
		/// An IRequest whose Status indicates success or failure, and whose Result
		/// may contain a friend reward amount as "gems".
		/// </returns>
		/// <param name='otherUID'>
		/// Other player's UID
		/// </param>
		public IRequest LinkPlayer(string otherUID)
		{
			if (!Registered) return new FailedRequest(RequestStatus.NOT_REGISTERED, "not registered");
			return new WWWRequest(LoginAnd(LinkPlayerActions(otherUID)));
		}
		
		/// <summary>
		/// Unlinks the player from another player (unfriend/unfollow)
		/// </summary>
		/// <returns>
		/// An IRequest whose Status indicates success or failure
		/// </returns>
		/// <param name='otherUID'>
		/// Other player's UID
		/// </param>
		public IRequest UnlinkPlayer(string otherUID)
		{
			if (!Registered) return new FailedRequest(RequestStatus.NOT_REGISTERED, "not registered");
			return new WWWRequest(LoginAnd(UnlinkPlayerActions(otherUID)));
		}
		
		/// <summary>
		/// Gets the Top N scores from a scoreboard
		/// </summary>
		/// <returns>
		/// An IRequest whose Result contains a "tab" entry.
		/// This is a List of IDictionary each containing
		/// "nik", "best" and "rank". The list is in ascending
		/// rank order.
		/// </returns>
		/// <param name='board'>
		/// Scoreboard name
		/// </param>
		/// <param name='topN'>
		/// Number of scores to fetch.
		/// </param>
		public IRequest TopNScores(string board, int topN)
		{
			return new WWWRequest(TopNScoresActions(board, topN));
		}
		
		/// <summary>
		/// Gets the player's score and rank from a scoreboard.
		/// </summary>
		/// <returns>
		/// An IRequest whose Result contains "best" and "rank".
		/// </returns>
		/// <param name='board'>
		/// Scoreboard name
		/// </param>
		public IRequest GetPlayerScoreAndRank(string board)
		{
			if (!Registered) return new FailedRequest(RequestStatus.NOT_REGISTERED, "not registered");
			return new WWWRequest(LoginAnd(GetScoreAndRankActions(board)));
		}
		
		/// <summary>
		/// Posts a score for the player.
		/// </summary>
		/// <returns>
		/// An IRequest whose Status indicates success or failure
		/// </returns>
		/// <param name='board'>
		/// Scoreboard name
		/// </param>
		/// <param name='score'>
		/// Score to post
		/// </param>
		public IRequest PostScore(string board, int score)
		{
			if (!Registered) return new FailedRequest(RequestStatus.NOT_REGISTERED, "not registered");
			return new WWWRequest(LoginAnd(PostScoreActions(board, score)));
		}
		
		/// <summary>
		/// Sends a string message to another player.
		/// </summary>
		/// <returns>
		/// An IRequest whose Status indicates success or failure to queue the message.
		/// </returns>
		/// <param name='to'>
		/// The UID of the recipient.
		/// </param>
		/// <param name='mesg'>
		/// A string which the recipient can decode into the content of the message.
		/// </param>
		public IRequest SendMessage(string to, string mesg)
		{
			if (!Registered) return new FailedRequest(RequestStatus.NOT_REGISTERED, "not registered");
			return new WWWRequest(LoginAnd(SendMessageActions(to, mesg)));
		}
		
		/// <summary>
		/// Receives messages from the message queue.
		/// </summary>
		/// <returns>
		/// An IRequest whose Result contains a List of messages "mss" and a mapping
		/// "who" from Nickname to UID for the senders.
		/// </returns>
		/// <param name='limit'>
		/// Limit to the number of messages that will be retrieved.
		/// </param>
		/// <description>
		/// The Result may be something like:
		/// <code>
		/// { “mss”: [	{“msg”: “23,6”, “nik”: “Bob”}, {“msg”: “23,2”, “nik”: “BloomFan”} ],
		///   “who”: { “Bob”: “93f684c7-2acb-4c8a-ba38-24f685448f55”, “BloomFan”: “214684c0-2acb-4c8a-ba30-87eb85148f25” }
		/// }
		/// </code>
		/// </description>
		public IRequest RecvMessages(int limit=100)
		{
			if (!Registered) return new FailedRequest(RequestStatus.NOT_REGISTERED, "not registered");
			return new WWWRequest(LoginAnd(RecvMessagesActions(limit)));
		}
		
		/// <summary>
		/// Rates another player (sends them a 'like' message and increments a score for them)
		/// </summary>
		/// <returns>
		/// An IRequest whose status indicates success or failure
		/// </returns>
		/// <param name='other'>
		/// Other player's UID
		/// </param>
		/// <param name='mesg'>
		/// Message text same as for SendMessage.
		/// </param>
		/// <param name='board'>
		/// Scoreboard on which the rating should be applied.
		/// </param>
		/// <param name='amount'>
		/// Amount to increment the other player's score (default 1)
		/// </param>
		public IRequest RatePlayer(string other, string mesg, string board, int amount=1)
		{
			if (!Registered) return new FailedRequest(RequestStatus.NOT_REGISTERED, "not registered");
			return new WWWRequest(LoginAnd(RatePlayerActions(other, mesg, board, amount)));
		}
		
		/// <summary>
		/// Gets details of a scoreboard.
		/// </summary>
		/// <returns>
		/// An IRequest whose Result contains details of the scoreboard
		///   "tit" : title
		///   "dsc" : description
		///   "ord" : order (pos or neg)
		///   "unit": score units
		/// </returns>
		/// <param name='board'>
		/// Scoreboard name.
		/// </param>
		public IRequest GetScoreboard(string board)
		{
			if (!Registered) return new FailedRequest(RequestStatus.NOT_REGISTERED, "not registered");
			return new WWWRequest(LoginAnd(GetScoreBoardActions(board)));
		}
		
		/// <summary>
		/// Gets configuration settings.
		/// </summary>
		/// <returns>
		/// An IRequest whose result contains a dictionary "strings" containing string-valued
		/// settings from the database.
		/// </returns>
		/// <param name='language'>
		/// Requested language for the strings, as a two-character language code.
		/// </param>
		public IRequest GetSettings(string language="en")
		{
			return new WWWRequest(GetSettingsActions(language));
		}

		/// <summary>
		/// Redeems a coupon.
		/// </summary>
		/// <returns>An IRequest whose result may contain entries "hearts" and "gems"
		/// with integer values for the number of hearts and gems the coupon is worth.
		/// Both are optional.
		/// </returns>
		/// <param name="coupon">Coupon code</param>
		public IRequest RedeemCoupon(string coupon)
		{
			return new WWWRequest(UnsignedRequestActions("/app/multi/coupon?code=" + UrlEncode(coupon), null, ""));
		}

		/// <summary>
		/// Gets a set of language-localised resources
		/// </summary>
		/// <example>
		/// The Result may contain something like (JSON notation):
		/// <code>
		/// {
		///   "pages":[
		///     {"head": "Pack 1",
		///      "code": "p1",
		///      "text": "A pack of things",
		///      "price": "1000 hearts"},
		///     {"head": "Pack 2",
		///      "code": "p2",
		///      "text": "A pack of other, better things",
		///      "price": "2000 hearts"},
		///   ],
		///   "lang":"en"
		///  }
		/// </code>
		/// </example>
		/// <returns>An IRequest whose Result contains a List entry "pages".
		/// The items in the list are objects containing keys and values.
		/// </returns>
		/// <param name="language">Language.</param>
		/// <param name="type">Type.</param>
		public IRequest GetResourceList(string language, string type="pack")
		{
			return new WWWRequest(GetResourceListActions(language, type));
		}
        /// <summary>
        /// Verifies a Google puchase receipt using the web service.
        /// You should confirm the result afterwards with CheckVerifiedPurchaseResult.
        /// </summary>
        /// <returns>IRequest whose result is the result of the web service call</returns>
        /// <param name="receipt">Receipt.</param>
        /// <param name="nonce">Nonce used for next verification step</param>
        public IRequest VerifyPurchase(string receipt, string nonce)
        {
            return new WWWRequest(VerifyPurchaseActions(receipt, nonce));
        }

        /// <summary>
        /// Checks the verified purchase result.
        /// </summary>
        /// <returns><c>true</c>, if purchase result matches the provided nonce, <c>false</c> otherwise.</returns>
        /// <param name="result">Result of the IRequest returned from VerifyPurchase</param>
        /// <param name="nonce">Nonce string from the original request.</param>
        public bool CheckVerifiedPurchaseResult(IDictionary<string, object> result, string nonce)
        {
            if (!result.ContainsKey("ver")) return false;
            string ver = result["ver"] as String;
            byte[] key = Encoding.UTF8.GetBytes(ApplicationKey);
            string expected = WebData.HexDigest(key, nonce + "OK");
            return expected == ver;
        }
		#endregion
				
		#region Utility Functions

		/// <summary>
		/// Current UTC time as seconds from UNIX epoch, adjusted using
		/// time offset from server time, if this has been set.
		/// </summary>
		/// <returns>
		/// Current UTC time as seconds from UNIX epoch
		/// </returns>
		public double UnixUtcNow() {
			DateTime now = DateTime.UtcNow + TimeOffsetFromServer;
			DateTime epoch = new DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
			return (now - epoch).TotalSeconds;
		}
		
		/// <summary>
		/// A WWW with an x-tsumanga-sign header signing the POST data
		/// using the specified key.
		/// </summary>
		/// <returns>
		/// The WWW object
		/// </returns>
		/// <param name='url'>
		/// URL (to be appended to the host from the constructor)
		/// </param>
		/// <param name='key'>
		/// A byte array that will be the HMAC key used to sign the data.
		/// </param>
		/// <param name='body'>
		/// An object that will be serialised to JSON and encoded as UTF8,
		/// which will then be used with the key to make an HMAC signature.
		/// </param>
		internal WWW SignedWWW(string url, byte[] key, object body, string methodOverride)
		{
			string json = (body is string) ? body as string : WebData.JsonSerialise(body);
			Dictionary<string,string> headers = new Dictionary<string,string>();
			headers["Content-Type"] = "application/json";
			byte[] data = Encoding.UTF8.GetBytes(json);
			// now use gzip content encoding if it makes the request smaller
			byte[] zippedData = WebData.Gzip(data);
			if (zippedData.Length < data.Length)
			{
				data = zippedData;
				headers["Content-Encoding"] = "gzip";
			}
			headers["x-tsumanga-sign"] = WebData.HexDigest(key, data);
			if (methodOverride != null) headers["X-HTTP-Method-Override"] = methodOverride;
			
			return new WWW(WebServiceHost + url, data, headers);
		}
		
		internal WWW SignedWWW(string url, byte[] key, object body)
		{
			return SignedWWW (url, key, body, null);
		}
		#endregion
		
		#region Private Members
		private const string ApplicationKey = "39f1eb64def7c835a7a12fbbb596d5cea772a869";
		private string Uid=null;
		private bool Banned=false;
		private string SecretKey=null;
		private byte[] SigningKey=null;
		private string Nick=null;
		private string WebServiceHost=null;
		private const long TicksPerSecond = 10000000; // ten million * 100 ns = 1s
		
		private TimeSpan TimeOffsetFromServer = new TimeSpan();
		private DateTime LoginExpiry = DateTime.UtcNow;
		#endregion
		
		#region Private Implementation
		/// <summary>
		/// gets a unique id for the device
		/// </summary>
		/// <returns>unique id</returns>
		private string GetUniqueDeviceID()
		{
			// If registered with facebook use the facebook ID as an identifier
			string UniqueID = SystemInfo.deviceUniqueIdentifier;
			if( PlayerPrefs.GetInt( "FacebookRegistered" , 0 ) == 1 )
			{
				UniqueID = PlayerPrefs.GetString( "FacebookID" );
			}
			
			/*
			#if UNITY_IPHONE
			// On iOS return the advertisingIdentifier as it's guaranteed to be the same even after removing/reinstalling the app
			return iPhone.advertisingIdentifier;
            #else
			return SystemInfo.deviceUniqueIdentifier;
            #endif
			*/
			
			return( UniqueID );
		}

		private IEnumerator RegisterPlayerActions(string nick, string locale, bool reclaim=false)
		{
			// This is the player's initial nickname
			Nick = nick;
			
			// generate secret key for new player
			RandomNumberGenerator gen = RandomNumberGenerator.Create();
			byte[] secretBytes = new byte[32];
			gen.GetBytes(secretBytes);
			SecretKey = WebData.BytesToHex(secretBytes);
			byte[] appKey = Encoding.UTF8.GetBytes(ApplicationKey);
			
			// yield a WWW object to get the new player's UUID
			IDictionary<string, object> reqBody = WebData.QuickDict(
				"nik", nick,
				"key", SecretKey,
				"dev", GetUniqueDeviceID(),
				"loc", locale
				);
			
			// if we want to re-register same name on same device as previously...
			if (reclaim) reqBody["rec"] = true;
			
			WWW req = SignedWWW("/app/multi/register", appKey, reqBody);
			yield return req;
			if (String.IsNullOrEmpty(req.error))
			{
				IDictionary<string, object> resDict = WebData.DictFromBytes(req.bytes);
				if (resDict != null)
				{
					Uid = resDict["uid"] as string;
					Banned = resDict.ContainsKey("ban") && (bool)(resDict["ban"]);
				}
				else
				{
					Uid = null;
					yield return RequestStatus.FAIL_UNKNOWN;
				}
			}
			else
			{
				Debug.LogError("RegisterPlayer: " + req.error);
			}
		}
		
		private IEnumerator UnsignedRequestActions(string path, object reqBody, string appKey)
		{
			Dictionary<string,string> headers = new Dictionary<string,string>();
			headers["x-tsumanga-app"] = appKey;
			WWW req = new WWW(WebServiceHost + path, Encoding.UTF8.GetBytes(WebData.JsonSerialise(reqBody)), headers);
			yield return req;
		}
		
		private WWW LoginWWW()
		{
			IDictionary<string, object> loginBody = WebData.QuickDict(
				"uid", Uid,
				"now", UnixUtcNow());
			return SignedWWW("/app/multi/login", Encoding.UTF8.GetBytes(SecretKey), loginBody);
		}
		
		private void AfterLogin(WWW login)
		{
			IDictionary<string, object> result = WebData.DictFromBytes(login.bytes);
			SigningKey = Encoding.UTF8.GetBytes(result["key"] as string);
			object ttl = result["ttl"];
			if (ttl is double)
			{
				Int64 ttlTicks = (Int64) ((double) ttl * TicksPerSecond); //  ten million ticks per second (100ns)
				LoginExpiry = DateTime.UtcNow + new TimeSpan(ttlTicks);
			}
		}
		
		/// <summary>
		/// Carry out a set of actions after first logging in, if that is necessary.
		/// </summary>
		/// <returns>
		/// Yields maybe a login WWW, and if that is sucessful or not needed, yields from actions specified.
		/// </returns>
		/// <param name='actions'>
		/// IEnumerator to yield from if login succeeds or is not needed.
		/// </param>
		private IEnumerator LoginAnd(IEnumerator actions)
		{
			if (!LoggedIn)
			{
				WWW login = LoginWWW();
				yield return login;
				if (!String.IsNullOrEmpty(login.error)) yield break; // give up now
				AfterLogin(login);
			}
			while (actions.MoveNext()) yield return actions.Current;
		}
		
		private IEnumerator ChangeNicknameActions(string newNick)
		{
			// send request to change nickname
			IDictionary<string, object> reqBody = WebData.QuickDict("uid", Uid, "nik", newNick);
			WWW req = SignedWWW("/app/multi/rename", SigningKey, reqBody);
			yield return req;
			
			if (String.IsNullOrEmpty(req.error))
			{
				Nick = newNick;
				Banned = false;
			}
		}
				
		private IEnumerator StorePlayerDataActions(string dataKey, object dataObject)
		{
			string URL = String.Format("/app/player/{0}/data/{1}", Uid, dataKey);
			WWW req = SignedWWW(URL, SigningKey, dataObject);
			yield return req;
		}

		private IEnumerator GetPlayerDataActions(string uid, string dataKey)
		{
			string URL = String.Format("{2}/app/player/{0}/data/{1}", uid, dataKey, WebServiceHost);
			yield return new WWW(URL);
		}

		private IEnumerator RestorePlayerDataActions(string dataKey)
		{
			string URL = "/app/player/unlock";
			IDictionary<string, object> reqBody = WebData.QuickDict("uid", Uid, "key", dataKey);
			WWW req = SignedWWW(URL, SigningKey, reqBody);
			yield return req;
		}

		private IEnumerator GetLinkedDataActions(string dataKey)
		{
			string URL = String.Format("/app/player/{0}/linked/{1}", Uid, dataKey);
			WWW req = SignedWWW(URL, SigningKey, null, "GET");
			yield return req;
		}
		
		private IEnumerator GetServerTimeActions()
		{
			WWW req = new WWW(WebServiceHost + "/app/multi/time");
			yield return req;
			if (String.IsNullOrEmpty(req.error))
			{
				IDictionary<string, object> result = WebData.DictFromBytes(req.bytes);
				if (result.ContainsKey("now"))
				{
					double server_now = (double) result["now"];
					double local_now = UnixUtcNow();
					double correction = server_now - local_now;
					TimeOffsetFromServer += new TimeSpan((long)(correction * TicksPerSecond));
				}
			}
		}
		
		private string UrlEncode(string input)
		{
			StringBuilder output = new StringBuilder();
			for (int i = 0; i < input.Length; ++i)
			{
				string s = input.Substring (i, 1);
				char c = s[0];
				bool safe = (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') ||
					(c >= 'a' && c <='z') || c == '-' || c == '_' || c == '.' ||
						c == '~';
				if (!safe)
				{
					byte[] bs = Encoding.UTF8.GetBytes(s);
					foreach (byte b in bs)
					{
						output.AppendFormat("%{0:x2}",b);
					}
				}
				else
				{
					output.Append(s);
				}
			}
			return output.ToString();
		}
		
		private IEnumerator SearchPlayerActions(string nickname, bool exact)
		{
			string URL = "/app/player/search/" + UrlEncode(nickname);
			if (exact) URL += "?exact";
			yield return SignedWWW(URL, Encoding.UTF8.GetBytes(ApplicationKey), null, "GET");
		}
        /// <summary>
        /// get list of previous players based on device id
        /// </summary>
        /// <returns></returns>

        private IEnumerator SearchPreviousPlayersActions()
		{
			string URL = "/app/player/device";
			IDictionary<string, object> reqbody = WebData.QuickDict("dev", GetUniqueDeviceID());
            Debug.Log(GetUniqueDeviceID());
			byte[] appKey = Encoding.UTF8.GetBytes(ApplicationKey);
			yield return SignedWWW(URL, appKey, reqbody);
		}
		
		private IEnumerator LinkedPlayersActions()
		{
			string URL = String.Format("/app/player/{0}/links", PlayerUid);
			yield return SignedWWW(URL, SigningKey, null, "GET");
		}
		
		private IEnumerator LinkPlayerActions(string other)
		{
			IDictionary<string, object> linkBody = WebData.QuickDict("uid", Uid, "lnk", other);
			yield return SignedWWW("/app/player/link", SigningKey, linkBody);
		}

		private IEnumerator UnlinkPlayerActions(string other)
		{
			IDictionary<string, object> linkBody = WebData.QuickDict("uid", Uid, "lnk", other);
			yield return SignedWWW("/app/player/unlink", SigningKey, linkBody);
		}
		
		private IEnumerator TopNScoresActions(string board, int topN)
		{
			yield return new WWW(WebServiceHost + String.Format("/app/board/{0}/scores?topN={1}", UrlEncode(board), topN));
		}
		
		private IEnumerator GetScoreAndRankActions(string board)
		{
			yield return new WWW(String.Format("{2}/app/player/{0}/score/{1}", Uid, UrlEncode(board), WebServiceHost));
		}
		
		private IEnumerator PostScoreActions(string board, int score)
		{
			IDictionary<string, object> body = WebData.QuickDict("score", score);
			yield return SignedWWW(String.Format("/app/player/{0}/score/{1}", Uid, UrlEncode(board)), SigningKey, body);
		}
		
		private IEnumerator SendMessageActions(string to, string mesg)
		{
			IDictionary<string, object> body = WebData.QuickDict("uid", Uid, "to", to, "msg", mesg);
			yield return SignedWWW("/app/message/send", SigningKey, body);
		}
		
		private IEnumerator RecvMessagesActions(int limit=100)
		{
			IDictionary<string, object> body = WebData.QuickDict("uid", Uid, "lim", limit);
			yield return SignedWWW("/app/message/recv", SigningKey, body);
		}
		
		private IEnumerator RatePlayerActions(string to, string mesg, string board, int amount)
		{
			IDictionary<string, object> body = WebData.QuickDict("uid", Uid, "to", to, "msg", mesg, "inc", amount);
			yield return SignedWWW("/app/message/rate/" + UrlEncode(board), SigningKey, body);
		}
		
		private IEnumerator GetScoreBoardActions(string board)
		{
			yield return new WWW(WebServiceHost + "/app/board/" + UrlEncode(board));
		}
		
		private IEnumerator GetSettingsActions(string language)
		{
			yield return new WWW(WebServiceHost + "/app/multi/settings/" + language);
		}

		private IEnumerator GetResourceListActions(string language, string type)
		{
			yield return new WWW(String.Format("{0}/app/pages/{1}?lang={2}&full=merge", WebServiceHost, UrlEncode(type), UrlEncode(language)));
		}
        private IEnumerator VerifyPurchaseActions(string receipt, string nonce)
        {
            byte[] key = Encoding.UTF8.GetBytes(ApplicationKey);
            // take off last closing brace in receipt
            int lastbrace = receipt.LastIndexOf('}');
            StringBuilder body = new StringBuilder(receipt.Substring(0, lastbrace));
            // add nonce field to the JSON receipt
            body.AppendFormat(", \"nonce\":\"{0}\"}}", nonce);
            string url = "/app/multi/receipt/google";
#if LITE
            url += "?variant=lite";
#endif
            yield return SignedWWW(url, key, body.ToString());
        }
		
		#endregion
	}

}