namespace recorder_fn
{
    public static class Constants
    {
        // Environment Variables
        public const string ACS_CONNECTION_STRING = "ACS_CONNECTION_STRING";
        public const string BLOB_CONNECTION_STRING = "BLOB_CONNECTION_STRING";
        public const string BLOB_CONTAINER_NAME = "BLOB_CONTAINER_NAME";
        public const string AZURE_WEBJOBS_STORAGE = "AzureWebJobsStorage";
        public const string SPEECH_API_URL = "SPEECH_API_URL";
        public const string AZURE_OPENAI_KEY = "AZURE_OPENAI_KEY";
        public const string AZURE_AI_RESOURCE_NAME = "AZURE_AI_RESOURCE_NAME";
        public const string AZURE_AI_RESOURCE_DEPLOYMENT_NAME = "AZURE_AI_RESOURCE_DEPLOYMENT_NAME";
        public const string AZURE_AI_RESOURCE_KEY = "AZURE_AI_RESOURCE_KEY";

        // Event Types
        public const string EVENT_TYPE_CALL_STARTED = "Microsoft.Communication.CallStarted";
        public const string EVENT_TYPE_RECORDING_FILE_STATUS_UPDATED = "Microsoft.Communication.RecordingFileStatusUpdated";
        public const string EVENT_TYPE_INCOMING_CALL = "Microsoft.Communication.IncomingCall";

        // Queue Names
        public const string QUEUE_TRANSCRIBE = "transcribe";

        // Table Names
        public const string TABLE_RECORDS = "records";

        // File Formats
        public const string AUDIO_FILE_FORMAT = "mp3";
        public const string METADATA_FILE_FORMAT = "json";

        // Date Formats
        public const string DATE_FORMAT_YYYYMMDD = "yyyyMMdd";
        public const string DATE_FORMAT_YYYYMMDDHHMMSS = "yyyyMMddHHmmss";

        // File Path Templates
        public const string AUDIO_FILE_PATH_TEMPLATE = "{0}/call-{1}.{2}";
        public const string METADATA_FILE_PATH_TEMPLATE = "{0}/metadata-{1}.{2}";

        // API Headers
        public const string HEADER_ACCEPT = "Accept";
        public const string HEADER_ACCEPT_VALUE = "application/json";
        public const string HEADER_OCP_APIM_SUBSCRIPTION_KEY = "Ocp-Apim-Subscription-Key";

        // Speech API Configuration
        public const string SPEECH_API_DEFINITION = @"{""locales"":[""en-us""],""profanityFilterMode"":""None"",""diarization"":{""maxSpeakers"":2,""enabled"":true}}";

        // Recording Options
        public const string RECORDING_CHANNEL_MIXED = "Mixed";
        public const string RECORDING_CONTENT_AUDIO = "Audio";
        public const string RECORDING_FORMAT_MP3 = "Mp3";

        // Local Development
        public const string LOCAL_DEV_URL = "http://localhost:7071/runtime/webhooks/EventGrid?functionName={0}";
    }
} 