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
using VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Model;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Repository;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Passenger.Tests
{
    public class ManageMultipleInfantCommandHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly ManageMultipleInfantCommandHandler _handler;
        private readonly Mock<ILogger<ManageMultipleInfantCommandHandler>> _mockLogger;
        private readonly Mock<ICacheConfig> _cacheConfig;

        public ManageMultipleInfantCommandHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _cacheConfig = new Mock<ICacheConfig>();
            _mockLogger = new Mock<ILogger<ManageMultipleInfantCommandHandler>>();
            _handler = new ManageMultipleInfantCommandHandler(_mediator.Object, _mockLogger.Object, _cacheConfig.Object);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ManageMultipleInfantCommandHandler(null, _mockLogger.Object, _cacheConfig.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ManageMultipleInfantCommandHandler(_mediator.Object, null, _cacheConfig.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenCacheConfigIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ManageMultipleInfantCommandHandler(_mediator.Object, _mockLogger.Object, null));
        }

        [Fact]
        public async Task Handler_Success()
        {
            _mediator.Setup(m => m.Send(It.IsAny<ManageMultipleInfantAir4Command>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Domain.Entities.ManageMultipleInfantV2Response());

            var result = await _handler.Handle(new ManageMultipleInfantCommand(), System.Threading.CancellationToken.None);

            Assert.True(result.IsSuccess);
        }
        [Theory]
        [InlineData("VHOLS")]
        public async Task Handler_Success_With_No_Erros_WhenChannelEntityIsNotNull(string channelId)
        {
            //Arrange
            var channelEntityData = TestDataProvider.GetTestData<ChannelEntity>("PassengerMS\\ChannelEntityData");
            var channleEntityDataFiltered = channelEntityData.RowKey.ToLower() == channelId.ToLower() ? channelEntityData : null;
            _cacheConfig.Setup(r => r.GetChannelEntity(channelId)).ReturnsAsync(channleEntityDataFiltered);
            var request = new ManageMultipleInfantCommand()
            {
                ChannelId = channelId,
                RequestInfo = new Domain.Entities.RequestInfoType(),

                InfantV2Info = new List<InfantV2InfoType>()
                {
                    new InfantV2InfoType()
                    { InfantProcessRequestOptions=new InfantProcessRequestOptionsType(),
                      InfantNameManagement= new InfantNameManagementType()
                      {
                          IndustryTravelType ="test",
                         InfantFrequentFlyerNumber ="test",
                         InfantPPRExtensionNumber ="test",
                         InfantStaffTravelPPRNumber ="test",
                         InfantTicketNumber ="test",
                         ZEDInfantCarrierCode ="test",
                       },

                       InfantPassengerInfo = new List<InfantPassengerInfoType>()
                       {
                           new  InfantPassengerInfoType(){
                           AdultGivenNameNumber ="test",
                            AdultSurnameNumber ="test",
                            InfantGivenName ="test",
                            InfantGivenNameNumber ="test",
                            InfantSurname ="test",
                            InfantSurnameNumber ="test",
                            NonRevenueInd ="test",
                           }

                        }.ToArray()
                    }

                }.ToArray()
            };
            _mediator.Setup(m => m.Send(It.IsAny<ManageMultipleInfantAir4Command>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Domain.Entities.ManageMultipleInfantV2Response());

            var result = await _handler.Handle(request, System.Threading.CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
        }
        [Theory]
        [InlineData("abcd")]
        public async Task Handler_Success_With_Erros_WhenChannelEntityIsNull(string channelId)
        {
            //Arrange
            var channelEntityData = TestDataProvider.GetTestData<ChannelEntity>("PassengerMS\\ChannelEntityData");
            var channleEntityDataFiltered = channelEntityData.RowKey.ToLower() == channelId.ToLower() ? channelEntityData : null;
            _cacheConfig.Setup(r => r.GetChannelEntity(channelId)).ReturnsAsync(channleEntityDataFiltered);
            _mediator.Setup(m => m.Send(It.IsAny<ManageMultipleInfantAir4Command>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Domain.Entities.ManageMultipleInfantV2Response());

            var result = await _handler.Handle(new ManageMultipleInfantCommand() { ChannelId = channelId, RequestInfo = new Domain.Entities.RequestInfoType() }, System.Threading.CancellationToken.None);

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
            _mediator.Setup(m => m.Send(It.IsAny<ManageMultipleInfantAir4Command>(), System.Threading.CancellationToken.None)).ThrowsAsync(
                new ExternalServiceIntegrationException("G028", new Exception("One of the mandatory field is missing or invalid in the request")));

            var result = await _handler.Handle(new ManageMultipleInfantCommand() { ChannelId = channelId, RequestInfo = new Domain.Entities.RequestInfoType() }, System.Threading.CancellationToken.None);

            Assert.True(result.IsFailure);

        }
    }
}
