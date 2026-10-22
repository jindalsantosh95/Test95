using System.Collections.Generic;

namespace VAA.EAPI.Common.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue value)
        {
            if (source is null) return;
            if (key is null) return;

            if (source.TryGetValue(key, out _))
                source[key] = value;
            else
                source.Add(key, value);
        }
    }
}
