using Azure.Storage.Blobs;

namespace EventEase.Services
{
    public class BlobStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public BlobStorageService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string containerName = "eventease-images")
        {
            if (file == null || file.Length == 0)
            {
                return string.Empty;
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var connectionString = _configuration.GetConnectionString("AzureBlobStorage");

            // If Azure Blob Storage is not configured locally, save into wwwroot/uploads so the app still works.
            if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("YOUR_BLOB_CONNECTION_STRING"))
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var localPath = Path.Combine(uploadsFolder, fileName);
                using var stream = new FileStream(localPath, FileMode.Create);
                await file.CopyToAsync(stream);

                return $"/uploads/{fileName}";
            }

            var containerClient = new BlobContainerClient(connectionString, containerName);
            await containerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(fileName);
            using var uploadStream = file.OpenReadStream();
            await blobClient.UploadAsync(uploadStream, overwrite: true);

            return blobClient.Uri.ToString();
        }
    }
}
