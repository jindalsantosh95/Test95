using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Common.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, TValue value)
        {
            if (source is null) return;
            if (key is null) return;

            value ??= default;

            if (source.TryGetValue(key, out _))
                source[key] = value;
            else
                source.Add(key, value);
        }
    }
}
