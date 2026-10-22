using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace VAA.EAPI.GraphQL.Framework.Tests
{
    [ExcludeFromCodeCoverage]
    public static class MockHttpRequestData
    {
        /// <summary>
        /// Create a Mocked instance of an HttpRequestData with 
        /// </summary>
        /// <returns></returns>
        public static Mock<HttpRequestData> Create(FunctionContext context)
        {
            var req = new Mock<HttpRequestData>(context);
            req.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(context);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });

            return req;
        }
    }
}