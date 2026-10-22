using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace VAA.EAPI.Common.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static string ReadRequestAsString(this HttpResponseMessage source) => source.ReadRequestAsStringAsync().GetAwaiter().GetResult();
        public static T ReadRequestFromJson<T>(this HttpResponseMessage source) => source.ReadRequestFromJsonAsync<T>().GetAwaiter().GetResult();
        public static string ReadResponseAsString(this HttpResponseMessage source) => source.ReadResponseAsStringAsync().GetAwaiter().GetResult();
        public static T ReadResponseFromJson<T>(this HttpResponseMessage source) => source.ReadResponseFromJsonAsync<T>().GetAwaiter().GetResult();

        public async static Task<string> ReadRequestAsStringAsync(this HttpResponseMessage source) => await source.RequestMessage.Content.ReadAsStringAsync();
        public async static Task<T> ReadRequestFromJsonAsync<T>(this HttpResponseMessage source) => JsonConvert.DeserializeObject<T>(await source.ReadRequestAsStringAsync());
        public async static Task<string> ReadResponseAsStringAsync(this HttpResponseMessage source) => await source.Content.ReadAsStringAsync();
        public async static Task<T> ReadResponseFromJsonAsync<T>(this HttpResponseMessage source) => JsonConvert.DeserializeObject<T>(await source.ReadResponseAsStringAsync());

        public static HttpResponseMessage Clone(this HttpResponseMessage source, HttpRequestMessage request)
        {
            var clone = source.Clone();

            clone.RequestMessage = request;

            return clone;
        }
        public static HttpResponseMessage Clone(this HttpResponseMessage source)
        {
            var httpResponseMessage = new HttpResponseMessage(source.StatusCode);

            if (source.Content != null)
            {
                source.Content.LoadIntoBufferAsync().GetAwaiter().GetResult();

                var memoryStream = new MemoryStream();

                source.Content.ReadAsStream().CopyTo(memoryStream);

                memoryStream.Position = 0;

                httpResponseMessage.Content = new StreamContent(memoryStream);

                if (source.Content.Headers != null)
                    foreach (var header in source.Content.Headers)
                        httpResponseMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            httpResponseMessage.Version = source.Version;
            httpResponseMessage.ReasonPhrase = source.ReasonPhrase;
            foreach (var header in source.Headers)
                httpResponseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return httpResponseMessage;
        }
    }
}
