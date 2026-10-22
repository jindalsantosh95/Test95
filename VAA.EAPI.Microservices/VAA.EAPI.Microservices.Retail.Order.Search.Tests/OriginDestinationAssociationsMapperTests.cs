using VAA.EAPI.Microservices.Retail.Order.Search.Mapper;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class OriginDestinationAssociationsMapperTests
    {
        [Fact]
        public void Test()
        {
            var result = OriginDestinationAssociationsMapper.Air4ToOrderSearchResponseMapper(null, null, null, null);
            Assert.Null(result);
        }
    }
}
