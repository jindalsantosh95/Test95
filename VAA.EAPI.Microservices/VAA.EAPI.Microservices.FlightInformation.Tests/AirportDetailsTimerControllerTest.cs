using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Repositories.AirportDetailsRepo;
using VAA.EAPI.Microservices.FlightInformation.Triggers.Timer;
using Xunit;

namespace VAA.EAPI.Microservices.FlightInformation.Tests
{
    public class AirportDetailsTimerControllerTest
    {
        private readonly Mock<ILogger<AirportDetailsTimerController>> _mockLogger;
        private readonly Mock<IMediator> _mediator;
        private readonly AirportDetailsRepoCommandHandler _handler;


        public AirportDetailsTimerControllerTest()
        {

            _mediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<AirportDetailsTimerController>>();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AirportDetailsTimerController(_mediator.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AirportDetailsTimerController(null, _mockLogger.Object));
        }
    }
}
