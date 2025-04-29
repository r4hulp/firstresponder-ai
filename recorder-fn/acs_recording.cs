// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using System;
using Azure.Communication.CallAutomation;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace RapidCircle.AISupport
{
    public class acs_recording
    {
        private readonly ILogger<acs_recording> _logger;

        public acs_recording(ILogger<acs_recording> logger)
        {
            _logger = logger;
        }

        [Function(nameof(acs_recording))]
        public async Task RunAsync([EventGridTrigger] EventGridEvent eventGridEvent)
        {
            if (eventGridEvent.EventType == "Microsoft.Communication.CallStarted")
            {
                _logger.LogInformation("Call started");
                var callEvent = eventGridEvent.Data.ToObjectFromJson<CallStartedEvent>();
				await startRecordingAsync(callEvent.serverCallId, _logger);

				// CallStartedEvent class is defined in documentation, but the objects looks like this:
				// public class CallStartedEvent
				// {
				//     public StartedBy startedBy { get; set; }
				//     public string serverCallId { get; set; }
				//     public Group group { get; set; }
				//     public bool isTwoParty { get; set; }
				//     public string correlationId { get; set; }
				//     public bool isRoomsCall { get; set; }
				// }
				// public class Group
				// {
				//     public string id { get; set; }
				// }
				// public class StartedBy
				// {
				//     public CommunicationIdentifier communicationIdentifier { get; set; }
				//     public string role { get; set; }
				// }
				// public class CommunicationIdentifier
				// {
				//     public string rawId { get; set; }
				//     public CommunicationUser communicationUser { get; set; }
				// }
				// public class CommunicationUser
				// {
				//     public string id { get; set; }
				// }
			}
        }


        public static async Task startRecordingAsync(String serverCallId, ILogger<acs_recording> logger)
        {
            CallAutomationClient callAutomationClient = new CallAutomationClient(Environment.GetEnvironmentVariable("ACS_CONNECTION_STRING"));
            StartRecordingOptions recordingOptions = new StartRecordingOptions(new ServerCallLocator(serverCallId));
            recordingOptions.RecordingChannel = RecordingChannel.Mixed;
            recordingOptions.RecordingContent = RecordingContent.Audio;
            recordingOptions.RecordingFormat = RecordingFormat.Mp3;
            recordingOptions.RecordingStorage = RecordingStorage.CreateAzureBlobContainerRecordingStorage(new Uri("https://rcsupportcallrecords.blob.core.windows.net/records"));
            var startRecordingResponse = await callAutomationClient.GetCallRecording().StartAsync(recordingOptions).ConfigureAwait(false);
        }

    }
}
