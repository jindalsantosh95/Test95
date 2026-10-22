using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Common.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Web.Search.Tests
{
    public sealed class HttpConnectorCookieTests
    {
        [Fact]
        public async Task SendAsync_WithCookieContainer_SendsCookieHeaderAndCapturesSetCookie()
        {
            var uri = new Uri("https://cookies.test/resource");
            var cookies = new CookieContainer();
            cookies.Add(uri, new Cookie("session", "abc", "/"));
            var handler = new RecordingHandler(_ =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}")
                };

                response.Headers.TryAddWithoutValidation("Set-Cookie", "theme=dark; Path=/; HttpOnly");

                return response;
            });
            using var httpClient = CreateHttpClient(handler);
            var connector = new HttpConnector(NullLogger<HttpConnector>.Instance);

            await connector.SendAsync(httpClient, new HttpRequestMessage(HttpMethod.Get, uri), cookies, CancellationToken.None);

            Assert.True(handler.LastRequest.Headers.TryGetValues("Cookie", out var values));
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
            var requests = new List<HttpRequestMessage>();
            var handler = new RecordingHandler(request =>
            {
                requests.Add(request);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}")
                };

                if (request.RequestUri == setCookieUri)
                {
                    response.Headers.TryAddWithoutValidation("Set-Cookie", "session=abc; Path=/; HttpOnly");
                    response.Headers.TryAddWithoutValidation("x-response-id", "first");
                }
                else
                {
                    response.Headers.TryAddWithoutValidation("x-response-id", "second");
                }

                return response;
            });
            using var httpClient = CreateHttpClient(handler);
            var connector = new HttpConnector(NullLogger<HttpConnector>.Instance);

            await connector.SendAsync(httpClient, new HttpRequestMessage(HttpMethod.Get, setCookieUri), firstCookies, CancellationToken.None);
            var actualSecondResponse = await connector.SendAsync(httpClient, new HttpRequestMessage(HttpMethod.Get, resourceUri), secondCookies, CancellationToken.None);

            Assert.Equal("abc", firstCookies.GetCookies(setCookieUri)["session"]?.Value);
            Assert.Null(secondCookies.GetCookies(resourceUri)["session"]);
            Assert.True(actualSecondResponse.Headers.TryGetValues("x-response-id", out var values));
            Assert.Contains("second", values);
            Assert.False(requests[1].Headers.Contains("Cookie"));
        }

        private static HttpClient CreateHttpClient(HttpMessageHandler handler)
        {
            return new HttpClient(new CookieDelegatingHandler(NullLogger<CookieDelegatingHandler>.Instance)
            {
                InnerHandler = handler
            });
        }

        private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
        {
            public HttpRequestMessage LastRequest { get; private set; } = new(HttpMethod.Get, "about:blank");

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;

                var response = responder(request);
                response.RequestMessage ??= request;

                return Task.FromResult(response);
            }
        }
    }
}
