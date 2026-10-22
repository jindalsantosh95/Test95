using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Search.Mapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class TimeLimitsMapperTests
    {
        [Fact]
        public void TimeLimitsMapper_ShouldSucceed_WithGoodResponse()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\RetrieveResposneTypeData");

            //Act
            var result = TimeLimitsMapper.Air4ResponseToTimeLimitsMapper(request);

            //Assert
            Assert.NotNull(result);
        }
    }
}
