using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using VAA.EAPI.GraphQL.Common.Extensions;

namespace VAA.EAPI.GraphQL.Core.Xunit.Http
{
    [Obsolete("Use new mocking functionality now ported for unit tests")]
    public sealed class RequestResponseDelegatingHandler : DelegatingHandler
    {
        private readonly ConcurrentDictionary<string, (HttpRequestMessage, HttpResponseMessage)> cache;
        private readonly ILogger logger;
        private readonly object @lock = new();
        private HttpMessageHandler? httpMessageHandler;

        public int MaxAutomaticRetries
        {
            get
            {
                if (httpMessageHandler is null)
                    lock (@lock)
                        if (httpMessageHandler is null)
                        {
                            HttpMessageHandler current = InnerHandler!;

                            while (current is not HttpClientHandler)
                            {
                                if (current is not DelegatingHandler) break;

                                current = ((DelegatingHandler)current).InnerHandler!;

                                if (current is null) break;
                            }

                            if (current is HttpClientHandler)
                                httpMessageHandler = (HttpClientHandler)current;

                            httpMessageHandler ??= new HttpClientHandler();
                        }

                return ((HttpClientHandler)httpMessageHandler).MaxAutomaticRedirections;
            }
        }


        public RequestResponseDelegatingHandler(ILogger logger, ConcurrentDictionary<string, (HttpRequestMessage, HttpResponseMessage)> cache) : base()
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.cache = cache;
        }
        public RequestResponseDelegatingHandler(HttpMessageHandler innerHandler, ILogger logger, ConcurrentDictionary<string, (HttpRequestMessage, HttpResponseMessage)> cache) : base(innerHandler)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.cache = cache;
        }


        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response;

            var key = request.BuildKey();

            if (cache.TryGetValue(key, out (HttpRequestMessage, HttpResponseMessage) value))
                response = Clone(value);
            else
                response = base.Send(request, cancellationToken);

            return response;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response;

            var key = request.BuildKey();

            if (cache.TryGetValue(key, out (HttpRequestMessage, HttpResponseMessage) value))
                response = Clone(value);
            else
                response = await base.SendAsync(request, cancellationToken);

            return response;
        }


        private static HttpResponseMessage Clone((HttpRequestMessage request, HttpResponseMessage response) source)
        {
            (HttpRequestMessage request, HttpResponseMessage response) = source;

            var httpResponseMessage = new HttpResponseMessage(response.StatusCode)
            {
                Version = response.Version,
                ReasonPhrase = response.ReasonPhrase,
                RequestMessage = response.RequestMessage ?? request
            };

            if (response.Content != null)
            {
                var memoryStream = new MemoryStream();

                response.Content.ReadAsStream().CopyTo(memoryStream);

                memoryStream.Position = 0;

                httpResponseMessage.Content = new StreamContent(memoryStream);

                if (response.Content.Headers != null)
                    foreach (var header in response.Content.Headers)
                        httpResponseMessage.Content.Headers.Add(header.Key, header.Value);
            }

            foreach (var header in response.Headers)
                httpResponseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return httpResponseMessage;
        }
    }
}
