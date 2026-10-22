using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;
using System.Net;
using VAA.EAPI.Infrastructure.Services;
using TestHttpClientFactory = VAA.EAPI.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.Infrastructure.Tests
{
    public sealed class HttpConnectorCookieTests
    {
        [Fact]
        public async Task SendAsync_WithCookieContainer_SendsCookieHeaderAndCapturesSetCookie()
        {
            var uri = new Uri("https://cookies.test/resource");
            var cookies = new CookieContainer();
            cookies.Add(uri, new Cookie("session", "abc", "/"));
            var httpClientFactory = CreateHttpClientFactory();
            var connector = new HttpConnector(NullLogger<HttpConnector>.Instance, httpClientFactory);

            await connector.SendAsync(uri.ToString(), null, null, HttpMethod.Get, string.Empty, cookies);

            var request = httpClientFactory.GetResponses(uri.ToString()).Single(response => response.RequestMessage?.RequestUri == uri).RequestMessage!;

            Assert.True(request.Headers.TryGetValues("Cookie", out var values));
            Assert.Contains("session=abc", string.Join("; ", values));
            Assert.Equal("dark", cookies.GetCookies(uri)["theme"]?.Value);
        }

        [Fact]
        public async Task SendAsync_WithDifferentCookieContainers_DoesNotShareCookiesBetweenRequests()
        {
            var setCookieUri = new Uri("https://cookies.test/set-cookie");
            var resourceUri = new Uri("https://cookies.test/no-cookie-resource");
            var firstCookies = new CookieContainer();
            var secondCookies = new CookieContainer();
            var httpClientFactory = CreateHttpClientFactory();
            var connector = new HttpConnector(NullLogger<HttpConnector>.Instance, httpClientFactory);

            await connector.SendAsync(setCookieUri.ToString(), null, null, HttpMethod.Get, string.Empty, firstCookies);
            var actualSecondResponse = await connector.SendAsync(resourceUri.ToString(), null, null, HttpMethod.Get, string.Empty, secondCookies);
            var secondRequest = httpClientFactory.GetResponses(resourceUri.ToString()).Single(response => response.RequestMessage?.RequestUri == resourceUri).RequestMessage!;

            Assert.Equal("abc", firstCookies.GetCookies(setCookieUri)["session"]?.Value);
            Assert.Null(secondCookies.GetCookies(resourceUri)["session"]);
            Assert.True(actualSecondResponse.Headers.TryGetValues("x-response-id", out var values));
            Assert.Contains("second", values);
            Assert.False(secondRequest.Headers.Contains("Cookie"));
        }

        private static TestHttpClientFactory CreateHttpClientFactory()
        {
            return new TestHttpClientFactory();
        }
    }
}
