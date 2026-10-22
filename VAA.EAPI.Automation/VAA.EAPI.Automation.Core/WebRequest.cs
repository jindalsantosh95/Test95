using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Http;
using VAA.EAPI.Automation.Core.Text;

namespace VAA.EAPI.Automation.Core
{
    public static class WebRequest
    {
        private static readonly object @lock = new();
        private static readonly ConcurrentDictionary<string, HttpClient> httpClients = new();
        private sealed class HttpPreload
        {
            public required string Key { get; init; }
            public required string Method { get; init; }
            public required string ContentType { get; init; }
            public required string Url { get; init; }
        }

        public static class Json
        {
            public static HttpResponseMessage Patch(string url, string content, NameValueCollection headers = null, X509Certificate x509Certificate = null) => WebRequest.Patch(url, new StringContent(content, Encoding.UTF8, "application/json"), headers, x509Certificate);

            public static HttpResponseMessage Post(string url, string content, NameValueCollection headers = null, X509Certificate x509Certificate = null) => WebRequest.Post(url, new StringContent(content, Encoding.UTF8, "application/json"), headers, x509Certificate);

            public static HttpResponseMessage Put(string url, string content, NameValueCollection headers = null, X509Certificate x509Certificate = null) => WebRequest.Put(url, new StringContent(content, Encoding.UTF8, "application/json"), headers, x509Certificate);
        }

        public static class Xml
        {
            public static HttpResponseMessage PostSoap(string url, string content, NameValueCollection headers = null, X509Certificate x509Certificate = null) => WebRequest.Post(url, new StringContent(content, Encoding.UTF8, "application/xml"), headers, x509Certificate);

            public static HttpResponseMessage Post(string url, string content, NameValueCollection headers = null, X509Certificate x509Certificate = null) => WebRequest.Post(url, new StringContent(content, Encoding.UTF8, "text/xml"), headers, x509Certificate);
        }

        public static class MultipartFormData
        {
            public static HttpResponseMessage Post(string url, string content, NameValueCollection headers = null, X509Certificate x509Certificate = null) => WebRequest.Post(url, content.ToMultipartFormDataContent(), headers, x509Certificate);
        }

        public static class FormUrlEncoded
        {
            public static HttpResponseMessage Post(string url, string content, NameValueCollection headers = null, X509Certificate x509Certificate = null) => WebRequest.Post(url, content.ToFormUrlEncodedContent(), headers, x509Certificate);
        }

        public static HttpResponseMessage Get(string url, NameValueCollection headers = null, X509Certificate x509Certificate = null) => Invoke(url, HttpMethod.Get, null, headers, x509Certificate);
        public static HttpResponseMessage Put(string url, HttpContent content, NameValueCollection headers = null, X509Certificate x509Certificate = null) => Invoke(url, HttpMethod.Put, content, headers, x509Certificate);
        public static HttpResponseMessage Post(string url, HttpContent content, NameValueCollection headers = null, X509Certificate x509Certificate = null) => Invoke(url, HttpMethod.Post, content, headers, x509Certificate);
        public static HttpResponseMessage Patch(string url, HttpContent content, NameValueCollection headers = null, X509Certificate x509Certificate = null) => Invoke(url, HttpMethod.Patch, content, headers, x509Certificate);
        public static HttpResponseMessage Delete(string url, NameValueCollection headers = null, X509Certificate x509Certificate = null) => Invoke(url, HttpMethod.Delete, null, headers, x509Certificate);


        /// <summary>
        /// Method to invoke http request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpMethod"></param>
        /// <param name="content"></param>
        /// <param name="headers"></param>
        /// <param name="x509Certificate"></param>
        /// <returns></returns>
        private static HttpResponseMessage Invoke(string url, HttpMethod httpMethod, HttpContent content = null, NameValueCollection headers = null, X509Certificate x509Certificate = null)
        {
            Uri baseUri = new Uri(url);

            string baseUrl = $"{baseUri.Scheme}://{baseUri.Host}:{baseUri.Port}".ToLower();

            if (!httpClients.ContainsKey(baseUrl))
                lock (@lock)
                    if (!httpClients.ContainsKey(baseUrl))
                    {
                        HttpClientHandler httpClientHandler = new HttpClientHandler()
                        {
                            AllowAutoRedirect = false,
                            AutomaticDecompression = System.Net.DecompressionMethods.All,
                            ServerCertificateCustomValidationCallback = (request, certificate, chain, sslPolicyErrors) => true,
                            ClientCertificates = { }
                        };

                        if (x509Certificate is not null)
                        {
                            httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
                            httpClientHandler.SslProtocols = SslProtocols.Tls12;
                            httpClientHandler.ClientCertificates.Add(x509Certificate);
                        }

                        HttpClient client = new HttpClient(new RedirectDelegatingHandler() { InnerHandler = httpClientHandler })
                        {
                            BaseAddress = baseUri,
                            Timeout = TimeSpan.FromSeconds(300)
                        };

                        client.DefaultRequestHeaders.Add("User-Agent", "TestAutomationHttpClient/v1.0");
                        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

                        httpClients.AddOrUpdate(baseUrl, client, (k, v) => client);
                    }

            HttpClient httpClient = httpClients[baseUrl];

            HttpRequestMessage httpRequestMessage = new(httpMethod, url);

            if (headers?.AllKeys.Any() ?? false)
                foreach (string key in headers.AllKeys)
                    httpRequestMessage.Headers.TryAddWithoutValidation(key, headers[key]);

            httpRequestMessage.Headers.TryAddWithoutValidation("api-channel", ConfigReader.Get("Source", "Automation-local"));
            httpRequestMessage.Headers.TryAddWithoutValidation("api-diagnostics", "true");

            httpRequestMessage.Content = content;

            return httpClient.Send(httpRequestMessage);
        }


        public static void PreLoadHttpClients()
        {
            IEnumerable<HttpPreload> preloads = ConfigReader.FilterVariables("^HTTP:PRELOAD:.*").Select(x =>
            {
                Match match = RegexCompiled.Match(x, "^HTTP:PRELOAD:([^:]+):([^:]+):(.*)$");

                return new HttpPreload
                {
                    Key = x,
                    Method = match.Groups[1].Value.ToUpper(),
                    ContentType = match.Groups[2].Value.ToUpper(),
                    Url = match.Groups[3].Value
                };
            });

            foreach (HttpPreload preload in preloads)
            {
                try
                {
                    switch (preload.Method)
                    {
                        case "GET":

                            WebRequest.Get(preload.Url);

                            break;
                        case "PUT":

                            WebRequest.Put(preload.Url, new StringContent(Environment.GetEnvironmentVariable(preload.Key, EnvironmentVariableTarget.Process), new System.Net.Http.Headers.MediaTypeHeaderValue(preload.ContentType)));

                            break;
                        case "POST":

                            WebRequest.Post(preload.Url, new StringContent(Environment.GetEnvironmentVariable(preload.Key, EnvironmentVariableTarget.Process), new System.Net.Http.Headers.MediaTypeHeaderValue(preload.ContentType)));

                            break;
                        case "DELETE":

                            WebRequest.Delete(preload.Url);

                            break;
                        case "PATCH":

                            WebRequest.Patch(preload.Url, new StringContent(Environment.GetEnvironmentVariable(preload.Key, EnvironmentVariableTarget.Process), new System.Net.Http.Headers.MediaTypeHeaderValue(preload.ContentType)));

                            break;
                    }
                }
                catch
                {
                }
            }
        }
    }
}
