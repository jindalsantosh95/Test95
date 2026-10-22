using Microsoft.Extensions.Logging;
using System.Net;
using VAA.EAPI.Web.Common.Extensions;

namespace VAA.EAPI.Web.Common.Infrastructure.Services
{
    public sealed class CookieDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger logger;

        public CookieDelegatingHandler(ILogger logger) : base() => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public CookieDelegatingHandler(HttpMessageHandler innerHandler, ILogger logger) : base(innerHandler) => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            AddCookieHeader(request);

            var response = base.Send(request, cancellationToken);

            CopyCookieContainerToResponseRequest(request, response);
            CaptureCookies(response);

            return response;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            AddCookieHeader(request);

            var response = await base.SendAsync(request, cancellationToken);

            CopyCookieContainerToResponseRequest(request, response);
            CaptureCookies(response);

            return response;
        }

        private static void AddCookieHeader(HttpRequestMessage request)
        {
            if (!request.TryGetCookieContainer(out var cookieContainer)) return;
            if (request.RequestUri is null) return;

            request.Headers.Remove("Cookie");

            var cookieHeader = cookieContainer.GetCookieHeader(request.RequestUri);

            if (cookieHeader.IsNullOrWhiteSpace()) return;

            request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
        }

        private static void CopyCookieContainerToResponseRequest(HttpRequestMessage request, HttpResponseMessage response)
        {
            if (response?.RequestMessage is null) return;
            if (!request.TryGetCookieContainer(out var cookieContainer)) return;

            response.RequestMessage.SetCookieContainer(cookieContainer);
        }

        private void CaptureCookies(HttpResponseMessage response)
        {
            if (response?.RequestMessage?.RequestUri is null) return;
            if (!response.RequestMessage.TryGetCookieContainer(out var cookieContainer)) return;
            if (!response.Headers.TryGetValues("Set-Cookie", out var values)) return;

            foreach (var value in values)
            {
                try
                {
                    cookieContainer.SetCookies(response.RequestMessage.RequestUri, value);
                }
                catch (CookieException ex)
                {
                    logger.LogWarning(ex, "Failed to capture downstream cookie header.");
                }
            }
        }
    }
}
