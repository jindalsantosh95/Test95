using Microsoft.Extensions.Logging;
using VAA.EAPI.Automation.Core.Azure.Functions;
using VAA.EAPI.Automation.Core.Diagnostics;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.Core.Http
{
    public sealed class DefaultDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger logger;

        public DefaultDelegatingHandler(ILogger logger) : base() => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public DefaultDelegatingHandler(HttpMessageHandler innerHandler, ILogger logger) : base(innerHandler) => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        private static void AddOrUpdateIfNotEmpty(HttpRequestMessage request, string name, object value)
        {
            var headerValue = value?.ToString();

            if (!string.IsNullOrWhiteSpace(headerValue))
                request.Headers.AddOrUpdate(name, headerValue);
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            VAA.EAPI.Automation.Core.Diagnostics.Trace<VAA.EAPI.Automation.Core.Http.DefaultDelegatingHandler> trace = Trace<DefaultDelegatingHandler>.Create(logger);

            request.Headers.AddOrUpdate("User-Agent", "HttpClientFactory/v1.0");
            request.Headers.AddOrUpdate("loadTest", ConfigReader.Try("DeltaSearchLoadTest", false).ToString());
            request.Headers.AddOrUpdate(AsyncContext.APIDiagnosticsHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APIDiagnosticsHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.APIChannelHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APIChannelHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.APISessionIdHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APISessionIdHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.APIOperationIdHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APIOperationIdHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.APICorrelationIdHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APICorrelationIdHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.OcpApimTraceHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.OcpApimTraceHeader].ToString());
            AddOrUpdateIfNotEmpty(request, AsyncContext.W3cTraceContextHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.W3cTraceContextHeader]);
            AddOrUpdateIfNotEmpty(request, AsyncContext.W3CBaggageHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.W3CBaggageHeader]);

            trace.SetHashCode(request.GetHashCode());

            System.Net.Http.HttpResponseMessage response = base.Send(request, cancellationToken);

            response.Headers.TryAddWithoutValidation($"http-execution-time-{GetType().Name}", trace.ElapsedMilliseconds.ToString());

            trace.Diagnostics.LogInformation($"{{0}}{request.Method}:{request.RequestUri}:{response.StatusCode}:{response.Content.Headers.ContentLength}:Send executed in {{2}}ms");

            // we do this force the downloading of the response so the logged timings
            // are absolute and not based on lazy loading times
            long contentLength = response.Content.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult().Length;

            response.Content.Headers.ContentLength ??= contentLength;

            response.Headers.TryAddWithoutValidation($"http-execution-time-{GetType().Name}", trace.ElapsedMilliseconds.ToString());

            trace.Diagnostics.LogInformation($"{{0}}{request.Method}:{request.RequestUri}:{response.StatusCode}:{response.Content.Headers.ContentLength}:ReadAsStringAsync executed in {{2}}ms");

            return response;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            VAA.EAPI.Automation.Core.Diagnostics.Trace<VAA.EAPI.Automation.Core.Http.DefaultDelegatingHandler> trace = Trace<DefaultDelegatingHandler>.Create(logger);

            request.Headers.AddOrUpdate("User-Agent", "HttpClientFactory/v1.0");
            request.Headers.AddOrUpdate("loadTest", ConfigReader.Try("DeltaSearchLoadTest", false).ToString());
            request.Headers.AddOrUpdate(AsyncContext.APIDiagnosticsHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APIDiagnosticsHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.APIChannelHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APIChannelHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.APISessionIdHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APISessionIdHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.APIOperationIdHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APIOperationIdHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.APICorrelationIdHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APICorrelationIdHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.OcpApimTraceHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.OcpApimTraceHeader].ToString());
            AddOrUpdateIfNotEmpty(request, AsyncContext.W3cTraceContextHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.W3cTraceContextHeader]);
            AddOrUpdateIfNotEmpty(request, AsyncContext.W3CBaggageHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.W3CBaggageHeader]);

            trace.SetHashCode(request.GetHashCode());

            System.Net.Http.HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            response.Headers.TryAddWithoutValidation($"http-execution-time-{GetType().Name}", trace.ElapsedMilliseconds.ToString());

            trace.Diagnostics.LogInformation($"{{0}}{request.Method}:{request.RequestUri}:{response.StatusCode}:{response.Content.Headers.ContentLength}:SendAsync executed in {{2}}ms");

            // we do this force the downloading of the response so the logged timings
            // are absolute and not based on lazy loading times
            long contentLength = (await response.Content.ReadAsStringAsync(cancellationToken)).Length;

            response.Content.Headers.ContentLength ??= contentLength;

            response.Headers.TryAddWithoutValidation($"http-execution-time-{GetType().Name}", trace.ElapsedMilliseconds.ToString());

            trace.Diagnostics.LogInformation($"{{0}}{request.Method}:{request.RequestUri}:{response.StatusCode}:{response.Content.Headers.ContentLength}:ReadAsStringAsync executed in {{2}}ms");

            return response;
        }
    }
}
