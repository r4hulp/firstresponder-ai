using recorder_fn.Models;
using System.Threading.Tasks;

namespace recorder_fn.Services
{
    public interface ICallRecordingService
    {
        Task SaveCallRecordingAsync(CallRecordingEntity entity);
        Task<CallRecordingEntity> GetCallRecordingAsync(string rowId, string partitionKey);
        Task UpdateCallRecordingAsync(CallRecordingEntity entity);
    }
} 