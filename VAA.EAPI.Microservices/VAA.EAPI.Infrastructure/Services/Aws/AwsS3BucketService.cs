using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Infrastructure.Services.Aws
{
    public sealed class AwsS3BucketService : IAwsS3BucketService
    {
        private const string BucektNameConfig = "AWS:S3:Bucket:Name";
        private const string AccessKeyConfig = "AWS:S3:Bucket:AccessKey";
        private const string SecretKeyConfig = "AWS:S3:Bucket:SecretKey";
        private const string RegionEndpointConfig = "AWS:S3:Bucket:RegionEndpoint";
        private const string TagsConfig = "AWS:S3:Bucket:Tags";

        private readonly ILogger<AwsS3BucketService> _logger;
        private readonly string _bucketName;
        private readonly AmazonS3Client _amazonS3Client;

        public string[] Tags { get; }

        public AwsS3BucketService(ILogger<AwsS3BucketService> logger) : this(ConfigReader.Get(BucektNameConfig), ConfigReader.Get(AccessKeyConfig), ConfigReader.Get(SecretKeyConfig), ConfigReader.Get(RegionEndpointConfig), logger, ConfigReader.Try(TagsConfig, Array.Empty<string>())) { }
        public AwsS3BucketService(string name, string accessKey, string secretKey, string regionEndpoint, ILogger<AwsS3BucketService> logger, string[] tags = null)
        {
            name = name?.Trim().Trim('/') ?? string.Empty;

            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));
            if (accessKey.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(accessKey));
            if (secretKey.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(secretKey));
            if (regionEndpoint.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(regionEndpoint));

            Tags = tags;

            _bucketName = name.Trim('/');
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            RegionEndpoint bucketRegion = typeof(RegionEndpoint).GetField(regionEndpoint, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)?.GetValue(typeof(RegionEndpoint)) as RegionEndpoint ?? RegionEndpoint.EUWest1;

            _amazonS3Client = new AmazonS3Client(accessKey, secretKey, bucketRegion);
        }

        public async Task<string[]> ListAsync(string prefix, string path = "")
        {
            try
            {
                path = path?.Trim().Trim('/') ?? string.Empty;

                ListObjectsV2Request listObjectsV2Request = new()
                {
                    BucketName = _bucketName,
                    Prefix = path.IsNullOrWhiteSpace() ? prefix : string.Join('/', path, prefix)
                };

                ListObjectsV2Response response = await _amazonS3Client.ListObjectsV2Async(listObjectsV2Request);

                if (response.KeyCount == 0) return [];

                return [.. response.S3Objects.Select(x => $"{x.Key}, {x.Size}")];
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, $"Error encountered on server. Message:'{ex.Message}' when listing objects");

                return [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unknown error encountered. Message:'{ex.Message}'");

                return [];
            }
        }

        public async Task<byte[]> DownloadAsync(string key, string path = "")
        {
            try
            {
                path = path?.Trim().Trim('/') ?? string.Empty;

                GetObjectRequest getObjectRequest = new()
                {
                    BucketName = _bucketName,
                    Key = path.IsNullOrWhiteSpace() ? key : string.Join('/', path, key)
                };

                using GetObjectResponse response = await _amazonS3Client.GetObjectAsync(getObjectRequest);

                byte[] buffer = new byte[response.ResponseStream.Length];

                response.ResponseStream.Read(buffer, 0, buffer.Length);

                return buffer;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, $"Error encountered on server. Message:'{ex.Message}' when reading object");

                return [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unknown error encountered. Message:'{ex.Message}'");

                return [];
            }
        }

        public async Task<bool> UploadAsync(string key, byte[] content, string path = "") => await UploadAsync(key, new MemoryStream(content) { Position = 0 }, path);
        public async Task<bool> UploadAsync(string key, Stream stream, string path = "")
        {
            try
            {
                path = path?.Trim().Trim('/') ?? string.Empty;

                PutObjectRequest putObjectRequest = new()
                {
                    BucketName = _bucketName,
                    Key = path.IsNullOrWhiteSpace() ? key : string.Join('/', path, key),
                    InputStream = stream
                };

                PutObjectResponse response = await _amazonS3Client.PutObjectAsync(putObjectRequest, CancellationToken.None);

                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, $"Error encountered on server. Message:'{ex.Message}' when uploading object");

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unknown error encountered. Message:'{ex.Message}'");

                return false;
            }
        }

        public async Task EmptyAsync(string prefix, string path = "")
        {
            try
            {
                path = path?.Trim().Trim('/') ?? string.Empty;

                string[] objects = await ListAsync(prefix, path);

                while (objects.Length > 0)
                {
                    DeleteObjectsRequest deleteObjectsRequest = new()
                    {
                        BucketName = _bucketName
                    };

                    foreach (string key in objects.Select(x => x.Split(',', StringSplitOptions.RemoveEmptyEntries)[0]))
                        deleteObjectsRequest.AddKey(key);

                    DeleteObjectsResponse deleteObjectsResponse = await _amazonS3Client.DeleteObjectsAsync(deleteObjectsRequest).ConfigureAwait(false);

                    objects = deleteObjectsResponse.HttpStatusCode == System.Net.HttpStatusCode.OK ? await ListAsync(prefix, path) : [];
                }
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, $"Error encountered on server. Message:'{ex.Message}' when emptying bucket");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unknown error encountered. Message:'{ex.Message}'");
            }
        }
    }
}
