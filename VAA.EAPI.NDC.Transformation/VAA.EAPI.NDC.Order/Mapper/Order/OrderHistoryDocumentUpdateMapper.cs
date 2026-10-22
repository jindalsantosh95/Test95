using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Mapper.Order
{
    public static class OrderHistoryDocumentUpdateMapper
    {
        public static OrderDocument HistoryDocumentCreation(OrderDocument oldOrderDoc, OrderDocument historyOrderDoc)
        {
            if (historyOrderDoc != null)
            {
                var orderHistories = historyOrderDoc.OrderHistory ?? Enumerable.Empty<OrderHistory>();
                var latestHistory = orderHistories.Where(x => x != null).OrderBy(GetHistoryVersion).LastOrDefault();

                if (latestHistory == null || GetHistoryVersion(latestHistory) != oldOrderDoc.Version)
                {
                    var updatedHistory = new OrderHistory()
                    {
                        DataList = oldOrderDoc.DataList,
                        OrderItems = oldOrderDoc.OrderItems,
                        OrderStatus = oldOrderDoc.OrderStatus,
                        Version = latestHistory != null ? Convert.ToString(GetHistoryVersion(latestHistory) + 1) : "1",
                        ChangeType = oldOrderDoc.ChangeType,
                        ChangeDateTime = oldOrderDoc.ChangeDateTime.UtcDateTime,
                        Warnings = (oldOrderDoc.Warnings?.Any() ?? false) ? oldOrderDoc.Warnings : null

                    };

                    historyOrderDoc.OrderHistory = orderHistories.Append(updatedHistory);
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

        private static int GetHistoryVersion(OrderHistory history)
        {
            return int.TryParse(history?.Version, out var version) ? version : 0;
        }

        public static OrderDocument ChildHistoryDocumentCreation(OrderDocument historyDocument, OrderDocument childOrderDoc)
        {
            var orderHistories = historyDocument.OrderHistory ?? Enumerable.Empty<OrderHistory>();
            var nonNullOrderHistories = orderHistories.Where(h => h != null);
            var filteredHistories = nonNullOrderHistories.Where(h => h.ChangeType != ChangeTypeEnum.IdentityDocumentDetailsUpdated);
            var matchingHistories = nonNullOrderHistories.Where(h => h.ChangeType == ChangeTypeEnum.IdentityDocumentDetailsUpdated).Where(h => h.DataList.CustomerDetails.Any(cust => cust.IsApisAdded == true && childOrderDoc.DataList.CustomerDetails.Any(childCust => childCust.CustId == cust.CustId)));
            var childHistories = filteredHistories.Concat(matchingHistories).OrderBy(GetHistoryVersion);

            return new OrderDocument()
            {
                Id = $"{childOrderDoc.Id}-History",
                PartitionKey = $"{childOrderDoc.Id}-History",
                DocType = historyDocument.DocType,
                FareTypeRequest = historyDocument.FareTypeRequest,
                PointOfSale = historyDocument.PointOfSale,
                CreationDateTime = historyDocument.CreationDateTime,
                OrderNumber = new EAPI.Domain.Virgin.Models.Order.OrderNumber
                {
                    OrderId = childOrderDoc.Id,
                    OwnerAirline = historyDocument.OrderNumber.OwnerAirline
                },
                BookingReference = historyDocument.BookingReference,
                Parties = historyDocument.Parties,
                OrderHistory = childHistories
            };
        }


        public static OrderDocument ParentHistoryDocumentCreation(OrderDocument historyDocument, OrderDocument parentOrderDoc)
        {
            var orderHistories = historyDocument.OrderHistory ?? Enumerable.Empty<OrderHistory>();
            var nonNullOrderHistories = orderHistories.Where(h => h != null);
            var filteredHistories = nonNullOrderHistories.Where(h => h.ChangeType != ChangeTypeEnum.IdentityDocumentDetailsUpdated);
            var matchingHistories = nonNullOrderHistories.Where(h => h.ChangeType == ChangeTypeEnum.IdentityDocumentDetailsUpdated).Where(h => h.DataList.CustomerDetails.Any(cust => cust.IsApisAdded == true && parentOrderDoc.DataList.CustomerDetails.Any(parentcust => parentcust.CustId == cust.CustId)));
            var parentHistories = filteredHistories.Concat(matchingHistories).OrderBy(GetHistoryVersion);

            return new OrderDocument
            {
                Id = $"{parentOrderDoc.Id}-History",
                PartitionKey = $"{parentOrderDoc.Id}-History",
                DocType = historyDocument.DocType,
                FareTypeRequest = historyDocument.FareTypeRequest,
                PointOfSale = historyDocument.PointOfSale,
                CreationDateTime = historyDocument.CreationDateTime,
                OrderNumber = new EAPI.Domain.Virgin.Models.Order.OrderNumber
                {
                    OrderId = parentOrderDoc.Id,
                    OwnerAirline = historyDocument.OrderNumber.OwnerAirline
                },
                BookingReference = historyDocument.BookingReference,
                Parties = historyDocument.Parties,
                OrderHistory = parentHistories
            };
        }

        public static OrderDocument AddSplitHistory(OrderDocument historyDoc, OrderDocument parentOrderDoc, OrderDocument childOrderDoc)
        {
            var orderHistories = historyDoc.OrderHistory ?? Enumerable.Empty<OrderHistory>();
            var latestHistory = orderHistories.Where(x => x != null).OrderBy(GetHistoryVersion).LastOrDefault();
            var newVersion = latestHistory != null ? (GetHistoryVersion(latestHistory) + 1).ToString() : "1";

            // Create parent history entry
            var parentHistory = new OrderHistory
            {
                DataList = parentOrderDoc.DataList,
                OrderItems = parentOrderDoc.OrderItems,
                OrderStatus = parentOrderDoc.OrderStatus,
                Version = newVersion,
                ChangeType = ChangeTypeEnum.SplitPNR,
                ChangeDateTime = parentOrderDoc.ChangeDateTime.UtcDateTime,
                IsSplit = true
            };

            // Create child history entry
            var childHistory = new OrderHistory
            {
                DataList = childOrderDoc.DataList,
                OrderItems = childOrderDoc.OrderItems,
                OrderStatus = childOrderDoc.OrderStatus,
                Version = newVersion,
                ChangeType = ChangeTypeEnum.SplitPNR,
                ChangeDateTime = childOrderDoc.ChangeDateTime.UtcDateTime,
                IsSplit = true
            };

            historyDoc.OrderHistory = orderHistories.Append(parentHistory).Append(childHistory);

            return historyDoc;
        }

    }
}

