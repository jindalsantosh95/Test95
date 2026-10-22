using System.IO;
using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.Services.Aws
{
    public interface IAwsS3BucketService
    {
        public string[] Tags { get; }

        public Task<string[]> ListAsync(string prefix, string path = "");

        public Task<bool> UploadAsync(string key, byte[] content, string path = "");
        public Task<bool> UploadAsync(string key, Stream stream, string path = "");

        public Task<byte[]> DownloadAsync(string key, string path = "");

        public Task EmptyAsync(string prefix, string path = "");
    }
}
