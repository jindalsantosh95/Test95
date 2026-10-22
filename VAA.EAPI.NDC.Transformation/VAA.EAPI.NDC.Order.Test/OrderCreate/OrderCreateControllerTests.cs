using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using VAA.EAPI.NDC.Order.Application.OrderCreate;
using VAA.EAPI.NDC.Order.Triggers.Http;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using Xunit;

namespace VAA.EAPI.NDC.Order.Tests.OrderCreate
{
    public class OrderCreateControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly OrderCreateController _controller;
        private readonly Mock<FunctionContext> _context;
        private readonly Mock<IOrderCreateIFGCommand> _orderCreateIFGCommand;
        private readonly Mock<IOrderCreateEasyPayCommand> _orderCreateEasyPayCommand;

        public OrderCreateControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _orderCreateIFGCommand = new Mock<IOrderCreateIFGCommand>();
            _orderCreateEasyPayCommand = new Mock<IOrderCreateEasyPayCommand>();
            _controller = new OrderCreateController(_mediator.Object, _orderCreateIFGCommand.Object, _orderCreateEasyPayCommand.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new OrderCreateController(null, _orderCreateIFGCommand.Object, _orderCreateEasyPayCommand.Object));
        }

        [Fact]
        public async Task OrderCreate_ControllerShouldReturnSuccess()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOrder\\OrderCreate\\OrderCreateRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderCreateCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<Domain.OrderViewRS.IATA_OrderViewRS>() { Status = EAPI.Domain.Entities.Common.Status.OK });

            //Act
            var result = await _controller.OrderCreate(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task OrderCreate_ControllerShouldReturn422()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOrder\\OrderCreate\\OrderCreateRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderCreateCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<Domain.OrderViewRS.IATA_OrderViewRS>() { Status = EAPI.Domain.Entities.Common.Status.BusinessException });

            //Act
            var result = await _controller.OrderCreate(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.UnprocessableEntity, result.StatusCode);
        }

        [Fact]
        public async Task OrderCreate_ControllerShouldReturn401()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOrder\\OrderCreate\\OrderCreateRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderCreateCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<Domain.OrderViewRS.IATA_OrderViewRS>() { Status = EAPI.Domain.Entities.Common.Status.Unauthorized });

            //Act
            var result = await _controller.OrderCreate(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task OrderCreate_ControllerShouldReturn400()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOrder\\OrderCreate\\OrderCreateRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderCreateCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<Domain.OrderViewRS.IATA_OrderViewRS>() { Status = EAPI.Domain.Entities.Common.Status.BadRequest });

            //Act
            var result = await _controller.OrderCreate(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task OrderCreate_ControllerShouldReturn500()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOrder\\OrderCreate\\OrderCreateRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderCreateCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<Domain.OrderViewRS.IATA_OrderViewRS>() { Status = EAPI.Domain.Entities.Common.Status.Error });

            //Act
            var result = await _controller.OrderCreate(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, result.StatusCode);
        }

    }
}
