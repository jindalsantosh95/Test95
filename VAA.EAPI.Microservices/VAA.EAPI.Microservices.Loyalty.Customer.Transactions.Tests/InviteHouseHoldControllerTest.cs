using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Framework.Tests;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.InviteHouseHold;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.InviteHouseHold;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class InviteHouseHoldControllerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<FunctionContext> _context;
        private readonly InviteHouseHoldController _controller;

        public InviteHouseHoldControllerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _controller = new InviteHouseHoldController(_mockMediator.Object);
            ConfigReader.Set("InviteHouseHold", "InviteHouseHold");
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new InviteHouseHoldController(null));
        }

        [Fact]
        public async Task GetHouseHoldInviterAsync_Success()
        {
            // Arrange
            var inviteDoc = TestDataProvider.GetTestData<IEnumerable<object>>("CustomerTransactionMS\\InviteControllerResponseDoc");
            var response = new Response<InviteHouseHoldResponse>
            {
                Status = Status.OK,
                Value = new InviteHouseHoldResponse(null, inviteDoc)
            };
            _mockMediator.Setup(m => m.Send(It.Is<GetHouseHoldInviterQuery>(item => item.InviterMemberNumber == "9106860647"), CancellationToken.None)).ReturnsAsync(response);

            var req = MockHttpRequestData.Create(new Mock<FunctionContext>().Object);

            // Act
            InviteHouseHoldController controller = new(_mockMediator.Object);
            var result = await controller.GetHouseHoldInviterAsync(req.Object, "9106860647");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            // Read in the body
            result.Body.Seek(0, SeekOrigin.Begin);
            StreamReader rdr = new(result.Body);
            var body = rdr.ReadToEnd();
            result.Body.Close();

            Assert.Contains("9107051782", body);
            Assert.Contains("9106860647", body);
            Assert.Contains("Pending Invite HouseHold", body);
        }

        [Fact]
        public async Task GetHouseHoldInviteeAsync_Success()
        {
            // Arrange
            var inviteDoc = TestDataProvider.GetTestData<IEnumerable<object>>("CustomerTransactionMS\\InviteControllerResponseDoc");
            var response = new Response<InviteHouseHoldResponse>
            {
                Status = Status.OK,
                Value = new InviteHouseHoldResponse(null, inviteDoc)
            };
            _mockMediator.Setup(m => m.Send(It.Is<GetHouseHoldInviteeQuery>(item => item.InviteeMemberNumber == "9107051782"), CancellationToken.None)).ReturnsAsync(response);

            var req = MockHttpRequestData.Create(new Mock<FunctionContext>().Object);

            // Act
            InviteHouseHoldController controller = new(_mockMediator.Object);
            var result = await controller.GetHouseHoldInviteeAsync(req.Object, "9107051782");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            // Read in the body
            result.Body.Seek(0, SeekOrigin.Begin);
            StreamReader rdr = new(result.Body);
            var body = rdr.ReadToEnd();
            result.Body.Close();

            Assert.Contains("9107051782", body);
            Assert.Contains("9106860647", body);
            Assert.Contains("Pending Invite HouseHold", body);
        }

        [Fact]
        public async Task GetHouseHoldInviterAsync_Failure()
        {
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _controller.GetHouseHoldInviterAsync(SetupTestDataRequests(string.Empty).Object, null));
            Assert.Equal("inviterMemberNumber", exception.ParamName);
        }

        [Fact]
        public async Task GetHouseHoldInviteeAsync_Failure()
        {
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _controller.GetHouseHoldInviteeAsync(SetupTestDataRequests(string.Empty).Object, null));
            Assert.Equal("inviteeMemberNumber", exception.ParamName);
        }


        private Mock<HttpRequestData> SetupTestDataRequests(string requestBody)
        {
            string body = requestBody;
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
