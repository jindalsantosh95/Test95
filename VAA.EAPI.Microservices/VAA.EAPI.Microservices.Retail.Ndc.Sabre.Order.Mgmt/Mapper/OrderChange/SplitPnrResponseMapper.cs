using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange
{
    public static class SplitPnrResponseMapper
    {
        public static OrderDocument SplitParentOrderDocument(GetReservationRS response, SplitPnrRequest request)
        {
            var parentOrderDoc = Json.Deserialize<OrderDocument>(Json.Serialize(request.OrderDocument));
             
            parentOrderDoc.DataList.CustomerDetails = parentOrderDoc.DataList.CustomerDetails.Where(cd => !request.splitPaxId.Contains(cd.CustId));

            foreach (var splitpax in request.splitPaxId)
            {
                foreach (var orderItem in parentOrderDoc.OrderItems.Where(x => x.Services.SelectMany(x => x.ServiceBeneficiaries).Contains(splitpax)))
                {
                    foreach(var service in orderItem.Services)
                    {
                        if (service.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.FlightEnum)
                        {
                            if (service.ServiceBeneficiaries.Count() > 1)
                            {
                                service.ServiceBeneficiaries = service.ServiceBeneficiaries.Where(sb => !sb.Is(splitpax));
                            }
                            else
                            {
                                parentOrderDoc.OrderItems = parentOrderDoc.OrderItems.Where(oi => !oi.OrderItemId.Is(orderItem.OrderItemId));
                            }
                        }
                        else if (service.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum)
                        {
                            parentOrderDoc.OrderItems = parentOrderDoc.OrderItems.Where(oi => !oi.OrderItemId.Is(orderItem.OrderItemId));
                        }
                        else
                        {
                            orderItem.Services = orderItem.Services.Where(s => !s.ServiceBeneficiaries.Contains(splitpax));
                        }
                    }
                   
                    orderItem.AccountableDocumentRefs = orderItem.AccountableDocumentRefs?.Where(a => !a.CustomerRefId.Contains(splitpax));

                }
            }

            if (parentOrderDoc.DataList.OrderInstructions.Any())
            {
                parentOrderDoc.DataList.OrderInstructions = parentOrderDoc.DataList.OrderInstructions.Where(oi => !request.splitPaxId.Contains(oi.ItemText.Split('/')[1]));
            }

            var airSegment = (response.Item as ReservationPNRB)?.PassengerReservation?.Segments?.Segment?.FirstOrDefault()?.Item as SegmentTypePNRBSegmentAir;

            var childRlocDetail = new ChildRLOCDetails
            {
                ChildRLOC = (response.Item as ReservationPNRB).BookingDetails.RecordLocator,
                ChildExternalRLOC = !string.IsNullOrEmpty(airSegment?.AirlineRefId) && airSegment.AirlineRefId.Length >= 6 ? airSegment.AirlineRefId[^6..] : null,
                ChildRLOCLink = true,
                SplitDateTime = DateTime.UtcNow.ToString(Constants.Application.DateTimeFormat)
            };

            parentOrderDoc.BookingReference.SplitRLOC ??= new SplitRLOC
            {
                ChildRLOCDetails = []
            };

            parentOrderDoc.BookingReference.SplitRLOC.ChildRLOCDetails = parentOrderDoc.BookingReference.SplitRLOC.ChildRLOCDetails.Concat([childRlocDetail]);

            var baggageIds = parentOrderDoc.OrderItems.SelectMany(oi => oi.Services).Where(s => s?.BaggageRefIds != null).SelectMany(service => service.BaggageRefIds).Distinct();
            parentOrderDoc.DataList.BaggageAllowances = parentOrderDoc.DataList.BaggageAllowances?.Where(ba => baggageIds.Contains(ba.BaggageAllowanceId));

            var orderItemId = parentOrderDoc.Id;

            parentOrderDoc.OrderItems = parentOrderDoc.OrderItems.Select((orderItem, i) =>
            {
                orderItem.OrderItemId = $"{orderItemId}-{i + 1}";
                return orderItem;
            }).ToArray();

            parentOrderDoc.ChangeType = Domain.ChangeTypeEnum.SplitPNR;
            parentOrderDoc.ChangeDateTime = new() { UtcDateTime = DateTime.UtcNow.ToString(Constants.Application.ZuluDateTimeFormat) };
            parentOrderDoc.Version = parentOrderDoc.Version + 1;

            return parentOrderDoc;
        }

        public static OrderDocument SplitChildOrderDocument(GetReservationRS response, SplitPnrRequest request, int version)
        {
            var childOrderDoc = Json.Deserialize<OrderDocument>(Json.Serialize(request.OrderDocument));

            childOrderDoc.DataList.CustomerDetails = childOrderDoc.DataList.CustomerDetails.Where(cd => request.splitPaxId.Contains(cd.CustId));

            if (childOrderDoc.DataList.OrderInstructions.Any())
            {
                childOrderDoc.DataList.OrderInstructions = childOrderDoc.DataList.OrderInstructions.Where(oi => request.splitPaxId.Contains(oi.ItemText.Split('/')[1]));
            }

            childOrderDoc.CreationDateTime.UtcDateTime = DateTime.UtcNow.ToString(Constants.Application.DateTimeFormat);

            var airSegment = (response.Item as ReservationPNRB)?.PassengerReservation?.Segments?.Segment?.FirstOrDefault()?.Item as SegmentTypePNRBSegmentAir;

            var childPnr = (response.Item as ReservationPNRB).BookingDetails.RecordLocator;
            var childDeltaPnr = !string.IsNullOrEmpty(airSegment?.AirlineRefId) && airSegment.AirlineRefId.Length >= 6 ? airSegment.AirlineRefId[^6..] : null;
            string orderId = $"VS932{childPnr}{DateTime.UtcNow:yy}";

            childOrderDoc.OrderNumber.OrderId = orderId;
            childOrderDoc.Id = orderId;
            childOrderDoc.PartitionKey = orderId;

            childOrderDoc.BookingReference = new BookingReference()
            {
                RLOC = childPnr,
                RLOCCreateDate = DateTime.UtcNow.ToString(Constants.Application.DateFormat),
                RLOCCreateDateTime = DateTime.UtcNow.ToString(Constants.Application.DateTimeFormat),
                ExternalRecordLocator = childDeltaPnr,
                ExternalSystem = "1S",
                SplitRLOC = new SplitRLOC()
                {
                    ParentRLOC = request.OrderDocument.BookingReference.RLOC,
                    ParentExternalRLOC = request.OrderDocument.BookingReference.ExternalRecordLocator,
                    ParentRLOCLink = true
                }
            };

            var parentPaxIds = request.OrderDocument.DataList.CustomerDetails?.Where(cd => !request.splitPaxId.Contains(cd.CustId)).Select(cd => cd.CustId);

            foreach (var parentPax in parentPaxIds)
            {
                foreach (var orderItem in childOrderDoc.OrderItems.Where(x => x.Services.SelectMany(x => x.ServiceBeneficiaries).Contains(parentPax)))
                {
                    foreach (var service in orderItem.Services)
                    {
                        if (service.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.FlightEnum)
                        {
                            if (service.ServiceBeneficiaries.Count() > 1)
                            {
                                service.ServiceBeneficiaries = service.ServiceBeneficiaries.Where(sb => !sb.Is(parentPax));
                            }
                            else
                            {
                                childOrderDoc.OrderItems = childOrderDoc.OrderItems.Where(oi => !oi.OrderItemId.Is(orderItem.OrderItemId));
                            }
                        }
                        else if (service.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum)
                        {
                            childOrderDoc.OrderItems = childOrderDoc.OrderItems.Where(oi => !oi.OrderItemId.Is(orderItem.OrderItemId));
                        }
                        else
                        {
                            orderItem.Services = orderItem.Services.Where(s => !s.ServiceBeneficiaries.Contains(parentPax));
                        }
                    }

                    orderItem.AccountableDocumentRefs = orderItem.AccountableDocumentRefs?.Where(a => !a.CustomerRefId.Contains(parentPax));
                }
            }

            var baggageIds = childOrderDoc.OrderItems.SelectMany(oi => oi.Services).Where(s => s?.BaggageRefIds != null).SelectMany(service => service.BaggageRefIds).Distinct();
            childOrderDoc.DataList.BaggageAllowances = childOrderDoc.DataList.BaggageAllowances?.Where(ba => baggageIds.Contains(ba.BaggageAllowanceId));

            int index = 1;
            foreach (var orderItem in childOrderDoc.OrderItems)
            {
                orderItem.OrderItemId = $"{orderId}-{index++}";
            }

            childOrderDoc.ChangeType = Domain.ChangeTypeEnum.SplitPNR;
            childOrderDoc.ChangeDateTime = new() { UtcDateTime = DateTime.UtcNow.ToString(Constants.Application.ZuluDateTimeFormat) };
            childOrderDoc.Version = version;

            return childOrderDoc;
        }

    }
}
