using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Infrastructure.HealthChecks
{
    public sealed class HttpHealthCheck : IHealthCheck
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly string endPoint;
        private readonly HttpMethod httpMethod;
        private readonly HttpStatusCode expectedHttpStatusCode;
        private readonly Dictionary<string, IEnumerable<string>> headers;
        private readonly HttpContent content;
        private readonly HttpClient httpClient;

        public HttpHealthCheck(IHttpClientFactory httpClientFactory, string endPoint, HttpMethod httpMethod, HttpStatusCode expectedHttpStatusCode) : this(httpClientFactory, endPoint, httpMethod, expectedHttpStatusCode, null) { }
        public HttpHealthCheck(IHttpClientFactory httpClientFactory, string endPoint, HttpMethod httpMethod, HttpStatusCode expectedHttpStatusCode, Dictionary<string, IEnumerable<string>> headers) : this(httpClientFactory, endPoint, httpMethod, expectedHttpStatusCode, headers, null) { }
        public HttpHealthCheck(IHttpClientFactory httpClientFactory, string endPoint, HttpMethod httpMethod, HttpStatusCode expectedHttpStatusCode, Dictionary<string, IEnumerable<string>> headers, X509Certificate2 certificate) : this(httpClientFactory, endPoint, httpMethod, expectedHttpStatusCode, headers, certificate, null) { }
        public HttpHealthCheck(IHttpClientFactory httpClientFactory, string endPoint, HttpMethod httpMethod, HttpStatusCode expectedHttpStatusCode, Dictionary<string, IEnumerable<string>> headers, X509Certificate2 certificate, HttpContent content)
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.endPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
            this.httpMethod = httpMethod;
            this.expectedHttpStatusCode = expectedHttpStatusCode;
            this.headers = headers ?? new Dictionary<string, IEnumerable<string>>();
            this.content = content;

            httpClient = httpClientFactory.CreateClient(endPoint);

            if (certificate is not null)
            {
                HttpMessageHandler httpMessageHandler = default;

                HttpMessageHandler current = typeof(HttpMessageInvoker).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).FirstOrDefault(x => x.Name.Is("_handler")).GetValue(httpClient) as HttpMessageHandler;

                while (current is not HttpClientHandler)
                {
                    if (current is not DelegatingHandler) break;

                    current = ((DelegatingHandler)current).InnerHandler;

                    if (current is null) break;
                }

                if (current is HttpClientHandler)
                    httpMessageHandler = (HttpClientHandler)current;

                ((HttpClientHandler)httpMessageHandler).ClientCertificateOptions = ClientCertificateOption.Manual;
                ((HttpClientHandler)httpMessageHandler).SslProtocols = SslProtocols.Tls12;
                ((HttpClientHandler)httpMessageHandler).ClientCertificates.Add(certificate);
            }
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            HealthCheckResult healthCheckResult;
            Dictionary<string, object> extraInformation = new Dictionary<string, object>();
            string key = endPoint.ToString();

            var request = new HttpRequestMessage(httpMethod, key) { Content = content };

            extraInformation.Add(nameof(endPoint), endPoint);
            extraInformation.Add(nameof(expectedHttpStatusCode), expectedHttpStatusCode);

            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);

                extraInformation.Add($"Header: {header.Key}", header.Value);
            }

            try
            {
                var response = await httpClient.SendAsync(request);

                extraInformation.Add(nameof(response.StatusCode), response.StatusCode);

                if (response.StatusCode == expectedHttpStatusCode)
                    healthCheckResult = HealthCheckResult.Healthy($"Endpoint responded with expected status code.", data: new ReadOnlyDictionary<string, object>(extraInformation));
                else
                    healthCheckResult = HealthCheckResult.Unhealthy($"Endpoint did not responded with expected status code.", data: new ReadOnlyDictionary<string, object>(extraInformation));
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException exception)
                    extraInformation.Add(nameof(exception.StatusCode), exception.StatusCode);

                ex.Data.Add(nameof(endPoint), endPoint);
                ex.Data.Add(nameof(expectedHttpStatusCode), expectedHttpStatusCode);
                ex.Data.Add(nameof(headers), headers);

                healthCheckResult = new HealthCheckResult(context?.Registration?.FailureStatus ?? HealthStatus.Unhealthy, ex.Message, ex, new ReadOnlyDictionary<string, object>(extraInformation));
            }

            return healthCheckResult;
        }
    }
}
