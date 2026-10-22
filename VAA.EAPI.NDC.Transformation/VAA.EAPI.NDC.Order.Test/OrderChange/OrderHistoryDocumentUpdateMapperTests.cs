using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Mapper.Order;
using Xunit;
using OrderHistoryDocument = VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS.OrderHistory;

namespace VAA.EAPI.NDC.Order.Tests.OrderChange
{
    public class OrderHistoryDocumentUpdateMapperTests
    {
        [Fact]
        public void HistoryDocumentCreation_CreatesInitialHistory_WhenExistingHistoryDocumentHasNoHistory()
        {
            var oldOrderDocument = new OrderDocument
            {
                Version = 2,
                OrderStatus = "Open",
                ChangeDateTime = new UtcAndLocalDateTime { UtcDateTime = "2026-01-01T00:00:00Z" }
            };
            var historyOrderDocument = new OrderDocument();

            var result = OrderHistoryDocumentUpdateMapper.HistoryDocumentCreation(oldOrderDocument, historyOrderDocument);

            var history = Assert.Single(result.OrderHistory);
            Assert.Same(historyOrderDocument, result);
            Assert.Equal("1", history.Version);
            Assert.Equal("Open", history.OrderStatus);
        }

        [Fact]
        public void HistoryDocumentCreation_UsesNumericVersionOrdering_WhenDeterminingLatestHistory()
        {
            var oldOrderDocument = new OrderDocument { Version = 10 };
            var historyOrderDocument = new OrderDocument
            {
                OrderHistory =
                [
                    new OrderHistoryDocument { Version = "2" },
                    new OrderHistoryDocument { Version = "10" }
                ]
            };

            var result = OrderHistoryDocumentUpdateMapper.HistoryDocumentCreation(oldOrderDocument, historyOrderDocument);

            Assert.Equal(2, result.OrderHistory.Count());
        }

        [Fact]
        public void AddSplitHistory_UsesNumericVersionOrdering_WhenCreatingSplitEntries()
        {
            var historyOrderDocument = new OrderDocument
            {
                OrderHistory =
                [
                    new OrderHistoryDocument { Version = "2" },
                    new OrderHistoryDocument { Version = "10" }
                ]
            };
            var parentOrderDocument = new OrderDocument
            {
                ChangeDateTime = new UtcAndLocalDateTime { UtcDateTime = "2026-01-01T00:00:00Z" }
            };
            var childOrderDocument = new OrderDocument
            {
                ChangeDateTime = new UtcAndLocalDateTime { UtcDateTime = "2026-01-01T00:00:00Z" }
            };

            var result = OrderHistoryDocumentUpdateMapper.AddSplitHistory(historyOrderDocument, parentOrderDocument, childOrderDocument);

            var splitHistories = result.OrderHistory.Skip(2).ToList();
            Assert.Equal(2, splitHistories.Count);
            Assert.All(splitHistories, history => Assert.Equal("11", history.Version));
        }
    }
}

