using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using event_management_system.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace event_management_system.Services
{
    public class blob_storage_service : Iblob_storage_service
    {
        private readonly string _connection_string;
        private readonly string _container_name;

        public blob_storage_service(IConfiguration configuration)
        {
            _connection_string = configuration["AzureStorage:ConnectionString"]
                ?? throw new InvalidOperationException("AzureStorage:ConnectionString is missing from appsettings.json");
            _container_name = configuration["AzureStorage:ContainerName"]
                ?? throw new InvalidOperationException("AzureStorage:ContainerName is missing from appsettings.json");
        }

        public async Task<string> upload_image_async(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null.");

            string[] allowed_extensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowed_extensions.Contains(extension))
                throw new ArgumentException($"File type not allowed. Allowed: {string.Join(", ", allowed_extensions)}");

            if (file.Length > 5 * 1024 * 1024)
                throw new ArgumentException("File is too large. Maximum 5 MB allowed.");

            string blob_name = $"{folder}/{Guid.NewGuid():N}{extension}";

            var container_client = new BlobContainerClient(_connection_string, _container_name);
            await container_client.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blob_client = container_client.GetBlobClient(blob_name);

            using (var stream = file.OpenReadStream())
            {
                await blob_client.UploadAsync(stream, new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                });
            }

            return blob_client.Uri.ToString();
        }

        public async Task delete_image_async(string image_url)
        {
            if (string.IsNullOrWhiteSpace(image_url))
                return;

            try
            {
                var uri = new Uri(image_url);
                string blob_name = uri.AbsolutePath.TrimStart('/');

                if (blob_name.StartsWith(_container_name + "/"))
                    blob_name = blob_name.Substring(_container_name.Length + 1);

                var container_client = new BlobContainerClient(_connection_string, _container_name);
                var blob_client = container_client.GetBlobClient(blob_name);

                await blob_client.DeleteIfExistsAsync();
            }
            catch
            {
                // silently ignore
            }
        }
    }
}