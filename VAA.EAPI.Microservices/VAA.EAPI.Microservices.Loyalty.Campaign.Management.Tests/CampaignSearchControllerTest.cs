using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Text;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignSearchControllerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly CampaignSearchController _controller;
        private readonly Mock<FunctionContext> _context;
        private readonly Mock<ILogger<CampaignSearchController>> _mockLogger;

        public CampaignSearchControllerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _mockLogger = new Mock<ILogger<CampaignSearchController>>();
            _controller = new CampaignSearchController(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignSearchController(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignSearchController(_mockMediator.Object, null));
        }

        [Fact]
        public void campaignSearchSuccess()
        {
            var req = SetupTestDataRequests("CampaignManagementMS\\CampaignSearchRequest");

            _mockMediator.Setup(m => m.Send(It.IsAny<CampaignSearchQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<CampaignSearchResponse> { });

            var Output = _controller.CampaignSearch(req.Object, null, "VIRGIN007", null, null, null, null, null, null, null);
            Assert.Equal(System.Net.HttpStatusCode.Accepted, Output.Result.StatusCode);
        }

        [Fact]
        public void campaignRetrieveSuccess()
        {
            var req = SetupTestDataRequests("CampaignManagementMS\\CampaignSearchRequest");

            _mockMediator.Setup(m => m.Send(It.IsAny<CampaignDetailsRetrieveQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<CampaignRetrieveResponse> { });

            var Output = _controller.CampaignRetrieve(req.Object, null, "VIRGIN007");
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
