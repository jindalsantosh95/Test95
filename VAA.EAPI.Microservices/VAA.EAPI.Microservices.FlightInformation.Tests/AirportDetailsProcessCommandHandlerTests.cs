using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.FlightInformation.Application.Command.Airportdetails;
using Xunit;

namespace VAA.EAPI.Microservices.FlightInformation.Tests
{
    public class AirportDetailsProcessCommandHandlerTests
    {
        private readonly Mock<ILogger<AirportDetailsProcessCommandHandler>> _mockLogger;
        private readonly Mock<IMediator> _mediator;
        private readonly AirportDetailsProcessCommandHandler _handler;
        public AirportDetailsProcessCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<AirportDetailsProcessCommandHandler>>();
            _handler = new AirportDetailsProcessCommandHandler(_mediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AirportDetailsProcessCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AirportDetailsProcessCommandHandler(null, _mockLogger.Object));
        }
    }
}


