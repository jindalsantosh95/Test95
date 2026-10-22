using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using VAA.EAPI.NDC.Order.Application.OrderRetrieve;
using VAA.EAPI.NDC.Order.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.OrderRetrieve
{
    public class OrderRetrieveControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly OrderRetrieveController _controller;
        private readonly Mock<FunctionContext> _context;

        public OrderRetrieveControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _controller = new OrderRetrieveController(_mediator.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderRetrieveController(null));
        }

        [Fact]
        public async Task OrderRetrieve_ControllerShouldReturnSuccess()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderRetrieveQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<Domain.OrderViewRS.IATA_OrderViewRS>() { Status = EAPI.Domain.Entities.Common.Status.OK });

            //Act
            var result = await _controller.OrderRetrieve(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task OrderRetrieve_ControllerShouldReturn422()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderRetrieveQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<Domain.OrderViewRS.IATA_OrderViewRS>() { Status = EAPI.Domain.Entities.Common.Status.BusinessException });

            //Act
            var result = await _controller.OrderRetrieve(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.UnprocessableEntity, result.StatusCode);
        }

        [Fact]
        public async Task OrderRetrieve_ControllerShouldReturn401()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderRetrieveQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<Domain.OrderViewRS.IATA_OrderViewRS>() { Status = EAPI.Domain.Entities.Common.Status.Unauthorized });

            //Act
            var result = await _controller.OrderRetrieve(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task OrderRetrieve_ControllerShouldReturn400()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderRetrieveQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<Domain.OrderViewRS.IATA_OrderViewRS>() { Status = EAPI.Domain.Entities.Common.Status.BadRequest });

            //Act
            var result = await _controller.OrderRetrieve(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task OrderRetrieve_ControllerShouldReturn500()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOrder\\OrderRetrieve\\OrderRetrieveRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderRetrieveQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<Domain.OrderViewRS.IATA_OrderViewRS>() { Status = EAPI.Domain.Entities.Common.Status.Error });

            //Act
            var result = await _controller.OrderRetrieve(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, result.StatusCode);
        }

    }
}


