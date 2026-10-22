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
    public class AddPhoneCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly AddPhoneCommandHandler _handler;
        private readonly Mock<ILogger<AddPhoneCommandHandler>> _mockLogger;
        private readonly Mock<ICacheConfig> _cacheConfig;

        public AddPhoneCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<AddPhoneCommandHandler>>();
            _cacheConfig = new Mock<ICacheConfig>();
            _handler = new AddPhoneCommandHandler(_mediator.Object, _mockLogger.Object, _cacheConfig.Object);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddPhoneCommandHandler(null, _mockLogger.Object, _cacheConfig.Object));
        }


        [Theory]
        [InlineData("VHOLS")]
        public async Task Handler_Success_With_No_Erros_WhenChannelEntityIsNotNull(string channelId)
        {
            //Arrange
            var channelEntityData = TestDataProvider.GetTestData<ChannelEntity>("PassengerMS\\ChannelEntityData");
            var channleEntityDataFiltered = channelEntityData.RowKey.ToLower() == channelId.ToLower() ? channelEntityData : null;

            _mediator.Setup(m => m.Send(It.IsAny<AddPhoneAir4Command>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Domain.Entities.AddPhoneResponseType());
            _cacheConfig.Setup(r => r.GetChannelEntity(channelId)).ReturnsAsync(channleEntityDataFiltered);
            //Act
            var result = await _handler.Handle(new AddPhoneCommand() { ChannelId = channelId, RequestInfo = new Domain.Entities.RequestInfoType() }, System.Threading.CancellationToken.None);

            //Assert

            Assert.Null(result.Value.Errors);

        }

        [Theory]
        [InlineData("abcd")]
        public async Task Handler_Success_With__Erros_WhenChannelEntityIsNull(string channelId)
        {
            //Arrange
            var channelEntityData = TestDataProvider.GetTestData<ChannelEntity>("PassengerMS\\ChannelEntityData");
            var channleEntityDataFiltered = channelEntityData.RowKey.ToLower() == channelId.ToLower() ? channelEntityData : null;
            _mediator.Setup(m => m.Send(It.IsAny<AddPhoneAir4Command>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Domain.Entities.AddPhoneResponseType());
            _cacheConfig.Setup(r => r.GetChannelEntity(channelId)).ReturnsAsync(channleEntityDataFiltered);
            //Act
            var result = await _handler.Handle(new AddPhoneCommand() { ChannelId = channelId, RequestInfo = new Domain.Entities.RequestInfoType() }, System.Threading.CancellationToken.None);

            //Assert

            Assert.NotNull(result.Value.Errors);

        }
        [Theory]
        [InlineData("VHOLS")]
        public async Task Handler_Faliure_UponFault(string channelId)
        {
            var channelEntityData = TestDataProvider.GetTestData<ChannelEntity>("PassengerMS\\ChannelEntityData");
            var channleEntityDataFiltered = channelEntityData.RowKey.ToLower() == channelId.ToLower() ? channelEntityData : null;
            _cacheConfig.Setup(r => r.GetChannelEntity(channelId)).ReturnsAsync(channleEntityDataFiltered);
            _mediator.Setup(m => m.Send(It.IsAny<AddPhoneAir4Command>(), System.Threading.CancellationToken.None)).ThrowsAsync(
                new ExternalServiceIntegrationException("G028", new Exception("One of the mandatory field is missing or invalid in the request")));

            var result = await _handler.Handle(new AddPhoneCommand() { ChannelId = channelId, RequestInfo = new Domain.Entities.RequestInfoType() }, System.Threading.CancellationToken.None);

            Assert.True(result.IsFailure);

        }
    }

}

