using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Framework.Tests;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.TargetedMemberships;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.TargetedMembership;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.TargetedMembership;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class TargetedMembershipControllerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<TargetedMembershipController>> _logger;
        private readonly Mock<TargetedMembershipController> _handler;
        public TargetedMembershipControllerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<TargetedMembershipController>>();
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TargetedMembershipController(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TargetedMembershipController(_mediator.Object, null));
        }

        [Fact]
        public async Task GetTargetedMembershipByCustomerID_ShouldSucceed_WithGoodCustID()
        {
            // Arrange
            string custID = TestDataProvider.GetTestData<string>("CustID");
            var dataResponse = TestDataProvider.GetTestData<CSharpFunctionalExtensions.Result<Response<TargetedMembershipResponse>>>("TargetedMembership\\TargetedMembership.Good.TargetedMembershipResponse");
            _mediator.Setup(m => m.Send(It.IsAny<TargetedMembershipQuery>(), CancellationToken.None)).ReturnsAsync(dataResponse);

            var req = MockHttpRequestData.Create(new Mock<FunctionContext>().Object);

            // Act
            TargetedMembershipController controller = new(_mediator.Object, _logger.Object);
            var result = await controller.GetTargetedMembershipByCustomerID(req.Object, custID);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            // Read in the body
            result.Body.Seek(0, SeekOrigin.Begin);
            StreamReader rdr = new(result.Body);
            var body = rdr.ReadToEnd();
            result.Body.Close();

            var output = Json.Deserialize<TargetedMembershipResponse>(body);
            Assert.Equal(output.CustomerId, custID);
            Assert.Equal(2, output.TargetedMembershipDocuments.Count()); // Sample response has 2 notes
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
        public async Task CreateTargetedMembership_ShouldReturnAccepted_GoodRequest()
        {
            //Arrange
            string custID = TestDataProvider.GetTestData<string>("CustID");
            var req = SetupChangeRequests("TargetedMembership\\TargetedMembership-POST-Good");

            var mockResponse = Result.Success(new Response<TargetedMembershipCommandResponse>()
            {
                Status = Status.Accepted
            });
            _mediator.Setup(m => m.Send(It.IsAny<TargetedMembershipCreateCommand>(), CancellationToken.None)).ReturnsAsync(mockResponse);

            //Act
            TargetedMembershipController controller = new(_mediator.Object, _logger.Object);
            var result = await controller.CreateTargetedMembership(req.Object, custID);

            //Assert
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);

        }

        [Fact]
        public async Task DeletTargetedMembership_ShouldReturnAccepted_GoodRequest()
        {
            //Arrange
            string custID = TestDataProvider.GetTestData<string>("CustID");
            var mockResponse = Result.Success(new Response<TargetedMembershipCommandResponse>()
            {
                Status = Status.Accepted
            });
            _mediator.Setup(m => m.Send(It.IsAny<TargetedMembershipDeleteCommand>(), CancellationToken.None)).ReturnsAsync(mockResponse);
            var req = SetupChangeRequests("TargetedMembership\\TargetedMembership-DELETE");

            //Act
            TargetedMembershipController controller = new(_mediator.Object, _logger.Object);
            var result = await controller.DeleteTargetedMembership(req.Object, custID);

            //Assert
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);

        }

        [Fact]
        public async Task PatchTargetedMembership_ShouldReturnAccepted_GoodRequest()
        {
            //Arrange
            string custID = TestDataProvider.GetTestData<string>("CustID");
            var mockResponse = Result.Success(new Response<TargetedMembershipCommandResponse>()
            {
                Status = Status.Accepted
            });
            _mediator.Setup(m => m.Send(It.IsAny<TargetedMembershipPatchCommand>(), CancellationToken.None)).ReturnsAsync(mockResponse);
            var req = SetupChangeRequests("TargetedMembership\\TargetedMembership-PATCH");

            //Act
            TargetedMembershipController controller = new(_mediator.Object, _logger.Object);
            var result = await controller.PatchTargetedMembership(req.Object, custID);

            //Assert
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);

        }
    }
}
