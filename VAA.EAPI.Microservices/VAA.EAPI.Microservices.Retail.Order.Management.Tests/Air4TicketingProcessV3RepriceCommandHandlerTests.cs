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
    public class Air4TicketingProcessV3RepriceCommandHandlerTests
    {
        private readonly Mock<IAir4TicketingProcessV3> _iAir4TicketingProcessV3;
        private readonly Mock<ILogger<Air4TicketingProcessV3RepriceCommandHandler>> _mockLogger;
        private readonly Air4TicketingProcessV3RepriceCommandHandler _handler;
        public Air4TicketingProcessV3RepriceCommandHandlerTests()
        {
            _iAir4TicketingProcessV3 = new Mock<IAir4TicketingProcessV3>();
            _mockLogger = new Mock<ILogger<Air4TicketingProcessV3RepriceCommandHandler>>();
            _handler = new Air4TicketingProcessV3RepriceCommandHandler(_iAir4TicketingProcessV3.Object, _mockLogger.Object);
            OrderManagementTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task Handler_ShouldReturnSuccessWithNoException()
        {
            //Arrange
            var channelEntityData = TestDataProvider.GetTestData<Channel>("ChannelInfo");
            AdvisoryMessageType[] ad = new AdvisoryMessageType[] { new AdvisoryMessageType() { FreeformText = "abc" } };
            var response = new RepriceResponseType()
            {
                AdvisoryMessage = ad
            };
            _iAir4TicketingProcessV3.Setup(r => r.Reprice(It.IsAny<RepriceRequestType>()).Result).Returns(response);
            //Act
            var result = await _handler.Handle(new Air4TicketingProV3RepriceCommand() { ChannelInfo = channelEntityData, Trvallers = new Domain.OrderChange.Traveller() { } }, new System.Threading.CancellationToken());
            //Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task Handler_ShouldReturnFailureWithException()
        {
            //Arrange
            _iAir4TicketingProcessV3.Setup(r => r.Reprice(It.IsAny<RepriceRequestType>())).Throws<ArgumentNullException>();
            //Act
            async Task result() => await _handler.Handle(new Air4TicketingProV3RepriceCommand() { Trvallers = new Domain.OrderChange.Traveller() { } }, new System.Threading.CancellationToken());
            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(result);
        }

        [Fact]
        public async Task Handler_ShouldReturnFailureWithExternalException()
        {
            //Arrange
            var channelEntityData = TestDataProvider.GetTestData<Channel>("ChannelInfo");
            _iAir4TicketingProcessV3.Setup(r => r.Reprice(It.IsAny<RepriceRequestType>())).Throws<ExternalServiceIntegrationException>();
            //Act
            async Task result() => await _handler.Handle(new Air4TicketingProV3RepriceCommand() { ChannelInfo = channelEntityData, Trvallers = new Domain.OrderChange.Traveller() { } }, new System.Threading.CancellationToken());
            //Assert
            await Assert.ThrowsAsync<ExternalServiceIntegrationException>(result);
        }
    }
}
