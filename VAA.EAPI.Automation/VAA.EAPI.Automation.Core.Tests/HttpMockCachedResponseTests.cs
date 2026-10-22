using System.Net;
using VAA.EAPI.Automation.Core.Http.Mocking;
using Xunit;

namespace VAA.EAPI.Automation.Core.Tests;

public sealed class HttpMockCachedResponseTests
{
    [Fact]
    public async Task CreateResponseReusesLargeCachedBodyUnderConcurrentLoad()
    {
        const int responseSize = 16 * 1024 * 1024;
        using HttpRequestMessage sourceRequest = new(HttpMethod.Get, "https://tempuri.org/large-response");
        using HttpResponseMessage sourceResponse = new(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(new byte[responseSize]),
            RequestMessage = sourceRequest
        };
        HttpMock mock = new(sourceRequest, sourceResponse);

        HttpResponseMessage[] responses = await Task.WhenAll(Enumerable.Range(0, 64).Select(_ => Task.Run(() =>
        {
            HttpRequestMessage request = new(HttpMethod.Get, "https://tempuri.org/large-response");
            return mock.CreateResponse(request);
        })));

        try
        {
            Assert.All(responses, response => Assert.Equal(responseSize, response.Content.Headers.ContentLength));
            Assert.Equal(64, responses.Distinct().Count());
        }
        finally
        {
            foreach (HttpResponseMessage response in responses)
            {
                response.RequestMessage?.Dispose();
                response.Dispose();
            }
        }
    }
}
