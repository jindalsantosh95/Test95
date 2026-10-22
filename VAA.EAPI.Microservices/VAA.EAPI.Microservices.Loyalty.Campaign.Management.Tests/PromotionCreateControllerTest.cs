using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System;
using System.IO;
using System.Text;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.PromotionCreate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCreate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class PromotionCreateControllerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly PromotionCreateController _controller;
        private readonly Mock<FunctionContext> _context;
        public PromotionCreateControllerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _controller = new PromotionCreateController(_mockMediator.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PromotionCreateController(null));
        }

        [Fact]
        public void PromotionCreateControllerSuccess()
        {
            var req = SetupTestDataRequests("CampaignManagementMS\\PromotionCreateValid");
            _mockMediator.Setup(m => m.Send(It.IsAny<PromotionCreateRequestCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            var Output = _controller.CreateCampaign(req.Object);
            Assert.Equal(System.Net.HttpStatusCode.Accepted, Output.Result.StatusCode);

        }

        [Fact]
        public void PromotionCreateResponseControllerSuccess()
        {
            var req = SetupTestDataRequests("CampaignManagementMS\\CampaignCreateResponse");
            _mockMediator.Setup(m => m.Send(It.IsAny<PromotionCreateResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<PromotionCreateResponse> { });
            var Output = _controller.CreatePromotionResponse(req.Object, "ffcb5fa6-ee84-4d55-a036-c3425a9e045f");
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
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });
            return req;
        }
    }
}
