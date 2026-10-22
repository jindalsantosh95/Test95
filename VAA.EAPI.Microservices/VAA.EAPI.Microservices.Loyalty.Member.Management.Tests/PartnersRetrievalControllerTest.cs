using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PartnersRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class PartnersRetrievalControllerTest
    {
        private readonly Mock<ILogger<PartnersRetrievalController>> _mockLogger;
        private readonly Mock<IMediator> _mockMediator;
        private readonly PartnersRetrievalController _controller;
        private readonly Mock<FunctionContext> _context;

        public PartnersRetrievalControllerTest()
        {
            _mockLogger = new Mock<ILogger<PartnersRetrievalController>>();
            _mockMediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _controller = new PartnersRetrievalController(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PartnersRetrievalController(_mockMediator.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PartnersRetrievalController(null, _mockLogger.Object));
        }

        [Fact]
        public void PartnersRetrievalSuccess()
        {
            var req = SetupTestDataRequests();
            _mockMediator.Setup(m => m.Send(It.IsAny<PartnersRetrievalQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<PartnersResponse> { });
            var Output = _controller.Partners(req.Object);
            Assert.Equal(System.Net.HttpStatusCode.Accepted, Output.Result.StatusCode);
        }

        private Mock<HttpRequestData> SetupTestDataRequests()
        {
            var req = new Mock<HttpRequestData>(_context.Object);
            req.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                return response.Object;
            });
            return req;
        }
    }
}
