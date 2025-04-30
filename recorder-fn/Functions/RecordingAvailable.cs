// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Azure;
using Azure.Communication.CallAutomation;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using recorder_fn.Models;
using recorder_fn.Services;

namespace recorder_fn.Functions
{
	public class RecordingAvailable
	{
		private readonly ILogger<RecordingAvailable> _logger;

		public RecordingAvailable(ILogger<RecordingAvailable> logger)
		{
			_logger = logger;
		}

		[Function(nameof(RecordingAvailable))]
		public async Task RunAsync([EventGridTrigger] EventGridEvent eventGridEvent)
		{
			if (eventGridEvent.EventType == Constants.EVENT_TYPE_RECORDING_FILE_STATUS_UPDATED)
			{
				_logger.LogInformation("Recording available event received");

				string acsConnectionString = Environment.GetEnvironmentVariable(Constants.ACS_CONNECTION_STRING);
				string blobConnectionString = Environment.GetEnvironmentVariable(Constants.BLOB_CONNECTION_STRING);
				string blobContainerName = Environment.GetEnvironmentVariable(Constants.BLOB_CONTAINER_NAME);
				string queueConnectionString = Environment.GetEnvironmentVariable(Constants.AZURE_WEBJOBS_STORAGE);

				if(string.IsNullOrEmpty(acsConnectionString))
				{
					_logger.LogError($"{Constants.ACS_CONNECTION_STRING} is not set");
					return;
				}

				if (string.IsNullOrEmpty(blobConnectionString))
				{
					_logger.LogError($"{Constants.BLOB_CONNECTION_STRING} is not set");
					return;
				}	

				if (string.IsNullOrEmpty(blobContainerName))
				{
					_logger.LogError($"{Constants.BLOB_CONTAINER_NAME} is not set");
					return;
				}

				if (string.IsNullOrEmpty(queueConnectionString))
				{
					_logger.LogError($"{Constants.AZURE_WEBJOBS_STORAGE} is not set");
					return;
				}

				CallAutomationClient callAutomationClient = new CallAutomationClient(acsConnectionString);
				BlobServiceClient blobServiceClient = new BlobServiceClient(blobConnectionString);
				BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);
				_logger.LogInformation("Call Recording Event Received");

				var callEvent = eventGridEvent.Data.ToObjectFromJson<CallRecordingDetails>();
				var recordingLocation = callEvent.recordingStorageInfo.recordingChunks[0].contentLocation;
				var metadataLocation = callEvent.recordingStorageInfo.recordingChunks[0].metadataLocation;
				var recordingTime = callEvent.recordingStartTime;
				var recordingTimeString = recordingTime.ToString(Constants.DATE_FORMAT_YYYYMMDDHHMMSS);
				var recordingDownloadUri = new Uri(recordingLocation);

				var today = DateTime.Today;
				var folderName = today.ToString(Constants.DATE_FORMAT_YYYYMMDD);
				var recordingGuid = Guid.NewGuid().ToString();
				var detailedFolderName = $"{folderName}/{recordingGuid}";
				var audioFilePath = string.Format(Constants.AUDIO_FILE_PATH_TEMPLATE, detailedFolderName, recordingTimeString, Constants.AUDIO_FILE_FORMAT);
				var transcriptFilePath = string.Format(Constants.METADATA_FILE_PATH_TEMPLATE, detailedFolderName, recordingTimeString, Constants.METADATA_FILE_FORMAT);

				var audioBlobClient = containerClient.GetBlobClient(audioFilePath);
				var transcriptBlobClient = containerClient.GetBlobClient(transcriptFilePath);

				if (audioBlobClient.Exists() && transcriptBlobClient.Exists())
				{
					_logger.LogInformation("Recording already exists");
				}
				else
				{
					try
					{
						_logger.LogInformation("Downloading recording");
						var recordingStream = callAutomationClient.GetCallRecording().DownloadStreaming(recordingDownloadUri).Value;
						await audioBlobClient.UploadAsync(recordingStream, true);

						_logger.LogInformation("Downloading call metadata");
						var transcriptStream = callAutomationClient.GetCallRecording().DownloadStreaming(new Uri(metadataLocation)).Value;
						var transcriptObject = JsonSerializer.Deserialize<CallMetadata>(transcriptStream);
						var transcriptJson = JsonSerializer.Serialize(transcriptObject);
						await transcriptBlobClient.UploadAsync(new BinaryData(transcriptJson), true);

						var tableService = new CallRecordingService(blobConnectionString, Constants.TABLE_RECORDS);
						string callId = Guid.NewGuid().ToString();
						string partitionKey = DateTime.UtcNow.ToString(Constants.DATE_FORMAT_YYYYMMDD);
						var caller = transcriptObject.participants.FirstOrDefault(p => !p.participantId.StartsWith("acs:"))?.participantId;

						var callEntity = new CallRecordingEntity
						{
							PartitionKey = partitionKey,
							RowKey = callId,
							Caller = caller ?? transcriptObject.participants[1].participantId,
							CallDuration = transcriptObject.chunkDuration.ToString(),
							StartTime = transcriptObject.chunkStartTime,
							SessionEndReason = callEvent.sessionEndReason,
							TranscriptUrl = transcriptBlobClient.Uri.ToString(),
							AudioUrl = audioBlobClient.Uri.ToString(),
							RelativeAudioUrl = audioFilePath,
							RelativeTranscriptUrl = transcriptFilePath,
							TranscribedOn = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
						};

						await tableService.SaveCallRecordingAsync(callEntity);

						var queueService = new QueueService(queueConnectionString, Constants.QUEUE_TRANSCRIBE);
						var queueIdentifier = $"{callId}:::{partitionKey}";
						var bytes = Encoding.UTF8.GetBytes(queueIdentifier);
						await queueService.SendMessageAsync(Convert.ToBase64String(bytes));

						_logger.LogInformation("Recording processing completed successfully");
					}
					catch (Exception e)
					{
						_logger.LogError($"Error processing recording: {e.Message}");
					}
				}
			}
		}
	}
} 