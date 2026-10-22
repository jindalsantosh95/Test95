using System.Linq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;
using VAA.EAPI.Microservices.Retail.Order.Data.Mapper;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class OrderHistoryDocumentUpdateMapperTests
    {
        [Fact]
        public void Update_AddsInitialHistory_WhenExistingHistoryDocumentHasNoHistoryEntries()
        {
            var orderDocument = new OrderDocument
            {
                Version = 2,
                ChangeDateTime = new UtcAndLocalDateTime { UtcDateTime = "2026-09-29T10:00:00Z" }
            };
            var historyDocument = new OrderDocument { OrderHistory = null };

            var result = OrderHistoryDocumentUpdateMapper.Update(orderDocument, historyDocument);

            var history = Assert.Single(result.OrderHistory);
            Assert.Same(historyDocument, result);
            Assert.Equal("1", history.Version);
            Assert.Equal(orderDocument.ChangeDateTime.UtcDateTime, history.ChangeDateTime);
        }
    }
}
