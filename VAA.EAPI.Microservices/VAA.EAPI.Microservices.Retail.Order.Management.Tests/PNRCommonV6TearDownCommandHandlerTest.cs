using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Tests
{
    public class PNRCommonV6TearDownCommandHandlerTest
    {
        private readonly Mock<ILogger<PNRCommonV6TearDownCommandHandler>> _mockLogger;
        private readonly Mock<IAir4PnrCommonV6> _iAir4PnrCommonV6;
        private readonly Mock<IMediator> _mockMediator;
        private readonly PNRCommonV6TearDownCommandHandler _handler;
        public PNRCommonV6TearDownCommandHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<PNRCommonV6TearDownCommandHandler>>();
            _iAir4PnrCommonV6 = new Mock<IAir4PnrCommonV6>();
            _handler = new PNRCommonV6TearDownCommandHandler(_iAir4PnrCommonV6.Object, _mockLogger.Object, _mockMediator.Object);
            OrderManagementTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PNRCommonV6TearDownCommandHandler(_iAir4PnrCommonV6.Object, null, _mockMediator.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PNRCommonV6TearDownCommandHandler(null, _mockLogger.Object, _mockMediator.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PNRCommonV6TearDownCommandHandler(_iAir4PnrCommonV6.Object, _mockLogger.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldThrowExceptionWhenExceptionOccurs()
        {
            //Arrange
            _iAir4PnrCommonV6.Setup(r => r.Air4TearDownContext(It.IsAny<TeardownContextRequestType>())).Throws<ArgumentNullException>();
            //Act
            async Task act() => await _handler.Handle(new PNRCommonV6TearDownCommand(), new System.Threading.CancellationToken());

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }
        [Fact]
        public async Task Handler_ShouldReturnNullWithEmptyDetails()
        {
            //Arrange
            var channelEntityData = TestDataProvider.GetTestData<Channel>("ChannelInfo");
            var request = TestDataProvider.GetTestData<PNRCommonV6TearDownCommand>("OrderChange.TearDownCommandData");
            request.ChannelInfo = channelEntityData;
            //Act
            var result = await _handler.Handle(request, new System.Threading.CancellationToken());
            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Handler_ShouldReturnSuccessPNRWithValidRequest()
        {
            //Arrange
            var channelEntityData = TestDataProvider.GetTestData<Channel>("ChannelInfo");
            var request = TestDataProvider.GetTestData<PNRCommonV6TearDownCommand>("OrderChange.TearDownCommandData");
            request.ChannelInfo = channelEntityData;
            var response = new TeardownContextResponseType()
            {
                Pnr = new PnrType() { SegmentDl = Array.Empty<SegmentDl>(), Passenger = Array.Empty<Passenger>() },
                DebugInfo = Array.Empty<DebugInfoType>(),
                ResponseInfo = Array.Empty<ResponseInfoType>()
            };

            _iAir4PnrCommonV6.Setup(r => r.Air4TearDownContext(It.IsAny<TeardownContextRequestType>())).Returns(response);
            //Act
            var result = await _handler.Handle(request, new System.Threading.CancellationToken());
            //Assert
            Assert.Equal(response.Pnr, result.Pnr);
            Assert.Equal(response.ResponseInfo, result.ResponseInfo);
        }
        [Fact]
        public async Task Handler_ShouldReturnSuccessWhenNoExceptionOccurs()
        {
            //Arrange
            var channelEntityData = TestDataProvider.GetTestData<Channel>("ChannelInfo");
            var response = new TeardownContextResponseType() { Pnr = new PnrType() { AdvisoryMessage = new AdvisoryMessageType[1] } };
            _iAir4PnrCommonV6.Setup(r => r.Air4TearDownContext(It.IsAny<TeardownContextRequestType>())).Returns(response);
            //Act
            var result = await _handler.Handle(new PNRCommonV6TearDownCommand() { ChannelInfo = channelEntityData }, new System.Threading.CancellationToken());
            //Assert
            Assert.Single(result.Pnr.AdvisoryMessage);

        }

        [Fact]
        public async Task Handler_ShouldThrowExternalExceptionWhenExceptionOccurs()
        {
            //Arrange
            var channelEntityData = TestDataProvider.GetTestData<Channel>("ChannelInfo");
            _iAir4PnrCommonV6.Setup(r => r.Air4TearDownContext(It.IsAny<TeardownContextRequestType>())).Throws<ExternalServiceIntegrationException>();
            //Act
            async Task act() => await _handler.Handle(new PNRCommonV6TearDownCommand() { ChannelInfo = channelEntityData }, new System.Threading.CancellationToken());
            //Assert
            await Assert.ThrowsAsync<ExternalServiceIntegrationException>(act);
        }
    }
}
