using CSharpFunctionalExtensions;
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
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.PartnerMembership;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.PartnerMembership;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.PartnerMembership;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class PartnerMembershipControllerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<FunctionContext> _context;
        private readonly PartnerMembershipController _controller;

        public PartnerMembershipControllerTest()
        {
            _mediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _controller = new PartnerMembershipController(_mediator.Object);
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PartnerMembershipController(null));
        }

        [Fact]
        public async Task GetPartnerMembershipByCustomerId_Success()
        {
            var req = SetupTestDataRequests("PartnerMembership\\PartnerMembershipResponse");
            _mediator.Setup(m => m.Send(It.IsAny<PartnerMembershipQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<PartnerMembershipResponse> { });
            var Output = _controller.GetPartnerMembershipByCustomerId(req.Object, "D971F4AE-4801-84D1-E053-6941D90A4EF2");
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
        public async Task CreatePartnerMembership_ShouldReturnAccepted_GoodRequest()
        {
            //Arrange
            string custID = TestDataProvider.GetTestData<string>("CustID");
            var req = SetupChangeRequests("PartnerMembership\\PartnerMembershipCreate-POST-Good");

            var mockResponse = Result.Success(new Response<PartnerMembershipCommandResponse>()
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new() { Key = Constants.Application.HeaderParameterlocation, Value = "http://localhost:7075/api/PartnerMemberships/status/f7b3c8d5-4075-45c1-979a-e799ecfdadef" } },
            });

            _mediator.Setup(m => m.Send(It.IsAny<PartnerMembershipCreateCommand>(), CancellationToken.None)).ReturnsAsync(mockResponse);

            //Act
            PartnerMembershipController controller = new(_mediator.Object);
            var result = await controller.CreatePartnerMembership(req.Object, custID);

            //Assert
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);
            Assert.True(result.Headers.Contains(Constants.Application.HeaderParameterlocation));
        }

        private Mock<HttpRequestData> SetupChangeRequests(string requestBodySampleFile)
        {
            //Arrange
            string body = TestDataProvider.GetTestData<string>(requestBodySampleFile);
            var byteArray = Encoding.ASCII.GetBytes(body);
            var bodyStream = new MemoryStream(byteArray);

            Mock<FunctionContext> context = new();

            context.Setup(c => c.Items).Returns(() =>
            {
                var items = new Dictionary<object, object>();
                items.Add("ClaimsPrincipal", TestUser());
                return items;
            });

            var req = new Mock<HttpRequestData>(context.Object);
            req.Setup(m => m.Body).Returns(bodyStream);

            req.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });

            return req;
        }

        [Fact]
        public async Task PatchPartnerMembership_ShouldReturnAccepted_GoodRequest()
        {
            //Arrange
            string custID = TestDataProvider.GetTestData<string>("CustID");
            var req = SetupChangeRequests("PartnerMembership\\PartnerMembershipUpdate-Patch-Good");

            var mockResponse = Result.Success(new Response<PartnerMembershipCommandResponse>()
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new() { Key = Constants.Application.HeaderParameterlocation, Value = "http://localhost:7075/api/PartnerMemberships/status/f7b3c8d5-4075-45c1-979a-e799ecfdadef" } },
            });

            _mediator.Setup(m => m.Send(It.IsAny<PartnerMembershipPatchCommand>(), CancellationToken.None)).ReturnsAsync(mockResponse);

            //Act
            PartnerMembershipController controller = new(_mediator.Object);
            var result = await controller.PatchPartnerMembership(req.Object, custID);

            //Assert
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);
            Assert.True(result.Headers.Contains(Constants.Application.HeaderParameterlocation));
        }

    }
}
