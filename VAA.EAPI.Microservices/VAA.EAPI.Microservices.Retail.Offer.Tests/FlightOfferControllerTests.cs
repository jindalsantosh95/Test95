using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Triggers.Http.Delta;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class FlightOfferControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly FlightOfferController _controller;
        private readonly Mock<FunctionContext> _context;

        public FlightOfferControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _controller = new FlightOfferController(_mediator.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new FlightOfferController(null));
        }

        [Fact]
        public void Controller_ShouldFail_WithBadRequest()
        {
            //Arrange
            var request = new Mock<HttpRequestData>(_context.Object);
            FlightOfferController controller = new FlightOfferController(_mediator.Object);

            //Act
            var result = controller.FlightOfferSearch(request.Object);

            //Asserts
            Assert.False(result.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task Controllertest_ForSpecificDateSearch()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("OfferMS\\SpecificDate_OfferMsRQ", _context);

            _mediator.Setup(m => m.Send(It.IsAny<FlightOfferQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<FlightOfferResponse>() { Status = Status.OK, Value = new FlightOfferResponse() });

            //Act
            var result = await _controller.FlightOfferSearch(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        }
    }
}
