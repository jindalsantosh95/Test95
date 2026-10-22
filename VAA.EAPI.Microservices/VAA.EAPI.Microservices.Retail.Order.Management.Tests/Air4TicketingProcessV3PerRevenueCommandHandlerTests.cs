using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Tests
{
    public class Air4TicketingProcessV3PerRevenueCommandHandlerTests
    {
        private readonly Mock<IAir4TicketingProcessV3> _iAir4TicketingProcessV3;
        private readonly Mock<ILogger<Air4TicketingProcessV3PerRevenueCommandHandler>> _mockLogger;
        private readonly Air4TicketingProcessV3PerRevenueCommandHandler _handler;
        public Air4TicketingProcessV3PerRevenueCommandHandlerTests()
        {
            _iAir4TicketingProcessV3 = new Mock<IAir4TicketingProcessV3>();
            _mockLogger = new Mock<ILogger<Air4TicketingProcessV3PerRevenueCommandHandler>>();
            _handler = new Air4TicketingProcessV3PerRevenueCommandHandler(_iAir4TicketingProcessV3.Object, _mockLogger.Object);
            OrderManagementTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task Handler_ShouldThrowExceptionWhenExceptionOccurs()
        {
            //Arrange
            _iAir4TicketingProcessV3.Setup(r => r.PerformRevenue(It.IsAny<PerformRevenueRequestType>())).Throws<ArgumentNullException>();

            //Act
            async Task result() => await _handler.Handle(new Air4TicketingProcessV3PerRevenueCommand(), new System.Threading.CancellationToken());

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(result);
        }

        [Fact]
        public async Task Handler_ShouldReturnSuccessWhenNoExceptionOccurs()
        {
            //Arrange
            var response = new PerformRevenueResponseType() { Pnr = new PnrType() { AdvisoryMessage = new AdvisoryMessageType[1] } };
            var channelEntityData = TestDataProvider.GetTestData<Channel>("ChannelInfo");
            _iAir4TicketingProcessV3.Setup(r => r.PerformRevenue(It.IsAny<PerformRevenueRequestType>()).Result).Returns(response);

            //Act
            var result = await _handler.Handle(new Air4TicketingProcessV3PerRevenueCommand() { ChannelInfo = channelEntityData }, new System.Threading.CancellationToken());

            //Assert
            Assert.Single(result.Pnr.AdvisoryMessage);

        }

        [Fact]
        public async Task Handler_ShouldThrowExternalExceptionWhenExceptionOccurs()
        {
            //Arrange
            _iAir4TicketingProcessV3.Setup(r => r.PerformRevenue(It.IsAny<PerformRevenueRequestType>())).Throws<ExternalServiceIntegrationException>();
            var channelEntityData = TestDataProvider.GetTestData<Channel>("ChannelInfo");
            //Act
            async Task result() => await _handler.Handle(new Air4TicketingProcessV3PerRevenueCommand() { ChannelInfo = channelEntityData }, new System.Threading.CancellationToken());
            //Assert
            await Assert.ThrowsAsync<ExternalServiceIntegrationException>(result);
        }
    }
}
