using MediatR;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.FlightInformation.Application.Query.Airportdetails;
using Xunit;

namespace VAA.EAPI.Microservices.FlightInformation.Tests
{
    public class AirportDetailsQueryHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        public AirportDetailsQueryHandlerTests()
        {
            _mediator = new Mock<IMediator>();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AirportDetailsQueryHandler(null));
        }
        [Fact]
        public async Task Handler_ShouldThrowNullExceptionWhenRequestIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            new AirportDetailsQueryHandler(_mediator.Object).Handle(null, System.Threading.CancellationToken.None));
        }
    }
}
