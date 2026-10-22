using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Microservices.Retail.Passenger.Application.Commands;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Model;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Services.Interfaces;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Passenger.Tests
{
    public class AddPassengerCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly AddPassengerCommandHandler _handler;
        private readonly Mock<ILogger<AddPassengerCommandHandler>> _mockLogger;
        private readonly Mock<ICacheConfig> _cacheConfig;
        private readonly Mock<IAir4PassengerV4> _iAir4PassengerV4;

        public AddPassengerCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<AddPassengerCommandHandler>>();
            _cacheConfig = new Mock<ICacheConfig>();
            _handler = new AddPassengerCommandHandler(_mediator.Object, _mockLogger.Object, _cacheConfig.Object);
            _iAir4PassengerV4 = new Mock<IAir4PassengerV4>();

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddPassengerCommandHandler(null, _mockLogger.Object, _cacheConfig.Object));
        }

        [Theory]
        [InlineData("VHOLS")]
        public async Task Handler_Success_With_No_Erros_WhenChannelIdIsValid(string channelId)
        {
            //Arrange
            var channelEntityData = TestDataProvider.GetTestData<ChannelEntity>("PassengerMS\\ChannelEntityData");
            var channleEntityDataFiltered = channelEntityData.RowKey.ToLower() == channelId.ToLower() ? channelEntityData : null;
            var addresponseType = new Domain.Entities.AddResponseType() { Pnr = PnrHelper.GetPnrObjct(), Error = new Domain.Entities.ErrorType(), ResponseInfo = new Domain.Entities.ResponseInfoType() };
            _cacheConfig.Setup(r => r.GetChannelEntity(null)).ReturnsAsync(channleEntityDataFiltered);
            _mediator.Setup(m => m.Send(It.IsAny<AddPassengerAir4Command>(), System.Threading.CancellationToken.None)).ReturnsAsync(addresponseType);

            var result = await _handler.Handle(new AddPassengerCommand() { RequestInfo = new Domain.Entities.RequestInfoType() }, System.Threading.CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
            Assert.Equal("test", result.Value.Value.Pnr.TicketNumberData1[0].FirstNameNumber);
            Assert.Equal("test", result.Value.Value.Pnr.SegmentDl[0].ActiveFlag);
        }

        [Theory]
        [InlineData("VHOLS")]
        public async Task Handler_Success_With__No_Erros_WhenChannelIdIsValidAndvalidInputProvided(string channelId)
        {
            //Arrange
            var channelEntityData = TestDataProvider.GetTestData<ChannelEntity>("PassengerMS\\ChannelEntityData");
            var channleEntityDataFiltered = channelEntityData.RowKey.ToLower() == channelId.ToLower() ? channelEntityData : null;
            var addresponseType = new Domain.Entities.AddResponseType()
            {
                Pnr = PnrHelper.GetPnrObjct(),
                Error = new Domain.Entities.ErrorType(),
                ResponseInfo = new Domain.Entities.ResponseInfoType(),
                AddErrorList = new List<Domain.Entities.AddErrorListType>()
                {
                    new Domain.Entities.AddErrorListType()
                    {
                        ErrorInfo="invalid application id provided",
                        Error=new Domain.Entities.ErrorType()
                        {
                            AdditionalInfo="appid is invalid",
                            ErrorCode="E002",
                            ErrorText="invalid application id provided",
                            ReasonCode="R001",
                            ReasonText="appid is invalid in request"
                        }

                    }
                }.ToArray()

            };

            _cacheConfig.Setup(r => r.GetChannelEntity(null)).ReturnsAsync(channleEntityDataFiltered);

            _mediator.Setup(m => m.Send(It.IsAny<AddPassengerAir4Command>(), System.Threading.CancellationToken.None)).ReturnsAsync(addresponseType);

            var result = await _handler.Handle(new AddPassengerCommand() { RequestInfo = new Domain.Entities.RequestInfoType() }, System.Threading.CancellationToken.None);

            Assert.True(result.IsSuccess);

            Assert.Equal("invalid application id provided", result.Value.Value.AddErrorList[0].ErrorInfo);

        }

        [Theory]
        [InlineData("abcd")]
        public async Task Handler_Success_With_Erros_WhenChannelIdIsInValid(string channelId)
        {
            var channelEntityData = TestDataProvider.GetTestData<ChannelEntity>("PassengerMS\\ChannelEntityData");
            var channleEntityDataFiltered = channelEntityData.RowKey.ToLower() == channelId.ToLower() ? channelEntityData : null;
            _cacheConfig.Setup(r => r.GetChannelEntity(null)).ReturnsAsync(channleEntityDataFiltered);

            _mediator.Setup(m => m.Send(It.IsAny<AddPassengerAir4Command>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Domain.Entities.AddResponseType());

            var result = await _handler.Handle(new AddPassengerCommand() { RequestInfo = new Domain.Entities.RequestInfoType() }, System.Threading.CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Errors);



        }

        [Theory]
        [InlineData("VHOLS")]
        public async Task Handler_Faliure_UponFault(string channelId)
        {
            var channelEntityData = TestDataProvider.GetTestData<ChannelEntity>("PassengerMS\\ChannelEntityData");
            var channleEntityDataFiltered = channelEntityData.RowKey.ToLower() == channelId.ToLower() ? channelEntityData : null;
            _cacheConfig.Setup(r => r.GetChannelEntity(null)).ReturnsAsync(channleEntityDataFiltered);
            _mediator.Setup(m => m.Send(It.IsAny<AddPassengerAir4Command>(), System.Threading.CancellationToken.None)).ThrowsAsync(
                new ExternalServiceIntegrationException("G028", new Exception("One of the mandatory field is missing or invalid in the request")));

            var result = await _handler.Handle(new AddPassengerCommand() { RequestInfo = new Domain.Entities.RequestInfoType() }, System.Threading.CancellationToken.None);

            Assert.True(result.IsFailure);

        }


    }
}
