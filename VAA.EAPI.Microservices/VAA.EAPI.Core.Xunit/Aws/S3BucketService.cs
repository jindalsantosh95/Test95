using System.Collections.Concurrent;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Services.Aws;

namespace VAA.EAPI.Core.Xunit.Aws
{
    public sealed class S3BucketService(params string[] tags) : IAwsS3BucketService
    {
        private readonly ConcurrentDictionary<string, byte[]> _cache = [];

        public string[] Tags { get; } = tags;
        public IReadOnlyDictionary<string, byte[]> Items => _cache;

        public Task<byte[]> DownloadAsync(string key, string path = "")
        {
            path = path?.Trim().Trim('/') ?? string.Empty;

            key = path.IsNullOrWhiteSpace() ? key : string.Join('/', key, path);

            if (_cache.TryGetValue(key, out var result)) return Task.FromResult(result);

            return Task.FromResult(default(byte[]));
        }

        public Task<string[]> ListAsync(string prefix, string path = "")
        {
            path = path?.Trim().Trim('/') ?? string.Empty;

            prefix = path.IsNullOrWhiteSpace() ? prefix : string.Join('/', path, prefix);

            return Task.FromResult(Items.Where(x => x.Key.StartsWith(prefix)).Select(x => $"{x.Key}, {x.Value.Length}").ToArray());
        }

        public Task<bool> UploadAsync(string key, byte[] content, string path = "")
        {
            path = path?.Trim().Trim('/') ?? string.Empty;

            key = path.IsNullOrWhiteSpace() ? key : string.Join('/', key, path);

            _cache.AddOrUpdate(key, content, (k, v) => content);

            return Task.FromResult(_cache.ContainsKey(key));
        }

        public Task<bool> UploadAsync(string key, Stream stream, string path = "")
        {
            if (!stream.CanRead) Task.FromResult(false);

            var content = Array.Empty<byte>();

            stream.Read(content, 0, content.Length);

            path = path?.Trim().Trim('/') ?? string.Empty;

            key = path.IsNullOrWhiteSpace() ? key : string.Join('/', key, path);

            _cache.AddOrUpdate(key, content, (k, v) => content);

            return Task.FromResult(_cache.ContainsKey(key));
        }

        public Task EmptyAsync(string prefix, string path = "")
        {
            path = path?.Trim().Trim('/') ?? string.Empty;

            foreach (var key in ListAsync(prefix, path).GetAwaiter().GetResult())
                _cache.TryRemove(key, out _);

            return Task.CompletedTask;
        }
    }
}
