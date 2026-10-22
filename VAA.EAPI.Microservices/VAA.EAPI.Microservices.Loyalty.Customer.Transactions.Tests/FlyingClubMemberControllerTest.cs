using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class FlyingClubMemberControllerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<FlyingClubMemberController>> _logger;
        private readonly Mock<FunctionContext> _context;
        private readonly FlyingClubMemberController _controller;

        public FlyingClubMemberControllerTest()
        {
            _mediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _logger = new Mock<ILogger<FlyingClubMemberController>>();
            _controller = new FlyingClubMemberController(_mediator.Object, _logger.Object);
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FlyingClubMemberController(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FlyingClubMemberController(_mediator.Object, null));
        }

        [Fact]
        public async Task FCMemberDetails_Success()
        {
            var req = SetupTestDataRequests("FlyingClubMember\\D4718738-BADA-E0ED-E053-6941D90AAD4B");
            _mediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<FlyingClubMemberResponse> { });
            var Output = _controller.FetchFlyingClubMemberDocumentByCustomerIdAsync(req.Object, "D4718738-BADA-E0ED-E053-6941D90AAD4B");
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

        [Fact]
        public async Task FCMemberDetailsBySSAMemberNumber_Success()
        {
            var req = SetupTestDataRequests("FlyingClubMember\\D4718738-BADA-E0ED-E053-6941D90AAD4B");
            _mediator.Setup(m => m.Send(It.IsAny<FCMDocBySSAMemberNumberQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<FlyingClubMemberResponse> { });
            var Output = _controller.FCMDocBySSAMemberNumberAsync(req.Object, "1058255355");
            Assert.Equal(System.Net.HttpStatusCode.Accepted, Output.Result.StatusCode);
        }
    }
}
