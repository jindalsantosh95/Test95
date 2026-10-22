using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
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
    public class ManageMultipleTravelDocsAndSecureFlightCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly ManageMultipleTravelDocsAndSecureFlightCommandHandler _handler;
        private readonly Mock<ILogger<ManageMultipleTravelDocsAndSecureFlightCommandHandler>> _mockLogger;
        private readonly Mock<ICacheConfig> _cacheConfig;

        public ManageMultipleTravelDocsAndSecureFlightCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _cacheConfig = new Mock<ICacheConfig>();
            _mockLogger = new Mock<ILogger<ManageMultipleTravelDocsAndSecureFlightCommandHandler>>();
            _handler = new ManageMultipleTravelDocsAndSecureFlightCommandHandler(_mediator.Object, _mockLogger.Object, _cacheConfig.Object);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ManageMultipleTravelDocsAndSecureFlightCommandHandler(null, _mockLogger.Object, _cacheConfig.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ManageMultipleTravelDocsAndSecureFlightCommandHandler(_mediator.Object, null, _cacheConfig.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenCacheConfigIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ManageMultipleTravelDocsAndSecureFlightCommandHandler(_mediator.Object, _mockLogger.Object, null));
        }

        [Theory]
        [InlineData("VHOLS")]
        public async Task Handler_Success_With_No_Erros_WhenChannelEntityIsNotNull(string channelId)
        {
            //Arrange
            var channelEntityData = TestDataProvider.GetTestData<ChannelEntity>("PassengerMS\\ChannelEntityData");
            var channleEntityDataFiltered = channelEntityData.RowKey.ToLower() == channelId.ToLower() ? channelEntityData : null;
            _cacheConfig.Setup(r => r.GetChannelEntity(null)).ReturnsAsync(channleEntityDataFiltered);
            var response = new Domain.Entities.ManageMultipleTravelDocsAndSecureFlightResponseType()
            {
                PassengerAddressInfo = new Domain.Entities.PassengerAddressInfoType()
                {
                    City = "test",
                    CountryOfAddress = "test",
                    PhoneNumber = "test",
                    PostalCode = "test",
                    ResidenceCountry = "test",
                    State = "test",
                    StreetAddress = "test"
                }
            };
            _mediator.Setup(m => m.Send(It.IsAny<ManageMultipleTravelDocsAndSecureFlightAir4Command>(), System.Threading.CancellationToken.None)).ReturnsAsync(response);


            var result = await _handler.Handle(new ManageMultipleTravelDocsAndSecureFlightCommand() { RequestInfo = new Domain.Entities.RequestInfoType() }, System.Threading.CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
            Assert.Equal("test", result.Value.Value.PassengerAddressInfo.State);
        }
        [Theory]
        [InlineData("abcd")]
        public async Task Handler_Success_With_Erros_WhenChannelEntityIsNull(string channelId)
        {
            //Arrange
            var channelEntityData = TestDataProvider.GetTestData<ChannelEntity>("PassengerMS\\ChannelEntityData");
            var channleEntityDataFiltered = channelEntityData.RowKey.ToLower() == channelId.ToLower() ? channelEntityData : null;
            _cacheConfig.Setup(r => r.GetChannelEntity(null)).ReturnsAsync(channleEntityDataFiltered);
            var response = new Domain.Entities.ManageMultipleTravelDocsAndSecureFlightResponseType()
            {
                PassengerAddressInfo = new Domain.Entities.PassengerAddressInfoType()
                {
                    City = "test",
                    CountryOfAddress = "test",
                    PhoneNumber = "test",
                    PostalCode = "test",
                    ResidenceCountry = "test",
                    State = "test",
                    StreetAddress = "test"
                }
            };
            _mediator.Setup(m => m.Send(It.IsAny<ManageMultipleTravelDocsAndSecureFlightAir4Command>(), System.Threading.CancellationToken.None)).ReturnsAsync(response);

            var result = await _handler.Handle(new ManageMultipleTravelDocsAndSecureFlightCommand() { RequestInfo = new Domain.Entities.RequestInfoType() }, System.Threading.CancellationToken.None);

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
            _mediator.Setup(m => m.Send(It.IsAny<ManageMultipleTravelDocsAndSecureFlightAir4Command>(), System.Threading.CancellationToken.None)).ThrowsAsync(new ExternalServiceIntegrationException("G028", new Exception("One of the mandatory field is missing or invalid in the request")));

            var result = await _handler.Handle(new ManageMultipleTravelDocsAndSecureFlightCommand() { RequestInfo = new Domain.Entities.RequestInfoType() }, System.Threading.CancellationToken.None);

            Assert.True(result.IsFailure);
        }

    }
}
