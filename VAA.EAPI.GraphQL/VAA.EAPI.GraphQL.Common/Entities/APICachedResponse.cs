using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.GraphQL.Common.Entities
{
    public class APICachedResponse<T>
    {
        public static explicit operator APICachedResponse<T>(APIResponse<T> source) => new APICachedResponse<T>()
        {
            CacheKey = source.CacheKey,
            Headers = source.Headers.AsParallel().ToDictionary(k => k.Key, v => v.Value),
            RawResponse = source.RawResponse,
            ResponseType = typeof(T).FullName
        };

        public string CacheKey { get; set; }
        public Dictionary<string, IEnumerable<string>> Headers { get; set; }
        public string RawResponse { get; set; }
        public string ResponseType { get; set; }
    }
}
