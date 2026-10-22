using System;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Mapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class AirportLocationMapperTests
    {
        [Fact]
        public void AirportLocationMapper_ShouldSucceed_WithGoodResponse()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AirportDetail>("OrderSearchMS\\AirportDetailsData");

            //Act
            var result = AirportLocationMapper.MaptoAirportLocation(request);

            //Assert
            Assert.NotNull(result);
        }
        [Fact]
        public void AirportLocationMapper_ShouldThrowNullException_WithNullRequest()
        {
            //Assert
            Assert.Throws<ArgumentNullException>(() => AirportLocationMapper.MaptoAirportLocation(null));
        }
    }
}
