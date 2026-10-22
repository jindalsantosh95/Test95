using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.EAPI.Infrastructure.Services
{
    public class HttpConnector : IHttpConnector
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpConnector(ILogger<HttpConnector> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        #region Verb-specific Helpers
        public async Task<T> GetAsync<T>(string uri, Dictionary<string, string> headers = null, string bearerToken = null) => await SendAsync<T>(uri, headers, bearerToken, HttpMethod.Get, (HttpContent)null);
        public async Task<HttpResponseMessage> GetAsync(string uri, Dictionary<string, string> headers, string bearerToken = null) => await SendAsync(uri, headers, bearerToken, HttpMethod.Get, (HttpContent)null);
        public async Task<HttpResponseMessage> PostAsync(string uri, HttpContent content, Dictionary<string, string> headers = null, string bearerToken = null) => await SendAsync(uri, headers, bearerToken, HttpMethod.Post, content);
        public async Task<T> PostAsync<T>(string uri, HttpContent content, Dictionary<string, string> headers = null, string bearerToken = null) => await SendAsync<T>(uri, headers, bearerToken, HttpMethod.Post, content);
        public async Task<T> DeleteAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken = null) => await SendAsync<T>(uri, headers, bearerToken, HttpMethod.Delete, (HttpContent)null);
        public async Task<T> PatchAsync<T>(string uri, HttpContent content, Dictionary<string, string> headers, string bearerToken = null) => await SendAsync<T>(uri, headers, bearerToken, HttpMethod.Patch, content);
        public async Task<T> PutAsync<T>(string uri, HttpContent content, Dictionary<string, string> headers, string bearerToken = null) => await SendAsync<T>(uri, headers, bearerToken, HttpMethod.Put, content);

        #endregion

        public async Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod method, string content, bool preventFollow202Location = false)
            => await SendAsync(uri, headers, bearerToken, method, content == null ? null : new StringContent(content, Encoding.UTF8, "application/json"), preventFollow202Location);
        public async Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod method, string content, CookieContainer cookies, bool preventFollow202Location = false)
            => await SendAsync(uri, headers, bearerToken, method, content == null ? null : new StringContent(content, Encoding.UTF8, "application/json"), cookies, preventFollow202Location);
        public async Task<T> SendAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod method, string content, bool preventFollow202Location = false)
            => await SendAsync<T>(uri, headers, bearerToken, method, content == null ? null : new StringContent(content, Encoding.UTF8, "application/json"), preventFollow202Location);
        public async Task<T> SendAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod method, string content, CookieContainer cookies, bool preventFollow202Location = false)
            => await SendAsync<T>(uri, headers, bearerToken, method, content == null ? null : new StringContent(content, Encoding.UTF8, "application/json"), cookies, preventFollow202Location);
        private static HttpRequestMessage BuildHttpRequestMessage(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod method, HttpContent content, bool preventFollow202Location = false, CookieContainer cookies = null)
        {
            // lets create a copy of the dictionary as if a shared/static
            // is passed this will cause an error with existing keys
            headers = headers?.Keys.ToDictionary(k => k, v => headers[v]) ?? new();

            var key = string.Empty;

            if (!bearerToken.IsNullOrWhiteSpace())
            {
                key = headers.Keys.FirstOrDefault(x => x.Is("Authorization"));

                if (key.IsNullOrEmpty())
                    headers.Add("Authorization", $"Bearer {bearerToken}");
                else
                    headers[key] = $"Bearer {bearerToken}";
            }

            key = headers.Keys.FirstOrDefault(x => x.Is("cache-control"));

            if (key.IsNullOrEmpty())
                headers.Add("cache-control", "no-cache");
            else
                headers[key] = "no-cache";

            if (!headers.Keys.Any(k => k.Is("Accept")))
                headers.Add("Accept", "*/*");

            HttpRequestMessage httpRequestMessage = new(method, uri)
            {
                Content = content
            };

            httpRequestMessage.Headers.TryAddWithoutValidation("x-nowait", preventFollow202Location.ToString());

            foreach (var header in headers)
                httpRequestMessage.Headers.Add(header.Key, header.Value);

            httpRequestMessage.SetCookieContainer(cookies);

            return httpRequestMessage;
        }

        public async Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod method, HttpContent content, bool preventFollow202Location = false)
        {
            var httpRequestMessage = BuildHttpRequestMessage(uri, headers, bearerToken, method, content, preventFollow202Location);

            HttpResponseMessage httpResponseMessage = await SendAsync(httpRequestMessage, CancellationToken.None);

            return httpResponseMessage;
        }

        public async Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod method, HttpContent content, CancellationToken cancellationToken, bool preventFollow202Location = false)
        {
            using var httpRequestMessage = BuildHttpRequestMessage(uri, headers, bearerToken, method, content, preventFollow202Location);

            return await SendAsync(httpRequestMessage, cancellationToken);
        }

        public async Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod method, HttpContent content, CookieContainer cookies, bool preventFollow202Location = false)
        {
            var httpRequestMessage = BuildHttpRequestMessage(uri, headers, bearerToken, method, content, preventFollow202Location, cookies);

            HttpResponseMessage httpResponseMessage = await SendAsync(httpRequestMessage, CancellationToken.None);

            return httpResponseMessage;
        }

        public async Task<T> SendAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod method, HttpContent content, bool preventFollow202Location = false)
        {
            var httpRequestMessage = BuildHttpRequestMessage(uri, headers, bearerToken, method, content, preventFollow202Location);

            return await SendAsync<T>(httpRequestMessage, CancellationToken.None);
        }

        public async Task<T> SendAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod method, HttpContent content, CookieContainer cookies, bool preventFollow202Location = false)
        {
            var httpRequestMessage = BuildHttpRequestMessage(uri, headers, bearerToken, method, content, preventFollow202Location, cookies);

            return await SendAsync<T>(httpRequestMessage, CancellationToken.None);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var trace = _logger.CreateTrace();

            trace.SetHashCode(request.GetHashCode());

            var httpClient = _httpClientFactory.CreateClient(request.RequestUri.ToString());

            HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            var loggedRequestTarget = request.IsRequestUriLoggingSuppressed()
                ? request.RequestUri.GetLeftPart(UriPartial.Authority)
                : request.RequestUri.ToString();
            trace.Diagnostics.LogInformation($"{{0}}{request.Method}:{loggedRequestTarget}:{httpResponseMessage.Content.Headers.ContentLength}:SendAsync executed in {{7}}ms");

            return httpResponseMessage;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CookieContainer cookies, CancellationToken cancellationToken)
        {
            request.SetCookieContainer(cookies);

            return await SendAsync(request, cancellationToken);
        }

        public async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpResponseMessage = await SendAsync(request, cancellationToken);
            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            T value = default;

            if (httpResponseMessage.IsSuccessStatusCode || httpResponseMessage.StatusCode == System.Net.HttpStatusCode.RedirectMethod)
            {
                if (typeof(T) == typeof(string))
                    value = (T)(object)response;
                else if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.NoContent || httpResponseMessage.StatusCode == System.Net.HttpStatusCode.RedirectMethod)
                    value = default;
                else if (httpResponseMessage.Content?.Headers.ContentType?.MediaType.Is("text/xml") ?? false)
                    value = Xml.Deserialize<T>(response);
                else
                    value = Json.Deserialize<T>(response);
            }
            else
            {
                var exception = new HttpRequestException("Http connector failed.", new HttpResponseException(httpResponseMessage), httpResponseMessage.StatusCode);

                exception.Data.Add("RequestUri", httpResponseMessage.RequestMessage.RequestUri);
                exception.Data.Add("StatusCode", httpResponseMessage.StatusCode.ToString());
                exception.Data.Add("ReasonPhrase", httpResponseMessage.ReasonPhrase);
                if (request.Content != null) exception.Data.Add("Request", await request.Content.ReadAsStringAsync(cancellationToken));
                exception.Data.Add("Response", response);

                var trace = _logger.CreateTrace();

                var debugTrace = $@"{{0}}> RequestUri: {exception.Data["RequestUri"]}
{{0}}> StatusCode: {exception.Data["StatusCode"]}
{{0}}> ReasonPhrase: {exception.Data["ReasonPhrase"]}
{{0}}> Request: {exception.Data["Request"]}
{{0}}> Response: {exception.Data["Response"]}";

                trace.Debug.WriteLine(debugTrace.MakeInterpolationSafe());

                trace.SetHashCode(httpResponseMessage.RequestMessage.GetHashCode());

                trace.Log.LogError(exception, $"{{0}}> {exception.Message}, {{2}}ms");

                throw exception;
            }

            return value;
        }

        public async Task<T> SendAsync<T>(HttpRequestMessage request, CookieContainer cookies, CancellationToken cancellationToken)
        {
            request.SetCookieContainer(cookies);

            return await SendAsync<T>(request, cancellationToken);
        }
    }
}
