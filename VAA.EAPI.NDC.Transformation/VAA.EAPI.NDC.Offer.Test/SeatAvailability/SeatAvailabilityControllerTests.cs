using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.NDC.Offer.Application.Query.SeatAvailability;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRS;
using VAA.EAPI.NDC.Offer.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.SeatAvailability
{
    public class SeatAvailabilityControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly SeatAvailabilityController _controller;
        private readonly Mock<FunctionContext> _context;

        public SeatAvailabilityControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _controller = new SeatAvailabilityController(_mediator.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SeatAvailabilityController(null));
        }

        [Fact]
        public async Task SeatAvailability_ControllerShouldReturnSuccess()
        {
            //Arrange
            var seatResponseData = TestDataProvider.GetXmlTestData("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRS");

            var request = TestDataProvider.GetMockHttpRequestWithMockResponse("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ", _context, seatResponseData, true);

            _mediator.Setup(x => x.Send(It.IsAny<SeatAvailabilityQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IATA_SeatAvailabilityRS>() { Status = EAPI.Domain.Entities.Common.Status.OK });

            //Act
            var result = await _controller.SeatAvailability(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);

            var seatReader = new StreamReader(result.Body);
            var seatResponse = Xml.Deserialize<IATA_SeatAvailabilityRS>(await seatReader.ReadToEndAsync());

            Assert.True(seatResponse.Items.Any());
        }

        [Fact]
        public async Task SeatAvailability_ControllerShouldReturn422()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<SeatAvailabilityQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IATA_SeatAvailabilityRS>() { Status = EAPI.Domain.Entities.Common.Status.BusinessException });

            //Act
            var result = await _controller.SeatAvailability(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.UnprocessableEntity, result.StatusCode);
        }

        [Fact]
        public async Task SeatAvailability_ControllerShouldReturn401()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<SeatAvailabilityQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IATA_SeatAvailabilityRS>() { Status = EAPI.Domain.Entities.Common.Status.Unauthorized });

            //Act
            var result = await _controller.SeatAvailability(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task SeatAvailability_ControllerShouldReturn400()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<SeatAvailabilityQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IATA_SeatAvailabilityRS>() { Status = EAPI.Domain.Entities.Common.Status.BadRequest });

            //Act
            var result = await _controller.SeatAvailability(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task SeatAvailability_ControllerShouldReturn500()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<SeatAvailabilityQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IATA_SeatAvailabilityRS>() { Status = EAPI.Domain.Entities.Common.Status.Error });

            //Act
            var result = await _controller.SeatAvailability(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, result.StatusCode);
        }
    }
}
