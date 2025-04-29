using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recorder_fn.Models
{
	public class CallRecordingEntity : ITableEntity
	{
		public string PartitionKey { get; set; } // e.g., Date or CallerID
		public string RowKey { get; set; } // Unique Call ID
		public List<string> Participants { get; set; }

		public string Caller { get; set; }
		public string CallDuration { get; set; } // in seconds
		public DateTimeOffset StartTime { get; set; }
		public string SessionEndReason { get; set; }
		public string TranscriptUrl { get; set; } // URL if stored in Blob Storage
		public string RelativeTranscriptUrl { get; set; }

		public string AudioUrl { get; set; }

		public string RelativeAudioUrl { get; set; }

		public bool IsTranscribed { get; set; }

		public string Transcription { get; set; }

		public DateTime TranscribedOn { get; set; }

		public string CallSummary { get; set; }

		public ETag ETag { get; set; }
		public DateTimeOffset? Timestamp { get; set; }
	}
}
