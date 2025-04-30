using System.Threading.Tasks;

namespace recorder_fn.Services
{
    public interface ISpeechService
    {
        Task<string> TranscribeAudioAsync(string audioFilePath);
        Task<string> GetCallSummaryAsync(string transcript);
    }
} 