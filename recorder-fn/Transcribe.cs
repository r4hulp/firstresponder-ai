using System;
using System.Net.Http.Headers;
using System.Text;
using Azure.AI.OpenAI;
using Azure;
using Azure.Communication.CallAutomation;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using recorder_fn.Models;
using recorder_fn.Services;

namespace recorder_fn
{
    public class Transcribe
    {
        private readonly ILogger<Transcribe> _logger;

        public Transcribe(ILogger<Transcribe> logger)
        {
            _logger = logger;
        }

        [Function(nameof(Transcribe))]
        public async Task Run([QueueTrigger("transcribe")] QueueMessage message)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");

            // convert message from base64 to string
            var inputMessage = Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));

            _logger.LogInformation($"Splitting the InputMessage: {inputMessage}");
            // split the callId into callId and partitionKey
            var callIdParts = inputMessage.Split(":::");
            _logger.LogInformation($"Found {callIdParts.Length} parts");
            var rowKey = callIdParts[0];
            var partitionKey = callIdParts[1];

            _logger.LogInformation($"RowKey: {rowKey}");
            _logger.LogInformation($"PartitionKey: {partitionKey}");

            // fetch table entry for callId
            var tableService = new CallRecordingService(Environment.GetEnvironmentVariable("BLOB_CONNECTION_STRING"), "records");

            var callRecordingEntity = tableService.GetCallRecordingAsync(rowKey, partitionKey).Result;

            _logger.LogInformation($"CallRecordingEntity: {callRecordingEntity}");

            string localFilePath = await DownloadBlobFileAsync(callRecordingEntity.RelativeAudioUrl, _logger);

            string transcription = await SendFileToSpeechAPIAsync(localFilePath, _logger);

            string callSummary = await GetCallSummary(transcription, _logger);

            if (callSummary != null)
            {
                callRecordingEntity.CallSummary = callSummary;
            }

			// update the call recording entity with the transcription
			callRecordingEntity.Transcription = transcription;
            callRecordingEntity.IsTranscribed = true;
            callRecordingEntity.TranscribedOn = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            await tableService.UpdateCallRecordingAsync(callRecordingEntity);

            _logger.LogInformation($"Transcription: {transcription}");

            // Clean up the temporary file
            if (File.Exists(localFilePath))
            {
                File.Delete(localFilePath);
            }
        }

        // Method to download file from Azure Blob Storage
        private static async Task<string> DownloadBlobFileAsync(string relativeBlobUrl, ILogger<Transcribe> logger)
        {
            string containerRelativeBlobPath =  relativeBlobUrl;

            logger.LogInformation($"ContainerRelativeBlobPath: {containerRelativeBlobPath}");
            var blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("BLOB_CONNECTION_STRING"));
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(Environment.GetEnvironmentVariable("BLOB_CONTAINER_NAME"));
            var blobClient = blobContainerClient.GetBlobClient(containerRelativeBlobPath);

            logger.LogInformation($"BlobClient for url: {blobClient.Uri.ToString()}"); 

            // Local file path where the blob will be saved
            string localFilePath = Path.Combine(Path.GetTempPath(), containerRelativeBlobPath);

            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(localFilePath));

            logger.LogInformation($"LocalFilePath: {localFilePath}");

            // Download the blob to the local file
            BlobDownloadInfo download = await blobClient.DownloadAsync();

            // Save the downloaded content to a file
            using (var fileStream = File.OpenWrite(localFilePath))
            {
                await download.Content.CopyToAsync(fileStream);
            }

            Console.WriteLine($"File downloaded to: {localFilePath}");

            return localFilePath;
        }

        private static async Task<string> SendFileToSpeechAPIAsync(string audioFilePath, ILogger<Transcribe> logger)
        {
            string apiUrl = "https://ai-rahulpatilai696216630889.cognitiveservices.azure.com/speechtotext/transcriptions:transcribe?api-version=2024-11-15";
            using (var client = new HttpClient())
            {
                // Set the request headers
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY"));

                using (var formData = new MultipartFormDataContent())
                {
                    // Read the audio file
                    var audioFileContent = new ByteArrayContent(File.ReadAllBytes(audioFilePath));
                    audioFileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/mp3"); // Adjust if needed

                    // Add audio file to the form
                    formData.Add(audioFileContent, "audio", Path.GetFileName(audioFilePath));

                    // Add the definition JSON to the form
                    var definition = @"{""locales"":[""en-us""],""profanityFilterMode"":""None"",""diarization"":{""maxSpeakers"":2,""enabled"":true}}";

                    logger.LogInformation(definition);

                    formData.Add(new StringContent(definition), "definition");

                    // Send the POST request
                    var response = await client.PostAsync(apiUrl, formData);
                    response.EnsureSuccessStatusCode();

                    // Read the response content
                    string responseContent = await response.Content.ReadAsStringAsync();
                    logger.LogInformation("Response from Speech API: " + responseContent);
                    return responseContent;
                }
            }
        }

		private async Task<string> GetCallSummary(string transcript, ILogger<Transcribe> _logger)
		{
            _logger.LogInformation($"https://{Environment.GetEnvironmentVariable("AZURE_AI_RESOURCE_NAME")}.openai.azure.com/");
			var endpoint = new Uri($"https://{Environment.GetEnvironmentVariable("AZURE_AI_RESOURCE_NAME")}.openai.azure.com/");
			var deploymentName = Environment.GetEnvironmentVariable("AZURE_AI_RESOURCE_DEPLOYMENT_NAME");
			var apiKey = Environment.GetEnvironmentVariable("AZURE_AI_RESOURCE_KEY");

            _logger.LogInformation($"API Key is ${apiKey}");
            _logger.LogInformation($"deployment name : {deploymentName}");

			AzureOpenAIClient azureClient = new(
				endpoint,
				new AzureKeyCredential(apiKey));
			ChatClient chatClient = azureClient.GetChatClient(deploymentName);

			string prompt = $@"
			You are an expert customer support agent quality analyst.
			You are given a transcript of a customer support call.
			Your job is to generate a summary of the call.
			The summary should be in the following format: [Summary of the call]
			Caller Information: [Caller Information]
			Call Duration: [Call Duration]
			Call Type: [Call Type]
			Caller Issue: [Caller Issue]
			Support Agent Resolution: [Support Agent Resolution]
			Caller Feedback: [Caller Feedback]
			Return the summary in markdown format..
			";
			List<ChatMessage> messages = new List<ChatMessage>()
			{
				new SystemChatMessage(prompt),
				new UserChatMessage("Here is the transcript of the call:"),
				new UserChatMessage(transcript),
			};

			var response = await chatClient.CompleteChatAsync(messages);

			return response.Value.Content[0].Text;
		}
	}
}
