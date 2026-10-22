using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Diagnostics;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Common.Http
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

        private static void AddTraceStateIfTraceParentPresent(HttpRequestMessage request)
        {
            if (request.Headers.Contains(AsyncContext.W3cTraceContextHeader))
                AddOrUpdateIfNotEmpty(request, AsyncContext.W3cTraceStateHeader, AsyncContext.Current?.W3CTraceState);
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var trace = Trace<DefaultDelegatingHandler>.Create(logger);

            request.Headers.AddOrUpdate("User-Agent", "HttpClientFactory/v1.0");
            request.Headers.AddOrUpdate("loadTest", ConfigReader.Try("DeltaSearchLoadTest", false).ToString());
            request.Headers.AddOrUpdate(AsyncContext.APIDiagnosticsHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APIDiagnosticsHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.APIChannelHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APIChannelHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.APISessionIdHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APISessionIdHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.APIOperationIdHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APIOperationIdHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.APICorrelationIdHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APICorrelationIdHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.OcpApimTraceHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.OcpApimTraceHeader].ToString());
            AddOrUpdateIfNotEmpty(request, AsyncContext.W3CBaggageHeader, AsyncContext.Current?.W3CBaggage);
            AddTraceStateIfTraceParentPresent(request);

            trace.SetHashCode(request.GetHashCode());

            var response = base.Send(request, cancellationToken);

            response.Headers.TryAddWithoutValidation($"http-execution-time-{GetType().Name}", trace.ElapsedMilliseconds.ToString());

            trace.Diagnostics.LogInformation($"{{0}}{request.Method}:{request.RequestUri}:{response.StatusCode}{(response.Headers.TryGetValues("x-data-source", out IEnumerable<string> values) ? $"({string.Join(',', values.Select(value => value.Replace("{", "{{").Replace("}", "}}")))})" : string.Empty)}:{response.Content.Headers.ContentLength}:Send executed in {{2}}ms");            

            response.Content = TrackCompletion(request, response, trace);

            return response;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var trace = Trace<DefaultDelegatingHandler>.Create(logger);

            request.Headers.AddOrUpdate("User-Agent", "HttpClientFactory/v1.0");
            request.Headers.AddOrUpdate("loadTest", ConfigReader.Try("DeltaSearchLoadTest", false).ToString());
            request.Headers.AddOrUpdate(AsyncContext.APIDiagnosticsHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APIDiagnosticsHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.APIChannelHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APIChannelHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.APISessionIdHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APISessionIdHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.APIOperationIdHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APIOperationIdHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.APICorrelationIdHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.APICorrelationIdHeader].ToString());
            request.Headers.AddOrUpdate(AsyncContext.OcpApimTraceHeader, Trace<DefaultDelegatingHandler>.DefaultCustomProperties[AsyncContext.OcpApimTraceHeader].ToString());
            AddOrUpdateIfNotEmpty(request, AsyncContext.W3CBaggageHeader, AsyncContext.Current?.W3CBaggage);
            AddTraceStateIfTraceParentPresent(request);

            trace.SetHashCode(request.GetHashCode());

            var response = await base.SendAsync(request, cancellationToken);

            response.Headers.TryAddWithoutValidation($"http-execution-time-{GetType().Name}", trace.ElapsedMilliseconds.ToString());

            trace.Diagnostics.LogInformation($"{{0}}{request.Method}:{request.RequestUri}:{response.StatusCode}{(response.Headers.TryGetValues("x-data-source", out IEnumerable<string> values) ? $"({string.Join(',', values.Select(value => value.Replace("{", "{{").Replace("}", "}}")))})" : string.Empty)}:{response.Content.Headers.ContentLength}:SendAsync executed in {{2}}ms");

            response.Content = TrackCompletion(request, response, trace);

            return response;
        }

        private CompletionTrackingHttpContent TrackCompletion(HttpRequestMessage request, HttpResponseMessage response, Trace<DefaultDelegatingHandler> trace)
            => new(response.Content, contentLength =>
            {
                response.Content.Headers.ContentLength ??= contentLength;
                response.Headers.Remove($"http-execution-time-{GetType().Name}");
                response.Headers.TryAddWithoutValidation($"http-execution-time-{GetType().Name}", trace.ElapsedMilliseconds.ToString());
                trace.Diagnostics.LogInformation($"{{0}}, {request.Method}:{request.RequestUri}:{response.StatusCode}:{response.Content.Headers.ContentLength}:Response content consumed in {{2}}ms");
            });
    }
}
