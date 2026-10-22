using Microsoft.Extensions.Logging;
using System.Net;
using VAA.EAPI.Web.Common.Diagnostics;

namespace VAA.EAPI.Web.Common.Infrastructure.Services
{
    internal sealed class RedirectDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger logger;
        private readonly object @lock = new();
        private HttpMessageHandler httpMessageHandler = default;

        public int MaxAutomaticRedirections
        {
            get
            {
                if (httpMessageHandler is null)
                    lock (@lock)
                        if (httpMessageHandler is null)
                        {
                            HttpMessageHandler current = InnerHandler;

                            while (current is not HttpClientHandler)
                            {
                                if (current is not DelegatingHandler) break;

                                current = ((DelegatingHandler)current).InnerHandler;

                                if (current is null) break;
                            }

                            if (current is HttpClientHandler)
                                httpMessageHandler = (HttpClientHandler)current;

                            httpMessageHandler ??= new HttpClientHandler();
                        }

                return ((HttpClientHandler)httpMessageHandler).MaxAutomaticRedirections;
            }
        }


        public RedirectDelegatingHandler(ILogger logger) : base() => this.logger = logger;

        public RedirectDelegatingHandler(HttpMessageHandler innerHandler, ILogger logger) : base(innerHandler) => this.logger = logger;

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = base.Send(request, cancellationToken);

            int retries = MaxAutomaticRedirections;

            while ((int)response.StatusCode >= 300 && (int)response.StatusCode <= 399 && retries > 0)
            {
                retries--;

                var location = response.Headers.Location;

                if (location == null) return response;

                var newRequest = Clone(request, location);

                switch (response.StatusCode)
                {
                    case HttpStatusCode.MovedPermanently:
                    case HttpStatusCode.Redirect:
                    case HttpStatusCode.RedirectMethod:

                        if (request.Method == HttpMethod.Post)
                            newRequest.Method = HttpMethod.Get;

                        response = base.Send(newRequest, cancellationToken);

                        break;
                    case HttpStatusCode.RedirectKeepVerb:
                    case HttpStatusCode.PermanentRedirect:

                        response = base.Send(newRequest, cancellationToken);

                        break;
                }
            }

            return response;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            int retries = MaxAutomaticRedirections;

            while ((int)response.StatusCode >= 300 && (int)response.StatusCode <= 399 && retries > 0)
            {
                retries--;

                var location = response.Headers.Location;

                if (location == null) return response;

                var newRequest = Clone(request, location);

                switch (response.StatusCode)
                {
                    case HttpStatusCode.MovedPermanently:
                    case HttpStatusCode.Redirect:
                    case HttpStatusCode.RedirectMethod:

                        if (request.Method == HttpMethod.Post)
                            newRequest.Method = HttpMethod.Get;

                        response = await base.SendAsync(newRequest, cancellationToken);

                        break;
                    case HttpStatusCode.RedirectKeepVerb:
                    case HttpStatusCode.PermanentRedirect:

                        response = await base.SendAsync(newRequest, cancellationToken);

                        break;
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
