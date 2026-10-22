using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Text;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignPromotionDelinking;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{

    public class CampaignPromotionDelinkingControllerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly CampaignPromotionDelinkingController _controller;
        private readonly Mock<ILogger<CampaignPromotionDelinkingController>> _mockLogger;
        private readonly Mock<FunctionContext> _context;
        public CampaignPromotionDelinkingControllerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<CampaignPromotionDelinkingController>>();
            _context = new Mock<FunctionContext>();
            _controller = new CampaignPromotionDelinkingController(_mockMediator.Object, _mockLogger.Object);
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignPromotionDelinkingController(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignPromotionDelinkingController(_mockMediator.Object, null));
        }
        [Fact]
        public void DelinkCampaignPromotion_Succsess()
        {
            var req = SetupTestDataRequests("CampaignManagementMS\\CampaignPromotionLinkingValid");
            _mockMediator.Setup(m => m.Send(It.IsAny<CampaignPromotionDelinkRequestCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            var Output = _controller.DelinkCampaignPromotion(req.Object);
            Assert.Equal(System.Net.HttpStatusCode.Accepted, Output.Result.StatusCode);

        }

        [Fact]
        public void DelinkCampaignPromotionControllerSuccess()
        {
            var req = SetupTestDataRequests("CampaignManagementMS\\CampaignCreateResponse");
            _mockMediator.Setup(m => m.Send(It.IsAny<CampaignCommandResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<CampaignCommandResponse> { });
            var Output = _controller.DelinkCampaignPromotionResponse(req.Object, "ffcb5fa6-ee84-4d55-a036-c3425a9e045f");
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
