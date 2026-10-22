using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Model;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Services.Interfaces;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Passenger.Tests
{
    public class AddPassengerAir4CommandHandlerTests
    {
        private readonly Mock<ICacheConfig> _cacheConfig;
        private readonly Mock<IAir4PassengerV4> _iAir4PassengerV4;
        private readonly AddPassengerAir4CommandHandler _handler;

        public AddPassengerAir4CommandHandlerTests()
        {

            _cacheConfig = new Mock<ICacheConfig>();
            _iAir4PassengerV4 = new Mock<IAir4PassengerV4>();
            _handler = new AddPassengerAir4CommandHandler(_iAir4PassengerV4.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenAir4ObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddPassengerAir4CommandHandler(null));
        }

        [Fact]
        public async Task HandlerTest_Success()
        {
            var request = TestDataProvider.GetTestData<AddPassengerAir4Command>("PassengerMS\\AddPassengerRequest");

            var response = new Infrastructure.Proxy.AddResponse()
            {
                AddResponse1 = new Infrastructure.Proxy.AddResponseType()
                {
                    Error = new Infrastructure.Proxy.ErrorType() { ErrorText = "dummy" }
                }
            };

            _cacheConfig.Setup(r => r.GetChannelEntities()).Returns(new List<ChannelEntity>() {
                TestDataProvider.GetTestData<ChannelEntity>("PassengerMS\\ChannelEntityData")
            });
            _iAir4PassengerV4.Setup(i => i.Air4AddPassenger(It.IsAny<Infrastructure.Proxy.AddRequestType>())).ReturnsAsync(response);

            //Act
            var result = await _handler.Handle(request, System.Threading.CancellationToken.None);

            //Assert

            Assert.NotNull(result.Error);
            Assert.Equal("dummy", result.Error.ErrorText);

        }
    }
}
