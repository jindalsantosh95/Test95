using System;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Data
{
    public static class OrderChangeHistoryUpdateMapper
    {
        public static OrderDocument UpdateOrderDocument(OrderDocument orderDoc, OrderDocument orderDocMgmt)
        {
            orderDocMgmt.OrderItems = orderDocMgmt.OrderItems.Select(orderItem =>
            {
                var orderMgmtOrderItem = orderItem.Services.SelectMany(x => x.ServiceBeneficiaries).Distinct();
                var oldOrderItem = orderDoc.OrderItems.FirstOrDefault(x => x.Services != null && x.Services.Any(service => service.Product.ProductType == orderItem.Services.First().Product.ProductType && service.ServiceBeneficiaries.Any(srv => orderMgmtOrderItem.Contains(srv))));

                if (oldOrderItem != null)
                {
                    orderItem.ExistingOrderItemId = oldOrderItem.OrderItemId;
                    orderItem.OrderItemId = GerOrderItemID(oldOrderItem.OrderItemId);
                }
                return orderItem;
            });
            UpdateRetainedJourneySeatOrderItems(orderDoc, orderDocMgmt);

            orderDocMgmt.FareTypeRequest = orderDoc.FareTypeRequest;
            orderDocMgmt.ChangeType = orderDocMgmt.ChangeType;
            orderDocMgmt.Version = Convert.ToInt32(orderDoc.Version) + 1;
            orderDocMgmt.ChangeDateTime = orderDocMgmt.ChangeDateTime;

            return orderDocMgmt;
        }

        private static void UpdateRetainedJourneySeatOrderItems(OrderDocument orderDoc, OrderDocument orderDocMgmt)
        {
            if (orderDoc.OrderItems.Any(x => x.Services.Any(srv => srv.Product != null && srv.Product.ProductType == Product.ProductTypes.SeatEnum)))
            {
                var retainedJourneySegments = orderDoc.DataList.SegmentDetails.Where(oldSeg => orderDocMgmt.DataList.SegmentDetails.Any(seg =>
                                                               seg.OriginAirportCode == oldSeg.OriginAirportCode &&
                                                               seg.DestinationAirportCode == oldSeg.DestinationAirportCode &&
                                                               seg.MarketingFlightNumber == oldSeg.MarketingFlightNumber &&
                                                               seg.ClassOfService == oldSeg.ClassOfService &&
                                                               Convert.ToDateTime(seg.ScheduledDepartureTime.LocalDateTime) == Convert.ToDateTime(oldSeg.ScheduledDepartureTime.LocalDateTime)
                                                               )).Select(x => x.SegmentId);

                if (retainedJourneySegments.Any())
                {
                    var seatOrderItems = orderDoc.OrderItems.Where(oi => oi.Services.Any(srv => srv.Product.ProductType == Product.ProductTypes.SeatEnum));
                    foreach (var orderItem in seatOrderItems)
                    {
                        if (orderItem.Services.Any(srv => srv.SegmentRefIds.Any(segId => retainedJourneySegments.Contains(segId))))
                        {
                            var retainedOrderItems = new EAPI.Domain.Virgin.Models.Order.OrderItems()
                            {
                                OrderItemId = orderItem.OrderItemId,
                                OwnerAirline = orderItem.OwnerAirline,
                                Services = orderItem.Services,
                                PriceDetails = orderItem.PriceDetails,
                                AccountableDocumentRefs = orderItem.AccountableDocumentRefs,
                                ItemStatus = orderItem.ItemStatus,
                                PaymentDetails = orderItem.PaymentDetails
                            };
                            orderDocMgmt.OrderItems = orderDocMgmt.OrderItems.Append(retainedOrderItems);
                        }
                    }
                }
            }
        }

        private static string GerOrderItemID(string orderItemId)
        {
            var lastPart = orderItemId.Split('-').Last();
            var prefix = orderItemId.Substring(0, orderItemId.LastIndexOf('-') + 1);

            if (char.IsLetter(lastPart.Last()))
            {
                var numberPart = lastPart.Substring(0, lastPart.Length - 1);
                var letter = lastPart.Last();
                if (letter < 'Z')
                {
                    char nextLetter = (char)(letter + 1);
                    return prefix + numberPart + nextLetter;
                }
            }
            else
            {
                return orderItemId + "A";
            }

            return orderItemId;
        }
    }
}
