using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System;
using System.IO;
using System.Text;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.PromotionUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class PromotionUpdateControllerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly PromotionUpdateController _controller;
        private readonly Mock<FunctionContext> _context;
        public PromotionUpdateControllerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _controller = new PromotionUpdateController(_mockMediator.Object);
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PromotionUpdateController(null));
        }

        [Fact]
        public void UpdatePromotion_Succsess()
        {
            var req = SetupTestDataRequests("CampaignManagementMS\\PromotionUpdateValid");
            //_context.Setup(c => c.FunctionDefinition.PathToAssembly).Returns(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            _mockMediator.Setup(m => m.Send(It.IsAny<PromotionUpdateRequestCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            var Output = _controller.UpdatePromotion(req.Object);
            Assert.Equal(System.Net.HttpStatusCode.Accepted, Output.Result.StatusCode);

        }

        [Fact]
        public void UpdatePromotionResponseControllerSuccess()
        {
            var req = SetupTestDataRequests("CampaignManagementMS\\CampaignUpdateResponse");
            _mockMediator.Setup(m => m.Send(It.IsAny<PromotionUpdateResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<PromotionUpdateResponse> { });
            var Output = _controller.UpdatePromotionResponse(req.Object, "ffcb5fa6-ee84-4d55-a036-c3425a9e045f");
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
