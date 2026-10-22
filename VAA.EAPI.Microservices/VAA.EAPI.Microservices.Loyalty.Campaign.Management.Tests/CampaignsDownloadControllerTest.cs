using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignsDownload;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignsDownloadControllerTest
    {
        private readonly Mock<ILogger<CampaignsDownloadController>> _mockLogger;
        private readonly Mock<IMediator> _mockMediator;
        private readonly CampaignsDownloadController _controller;
        private readonly Mock<FunctionContext> _context;

        public CampaignsDownloadControllerTest()
        {
            _mockLogger = new Mock<ILogger<CampaignsDownloadController>>();
            _mockMediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _controller = new CampaignsDownloadController(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignsDownloadController(_mockMediator.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignsDownloadController(null, _mockLogger.Object));
        }

        [Fact]
        public void CampaignsDownloadSuccsess()
        {
            var req = SetupTestDataRequests();
            _mockMediator.Setup(m => m.Send(It.IsAny<CampaignsDownloadQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<CampaignsDownloadResponse> { });
            var Output = _controller.CampaignsDownload(req.Object);
            Assert.Equal(System.Net.HttpStatusCode.Accepted, Output.Result.StatusCode);
        }

        [Fact]
        public void AssociatedMembersDownloadSuccsess()
        {
            var req = SetupTestDataRequests();
            _mockMediator.Setup(m => m.Send(It.IsAny<AssociatedMembersDownloadQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<AssociatedMembersDownloadResponse> { });
            var Output = _controller.AssociatedMembersDownload(req.Object, "QWER");
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
