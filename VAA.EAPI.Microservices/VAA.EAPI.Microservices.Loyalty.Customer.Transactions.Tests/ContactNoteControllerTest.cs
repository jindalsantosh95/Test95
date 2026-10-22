using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
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
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.ContactNotes;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.ContactNotes;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ContactNotes;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class ContactNoteControllerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ContactNotesController> _handler;
        public ContactNoteControllerTest()
        {
            _mediator = new Mock<IMediator>();
            CampaignTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ContactNotesController(null));
        }

        [Fact]
        public async Task GetContactNotesByCustomerID_ShouldSucceed_WithGoodCustID()
        {
            // Arrange
            string custID = TestDataProvider.GetTestData<string>("CustID");
            var dataResponse = TestDataProvider.GetTestData<CSharpFunctionalExtensions.Result<Response<ContactNotesResponse>>>("ContactNotes.Good.ContactNotesResponse");
            _mediator.Setup(m => m.Send(It.IsAny<ContactNotesQuery>(), CancellationToken.None)).ReturnsAsync(dataResponse);

            var req = MockHttpRequestData.Create(new Mock<FunctionContext>().Object);

            // Act
            ContactNotesController controller = new(_mediator.Object);
            var result = await controller.GetContactNotesByCustomerID(req.Object, custID);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            // Read in the body
            result.Body.Seek(0, SeekOrigin.Begin);
            StreamReader rdr = new StreamReader(result.Body);
            var body = rdr.ReadToEnd();
            result.Body.Close();

            var output = Json.Deserialize<ContactNotesResponse>(body);
            Assert.Equal(output.CustomerId, custID);
            Assert.Equal(2, output.ContactNotes.Count()); // Sample response has 2 notes
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
        public async Task CreateContactNote_ShouldReturnAccepted_GoodRequest()
        {
            //Arrange
            string custID = TestDataProvider.GetTestData<string>("CustID");
            var req = SetupChangeRequests("ContactNote-POST-Good");

            var mockResponse = Result.Success(new Response<ContactNoteCommandResponse>()
            {
                Status = Status.Accepted
            });
            _mediator.Setup(m => m.Send(It.IsAny<ContactNoteCreateCommand>(), CancellationToken.None)).ReturnsAsync(mockResponse);

            //Act
            ContactNotesController controller = new(_mediator.Object);
            var result = await controller.CreateContactNote(req.Object, custID);

            //Assert
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);

        }

        [Fact]
        public async Task DeletContactNote_ShouldReturnAccepted_GoodRequest()
        {
            //Arrange
            string custID = TestDataProvider.GetTestData<string>("CustID");
            var mockResponse = Result.Success(new Response<ContactNoteCommandResponse>()
            {
                Status = Status.Accepted
            });
            _mediator.Setup(m => m.Send(It.IsAny<ContactNoteDeleteCommand>(), CancellationToken.None)).ReturnsAsync(mockResponse);
            var req = SetupChangeRequests("ContactNote-DELETE");

            //Act
            ContactNotesController controller = new(_mediator.Object);
            var result = await controller.DeleteContactNote(req.Object, custID, "12345");

            //Assert
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);

        }

        [Fact]
        public async Task PatchContactNote_ShouldReturnAccepted_GoodRequest()
        {
            //Arrange
            string custID = TestDataProvider.GetTestData<string>("CustID");
            var mockResponse = Result.Success(new Response<ContactNoteCommandResponse>()
            {
                Status = Status.Accepted
            });
            _mediator.Setup(m => m.Send(It.IsAny<ContactNotePatchCommand>(), CancellationToken.None)).ReturnsAsync(mockResponse);
            var req = SetupChangeRequests("ContactNote-PATCH");

            //Act
            ContactNotesController controller = new(_mediator.Object);
            var result = await controller.PatchContactNote(req.Object, custID);

            //Assert
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);

        }
    }
}
