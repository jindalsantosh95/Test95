using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Http.Mocking;

namespace VAA.EAPI.GraphQL.Common.Http
{
    internal sealed class MockDelegatingHandler(ILogger logger) : DelegatingHandler
    {
        private const string ResponseDelayHeader = "Response-Delay";
        private const string XMockRawRequestHeader = "x-mock-raw-request";
        private const string ConfigEnabled = "MockDelegatingHandler:Enabled";
        private const string ConfigMockServiceUri = "MockDelegatingHandler:MockServiceUri";

        public static bool Enabled => ConfigReader.Try(ConfigEnabled, false) && MockServiceUri is not null;
        public static Uri MockServiceUri => ConfigReader.Try<Uri>(ConfigMockServiceUri, null);

        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ConcurrentBag<HttpMock> _items = [];

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!Enabled) return base.Send(request, cancellationToken);

            var mock = _items.FindMock(request, _logger);

            HttpResponseMessage response;

            if (mock is not null)
            {
                response = mock.CreateResponse(request);

                if (response.Headers.TryGetValues(ResponseDelayHeader, out var values) && string.Join(string.Empty, values).TryParse(out TimeSpan value) && value.TotalMilliseconds > 0)
                    Thread.Sleep((int)value.TotalMilliseconds);

                return response;
            }

            Uri location = new($"{MockServiceUri.ToString().TrimEnd('/')}/{HttpUtility.UrlEncode(request.RequestUri.ToString())}");

            var newRequest = request.Clone(location);

            response = base.Send(newRequest, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotImplemented)
            {
                response.Dispose();

                return base.Send(request, cancellationToken);
            }
            else
            {
                response.Content?.LoadIntoBufferAsync().GetAwaiter().GetResult();

                var shouldCache = HttpMock.TryExtractMockRequest(response, out var mockRequest);

                response.Headers.Remove(XMockRawRequestHeader);

                if (shouldCache)
                    _items.Add(new HttpMock(mockRequest, response));
            }

            return response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!Enabled) return base.SendAsync(request, cancellationToken);

            var mock = _items.FindMock(request, _logger);

            HttpResponseMessage response;

            if (mock is not null)
            {
                response = mock.CreateResponse(request);

                var delay = 0;

                if (response.Headers.TryGetValues(ResponseDelayHeader, out var values) && string.Join(string.Empty, values).TryParse(out TimeSpan value) && value.TotalMilliseconds > 0)
                    delay = (int)value.TotalMilliseconds;

                return Task.Delay(delay, cancellationToken)
                           .ContinueWith(_ => response, cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }

            Uri location = new($"{MockServiceUri.ToString().TrimEnd('/')}/{HttpUtility.UrlEncode(request.RequestUri.ToString())}");

            var newRequest = request.Clone(location);

            //we don't to await the other calls only this one
            var sendAsyncTask = base.SendAsync(newRequest, cancellationToken);

            response = sendAsyncTask.GetAwaiter().GetResult();

            if (response.StatusCode == System.Net.HttpStatusCode.NotImplemented)
            {
                response.Dispose();

                return base.SendAsync(request, cancellationToken);
            }
            else
            {
                response.Content?.LoadIntoBufferAsync().GetAwaiter().GetResult();

                var shouldCache = HttpMock.TryExtractMockRequest(response, out var mockRequest);

                response.Headers.Remove(XMockRawRequestHeader);

                if (shouldCache)
                    _items.Add(new HttpMock(mockRequest, response));
            }

            return sendAsyncTask;
        }
    }
}
