using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Http.Mocking;
using Xunit;

namespace VAA.EAPI.GraphQL.Common.Tests
{
    public sealed class HttpMockCachedResponseTests
    {
        [Fact]
        public async Task CreateResponseReusesLargeCachedBodyUnderConcurrentLoad()
        {
            const int responseSize = 16 * 1024 * 1024;
            using var sourceRequest = new HttpRequestMessage(HttpMethod.Get, "https://tempuri.org/large-response");
            using var sourceResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(new byte[responseSize]),
                RequestMessage = sourceRequest
            };
            var mock = new HttpMock(sourceRequest, sourceResponse);

            var responses = await Task.WhenAll(Enumerable.Range(0, 64).Select(_ => Task.Run(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://tempuri.org/large-response");
                return mock.CreateResponse(request);
            })));

            try
            {
                Assert.All(responses, response => Assert.Equal(responseSize, response.Content.Headers.ContentLength));
                Assert.Equal(64, responses.Distinct().Count());
            }
            finally
            {
                foreach (var response in responses)
                {
                    response.RequestMessage?.Dispose();
                    response.Dispose();
                }
            }
        }
    }
}
