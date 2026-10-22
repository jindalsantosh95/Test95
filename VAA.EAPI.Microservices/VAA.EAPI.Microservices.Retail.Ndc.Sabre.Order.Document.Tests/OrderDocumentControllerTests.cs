using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Net;
using System.Text;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Application.Commands;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Triggers.Http;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Tests
{
    public class OrderDocumentControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<FunctionContext> _context;
        private readonly OrderDocumentController _controller;

        public OrderDocumentControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _controller = new OrderDocumentController(_mediator.Object);
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderDocumentController(null));
        }

        [Fact]
        public async Task Controller_ShouldFail_WithBadRequest()
        {
            //Arrange
            var request = new Mock<HttpRequestData>(_context.Object);
            OrderDocumentController controller = new(_mediator.Object);

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => controller.OrderDocument(request.Object));
        }

        [Fact]
        public async Task Controllertest_ForOrderDocument_ShouldSucceedWithGoodResponse()
        {
            //Arrange            
            string body = TestDataProvider.GetTestData<string>("SabreOrderDocument\\IssueTicketRequest");
            var byteArray = Encoding.ASCII.GetBytes(body);
            var bodyStream = new MemoryStream(byteArray);
            var request = new Mock<HttpRequestData>(_context.Object);
            request.Setup(r => r.Body).Returns(bodyStream);
            request.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });
            _mediator.Setup(m => m.Send(It.IsAny<IssueTicketCommand>(), CancellationToken.None)).ReturnsAsync(new Response<IssueTicketResponse>() { Status = Status.OK });

            //Act
            var result = await _controller.OrderDocument(request.Object);

            //Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}
