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

namespace recorder_fn.Functions
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
            _logger.LogInformation("Processing transcription queue message");

            var inputMessage = Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));
            var callIdParts = inputMessage.Split(":::");
            var rowKey = callIdParts[0];
            var partitionKey = callIdParts[1];

            _logger.LogInformation($"Processing call recording - RowKey: {rowKey}, PartitionKey: {partitionKey}");

            var tableService = new CallRecordingService(Environment.GetEnvironmentVariable("BLOB_CONNECTION_STRING"), "records");
            var callRecordingEntity = tableService.GetCallRecordingAsync(rowKey, partitionKey).Result;

            string localFilePath = await DownloadBlobFileAsync(callRecordingEntity.RelativeAudioUrl, _logger);
            string transcription = await SendFileToSpeechAPIAsync(localFilePath, _logger);
            string callSummary = await GetCallSummary(transcription, _logger);

            if (callSummary != null)
            {
                callRecordingEntity.CallSummary = callSummary;
            }

            callRecordingEntity.Transcription = transcription;
            callRecordingEntity.IsTranscribed = true;
            callRecordingEntity.TranscribedOn = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            await tableService.UpdateCallRecordingAsync(callRecordingEntity);

            _logger.LogInformation("Transcription completed successfully");

            if (File.Exists(localFilePath))
            {
                File.Delete(localFilePath);
            }
        }

        private static async Task<string> DownloadBlobFileAsync(string relativeBlobUrl, ILogger<Transcribe> logger)
        {
            string containerRelativeBlobPath = relativeBlobUrl;
            var blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("BLOB_CONNECTION_STRING"));
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(Environment.GetEnvironmentVariable("BLOB_CONTAINER_NAME"));
            var blobClient = blobContainerClient.GetBlobClient(containerRelativeBlobPath);

            string localFilePath = Path.Combine(Path.GetTempPath(), containerRelativeBlobPath);
            Directory.CreateDirectory(Path.GetDirectoryName(localFilePath));

            BlobDownloadInfo download = await blobClient.DownloadAsync();

            using (var fileStream = File.OpenWrite(localFilePath))
            {
                await download.Content.CopyToAsync(fileStream);
            }

            return localFilePath;
        }

        private static async Task<string> SendFileToSpeechAPIAsync(string audioFilePath, ILogger<Transcribe> logger)
        {
            string apiUrl = Environment.GetEnvironmentVariable("SPEECH_API_URL");
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY"));

                using (var formData = new MultipartFormDataContent())
                {
                    var audioFileContent = new ByteArrayContent(File.ReadAllBytes(audioFilePath));
                    audioFileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/mp3");

                    formData.Add(audioFileContent, "audio", Path.GetFileName(audioFilePath));

                    var definition = @"{""locales"":[""en-us""],""profanityFilterMode"":""None"",""diarization"":{""maxSpeakers"":2,""enabled"":true}}";
                    formData.Add(new StringContent(definition), "definition");

                    var response = await client.PostAsync(apiUrl, formData);
                    response.EnsureSuccessStatusCode();

                    string responseContent = await response.Content.ReadAsStringAsync();
                    return responseContent;
                }
            }
        }

        private async Task<string> GetCallSummary(string transcript, ILogger<Transcribe> _logger)
        {
            var endpoint = new Uri($"https://{Environment.GetEnvironmentVariable("AZURE_AI_RESOURCE_NAME")}.openai.azure.com/");
            var deploymentName = Environment.GetEnvironmentVariable("AZURE_AI_RESOURCE_DEPLOYMENT_NAME");
            var apiKey = Environment.GetEnvironmentVariable("AZURE_AI_RESOURCE_KEY");

            AzureOpenAIClient azureClient = new(endpoint, new AzureKeyCredential(apiKey));
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