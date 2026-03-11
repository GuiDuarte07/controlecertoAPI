using Amazon.S3;
using Amazon.S3.Model;
using ControleCerto.Services.Interfaces;

namespace ControleCerto.Services
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _bucketUrl;

        public S3Service(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration["AWS:BucketName"]!;
            _bucketUrl = configuration["AWS:BucketUrl"]!;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string key)
        {
            using var stream = file.OpenReadStream();

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream,
                ContentType = file.ContentType,
            };

            await _s3Client.PutObjectAsync(request);

            return $"{_bucketUrl.TrimEnd('/')}/{key}";
        }

        public async Task DeleteFileAsync(string key)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
            };

            await _s3Client.DeleteObjectAsync(request);
        }
    }
}
