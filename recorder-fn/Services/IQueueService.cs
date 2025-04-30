using System.Threading.Tasks;

namespace recorder_fn.Services
{
    public interface IQueueService
    {
        Task SendMessageAsync(string message);
    }
} 