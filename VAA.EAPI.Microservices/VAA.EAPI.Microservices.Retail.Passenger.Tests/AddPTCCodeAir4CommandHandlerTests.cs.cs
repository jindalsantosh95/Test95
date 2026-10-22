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
    public class AddPTCCodeAir4CommandHandlerTests
    {
        private readonly Mock<ICacheConfig> _cacheConfig;
        private readonly Mock<IAir4PassengerV4> _iAir4PassengerV4;
        private readonly AddPTCCodeAir4CommandHandler _handler;
        public AddPTCCodeAir4CommandHandlerTests()
        {
            _cacheConfig = new Mock<ICacheConfig>();
            _iAir4PassengerV4 = new Mock<IAir4PassengerV4>();
            _handler = new AddPTCCodeAir4CommandHandler(_iAir4PassengerV4.Object, _cacheConfig.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenAir4PassengerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddPTCCodeAir4CommandHandler(null, _cacheConfig.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenCacheObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddPTCCodeAir4CommandHandler(_iAir4PassengerV4.Object, null));
        }

        [Fact]
        public async Task HandlerTest_Success()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AddPTCCodeAir4Command>("PassengerMS\\AddPTCCodesRequest");

            var response = new Infrastructure.Proxy.AddPTCCodesResponse()
            {
                AddPTCCodesResponse1 = new Infrastructure.Proxy.AddPTCCodesResponseType()
                {
                    ResponseInfo = new Infrastructure.Proxy.ResponseInfoType() { ServiceTrackingId = "dummy" }
                }
            };

            _cacheConfig.Setup(r => r.GetChannelEntities()).Returns(new List<ChannelEntity>() {
                TestDataProvider.GetTestData<ChannelEntity>("PassengerMS\\ChannelEntityData")
            });

            _iAir4PassengerV4.Setup(i => i.Air4AddPTC(It.IsAny<Infrastructure.Proxy.AddPTCCodesRequestType>())).ReturnsAsync(response);

            //Act
            var result = await _handler.Handle(request, System.Threading.CancellationToken.None);

            //Assert

            Assert.NotNull(result.ResponseInfo);
            Assert.Equal("dummy", result.ResponseInfo.ServiceTrackingId);

        }
    }
}
