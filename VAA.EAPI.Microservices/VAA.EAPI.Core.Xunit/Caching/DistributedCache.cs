using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Concurrent;

namespace VAA.EAPI.Core.Xunit.Caching
{
    public sealed class DistributedCache : IDistributedCache, IDisposable
    {
        public static readonly DistributedCache Instance = new();

        private readonly ConcurrentDictionary<string, CacheEntry> cache = new();
        private readonly System.Timers.Timer cacheTimer;
        private bool isWorking = false;

        public DistributedCache()
        {
            cacheTimer = new(1000);
            cacheTimer.Elapsed += (s, e) =>
            {
                if (isWorking) return;

                isWorking = true;

                try
                {
                    var keys = cache.Keys.ToArray();

                    for (int i = keys.Length - 1; i >= 0; i--)
                    {
                        if (!cache[keys[i]].HasExpired) continue;

                        cache.TryRemove(keys[i], out _);
                    }
                }
                catch { }
                finally
                {
                    isWorking = false;
                }

            };
            cacheTimer.Start();
        }

        public byte[]? Get(string key)
        {
            if (!cache.ContainsKey(key)) return default;

            if (cache[key].Options.SlidingExpiration is not null)
                Refresh(key);

            return cache[key].Value;
        }

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => Task.FromResult(Get(key));

        public void Refresh(string key)
        {
            if (!cache.ContainsKey(key)) return;

            cache[key].TimeStamp = DateTime.Now;
        }

        public Task RefreshAsync(string key, CancellationToken token = default) => Task.Run(() => Refresh(key));

        public void Remove(string key) => cache.TryRemove(key, out _);

        public Task RemoveAsync(string key, CancellationToken token = default) => Task.Run(() => Remove(key));

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            var entry = (CacheEntry)(key, value, options);

            cache.AddOrUpdate(entry.Key, entry, (k, v) =>
            {
                if (entry.Options.SlidingExpiration is not null)
                    entry.TimeStamp = DateTime.Now;

                return entry;
            });
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) => Task.Run(() => Set(key, value, options), token);

        public void Dispose()
        {
            cacheTimer.Stop();
            cache.Clear();
        }

        private sealed class CacheEntry
        {
            public static explicit operator CacheEntry((string, byte[], DistributedCacheEntryOptions) source) => new()
            {
                Key = source.Item1,
                Value = source.Item2,
                Options = source.Item3
            };

            public CacheEntry() => this.TimeStamp = DateTime.Now;

            public string Key { get; set; }
            public byte[] Value { get; set; }
            public DistributedCacheEntryOptions Options { get; set; }
            public DateTime TimeStamp { get; set; }
            public bool HasExpired
            {
                get
                {
                    if (Options.AbsoluteExpiration is not null)
                        return DateTime.Now >= Options.AbsoluteExpiration.Value;

                    if (Options.AbsoluteExpirationRelativeToNow is not null)
                        return DateTime.Now >= TimeStamp.Add(Options.AbsoluteExpirationRelativeToNow.Value);

                    if (Options.SlidingExpiration is not null)
                        return DateTime.Now >= TimeStamp.Add(Options.SlidingExpiration.Value);

                    return false;
                }
            }
        }
    }
}
