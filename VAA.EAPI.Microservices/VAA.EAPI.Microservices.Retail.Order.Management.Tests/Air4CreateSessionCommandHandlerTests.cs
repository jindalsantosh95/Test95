using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Tests
{
    public class Air4CreateSessionCommandHandlerTests
    {
        private readonly Mock<IAir4PnrCommonV6> _iAir4PnrCommonV6;
        private readonly Mock<ILogger<Air4CreateSessionCommandHandler>> _mockLogger;
        private readonly Air4CreateSessionCommandHandler _handler;
        public Air4CreateSessionCommandHandlerTests()
        {
            _iAir4PnrCommonV6 = new Mock<IAir4PnrCommonV6>();
            _mockLogger = new Mock<ILogger<Air4CreateSessionCommandHandler>>();
            _handler = new Air4CreateSessionCommandHandler(_iAir4PnrCommonV6.Object, _mockLogger.Object);
            OrderManagementTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task Handler_ShouldReturnSuccessWithGoodRequest()
        {
            //Arrange
            var channelEntityData = TestDataProvider.GetTestData<Channel>("ChannelInfo");
            var request = new Air4CreateSessionCommand() { location = "abc", pnrNumber = "123456", transactionId = "101", ChannelInfo = channelEntityData };
            var response = new SetupContextResponseType()
            {
                SessionId = "123",
                Pnr = new PnrType() { SegmentDl = Array.Empty<SegmentDl>(), Passenger = Array.Empty<Passenger>() },
                DebugInfo = Array.Empty<DebugInfoType>(),
                ResponseInfo = Array.Empty<ResponseInfoType>()
            };
            _iAir4PnrCommonV6.Setup(r => r.Air4SetupContext(It.IsAny<SetupContextRequestType>())).Returns(response);

            //Act
            var result = await _handler.Handle(request, new System.Threading.CancellationToken());
            //Assert
            Assert.Equal(response.SessionId, result.SessionId);
        }

        [Fact]
        public async Task Handler_ShouldThrowExceptionWhenExceptionOccurs()
        {
            //Arrange
            _iAir4PnrCommonV6.Setup(r => r.Air4SetupContext(It.IsAny<SetupContextRequestType>())).Throws<ArgumentNullException>();
            //Act
            async Task act() => await _handler.Handle(new Air4CreateSessionCommand(), new System.Threading.CancellationToken());

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }

        [Fact]
        public async Task Handler_ShouldThrowExternalExceptionWhenExceptionOccurs()
        {
            //Arrange
            _iAir4PnrCommonV6.Setup(r => r.Air4SetupContext(It.IsAny<SetupContextRequestType>())).Throws<ExternalServiceIntegrationException>();
            var channelEntityData = TestDataProvider.GetTestData<Channel>("ChannelInfo");
            //Act
            async Task act() => await _handler.Handle(new Air4CreateSessionCommand() { ChannelInfo = channelEntityData }, new System.Threading.CancellationToken());
            //Assert
            await Assert.ThrowsAsync<ExternalServiceIntegrationException>(act);
        }
    }
}
