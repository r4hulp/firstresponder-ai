using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Threading.Tasks;

namespace recorder_fn.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(string connectionString, string containerName)
        {
            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        }

        public async Task<string> DownloadBlobFileAsync(string relativeBlobUrl)
        {
            var blobClient = _containerClient.GetBlobClient(relativeBlobUrl);
            string localFilePath = Path.Combine(Path.GetTempPath(), relativeBlobUrl);
            Directory.CreateDirectory(Path.GetDirectoryName(localFilePath));

            var download = await blobClient.DownloadAsync();
            using (var fileStream = File.OpenWrite(localFilePath))
            {
                await download.Value.Content.CopyToAsync(fileStream);
            }

            return localFilePath;
        }

        public async Task UploadBlobAsync(string blobPath, Stream content)
        {
            var blobClient = _containerClient.GetBlobClient(blobPath);
            await blobClient.UploadAsync(content, true);
        }

        public async Task<bool> BlobExistsAsync(string blobPath)
        {
            var blobClient = _containerClient.GetBlobClient(blobPath);
            return await blobClient.ExistsAsync();
        }
    }
} 