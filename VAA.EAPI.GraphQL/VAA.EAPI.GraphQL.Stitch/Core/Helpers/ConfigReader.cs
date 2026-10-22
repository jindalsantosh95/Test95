using System.Collections.Concurrent;
using VAA.EAPI.GraphQL.Stitch.Core.Extensions;

namespace VAA.EAPI.GraphQL.Stitch.Core.Helpers
{
    public static class ConfigReader
    {
        private static readonly ConcurrentDictionary<string, object> cache = new();

        public static string Get(string key) => Get<string>(key);
        public static T Get<T>(string key)
        {
            if (key.IsNullOrWhiteSpace()) throw new ArgumentNullException(key);

            key = key.ToLower();

            if (cache.ContainsKey(key)) return (T)cache[key];

            string configValue = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process) ?? Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User) ?? Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Machine);

            if (configValue.IsNullOrEmpty())
                throw new NullReferenceException(key);

            if (!configValue.TryParse(out T result))
                throw new NotSupportedException($"{typeof(T)} is not supported.");

            while (!cache.ContainsKey(key) && !cache.TryAdd(key, result))
                if (!Thread.Yield())
                    Thread.Sleep(0);

            return result;
        }

        public static void Set(string key, string value)
        {
            Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Process);
            cache.TryRemove(key.ToLower(), out _);
        }

        public static string Try(string key, string @default = default) => Try<string>(key, @default);

        public static T Try<T>(string key, T @default = default)
        {
            if (key.IsNullOrWhiteSpace()) return @default;

            key = key.ToLower();

            if (cache.ContainsKey(key)) return (T)cache[key];

            string configValue = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process) ?? Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User) ?? Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Machine);

            if (configValue.IsNullOrWhiteSpace())
                return @default;

            _ = configValue.TryParse(out T result, @default);

            while (!cache.ContainsKey(key) && !cache.TryAdd(key, result ?? @default))
                if (!Thread.Yield())
                    Thread.Sleep(0);

            return result ?? @default;
        }
    }
}
