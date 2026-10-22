using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Mapper.CommonMapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.Common
{
    public class OrderDataMapperTests
    {
        [Fact]
        public void RetainPriceDetails_ShouldRetainPriceDetails_WhenCalled()
        {
            // Arrange
            var orderMSOrderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderDocument\\OrderDataResponse");
            var sabreOrderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderDocument\\SabreResponse");

            // Act
            var result = OrderDataMsToSaberDocumentMapper.RetainPriceDetails(orderMSOrderDoc, sabreOrderDoc);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("-1A", result.OrderItems.First().OrderItemId);
        }
    }
}
