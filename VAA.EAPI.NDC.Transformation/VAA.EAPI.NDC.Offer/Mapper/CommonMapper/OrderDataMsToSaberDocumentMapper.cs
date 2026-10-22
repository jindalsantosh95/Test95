using System;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;

namespace VAA.EAPI.NDC.Offer.Mapper.CommonMapper
{
    public static class OrderDataMsToSaberDocumentMapper
    {
        public static OrderDocument RetainPriceDetails(OrderDocument orderMSOrderDoc, OrderDocument sabreOrderDoc)
        {
            var ticketedPnr = sabreOrderDoc.OrderItems.First().AccountableDocumentRefs;
            var priceClassInfo = sabreOrderDoc.DataList.PriceClass?.FirstOrDefault()?.FareBasisCode;
            var productName = sabreOrderDoc.DataList.PriceClass?.FirstOrDefault()?.ProductName;
            var fareBasisCode = sabreOrderDoc.DataList.PriceClass.FirstOrDefault()?.FareBasisCode;

            sabreOrderDoc.PointOfSale = new()
            {
                CityCode = orderMSOrderDoc.PointOfSale?.CityCode,
                CountryCode = orderMSOrderDoc.PointOfSale?.CountryCode,
                CurrencyCode = orderMSOrderDoc.OrderItems.FirstOrDefault()?.PriceDetails?.FirstOrDefault()?.Amount?.BaseCurrency
            };

            if (orderMSOrderDoc.OrderItems.All(x => x.ExistingOrderItemId != null))
            {
                sabreOrderDoc.OrderItems = sabreOrderDoc.OrderItems.Select(oi =>
                {
                    var orderMgmtOrderItem = oi.Services.SelectMany(x => x.ServiceBeneficiaries).Distinct();
                    var existingOrderItem = orderMSOrderDoc.OrderItems.FirstOrDefault(x => x.Services?.SelectMany(service => service.ServiceBeneficiaries).Any(srv => orderMgmtOrderItem.Contains(srv)) == true);

                    if (existingOrderItem != null)
                    {
                        oi.OrderItemId = existingOrderItem.OrderItemId;
                    }

                    return oi;
                });
            }

            var oItems = from ndcOI in (orderMSOrderDoc.DataList?.OrderInstructions ?? Enumerable.Empty<OrderInstructions>())
                         from sabreOI in (sabreOrderDoc.DataList?.OrderInstructions ?? Enumerable.Empty<OrderInstructions>())
                         where sabreOI.ItemCode == ndcOI.ItemCode && sabreOI.InstructionId == ndcOI.InstructionId
                         select new { sabreOI, ndcOI };

            foreach (var item in oItems)
            {
                item.sabreOI.ItemText = item.ndcOI.ItemText;
            }

            if (ticketedPnr is null || priceClassInfo is null || fareBasisCode is null || sabreOrderDoc.OrderItems.First().PriceDetails.First().FareCalculation is null)
            {
                if (priceClassInfo is null) sabreOrderDoc.DataList.PriceClass = orderMSOrderDoc.DataList.PriceClass;

                if (productName is null) sabreOrderDoc.DataList.PriceClass = sabreOrderDoc.DataList.PriceClass.Select(pc =>
                {
                    pc.ProductName = orderMSOrderDoc.DataList.PriceClass.FirstOrDefault(p => p.Id == pc.Id)?.ProductName;
                    pc.FareBasisCode = orderMSOrderDoc.DataList.PriceClass.FirstOrDefault(p => p.Id == pc.Id)?.FareBasisCode;
                    return pc;
                });

                sabreOrderDoc.TimeLimits = orderMSOrderDoc.TimeLimits;
                var orderDocuments = from orderItem in orderMSOrderDoc.OrderItems.Where(x => x.Services.First().Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.FlightEnum)
                                     from service in orderItem.Services
                                     from sabreOrderItem in sabreOrderDoc.OrderItems.Where(x => x.Services.First().Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.FlightEnum)
                                     from sabreService in sabreOrderItem.Services
                                     where service.ServiceBeneficiaries.Any(b => sabreService.ServiceBeneficiaries.Contains(b)) && service.SegmentRefIds.SequenceEqual(sabreService.SegmentRefIds)
                                     select new { orderItem, sabreOrderItem };

                foreach (var docs in orderDocuments)
                {
                    if (docs.orderItem.PriceDetails != null) docs.sabreOrderItem.PriceDetails = docs.orderItem.PriceDetails; //null check to avoid assigning value from previous order doc with null price details (in case of OCN )
                    foreach (var service in docs.sabreOrderItem.Services)
                    {
                        if (service.PriceClassRefIds.IsNullOrEmpty()) service.PriceClassRefIds = docs.orderItem.Services.FirstOrDefault(s => s.SegmentRefIds.SequenceEqual(service.SegmentRefIds))?.PriceClassRefIds;
                    }
                }
            }

            foreach (var sabreItem in sabreOrderDoc.OrderItems)
            {
                var sabreProductType = sabreItem.Services.Select(s => s?.Product?.ProductType).FirstOrDefault();

                if (!sabreProductType.HasValue)
                {
                    sabreItem.PaymentDetails = null;
                    continue;
                }

                var matchingOrderItem = orderMSOrderDoc.OrderItems.FirstOrDefault(orderItem =>
                {
                    var orderProductType = orderItem.Services.Select(s => s?.Product?.ProductType).FirstOrDefault();

                    if (!orderProductType.HasValue)
                        return false;

                    var orderBeneficiaries = orderItem.Services.SelectMany(s => s.ServiceBeneficiaries ?? Enumerable.Empty<string>()).Distinct().OrderBy(x => x);
                    var sabreBeneficiaries = sabreItem.Services.SelectMany(s => s.ServiceBeneficiaries ?? Enumerable.Empty<string>()).Distinct().OrderBy(x => x);
                    var orderSegments = orderItem.Services.SelectMany(s => s.SegmentRefIds ?? Enumerable.Empty<string>()).Distinct().OrderBy(x => x);
                    var sabreSegments = sabreItem.Services.SelectMany(s => s.SegmentRefIds ?? Enumerable.Empty<string>()).Distinct().OrderBy(x => x);

                    return orderProductType == sabreProductType && orderBeneficiaries.SequenceEqual(sabreBeneficiaries) && orderSegments.SequenceEqual(sabreSegments);
                });

                if (matchingOrderItem != null)
                {
                    sabreItem.PaymentDetails = matchingOrderItem.PaymentDetails;
                }
            }

            return sabreOrderDoc;
        }

