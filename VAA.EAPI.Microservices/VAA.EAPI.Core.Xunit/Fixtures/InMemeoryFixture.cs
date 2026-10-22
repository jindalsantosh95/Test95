using System.Collections.Concurrent;

namespace VAA.EAPI.Core.Xunit.Fixtures
{
    public sealed class InMemoryFixture : IDisposable
    {
        private readonly ConcurrentDictionary<string, object> _cache = [];

        public bool TryAdd(string key, object value) => _cache.TryAdd(key, value);
        public T? TryGet<T>(string key) => _cache.TryGetValue(key, out var value) ? (T)value : default;
        public bool TryRemove(string key) => _cache.TryRemove(key, out _);

        public void Dispose()
        {
            _cache.Clear();
        }
    }
}