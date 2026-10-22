using System.Collections.Concurrent;
using System.Data;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.Core.Xunit.Fixtures
{
    public sealed class DataReaderFixture : IDisposable
    {
        private static readonly ConcurrentDictionary<string, DataTable> cache = new();

        public void Initialize<T>(string path)
        {
            if (cache.ContainsKey(typeof(T).FullName))
                return;
            if (!File.Exists(path))
                throw new FileNotFoundException();

            DataTable dataTable = DataGenerator.LoadDataSource(path);

            if (dataTable.Rows.Count == 0)
                throw new Exception($"{nameof(dataTable.Rows)} was empty.");

            while (!cache.ContainsKey(typeof(T).FullName) && !cache.TryAdd(typeof(T).FullName, dataTable))
                if (Thread.Yield())
                    Thread.Sleep(0);
        }

        public string GetValue<T>(string key)
        {
            if (!cache.ContainsKey(typeof(T).FullName))
                throw new KeyNotFoundException();
            if (!cache[typeof(T).FullName].Columns.Contains(key))
                throw new KeyNotFoundException();

            return cache[typeof(T).FullName].Rows[0][key].ToString();
        }
        public string GetValue<T>(string key, string target, string value)
        {
            if (!cache.ContainsKey(typeof(T).FullName))
                throw new KeyNotFoundException();
            if (!cache[typeof(T).FullName].Columns.Contains(key))
                throw new KeyNotFoundException(nameof(key));
            if (!cache[typeof(T).FullName].Columns.Contains(target))
                throw new KeyNotFoundException(nameof(target));

            DataRow[] records = cache[typeof(T).FullName].Select($"[{target}]='{value}'");

            return records.Any() ? records[0][key]?.ToString() : string.Empty;
        }

        public void Dispose()
        {
        }
    }
}
