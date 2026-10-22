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
    public class ManageMultipleTravelDocsAndSecureFlightAir4CommandHandlerTests
    {
        private readonly Mock<ICacheConfig> _cacheConfig;
        private readonly Mock<IAir4PassengerV4> _iAir4PassengerV4;
        private readonly ManageMultipleTravelDocsAndSecureFlightAir4CommandHandler _handler;

        public ManageMultipleTravelDocsAndSecureFlightAir4CommandHandlerTests()
        {
            _cacheConfig = new Mock<ICacheConfig>();
            _iAir4PassengerV4 = new Mock<IAir4PassengerV4>();
            _handler = new ManageMultipleTravelDocsAndSecureFlightAir4CommandHandler(_iAir4PassengerV4.Object);
        }


        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenAir4ObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ManageMultipleTravelDocsAndSecureFlightAir4CommandHandler(null));
        }
        [Fact]
        public async Task HandlerTest_Success()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<ManageMultipleTravelDocsAndSecureFlightAir4Command>("PassengerMS\\ManageMultipleTravelDocsAndSecureFlightRequest");

            var response = new Infrastructure.Proxy.ManageMultipleTravelDocsAndSecureFlightResponse()
            {
                ManageMultipleTravelDocsAndSecureFlightResponse1 = new Infrastructure.Proxy.ManageMultipleTravelDocsAndSecureFlightResponseType()
                {
                    SecureFlightPassengerData = new Infrastructure.Proxy.SecureFlightPassengerDataType() { SurName = "Test" }
                }
            };

            _cacheConfig.Setup(r => r.GetChannelEntities()).Returns(new List<ChannelEntity>() {
                TestDataProvider.GetTestData<ChannelEntity>("PassengerMS\\ChannelEntityData")
            });

            _iAir4PassengerV4.Setup(i => i.Air4ManageMultipleTravelDocsAndSecureFlight(It.IsAny<Infrastructure.Proxy.ManageMultipleTravelDocsAndSecureFlightRequestType>())).ReturnsAsync(response);

            //Act
            var result = await _handler.Handle(request, System.Threading.CancellationToken.None);

            //Assert

            Assert.NotNull(result.SecureFlightPassengerData.SurName);
            Assert.Equal("Test", result.SecureFlightPassengerData.SurName);
        }
    }
}
