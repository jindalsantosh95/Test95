using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using VAA.EAPI.NDC.Offer.Application.Query.OrderReshop;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OrderReshop
{
    public class OrderReshopControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly OrderReshopController _controller;
        private readonly Mock<FunctionContext> _context;

        public OrderReshopControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _controller = new OrderReshopController(_mediator.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderReshopController(null));
        }

        [Fact]
        public async Task OrderOrderReshop_ControllerShouldReturnSuccess()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOffer\\OrderReshop\\OrderReshopRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderReshopQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IATA_OrderReshopRS>() { Status = EAPI.Domain.Entities.Common.Status.OK });

            //Act
            var result = await _controller.OrderReshop(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);

        }

        [Fact]
        public async Task OrderReshop_ControllerShouldReturn422()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOffer\\OrderReshop\\OrderReshopRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderReshopQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IATA_OrderReshopRS>() { Status = EAPI.Domain.Entities.Common.Status.BusinessException });

            //Act
            var result = await _controller.OrderReshop(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.UnprocessableEntity, result.StatusCode);
        }

        [Fact]
        public async Task OrderReshop_ControllerShouldReturn401()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOffer\\OrderReshop\\OrderReshopRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderReshopQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IATA_OrderReshopRS>() { Status = EAPI.Domain.Entities.Common.Status.Unauthorized });

            //Act
            var result = await _controller.OrderReshop(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task OrderReshop_ControllerShouldReturn400()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOffer\\OrderReshop\\OrderReshopRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderReshopQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IATA_OrderReshopRS>() { Status = EAPI.Domain.Entities.Common.Status.BadRequest });

            //Act
            var result = await _controller.OrderReshop(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task OrderReshop_ControllerShouldReturn500()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOffer\\OrderReshop\\OrderReshopRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderReshopQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IATA_OrderReshopRS>() { Status = EAPI.Domain.Entities.Common.Status.Error });

            //Act
            var result = await _controller.OrderReshop(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task OrderCancel_CheckIsItValidCancelOrder()       {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderReshopRQ>("NDCOffer\\OrderCancel\\OrderCancelRQ");

            //Act
            var result = ((request.Request.UpdateOrder?.Item) as CancelOrderRefType);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.OrderRefID);
            Assert.Equal("VS932EXB8JI24", result.OrderRefID);
        }
    }
}
