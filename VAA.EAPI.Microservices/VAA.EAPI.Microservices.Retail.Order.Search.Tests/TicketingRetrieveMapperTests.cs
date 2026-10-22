using VAA.EAPI.Microservices.Retail.Order.Search.Mapper;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class TicketingRetrieveMapperTests
    {
        public TicketingRetrieveMapperTests()
        {
            OrderSearchTestsCommon.CreateTestEnvironmentSettings();
        }
        [Fact]
        public void TicketingRetrieveMapper_ShouldSucceed_WithGoodResponse()
        {
            //Arrange
            //var request = TestDataProvider.GetTestData<OrderSearchRequest>("OrderSearchMS\\OrderSearchRequestData");
            string ticketNumber = "9886567875456";
            //Act
            var result = TicketingRetrieveMapper.TicketingRetrieveRequestMapper(ticketNumber);

            //Assert
            Assert.NotNull(result);
        }
    }
}
