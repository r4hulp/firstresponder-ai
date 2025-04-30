// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using System;
using Azure.Communication.CallAutomation;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace recorder_fn.Functions
{
    public class GetRecording
    {
        private readonly ILogger<GetRecording> _logger;

        public GetRecording(ILogger<GetRecording> logger)
        {
            _logger = logger;
        }

        [Function(nameof(GetRecording))]
        public void Run([EventGridTrigger] EventGridEvent eventGridEvent)
        {
            _logger.LogInformation("Event type: {type}, Event subject: {subject}", eventGridEvent.EventType, eventGridEvent.Subject);
        }
    }
} 