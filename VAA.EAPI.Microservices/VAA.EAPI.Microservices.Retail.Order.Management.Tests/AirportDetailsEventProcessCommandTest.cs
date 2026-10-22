using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.AirportDetails;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.AirportDetails;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Tests
{
    public class AirportDetailsEventProcessCommandTest
    {

        private readonly Mock<ILogger<AirportDetailsEventProcessCommandhandler>> _mocklogger;
        private readonly Mock<TableStorageService<AirportLocation>> _tableStorageService;
        public AirportDetailsEventProcessCommandTest()
        {

            _mocklogger = new Mock<ILogger<AirportDetailsEventProcessCommandhandler>>();
            _tableStorageService = new Mock<TableStorageService<AirportLocation>>();
            OrderManagementTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AirportDetailsEventProcessCommandhandler(null, _tableStorageService.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenTableServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AirportDetailsEventProcessCommandhandler(_mocklogger.Object, null));
        }
    }
}
