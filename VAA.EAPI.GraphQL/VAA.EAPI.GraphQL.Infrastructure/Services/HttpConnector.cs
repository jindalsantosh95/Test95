using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Infrastructure.Services
{
    public sealed class HttpConnector(ILogger<HttpConnector> logger, IHttpClientFactory httpClientFactory) : IHttpConnector
    {
        private readonly ILogger<HttpConnector> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

        #region Verb-specific Helpers
        public async Task<APIResponse<T>> GetAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken) => await SendAsync<T>(uri, headers, bearerToken, HttpMethod.Get, string.Empty);
        public async Task<APIResponse<T>> DeleteAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken) => await SendAsync<T>(uri, headers, bearerToken, HttpMethod.Delete, string.Empty);
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="uri">Endpoint to send request to</param>
        /// <param name="headers">Any specific headers for downstream service</param>
        /// <param name="bearerToken">Bearer Token for downstream system authorization</param>
        /// <param name="httpMethod">HTTP Verb: GET, POST etc.</param>
        /// <param name="content">Body content</param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<APIResponse<T>> SendAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod httpMethod, string content, bool noWait = false)
        {
            var httpResponseMessage = await SendAsync(uri, headers, bearerToken, httpMethod, content, noWait);
            var response = await httpResponseMessage.Content.ReadAsStringAsync();

            // Grab the response headers
            APIResponse<T> apiResponse = new()
            {
                Headers = httpResponseMessage.Headers,
                RawResponse = response,
                Response = Json.Deserialize<T>(response),
                ResponseMessage = httpResponseMessage
            };

            return apiResponse;
        }

        public async Task<APIResponse<T>> SendAsync<T>(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod httpMethod, string content, CookieContainer cookies, bool noWait = false)
        {
            var httpResponseMessage = await SendAsync(uri, headers, bearerToken, httpMethod, content, cookies, noWait);
            var response = await httpResponseMessage.Content.ReadAsStringAsync();

            APIResponse<T> apiResponse = new()
            {
                Headers = httpResponseMessage.Headers,
                RawResponse = response,
                Response = Json.Deserialize<T>(response),
                ResponseMessage = httpResponseMessage
            };

            return apiResponse;
        }

        public async Task<APIResponse<T>> SendAsync<T>(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken, bool noWait = false)
        {
            httpRequestMessage.Headers.TryAddWithoutValidation("x-nowait", noWait.ToString());

            var httpResponseMessage = await SendAsync(httpRequestMessage, cancellationToken, noWait);
            var response = await httpResponseMessage.Content.ReadAsStringAsync();

            // Grab the response headers
            APIResponse<T> apiResponse = new()
            {
                Headers = httpResponseMessage.Headers,
                RawResponse = response,
                Response = Json.Deserialize<T>(response),
                ResponseMessage = httpResponseMessage
            };

            return apiResponse;
        }

        public async Task<APIResponse<T>> SendAsync<T>(HttpRequestMessage httpRequestMessage, CookieContainer cookies, CancellationToken cancellationToken, bool noWait = false)
        {
            httpRequestMessage.SetCookieContainer(cookies);

            return await SendAsync<T>(httpRequestMessage, cancellationToken, noWait);
        }

        public async Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod httpMethod, string content, bool noWait = false)
        {
            var httpRequestMessage = BuildHttpRequestMessage(uri, headers, bearerToken, httpMethod, content);

            return await SendAsync(httpRequestMessage, CancellationToken.None, noWait);
        }

        public async Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod httpMethod, string content, CookieContainer cookies, bool noWait = false)
        {
            var httpRequestMessage = BuildHttpRequestMessage(uri, headers, bearerToken, httpMethod, content);

            httpRequestMessage.SetCookieContainer(cookies);

            return await SendAsync(httpRequestMessage, CancellationToken.None, noWait);
        }

        private static HttpRequestMessage BuildHttpRequestMessage(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod httpMethod, string content)
        {
            headers ??= [];

            // lets create a copy of the dictionary as if a shared/static
            // is passed this will cause an error with existing keys
            headers = headers.Keys.ToDictionary(k => k, v => headers[v]);

            if (!bearerToken.IsNullOrWhiteSpace())
                if (headers.Any(x => x.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase)))
                    headers["Authorization"] = $"Bearer {bearerToken}";
                else
                    headers.Add("Authorization", $"Bearer {bearerToken}");

            if (headers.Any(x => x.Key.Equals("cache-control", StringComparison.OrdinalIgnoreCase)))
                headers["cache-control"] = "no-cache";
            else
                headers.Add("cache-control", "no-cache");

            HttpRequestMessage httpRequestMessage = new(httpMethod, uri);

            foreach (var header in headers)
                httpRequestMessage.Headers.Add(header.Key, header.Value);

            if (httpMethod != HttpMethod.Get && httpMethod != HttpMethod.Delete)
                httpRequestMessage.Content = new StringContent(JsonConvert.DeserializeObject<JToken>(content).ToString(Formatting.None), Encoding.UTF8, "application/json");

            return httpRequestMessage;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken, bool noWait = false)
        {
            httpRequestMessage.Headers.TryAddWithoutValidation("x-nowait", noWait.ToString());

            var httpResponseMessage = await SendAsync(httpRequestMessage, cancellationToken);
            if (!(httpResponseMessage.IsSuccessStatusCode || httpResponseMessage.StatusCode == System.Net.HttpStatusCode.RedirectMethod))
            {
                var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                var exception = new HttpRequestException("Http connector failed.", new HttpResponseException(httpResponseMessage), httpResponseMessage.StatusCode);

                exception.Data.Add("RequestUri", httpResponseMessage.RequestMessage.RequestUri);
                exception.Data.Add("StatusCode", httpResponseMessage.StatusCode.ToString());
                exception.Data.Add("ReasonPhrase", httpResponseMessage.ReasonPhrase);
                var sensitive = IsSensitive(httpRequestMessage.RequestUri);
                exception.Data.Add("Sensitive", sensitive);
                var contentTask = httpRequestMessage.Content?.ReadAsStringAsync();
                exception.Data.Add("Request", sensitive ? "[REDACTED]" : contentTask == null ? string.Empty : await contentTask);
                exception.Data.Add("Response", sensitive ? "[REDACTED]" : response);

                var trace = _logger.CreateTrace();

                var debugTrace = $@"{{0}}> RequestUri: {exception.Data["RequestUri"]}
{{0}}> StatusCode: {exception.Data["StatusCode"]}
{{0}}> ReasonPhrase: {exception.Data["ReasonPhrase"]}
{{0}}> Request: {exception.Data["Request"]}
{{0}}> Response: {exception.Data["Response"]}";

                trace.Debug.WriteLine(debugTrace.MakeInterpolationSafe());

                trace.SetHashCode(httpRequestMessage.GetHashCode());

                trace.Log.LogError(exception, $"{{0}}> {exception.Message}, {{2}}ms");

                throw exception;
            }

            return httpResponseMessage;
        }

        private static bool IsSensitive(Uri requestUri) =>
            requestUri?.AbsolutePath.Contains("/scv/orders/by-flight-date", StringComparison.OrdinalIgnoreCase) == true ||
            requestUri?.AbsolutePath.Contains("/scv/orders/previous-trips", StringComparison.OrdinalIgnoreCase) == true ||
            requestUri?.AbsolutePath.Contains("account-linking-proof", StringComparison.OrdinalIgnoreCase) == true ||
            requestUri?.AbsolutePath.Contains("linked-flying-club-broker-token", StringComparison.OrdinalIgnoreCase) == true ||
            requestUri?.AbsolutePath.Contains("voucherredemption", StringComparison.OrdinalIgnoreCase) == true;

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CookieContainer cookies, CancellationToken cancellationToken, bool noWait = false)
        {
            httpRequestMessage.SetCookieContainer(cookies);

            return await SendAsync(httpRequestMessage, cancellationToken, noWait);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var trace = _logger.CreateTrace();

            trace.SetHashCode(request.GetHashCode());

            var httpClient = _httpClientFactory.CreateClient(request.RequestUri.ToString());

            HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            trace.Diagnostics.LogInformation($"{{0}}{request.Method}:{request.RequestUri}:{httpResponseMessage.Content.Headers.ContentLength}:SendAsync executed in {{7}}ms");

            return httpResponseMessage;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CookieContainer cookies, CancellationToken cancellationToken)
        {
            request.SetCookieContainer(cookies);

            return await SendAsync(request, cancellationToken);
        }
    }
}

