using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Application.Query.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Triggers.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Tests
{
    public class OrderChangeControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<FunctionContext> _context;
        public OrderChangeControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            OrderManagementTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void TestController_Failure()
        {
            //Arrange
            var request = new Mock<HttpRequestData>(_context.Object); //Empty request
            OrderChangeController controller = new OrderChangeController(_mockMediator.Object);
            //Ac
            var result = controller.ServiceOrder(request.Object, string.Empty);
            //Assert
            Assert.False(result.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task TestController_Passed()
        {
            //Arrange
            OrderChangeController controller = new(_mockMediator.Object);
            string body = "{\"rloc\":\"abc\"}";
            var byteArray = Encoding.ASCII.GetBytes(body);
            var bodyStream = new MemoryStream(byteArray);
            var request = new Mock<HttpRequestData>(_context.Object);
            request.Setup(r => r.Body).Returns(bodyStream);
            request.Setup(r => r.Headers).Returns(new HttpHeadersCollection(new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("ChannelId", "VHOLS") }));
            request.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });
            _mockMediator.Setup(m => m.Send(It.IsAny<OrderChangeRequestCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string>() { Status = Status.Accepted });
            //Act
            var result = await controller.ServiceOrder(request.Object, string.Empty);
            //Assert
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);

        }

        [Fact]
        public async Task TestOrderChangeResponseController()
        {
            //Arrange
            OrderChangeController enrolmentController = new(_mockMediator.Object);
            _context.Setup(c => c.FunctionDefinition.PathToAssembly);
            var request = new Mock<HttpRequestData>(_context.Object);
            request.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                return response.Object;
            });
            _mockMediator.Setup(m => m.Send(It.IsAny<OrderChangeResponseQuery>(), new System.Threading.CancellationToken()).Result).Returns(new Response<OrderChangeResponse>() { Status = Status.Accepted });
            //Act
            HttpResponseData result = await enrolmentController.OrderChangeResponse(request.Object, "123");
            //Assert
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);
        }

    }
}
