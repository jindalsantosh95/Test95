using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.FlightInformation.Application.Command.AirportDetails;
using VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Entities;
using Xunit;

namespace VAA.EAPI.Microservices.FlightInformation.Tests
{
    public class AirportDetailsMergeCommandHandlerTests
    {
        private readonly Mock<ILogger<AirportDetailsMergeCommandHandler>> _mockLogger;
        private readonly Mock<TableStorageService<AirportDatabaseEntity>> _mockService;
        private readonly AirportDetailsMergeCommandHandler _handler;
        public AirportDetailsMergeCommandHandlerTests()
        {
            _mockService = new Mock<TableStorageService<AirportDatabaseEntity>>();
            _mockLogger = new Mock<ILogger<AirportDetailsMergeCommandHandler>>();
            _handler = new AirportDetailsMergeCommandHandler(_mockLogger.Object, _mockService.Object);
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AirportDetailsMergeCommandHandler(null, _mockService.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AirportDetailsMergeCommandHandler(_mockLogger.Object, null));
        }
        [Fact]
        public async Task Handler_ShouldThrowNullExceptionWhenRequestIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            new AirportDetailsMergeCommandHandler(_mockLogger.Object, _mockService.Object).Handle(null, System.Threading.CancellationToken.None));
        }
    }
}
