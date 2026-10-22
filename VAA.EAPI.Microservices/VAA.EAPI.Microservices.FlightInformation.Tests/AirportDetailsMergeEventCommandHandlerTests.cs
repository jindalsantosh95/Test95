using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.FlightInformation.Application.Command.Airportdetails;
using Xunit;

namespace VAA.EAPI.Microservices.FlightInformation.Tests
{
    public class AirportDetailsMergeEventCommandHandlerTests
    {
        private readonly Mock<ILogger<AirportDetailsMergeEventCommandHandler>> _mockLogger;
        private readonly Mock<IMediator> _mediator;
        private readonly AirportDetailsMergeEventCommandHandler _handler;
        public AirportDetailsMergeEventCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<AirportDetailsMergeEventCommandHandler>>();
            _handler = new AirportDetailsMergeEventCommandHandler(_mediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AirportDetailsMergeEventCommandHandler(_mediator.Object, null));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AirportDetailsMergeEventCommandHandler(null, _mockLogger.Object));
        }
        [Fact]
        public async Task Handler_ShouldThrowNullExceptionWhenRequestIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            new AirportDetailsMergeEventCommandHandler(_mediator.Object, _mockLogger.Object).Handle(null, System.Threading.CancellationToken.None));
        }
    }
}
