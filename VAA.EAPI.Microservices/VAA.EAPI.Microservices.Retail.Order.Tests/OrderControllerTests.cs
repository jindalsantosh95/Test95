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
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Application.Commands.OrderCreate;
using VAA.EAPI.Microservices.Retail.Order.Application.Query.OrderCreate;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;
using VAA.EAPI.Microservices.Retail.Order.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Tests
{
    public class OrderControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly OrderController _controller;
        private readonly Mock<FunctionContext> _context;

        public OrderControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _controller = new OrderController(_mediator.Object);
            _context = new Mock<FunctionContext>();
            OrderTestsCommon.CreateTestEnvironmentSettings();
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderController(null));
        }

        [Fact]
        public void Controller_ShouldFail_WithBadRequest()
        {
            //Arrange
            var request = new Mock<HttpRequestData>(_context.Object);
            OrderController controller = new OrderController(_mediator.Object);

            //Act
            var result = controller.Order(request.Object);

            //Asserts
            Assert.False(result.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task Controllertest_ForCreateOrder_ShouldSucceedWithGoodResponse()
        {
            //Arrange            
            string body = TestDataProvider.GetTestData<string>("OrderMS\\OrderMsRequest");
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
            _mediator.Setup(m => m.Send(It.IsAny<OrderCreateCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string>() { Status = Status.Accepted, HeaderParameters = new List<ResponseHeader> { new ResponseHeader { Key = Constants.Application.HeaaderParameterlocation, Value = ConfigReader.Get(Constants.Environment.OrderCreateResponseUrl).Replace("{correlationid}", "Test123") } } });

            //Act
            var result = await _controller.Order(request.Object);

            //Assert
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);
            Assert.Equal("http://localhost:7071/order/response/Test123", result.Headers.GetValues("LOCATION").First());
        }
        [Fact]
        public async Task TestOrderCreateResponseController()
        {
            //Arrange

            _context.Setup(c => c.FunctionDefinition.PathToAssembly);
            var request = new Mock<HttpRequestData>(_context.Object);
            request.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                return response.Object;
            });
            _mediator.Setup(m => m.Send(It.IsAny<OrderCreateResponseQuery>(), new System.Threading.CancellationToken()).Result).Returns(new Response<OrderResponse>() { Status = Status.Accepted });
            //Act
            HttpResponseData result = await _controller.OrderResponse(request.Object, "123");
            //Assert
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);
        }
    }
}
