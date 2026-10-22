using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using VAA.EAPI.NDC.Offer.Application.Query.OfferPrice;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRS;
using VAA.EAPI.NDC.Offer.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OfferPrice
{
    public class OfferPriceControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly OfferPriceController _controller;
        private readonly Mock<FunctionContext> _context;

        public OfferPriceControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _controller = new OfferPriceController(_mediator.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OfferPriceController(null));
        }

        [Fact]
        public async Task OfferPrice_ControllerShouldReturnSuccess()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOffer\\OfferPrice\\OfferPriceRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OfferPriceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IATA_OfferPriceRS>() { Status = EAPI.Domain.Entities.Common.Status.OK });

            //Act
            var result = await _controller.OfferPrice(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task OfferPrice_ControllerShouldReturn422()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOffer\\OfferPrice\\OfferPriceRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OfferPriceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IATA_OfferPriceRS>() { Status = EAPI.Domain.Entities.Common.Status.BusinessException });

            //Act
            var result = await _controller.OfferPrice(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.UnprocessableEntity, result.StatusCode);
        }

        [Fact]
        public async Task OfferPrice_ControllerShouldReturn401()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOffer\\OfferPrice\\OfferPriceRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OfferPriceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IATA_OfferPriceRS>() { Status = EAPI.Domain.Entities.Common.Status.Unauthorized });

            //Act
            var result = await _controller.OfferPrice(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task OfferPrice_ControllerShouldReturn400()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOffer\\OfferPrice\\OfferPriceRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OfferPriceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IATA_OfferPriceRS>() { Status = EAPI.Domain.Entities.Common.Status.BadRequest });

            //Act
            var result = await _controller.OfferPrice(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task OfferPrice_ControllerShouldReturn500()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOffer\\OfferPrice\\OfferPriceRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OfferPriceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IATA_OfferPriceRS>() { Status = EAPI.Domain.Entities.Common.Status.Error });

            //Act
            var result = await _controller.OfferPrice(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, result.StatusCode);
        }
    }
}
