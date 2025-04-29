using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace recorder_fn
{
	public class CallDetails
	{
		[JsonPropertyName("to")]
		public CallParticipant To { get; set; }
		[JsonPropertyName("from")]
		public CallParticipant From { get; set; }
		[JsonPropertyName("serverCallId")]
		public string ServerCallId { get; set; }
		[JsonPropertyName("callerDisplayName")]
		public string CallerDisplayName { get; set; }
		[JsonPropertyName("incomingCallContext")]
		public string IncomingCallContext { get; set; }
		[JsonPropertyName("shrToken")]
		public string ShrToken { get; set; }
	}

	public class CallParticipant
	{
		[JsonPropertyName("kind")]
		public string Kind { get; set; }
		[JsonPropertyName("rawId")]
		public string RawId { get; set; }
		[JsonPropertyName("phoneNumber")]
		public PhoneNumberDetails PhoneNumber { get; set; }
	}

	public class PhoneNumberDetails
	{
		[JsonPropertyName("value")]
		public string Value { get; set; }
	}
}