        public static OrderDocument MapOrderItems(OrderDocument orderdoc, OrderDocument sabredoc)
        {
            foreach (var orderItem in orderdoc.OrderItems)
            {
                var orderBeneficiaries = orderItem.Services.SelectMany(s => s.ServiceBeneficiaries).Distinct();

                var matchingSabreItem = sabredoc.OrderItems.FirstOrDefault(sabreItem =>
                {
                    var sabreBeneficiaries = sabreItem.Services.SelectMany(s => s.ServiceBeneficiaries).Distinct();

                    return orderBeneficiaries.All(b => sabreBeneficiaries.Contains(b)) && sabreBeneficiaries.All(b => orderBeneficiaries.Contains(b));
                });

                if (matchingSabreItem != null)
                {
                    matchingSabreItem.OrderItemId = orderItem.OrderItemId;
                    matchingSabreItem.ExistingOrderItemId = orderItem.ExistingOrderItemId;
                    matchingSabreItem.PaymentDetails = orderItem.PaymentDetails;
                }
            }

            if (orderdoc.OrderHistory != null)
            {
                sabredoc.OrderHistory = (sabredoc.OrderHistory ?? Enumerable.Empty<OrderHistory>()).Concat(orderdoc.OrderHistory);
            }
            return sabredoc;
        }
    }
}
