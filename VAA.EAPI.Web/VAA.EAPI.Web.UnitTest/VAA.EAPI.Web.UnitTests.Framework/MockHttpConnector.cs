using Moq;
using System;
using VAA.EAPI.Web.Common.Infrastructure.Interfaces;

namespace VAA.EAPI.Web.UnitTests.Framework
{
    public class MockHttpConnector
    {
        public Mock<IHttpConnector> Create(Func<string, MockHttpConnectorParams> callBack)
        {

            Mock<IHttpConnector> mockConnector = new();

            //Task<HttpResponseMessage> GetAsync(string uri, Dictionary<string, string> headers, string bearerToken);

            //Task<HttpResponseMessage> SendAsync(string uri, Dictionary<string, string> headers, string bearerToken, HttpMethod httpMethod, string content, bool noWait = false);

            //Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken, bool noWait = false);

            //Task<HttpResponseMessage> SendAsync(HttpClient httpClient, HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken, bool noWait = false);

            //Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);

            //Task<HttpResponseMessage> DeleteAsync(string uri, Dictionary<string, string> headers, string bearerToken);

            return mockConnector;
        }
    }
    public class MockHttpConnectorParams
    {

    }
}

