using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Text;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignRegistrationControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<CampaignRegistrationController>> _mockLogger;
        private readonly CampaignRegistrationController _controller;
        private readonly Mock<FunctionContext> _context;
        public CampaignRegistrationControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<CampaignRegistrationController>>();
            _controller = new CampaignRegistrationController(_mockMediator.Object, _mockLogger.Object);
            _context = new Mock<FunctionContext>();
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignRegistrationController(_mockMediator.Object, null));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignRegistrationController(null, _mockLogger.Object));
        }
        [Fact]
        public void RegisterCampaign_Succsess()
        {
            var req = SetupTestDataRequests("CampaignManagementMS\\CampaignRegisterValid");
            _mockMediator.Setup(m => m.Send(It.IsAny<CampaignRegisterRequestCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { Value = "Campaign registeration initiated successfully " });
            var Output = _controller.RegisterCampaign(req.Object);
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
