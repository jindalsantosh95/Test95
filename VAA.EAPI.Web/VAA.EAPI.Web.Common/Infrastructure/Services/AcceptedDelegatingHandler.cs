using Microsoft.Extensions.Logging;
using System.Net;
using VAA.EAPI.Web.Common.Diagnostics;
using VAA.EAPI.Web.Common.Extensions;
using VAA.EAPI.Web.Common.Helper;

namespace VAA.EAPI.Web.Common.Infrastructure.Services
{
    internal sealed class AcceptedDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger logger;

        public AcceptedDelegatingHandler(ILogger logger) : base() => this.logger = logger;

        public AcceptedDelegatingHandler(HttpMessageHandler innerHandler, ILogger logger) : base(innerHandler) => this.logger = logger;

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = base.Send(request, cancellationToken);

            if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted)
            {
                var location = response.Headers.Location;

                if (location == null) return response;

                var noWait = request.Headers.TryGetValues("x-nowait", out var values) && string.Join(string.Empty, values).Parse<bool>();
                var newRequest = Clone(request, location);

                newRequest.Method = HttpMethod.Get;

                if (!noWait)
                {
                    // give the host a chance before hammering the next call otherwise all calls have an instant double hit.
                    Task.Delay(ConfigReader.Try(Constants.Environment.TimeToWaitBeforeNextRecall, 50)).Wait();

                    response = base.Send(newRequest, cancellationToken);
                }
            }

            return response;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted)
            {
                var location = response.Headers.Location;

                if (location == null) return response;

                var noWait = request.Headers.TryGetValues("x-nowait", out var values) && string.Join(string.Empty, values).Parse<bool>();
                var newRequest = Clone(request, location);

                newRequest.Method = HttpMethod.Get;

                if (!noWait)
                {
                    // give the host a chance before hammering the next call otherwise all calls have an instant double hit.
                    Task.Delay(ConfigReader.Try(Constants.Environment.TimeToWaitBeforeNextRecall, 50)).Wait();

                    response = await base.SendAsync(newRequest, cancellationToken);
                }
            }

            return response;
        }

        private static HttpRequestMessage Clone(HttpRequestMessage request, Uri location)
        {
            var httpRequestMessage = new HttpRequestMessage(request.Method, location);
            if (request.Content != null)
            {
                var memoryStream = new MemoryStream();
                request.Content.ReadAsStream().CopyTo(memoryStream);
                memoryStream.Position = 0;
                httpRequestMessage.Content = new StreamContent(memoryStream);

                if (request.Content.Headers != null)
                    foreach (var header in request.Content.Headers)
                        httpRequestMessage.Content.Headers.Add(header.Key, header.Value);
            }

            httpRequestMessage.Version = request.Version;

            foreach (var option in request.Options)
                httpRequestMessage.Options.Set(new HttpRequestOptionsKey<object>(option.Key), option.Value);

            foreach (var header in request.Headers)
                httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return httpRequestMessage;
        }
    }
}
