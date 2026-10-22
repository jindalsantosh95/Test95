using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Microservices.Retail.Passenger.Application.Commands;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Model;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Repository;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Passenger.Tests
{
    public class AddPTCCodeCommandHandleTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly AddPTCCodeCommandHandler _handler;
        private readonly Mock<ILogger<AddPTCCodeCommandHandler>> _mockLogger;
        private readonly Mock<ICacheConfig> _cacheConfig;

        public AddPTCCodeCommandHandleTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<AddPTCCodeCommandHandler>>();
            _cacheConfig = new Mock<ICacheConfig>();
            _handler = new AddPTCCodeCommandHandler(_mockMediator.Object, _mockLogger.Object, _cacheConfig.Object);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddPTCCodeCommandHandler(null, _mockLogger.Object, _cacheConfig.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddPTCCodeCommandHandler(_mockMediator.Object, null, _cacheConfig.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenCacheConfigIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddPTCCodeCommandHandler(_mockMediator.Object, _mockLogger.Object, null));
        }

        [Theory]
        [InlineData("VHOLS")]
        public async Task Handler_Success_With_No_Erros_WhenChannelEntityIsNotNull(string channelId)
        {
            //Arrange
            var channelEntityData = TestDataProvider.GetTestData<ChannelEntity>("PassengerMS\\ChannelEntityData");
            var channleEntityDataFiltered = channelEntityData.RowKey.ToLower() == channelId.ToLower() ? channelEntityData : null;
            _cacheConfig.Setup(r => r.GetChannelEntity(channelId)).ReturnsAsync(channleEntityDataFiltered);
            _mockMediator.Setup(m => m.Send(It.IsAny<AddPTCCodeAir4CommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Domain.Entities.AddResponseType());

            var result = await _handler.Handle(new AddPTCCodeCommand() { ChannelId = channelId, RequestInfo = new Domain.Entities.RequestInfoType() }, System.Threading.CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
        }
        [Theory]
        [InlineData("abcd")]
        public async Task Handler_Success_With_Erros_WhenChannelEntityIsNull(string channelId)
        {
            var channelEntityData = TestDataProvider.GetTestData<ChannelEntity>("PassengerMS\\ChannelEntityData");
            var channleEntityDataFiltered = channelEntityData.RowKey.ToLower() == channelId.ToLower() ? channelEntityData : null;
            _cacheConfig.Setup(r => r.GetChannelEntity(channelId)).ReturnsAsync(channleEntityDataFiltered);
            var response = new Domain.Entities.AddPTCCodesResponse()
            {
                AddPTCCodesResponse1 = new Domain.Entities.AddPTCCodesResponseType()
                {
                    Pnr = new Domain.Entities.PnrType(),
                    ResponseInfo = new Domain.Entities.ResponseInfoType()
                    {
                        AdditionalInfo = "PTC processed",
                        ServiceTrackingId = "#test",
                        TransactionId = "#trid"
                    }
                }
            };
            _mockMediator.Setup(m => m.Send(It.IsAny<AddPTCCodeAir4CommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(response);

            var result = await _handler.Handle(new AddPTCCodeCommand() { ChannelId = channelId, RequestInfo = new Domain.Entities.RequestInfoType() }, System.Threading.CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Errors);

        }
        [Theory]
        [InlineData("VHOLS")]
        public async Task Handler_Faliure_UponFault(string channelId)
        {
            var channelEntityData = TestDataProvider.GetTestData<ChannelEntity>("PassengerMS\\ChannelEntityData");
            var channleEntityDataFiltered = channelEntityData.RowKey.ToLower() == channelId.ToLower() ? channelEntityData : null;
            _cacheConfig.Setup(r => r.GetChannelEntity(channelId)).ReturnsAsync(channleEntityDataFiltered);
            _mockMediator.Setup(m => m.Send(It.IsAny<AddPTCCodeAir4Command>(), System.Threading.CancellationToken.None)).ThrowsAsync(
                new ExternalServiceIntegrationException("G028", new Exception("One of the mandatory field is missing or invalid in the request")));

            var result = await _handler.Handle(new AddPTCCodeCommand() { ChannelId = channelId, RequestInfo = new Domain.Entities.RequestInfoType() }, System.Threading.CancellationToken.None);

            Assert.True(result.IsFailure);

        }
    }
}
