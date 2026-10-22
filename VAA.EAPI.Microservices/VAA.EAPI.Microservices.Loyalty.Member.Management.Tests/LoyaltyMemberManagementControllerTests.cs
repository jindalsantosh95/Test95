using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Text;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PromotionEnrolledMembers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class LoyaltyMemberManagementControllerTests
    {
        private readonly Mock<ILogger<LoyaltyMemberManagementController>> _mockLogger;
        private readonly Mock<IMediator> _mockMediator;
        private readonly LoyaltyMemberManagementController _controller;
        private readonly Mock<FunctionContext> _context;

        public LoyaltyMemberManagementControllerTests()
        {
            _mockLogger = new Mock<ILogger<LoyaltyMemberManagementController>>();
            _mockMediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _controller = new LoyaltyMemberManagementController(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new LoyaltyMemberManagementController(_mockMediator.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new LoyaltyMemberManagementController(null, _mockLogger.Object));
        }

        [Fact]
        public void LoyaltyMemberManagementSuccsess()
        {
            var req = SetupTestDataRequests("MemberManagement\\RetrieveMemberAccountDetailsRequest");
            _mockMediator.Setup(m => m.Send(It.IsAny<MemberAccountDetailsQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<MemberAccountResponse> { });
            var Output = _controller.GetLoyaltyMember(req.Object, "c220a7fb-8b85-4f4f-bac8-db94252a3e45");
            Assert.Equal(System.Net.HttpStatusCode.Accepted, Output.Result.StatusCode);
        }
        [Fact]
        public void PromotionEnrolledMembersSuccsess()
        {
            var req = SetupTestDataRequests("MemberManagement\\EnrolledMemberForPromotionRequest");
            _mockMediator.Setup(m => m.Send(It.IsAny<PromotionEnrolledMemberDetailsQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<PromotionEnrolledMemberDetailsResponse> { });
            var Output = _controller.PromotionEnrolledMembers(req.Object, "1-9M3EFT");
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
        private Mock<HttpRequestData> SetupTestDataRequestsEnroll()
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
