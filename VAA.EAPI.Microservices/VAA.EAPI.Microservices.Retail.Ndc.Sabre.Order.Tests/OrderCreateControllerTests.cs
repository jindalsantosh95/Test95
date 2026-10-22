using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Net;
using System.Text;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Application.Commands;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Application.Queries;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Triggers.Http;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Tests
{
    public class OrderCreateControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<FunctionContext> _context;
        private readonly OrderCreateController _controller;

        public OrderCreateControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _controller = new OrderCreateController(_mediator.Object);
            ConfigReader.Set("OrderCreateResponseUrl", "http://localhost:7071/order/response/{correlationid}");
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderCreateController(null));
        }

        [Fact]
        public void Controller_ShouldFail_WithBadRequest()
        {
            //Arrange
            var request = new Mock<HttpRequestData>(_context.Object);
            OrderCreateController controller = new(_mediator.Object);

            //Act
            var result = controller.OrderCreate(request.Object);

            //Asserts
            Assert.False(result.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task Controllertest_ForOrderCreate_ShouldSucceedWithGoodResponse()
        {
            //Arrange            
            string body = TestDataProvider.GetTestData<string>("NDCOrderMS\\OrderCreateRequest");
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
            _mediator.Setup(m => m.Send(It.IsAny<OrderCreateCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string>() { Status = Status.Accepted, HeaderParameters = new List<ResponseHeader> { new() { Key = Constants.Application.HeaderParameterlocation, Value = ConfigReader.Get(Constants.Environment.OrderCreateResponseUrl).Replace("{correlationid}", "Test123") } } });

            //Act
            var result = await _controller.OrderCreate(request.Object);

            //Assert
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);
            Assert.Equal("http://localhost:7071/order/response/Test123", result.Headers.GetValues("LOCATION").First());
        }

        [Fact]
        public async Task Controllertest_ForOrderResponse_ShouldSucceedWithGoodResponse()
        {
            //Arrange            
            var request = new Mock<HttpRequestData>(_context.Object);
            request.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });
            _mediator.Setup(m => m.Send(It.Is<OrderCreateResponseQuery>(x => x.CorrelationId == "ABC123"), CancellationToken.None)).ReturnsAsync(new Response<Domain.OrderDocument>() { Value = new Domain.OrderDocument() { Id = "ABC123", CreatedBy = "Test"}, Status = Status.OK});

            //Act
            var result = await _controller.OrderResponse(request.Object, "ABC123");

            //Assert
            var sr = new StreamReader(result.Body);
            sr.BaseStream.Position = 0;
            var resultBody = Json.Deserialize<OrderDocument>(sr.ReadToEnd());

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("ABC123",resultBody.Id);
            Assert.Equal("Test",resultBody.CreatedBy);
        }
    }
}