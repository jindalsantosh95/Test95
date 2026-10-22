using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Repositories.AirportDetailsRepo;
using VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Services.CommonRefDb.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.FlightInformation.Tests
{
    public class AirportDetailsRepoCommandHandlerTests
    {
        private readonly Mock<ILogger<AirportDetailsRepoCommandHandler>> _mockLogger;
        private readonly Mock<ICommonRefDBService> _mockService;
        private readonly AirportDetailsRepoCommandHandler _handler;


        public AirportDetailsRepoCommandHandlerTests()
        {

            _mockService = new Mock<ICommonRefDBService>();
            _mockLogger = new Mock<ILogger<AirportDetailsRepoCommandHandler>>();
            _handler = new AirportDetailsRepoCommandHandler(_mockService.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AirportDetailsRepoCommandHandler(_mockService.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AirportDetailsRepoCommandHandler(null, _mockLogger.Object));
        }
    }
}


