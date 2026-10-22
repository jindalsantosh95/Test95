using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Http;

namespace VAA.EAPI.Core.Xunit.Http
{
    internal sealed class MockDelegatingHandler : DelegatingHandler
    {
        private const string ResponseDelayHeader = "Response-Delay";
        private const string MocksDirectory = ".mocks";
        private const string MocksDirectorySearchPattern = "*.mock";

        private readonly ConcurrentQueue<HttpResponseMessage> _responses = [];

        public IReadOnlyList<HttpResponseMessage> Responses => _responses.ToList();

        private readonly ILogger _logger;
        private readonly HttpMock[] _items = [];

        public MockDelegatingHandler(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (!Directory.Exists(MocksDirectory))
                Directory.CreateDirectory(MocksDirectory);

            _items = [.. Directory.GetFiles(MocksDirectory, MocksDirectorySearchPattern, SearchOption.TopDirectoryOnly)
                                  .Select(path =>
                                  {
                                      try
                                      {
                                          return (HttpMock)new FileInfo(path);
                                      }
                                      catch (Exception ex)
                                      {
                                          _logger.LogError(ex, "Failed to load mock file {MockFile}. Skipping.", path);
                                          return null;
                                      }
                                  })
                                  .Where(m => m is not null)
                                  .Select(m => m!)];
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var responseMessage = _items.FindMock(request, _logger)?.CreateResponse(request);

            if (responseMessage is not null)
            {
                if (responseMessage.Headers.TryGetValues(ResponseDelayHeader, out var values))
                    Thread.Sleep(TimeSpan.Parse(string.Join(string.Empty, values)));

                ParseResponseContent(ref responseMessage);

                _responses.Enqueue(responseMessage);

                return responseMessage;
            }

            return base.Send(request, cancellationToken);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var responseMessage = _items.FindMock(request, _logger)?.CreateResponse(request);

            if (responseMessage is not null)
            {
                if (responseMessage.Headers.TryGetValues(ResponseDelayHeader, out var values))
                    Task.Delay(TimeSpan.Parse(string.Join(string.Empty, values)), cancellationToken)
                        .GetAwaiter()
                        .GetResult();

                ParseResponseContent(ref responseMessage);

                _responses.Enqueue(responseMessage);

                return Task.FromResult(responseMessage);
            }

            return base.SendAsync(request, cancellationToken);
        }

        private static void ParseResponseContent(ref HttpResponseMessage responseMessage)
        {
            if (responseMessage.Content is null) return;

            var content = responseMessage.Content.ReadAsString();

            if (content.IsNullOrWhiteSpace()) return;

            if (responseMessage.Content.Headers.ContentType is null)
                responseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("plain/text");

            using var dynamicDataGenerator = (CommandParser)content;

            content = dynamicDataGenerator.Parse();

            responseMessage.Content = new StringContent(content, System.Text.Encoding.UTF8, responseMessage.Content.Headers.ContentType!.MediaType!);
        }
    }
}
