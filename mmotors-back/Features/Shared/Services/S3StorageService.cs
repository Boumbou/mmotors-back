
using Amazon.S3;
using mmotors_back.Features.Shared.Interfaces;
using Amazon.S3.Model;


namespace mmotors_back.Features.Shared.Services
{
    public class S3FileStorageService : IStorageService
    {
        private readonly IAmazonS3 _s3;
        private readonly IConfiguration _config;
        private readonly string _bucket;

        public S3FileStorageService(IAmazonS3 s3, IConfiguration config)
        {
            _s3 = s3;
            _config = config;
            _bucket = _config["Storage:S3:BucketName"]?? 
                throw new InvalidOperationException(
                    "Storage:S3:BucketName est introuvable."
                );

        }

        public async Task DeleteFileAsync(string key, string subfolder = "")
        {

            var request = new DeleteObjectRequest
            {
                BucketName = _bucket,
                Key = key
            };

            await _s3.DeleteObjectAsync(request);
        }

        public async Task<(string Url, string Key)> UploadFileAsync(IFormFile file, string subfolder = "")
        {
            var key = string.IsNullOrEmpty(subfolder)
                ? $"documents/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}"
                : $"{subfolder}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            await using var stream = file.OpenReadStream();

            var request = new PutObjectRequest
            {
                BucketName = _bucket,
                Key = key,
                InputStream = stream,
                ContentType = file.ContentType
            };

            Console.WriteLine("S3 upload started");

            await _s3.PutObjectAsync(request);

            Console.WriteLine("S3 upload completed");
            
            var url = $"https://{_bucket}.s3.amazonaws.com/{key}";

            return (url, key);
        }

        public async Task<Stream> GetFileAsync(string key, string subfolder = "")
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucket,
                Key = key
            };

            var response = await _s3.GetObjectAsync(request);
            return response.ResponseStream;
        }

        public string GetFileUrl(string key, string subfolder = "")
        {

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucket,
                Key = key,
                Expires = DateTime.UtcNow.AddMinutes(15)
            };

            return _s3.GetPreSignedURL(request);
        }
    }
}