using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignTypeRetrieval;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignTypeRetrieveControllerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<CampaignTypeRetrieveController>> _mockLogger;
        private readonly CampaignTypeRetrieveController _controller;
        private readonly Mock<FunctionContext> _context;

        public CampaignTypeRetrieveControllerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<CampaignTypeRetrieveController>>();
            _context = new Mock<FunctionContext>();
            _controller = new CampaignTypeRetrieveController(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignTypeRetrieveController(_mockMediator.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignTypeRetrieveController(null, _mockLogger.Object));
        }

        [Fact]
        public void CampaignTypeRetrieveControllerSuccsess()
        {
            var req = SetupTestDataRequests();
            _mockMediator.Setup(m => m.Send(It.IsAny<CampaignTypeRetrievalQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<CampaignTypeResponse> { });
            var Output = _controller.RetrieveCampaignTypes(req.Object);
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
