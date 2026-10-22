using Microsoft.Azure.Functions.Worker.Http;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class HttpRequestDataExtensions
    {
        public static HttpRequestMessage ToHttpRequestMessage(this HttpRequestData requestData, string? uri = default) => uri?.IsNullOrWhiteSpace() == false ? requestData.ToHttpRequestMessage(uri.Parse<Uri>()) : requestData.ToHttpRequestMessage(default(Uri));
        public static HttpRequestMessage ToHttpRequestMessage(this HttpRequestData requestData, Uri? uri = default)
        {
            if (requestData == null)
                throw new ArgumentNullException(nameof(requestData));

            System.Net.Http.HttpMethod method = new HttpMethod(requestData.Method);
            System.Net.Http.HttpRequestMessage requestMessage = new HttpRequestMessage(method, uri ?? requestData.Url);

            if (requestData.Body != null)
                requestMessage.Content = new StreamContent(requestData.Body);

            foreach (KeyValuePair<string, IEnumerable<string>> header in requestData.Headers)
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value))
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return requestMessage;
        }
    }
}
