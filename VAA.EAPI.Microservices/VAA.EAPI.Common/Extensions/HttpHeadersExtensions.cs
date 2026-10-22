using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;

namespace VAA.EAPI.Common.Extensions
{
    public static class HttpHeadersExtensions
    {
        private static readonly FieldInfo valueField = typeof(KeyValuePair<string, IEnumerable<string>>).GetField("value", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void AddOrUpdate(this HttpHeaders source, string key, string value) => source.AddOrUpdate(key, new string[] { value });
        public static void AddOrUpdate(this HttpHeaders source, string key, IEnumerable<string> values)
        {
            if (source is null) return;
            if (key.IsNullOrWhiteSpace()) return;

            values ??= Array.Empty<string>();

            KeyValuePair<string, IEnumerable<string>>? header = source.FirstOrDefault(x => x.Key.Is(key));

            if (header.GetValueOrDefault().Key is null && header.GetValueOrDefault().Value is null)
                source.Add(key, values);
            else
                valueField.SetValue(header, values);
        }
    }
}
