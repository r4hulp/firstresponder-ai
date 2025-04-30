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

namespace recorder_fn
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
			if (eventGridEvent.EventType == "Microsoft.Communication.RecordingFileStatusUpdated")
			{

				_logger.LogInformation("Recording available event received");

				string acsConnectionString = Environment.GetEnvironmentVariable("ACS_CONNECTION_STRING");
				string blobConnectionString = Environment.GetEnvironmentVariable("BLOB_CONNECTION_STRING");
				string blobContainerName = Environment.GetEnvironmentVariable("BLOB_CONTAINER_NAME");
				string queueConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

				if(string.IsNullOrEmpty(acsConnectionString))
				{
					_logger.LogError("ACS_CONNECTION_STRING is not set");
					return;
				}

				if (string.IsNullOrEmpty(blobConnectionString))
				{
					_logger.LogError("BLOB_CONNECTION_STRING is not set");
					return;
				}	

				if (string.IsNullOrEmpty(blobContainerName))
				{
					_logger.LogError("BLOB_CONTAINER_NAME is not set");
					return;
				}

				if (string.IsNullOrEmpty(queueConnectionString))
				{
					_logger.LogError("AzureWebJobsStorage is not set");
					return;
				}
			

				_logger.LogInformation("ACS_CONNECTION_STRING: " + acsConnectionString);
				_logger.LogInformation("BLOB_CONNECTION_STRING: " + blobConnectionString);
				_logger.LogInformation("BLOB_CONTAINER_NAME: " + blobContainerName);
				_logger.LogInformation("AzureWebJobsStorage: " + queueConnectionString);
	
				CallAutomationClient callAutomationClient = new CallAutomationClient(acsConnectionString);
				BlobServiceClient blobServiceClient = new BlobServiceClient(blobConnectionString);
				BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);
				_logger.LogInformation($"Call Recording Event Received, {eventGridEvent.Data.ToString()}");

				var callEvent = eventGridEvent.Data.ToObjectFromJson<CallRecordingDetails>();
				var recordingLocation = callEvent.recordingStorageInfo.recordingChunks[0].contentLocation;
				var metadataLocation = callEvent.recordingStorageInfo.recordingChunks[0].metadataLocation;
				var recordingTime = callEvent.recordingStartTime;
				var recordingTimeString = recordingTime.ToString("yyyyMMddHHmmss");
				var recordingDownloadUri = new Uri(recordingLocation);


				_logger.LogInformation("Recording downloaded call");


				// create a folder with today's date.. if it doesn't exist
				var today = DateTime.Today;
				var folderName = today.ToString("yyyyMMdd");
				var recordingGuid = Guid.NewGuid().ToString();
				var detailedFolderName = $"{folderName}/{recordingGuid}";
				var audioFilePath = $"{detailedFolderName}/call-{recordingTimeString}.mp3";
				var transcriptFilePath = $"{detailedFolderName}/metadata-{recordingTimeString}.json";


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
						_logger.LogInformation($"Downloading recording from {recordingDownloadUri}");
						var recordingStream = callAutomationClient.GetCallRecording().DownloadStreaming(recordingDownloadUri).Value;

						await audioBlobClient.UploadAsync(recordingStream, true);

						_logger.LogInformation($"downloading call metadata from {metadataLocation}");

						var transcriptStream = callAutomationClient.GetCallRecording().DownloadStreaming(new Uri(metadataLocation)).Value;

						_logger.LogInformation($"Metadata is: {transcriptStream}");

						// parse the transcript stream into a json object
						var transcriptObject = JsonSerializer.Deserialize<CallMetadata>(transcriptStream);

						//convert transcriptObject to json string
						var transcriptJson = JsonSerializer.Serialize(transcriptObject);

						_logger.LogInformation($"Transcript object: {transcriptJson}");


						await transcriptBlobClient.UploadAsync(new BinaryData(transcriptJson), true);

						_logger.LogInformation($"Transcript uploaded to BLOB");

						// update information

						var tableService = new CallRecordingService(blobConnectionString, "records");

						string callId = Guid.NewGuid().ToString();

						string partitionKey = DateTime.UtcNow.ToString("yyyyMMdd");

						// get caller that doesn't start with acs:
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

						_logger.LogInformation("Saving the call information in table");
						await tableService.SaveCallRecordingAsync(callEntity);

						// send a message to the queue
						_logger.LogInformation("Sending a queue message for transcription");
						var queueService = new QueueService(queueConnectionString, "transcribe");
						var queueIdentifier = $"{callId}:::{partitionKey}";
						var bytes = Encoding.UTF8.GetBytes(queueIdentifier);
						await queueService.SendMessageAsync(Convert.ToBase64String(bytes));

					}
					catch (Exception e)
					{
						_logger.LogInformation("Could not save the call information in table");
						_logger.LogError($"error: {e.Message}");
					}
				}
				_logger.LogInformation("Recording uploaded to BLOB");
			}


		}
	}
}
