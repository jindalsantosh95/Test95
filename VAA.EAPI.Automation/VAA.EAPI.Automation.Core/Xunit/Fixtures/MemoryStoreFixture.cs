using System.Collections.Concurrent;
namespace VAA.EAPI.Automation.Core.Xunit.Fixtures
{
    public sealed class MemoryStoreFixture : IDisposable
    {
        private static readonly object @lock = new();
        private static readonly ConcurrentDictionary<string, object> cache = new();

        public T Get<T>(string key) => !cache.ContainsKey(key) ? default : (T)cache[key];

        public void SetOnce<T>(string key, T value) => SetOnce(key, () => value);
        public void SetOnce<T>(string key, Func<T> selector)
        {
            if (cache.ContainsKey(key))
                return;

            lock (@lock)
            {
                if (cache.ContainsKey(key))
                    return;

                Set(key, selector());
            }
        }
        public void Set<T>(string key, T value)
        {
            if (cache.ContainsKey(key))
                cache[key] = value;
            else
                while (!cache.ContainsKey(key) && !cache.TryAdd(key, value))
                    if (!Thread.Yield())
                        Thread.Sleep(0);
        }

        public void Dispose()
        {
        }
    }
}
