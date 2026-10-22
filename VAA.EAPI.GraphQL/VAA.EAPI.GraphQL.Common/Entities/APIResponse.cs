using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using VAA.EAPI.GraphQL.Common.Serialisation;

namespace VAA.EAPI.GraphQL.Common.Entities
{
    public class APIResponse<T>
    {
        private static readonly ConstructorInfo httpResponseHeadersConstructorInfo = typeof(HttpResponseHeaders).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];

        public static explicit operator APIResponse<T>(APICachedResponse<T> source)
        {
            /// Because we passing a non serialisable object, HttpResponseHeaders, we can't deserilise back to its orignal cast
            /// so with a bit of reflection we can access the private constructor to initialise correctly

            HttpResponseHeaders httpResponseHeaders = (HttpResponseHeaders)httpResponseHeadersConstructorInfo.Invoke(new object[] { false });

            foreach (var header in source.Headers)
                httpResponseHeaders.Add(header.Key, header.Value);

            return new APIResponse<T>()
            {
                CacheKey = source.CacheKey,
                Headers = httpResponseHeaders,
                RawResponse = source.RawResponse,
                Response = typeof(T) == typeof(string) ? (T)(object)source.RawResponse : !string.IsNullOrWhiteSpace(source.RawResponse) ? Json.Deserialize<T>(source.RawResponse) : default
            };
        }

        public string CacheKey { get; set; }
        public T Response { get; set; }
        public HttpResponseHeaders Headers { get; set; }
        public string RawResponse { get; set; }
        public HttpResponseMessage ResponseMessage { get; set; }
    }
}
