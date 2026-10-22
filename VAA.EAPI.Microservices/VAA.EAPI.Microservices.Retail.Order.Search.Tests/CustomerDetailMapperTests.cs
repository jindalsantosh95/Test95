using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Response;
using VAA.EAPI.Microservices.Retail.Order.Search.Mapper.GetBooking;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class CustomerDetailMapperTests
    {
        [Fact]
        public void CustomerDetailMapper_ShouldSucceed_WithGoodResponse()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<List<OrderDetail>>("OrderSearchMS\\OrderDetailData");

            //Act
            var result = CustomerDetailMapper.MapCustomerDetails(request);

            //Assert
            Assert.NotNull(result);
        }
    }
}
