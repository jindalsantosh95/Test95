using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Mapper;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Tests
{
    public class OrderChangeNotifRQMapperTests
    {
        [Fact]
        public void ShouldMapReactiveTtlRequestCorrectly_WhenValidRequestIsPassed()
        {
            // Arrange
            var body = TestDataProvider.GetTestData<OrderChangeNotifRequest>("NDCOrderNotificationMS\\ReactiveTTLExpiryRequest");

            // Act
            var result = OrderChangeNotifRQMapper.MapReactiveTTLRequest(body.OrderDocument);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Notification);
            Assert.NotNull(result.Notification.ChangeGroup);

            var warning = result.Notification.ChangeGroup[0].ChangeOperation[0].New.Warning;
            Assert.NotNull(warning);
            Assert.Single(warning);
            Assert.Equal("TTL expiry - Reactive", warning[0].TagText);
        }
    }
}
