using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Net;
using System.Text;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Application;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Triggers.Http;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Tests
{
    public class OrderChangeNotifControllerTests
    {
        private readonly Mock<IOrderChangeNotifCommand> _command;
        private readonly Mock<FunctionContext> _context;
        private readonly OrderChangeNotifController _controller;

        public OrderChangeNotifControllerTests()
        {
            _command = new Mock<IOrderChangeNotifCommand>();
            _context = new Mock<FunctionContext>();
            _controller = new OrderChangeNotifController(_command.Object);
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderChangeNotifController(null));
        }

        [Fact]
        public async Task Controller_ShouldFail_WithBadRequest()
        {
            //Arrange
            var request = new Mock<HttpRequestData>(_context.Object);
            OrderChangeNotifController controller = new(_command.Object);

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => controller.Send(request.Object));
        }

        [Fact]
        public async Task Controllertest_ShouldSucceedWithGoodResponse()
        {
            //Arrange            
            string body = TestDataProvider.GetTestData<string>("NDCOrderNotificationMS\\OrderNotificationRequest");
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

            _command.Setup(x => x.Handle(It.IsAny<OrderChangeNotifRequest>())).ReturnsAsync(new Response<OrderChangeNotifResponse>
            {
                Status = Status.OK,
                Value = new OrderChangeNotifResponse()
                {
                    AgentResponse =
                    [
                        new AgentResponse()
                        {
                            AgentId = "testAgent",
                            AcknowledgementRcvd = true
                        }
                    ]
                }
            });

            //Act
            var result = await _controller.Send(request.Object);

            //Assert
            var sr = new StreamReader(result.Body);
            sr.BaseStream.Position = 0;
            var resultBody = sr.ReadToEnd().ToString();

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var response = Json.Deserialize<OrderChangeNotifResponse>(resultBody);
            Assert.Single(response.AgentResponse);
            Assert.Equal("testAgent", response.AgentResponse.First().AgentId);
            Assert.True(response.AgentResponse.First().AcknowledgementRcvd);
        }
    }
}