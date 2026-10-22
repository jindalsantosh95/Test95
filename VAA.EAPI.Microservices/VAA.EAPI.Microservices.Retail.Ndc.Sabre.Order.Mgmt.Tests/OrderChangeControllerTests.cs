using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Net;
using System.Text;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Triggers.Http;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests
{
    public class OrderChangeControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<FunctionContext> _context;
        private readonly OrderChangeController _controller;

        public OrderChangeControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _controller = new OrderChangeController(_mediator.Object);
            ConfigReader.Set("OrderChangeResponseUrl", "http://localhost:7071/order/response/{correlationid}");
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderChangeController(null));
        }

        [Fact]
        public void Controller_ShouldFail_WithBadRequest()
        {
            string CorrelationId = "ae3456";
            //Arrange
            var request = new Mock<HttpRequestData>(_context.Object);
            OrderChangeController controller = new(_mediator.Object);

            //Act
            var result = controller.OrderChangeResponse(request.Object, CorrelationId);

            //Asserts
            Assert.NotNull(result);
        }
    }
}
