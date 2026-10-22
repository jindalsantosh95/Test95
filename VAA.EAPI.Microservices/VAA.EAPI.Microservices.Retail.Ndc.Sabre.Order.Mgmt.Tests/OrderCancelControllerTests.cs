using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Triggers.Http;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests
{
    public class OrderCancelControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<FunctionContext> _context;
        private readonly OrderCancelController _controller;

        public OrderCancelControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _controller = new OrderCancelController(_mediator.Object);
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderCancelController(null));
        }

        [Fact]
        public async Task Controller_ShouldFail_WithBadRequest()
        {
            //Arrange
            var request = new Mock<HttpRequestData>(_context.Object);
            OrderCancelController controller = new(_mediator.Object);

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => controller.OrderCancel(request.Object));
        }
    }
}
