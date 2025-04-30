using System.IO;
using System.Threading.Tasks;

namespace recorder_fn.Services
{
    public interface IBlobStorageService
    {
        Task<string> DownloadBlobFileAsync(string relativeBlobUrl);
        Task UploadBlobAsync(string blobPath, Stream content);
        Task<bool> BlobExistsAsync(string blobPath);
    }
} 