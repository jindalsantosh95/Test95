using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Serialisation;

namespace VAA.EAPI.GraphQL.Common.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task ValidateResponseAsync<T>(this HttpResponseMessage response, Common.Diagnostics.Trace<T> trace)
        {
            if (!(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.RedirectMethod))
            {
                trace.Diagnostics.LogInformation($"{response.StatusCode}:{response.RequestMessage.RequestUri}:{response.Content.Headers.ContentLength}:ReadAsStringAsync executed in {{2}}ms");

                string responseContent = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty;
                string requestContent = response.RequestMessage.Content != null ? await response.RequestMessage.Content.ReadAsStringAsync() : string.Empty;

                var exception = new HttpRequestException("Http connector failed.", new HttpResponseException(response), response.StatusCode);

                exception.Data.Add("RequestUri", response.RequestMessage.RequestUri);
                exception.Data.Add("StatusCode", response.StatusCode.ToString());
                exception.Data.Add("ReasonPhrase", response.ReasonPhrase);
                exception.Data.Add("Response", responseContent);
                exception.Data.Add("Request", requestContent);

                trace.Log.LogError(exception, $"{{0}}> {exception.Message}, {{2}}ms");

                throw exception;
            }
        }

        public static async Task<APIResponse<R>> ConvertToAPIResponseAsync<R, T>(this HttpResponseMessage response, Common.Diagnostics.Trace<T> trace)
        {
            var content = await response.Content.ReadAsStringAsync();

            trace.Diagnostics.LogInformation($"{response.StatusCode}:{response.RequestMessage.RequestUri}:{response.Content.Headers.ContentLength}:ReadAsStringAsync executed in {{2}}ms");

            return new APIResponse<R>
            {
                Headers = response.Headers,
                RawResponse = content,
                Response = Json.Deserialize<R>(content),
                ResponseMessage = response
            };
        }

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
