using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Text;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignUnassign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUnassign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignUnassignControllerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<FunctionContext> _context;
        private readonly CampaignUnassignController _controller;
        private readonly Mock<ILogger<CampaignUnassignController>> _mockLogger;

        public CampaignUnassignControllerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _mockLogger = new Mock<ILogger<CampaignUnassignController>>();
            _controller = new CampaignUnassignController(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectAndLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignUnassignController(null, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignUnassignController(_mockMediator.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignUnassignController(null, _mockLogger.Object));
        }

        [Fact]
        public void UnassignCampaign_Succsess()
        {
            var req = SetupTestDataRequests("CampaignManagementMS\\CampaignUnassignValid");
            _mockMediator.Setup(m => m.Send(It.IsAny<CampaignUnassignRequestCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "Campaign unassign initiated successfully " });
            var Output = _controller.UnassignCampaign(req.Object);
            Assert.Equal(System.Net.HttpStatusCode.Accepted, Output.Result.StatusCode);
        }

        [Fact]
        public void UnassignCampaignResponseControllerSuccess()
        {
            var req = SetupTestDataRequests("CampaignManagementMS\\CampaignCreateResponse");
            _mockMediator.Setup(m => m.Send(It.IsAny<CampaignUnassignResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<CampaignUnassignResponse> { });
            var Output = _controller.CampaignUnassignResponseAsync(req.Object, "ffcb5fa6-ee84-4d55-a036-c3425a9e045f");
            Assert.Equal(System.Net.HttpStatusCode.Accepted, Output.Result.StatusCode);
        }

        private Mock<HttpRequestData> SetupTestDataRequests(string requestBodySampleFile)
        {
            string body = TestDataProvider.GetTestData<string>(requestBodySampleFile);
            var byteArray = Encoding.ASCII.GetBytes(body);
            var bodyStream = new MemoryStream(byteArray);

            var req = new Mock<HttpRequestData>(_context.Object);
            req.Setup(m => m.Body).Returns(bodyStream);
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