using System;
using System.Linq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Mapper
{
    public static class OrderHistoryDocumentUpdateMapper
    {
        public static OrderDocument Update(OrderDocument oldOrderDoc, OrderDocument historyOrderDoc)
        {
            if (historyOrderDoc != null)
            {
                var latestHistory = historyOrderDoc.OrderHistory?.OrderBy(x => Convert.ToInt32(x.Version)).LastOrDefault();

                if (latestHistory == null || Convert.ToInt32(latestHistory.Version) != oldOrderDoc.Version)
                {
                    var updatedHistory = new OrderHistory()
                    {
                        DataList = oldOrderDoc.DataList,
                        OrderItems = oldOrderDoc.OrderItems,
                        OrderStatus = oldOrderDoc.OrderStatus,
                        Version = latestHistory != null ? Convert.ToString(Convert.ToInt32(latestHistory.Version) + 1) : "1",
                        ChangeType = oldOrderDoc.ChangeType,
                        ChangeDateTime = oldOrderDoc.ChangeDateTime.UtcDateTime,
                        Warnings = (oldOrderDoc.Warnings?.Any() ?? false) ? oldOrderDoc.Warnings : null
                    }; 

                    historyOrderDoc.OrderHistory = (historyOrderDoc.OrderHistory ?? Enumerable.Empty<OrderHistory>()).Append(updatedHistory);
                }
            }
            else
            {
                historyOrderDoc = new OrderDocument()
                {
                    DocType = oldOrderDoc.DocType,
                    FareTypeRequest = oldOrderDoc.FareTypeRequest,
                    PointOfSale = oldOrderDoc.PointOfSale,
                    CreationDateTime = oldOrderDoc.CreationDateTime,
                    BookingReference = oldOrderDoc.BookingReference,
                    OrderNumber = oldOrderDoc.OrderNumber,
                    Parties = oldOrderDoc.Parties,
                    TimeLimits = oldOrderDoc.TimeLimits,
                    OrderHistory = [new OrderHistory()
                    {
                        DataList = oldOrderDoc.DataList,
                        OrderItems = oldOrderDoc.OrderItems,
                        Version = "1",
                        OrderStatus = oldOrderDoc.OrderStatus,
                        ChangeType = ChangeTypeEnum.OrderItemAdded,
                        ChangeDateTime = oldOrderDoc.CreationDateTime.UtcDateTime,
                        Warnings = (oldOrderDoc.Warnings?.Any() ?? false) ? oldOrderDoc.Warnings : null

                    }],
                    Id = $"{oldOrderDoc.Id}-History",
                    PartitionKey = $"{oldOrderDoc.Id}-History",
                };
            }

            return historyOrderDoc;
        }
    }
}
