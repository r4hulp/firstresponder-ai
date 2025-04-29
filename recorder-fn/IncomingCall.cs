// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using System;
using Azure.Communication.CallAutomation;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RapidCircle.AISupport;

namespace recorder_fn
{
    public class IncomingCall
    {
        private readonly ILogger<IncomingCall> _logger;

        public IncomingCall(ILogger<IncomingCall> logger)
        {
            _logger = logger;
        }

        [Function(nameof(IncomingCall))]
        public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
        {
            _logger.LogInformation("Event type: {type}, Event subject: {subject}", eventGridEvent.EventType, eventGridEvent.Subject);

            _logger.LogInformation(eventGridEvent.Data.ToString());

            if (eventGridEvent.EventType == "Microsoft.Communication.IncomingCall")
            {
                _logger.LogInformation("Incoming call, starting the recording");

                var callerData = eventGridEvent.Data.ToObjectFromJson<CallDetails>();

                if (callerData != null)
                {

                    _logger.LogInformation($"the server ID is : ${callerData.ServerCallId}");

                    await startRecordingAsync(callerData.ServerCallId, _logger);
                }
            }
        }

        public static async Task startRecordingAsync(String serverCallId, ILogger<IncomingCall> logger)
        {
            CallAutomationClient callAutomationClient = new CallAutomationClient(Environment.GetEnvironmentVariable("ACS_CONNECTION_STRING"));
            StartRecordingOptions recordingOptions = new StartRecordingOptions(new ServerCallLocator(serverCallId));
            recordingOptions.RecordingChannel = RecordingChannel.Mixed;
            recordingOptions.RecordingContent = RecordingContent.Audio;
            recordingOptions.RecordingFormat = RecordingFormat.Mp3;
            recordingOptions.RecordingStorage = RecordingStorage.CreateAzureCommunicationsRecordingStorage();
            var startRecordingResponse = await callAutomationClient.GetCallRecording().StartAsync(recordingOptions).ConfigureAwait(false);
        }
    }
}
