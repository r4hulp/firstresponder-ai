using Azure.AI.OpenAI;
using Azure;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using OpenAI.Chat;

namespace recorder_fn.Services
{
    public class SpeechService : ISpeechService
    {
        private readonly string _speechApiUrl;
        private readonly string _openAiKey;
        private readonly string _azureAiResourceName;
        private readonly string _azureAiDeploymentName;
        private readonly string _azureAiKey;

        public SpeechService(
            string speechApiUrl,
            string openAiKey,
            string azureAiResourceName,
            string azureAiDeploymentName,
            string azureAiKey)
        {
            _speechApiUrl = speechApiUrl;
            _openAiKey = openAiKey;
            _azureAiResourceName = azureAiResourceName;
            _azureAiDeploymentName = azureAiDeploymentName;
            _azureAiKey = azureAiKey;
        }

        public async Task<string> TranscribeAudioAsync(string audioFilePath)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add(Constants.HEADER_ACCEPT, Constants.HEADER_ACCEPT_VALUE);
                client.DefaultRequestHeaders.Add(Constants.HEADER_OCP_APIM_SUBSCRIPTION_KEY, _openAiKey);

                using (var formData = new MultipartFormDataContent())
                {
                    var audioFileContent = new ByteArrayContent(File.ReadAllBytes(audioFilePath));
                    audioFileContent.Headers.ContentType = new MediaTypeHeaderValue($"audio/{Constants.AUDIO_FILE_FORMAT}");

                    formData.Add(audioFileContent, "audio", Path.GetFileName(audioFilePath));
                    formData.Add(new StringContent(Constants.SPEECH_API_DEFINITION), "definition");

                    var response = await client.PostAsync(_speechApiUrl, formData);
                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<string> GetCallSummaryAsync(string transcript)
        {
            var endpoint = new Uri($"https://{_azureAiResourceName}.openai.azure.com/");
            var azureClient = new AzureOpenAIClient(endpoint, new AzureKeyCredential(_azureAiKey));
            var chatClient = azureClient.GetChatClient(_azureAiDeploymentName);

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

            var messages = new List<ChatMessage>()
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