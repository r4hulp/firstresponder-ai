using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recorder_fn.Models
{
	public class RecordingChunk
	{
		public string documentId { get; set; }
		public int index { get; set; }
		public string endReason { get; set; }
		public string contentLocation { get; set; }
		public string metadataLocation { get; set; }
	}

	public class RecordingStorageInfo
	{
		public List<RecordingChunk> recordingChunks { get; set; }
	}

	public class CallRecordingDetails
	{
		public RecordingStorageInfo recordingStorageInfo { get; set; }
		public DateTime recordingStartTime { get; set; }
		public int recordingDurationMs { get; set; }
		public string recordingId { get; set; }
		public string storageType { get; set; }
		public string sessionEndReason { get; set; }
	}
} 