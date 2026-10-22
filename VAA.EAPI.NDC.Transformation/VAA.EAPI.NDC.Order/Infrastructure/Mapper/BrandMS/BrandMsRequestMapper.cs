using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.NDC.Order.Infrastructure.Model;
using VAA.EAPI.NDC.Order.Infrastructure.Model.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Mapper.BrandMS
{
    public class BrandMSRequestMapper
    {
        public static BrandMSCommand MapBrandMSRequest(OfferDetails offer, OfferResponse offerResponse, bool isPartialFlown = false)
        {
            bool isPrivateFare = offerResponse?.FareTypeRequest?.FareTypeCode == Constants.Application.PrivateFareTypeCode;
            bool includeTicketDesignator = ShouldIncludeTicketDesignator(offerResponse?.FareTypeRequest?.FareTypeCode);
            var accountCode = offerResponse.Parties.Participants.First(x => x.PartyType.Value == Participant.PartyTypeEnum.Seller).AccountCode;

            return new BrandMSCommand()
            {
                Currency = offerResponse?.Pos?.CurrencyCode,
                Itinerary = GetItinerariesForOffer(offer, offerResponse, isPrivateFare, includeTicketDesignator, isPartialFlown),
                PseudoCityCode = (string.Equals(offerResponse?.FareTypeRequest?.FareTypeCode, Constants.Application.FareTypeCode, StringComparison.Ordinal) && offerResponse?.DataList?.PriceClass?.Any(x => x.TicketDesigCode == Constants.Application.TicketDesigCode) == true) || isPrivateFare ? Constants.Application.PseudoCityCode : null,
                AccountCode = isPrivateFare ? accountCode : null
              
            };
        }

        public static BrandMSCommandPTC MapBrandMSPTCRequest(OfferDetails offer, OfferResponse offerResponse, bool isPartialFlown = false)
        {
            bool isPrivateFare = offerResponse?.FareTypeRequest?.FareTypeCode == Constants.Application.PrivateFareTypeCode;
            bool includeTicketDesignator = ShouldIncludeTicketDesignator(offerResponse?.FareTypeRequest?.FareTypeCode);
            var accountCode = offerResponse.Parties.Participants.First(x => x.PartyType.Value == Participant.PartyTypeEnum.Seller).AccountCode;

            return new BrandMSCommandPTC()
            {
                Currency = offerResponse?.Pos?.CurrencyCode,
                Itinerary = GetItinerariesForOffer(offer, offerResponse, isPrivateFare, includeTicketDesignator, isPartialFlown),
                PseudoCityCode = (string.Equals(offerResponse?.FareTypeRequest?.FareTypeCode, Constants.Application.FareTypeCode, StringComparison.Ordinal) && offerResponse?.DataList?.PriceClass?.Any(x => x.TicketDesigCode == Constants.Application.TicketDesigCode) == true) || isPrivateFare ? Constants.Application.PseudoCityCode : null,
                AccountCode = isPrivateFare ? accountCode : null
            };
        }

        private static List<BrandQueryRequestTrip> GetItinerariesForOffer(OfferDetails offer, OfferResponse offerResponse, bool isPrivateFare, bool includeTicketDesignator, bool isPartialFlown)
        {
            var itinerary = new List<BrandQueryRequestTrip>();
            var segments = new List<BrandQueryRequestSegment>();

            var passengerTypeCodes = offerResponse.DataList.CustomerDetails.Select(c => c.Ptc);
            passengerTypeCodes = passengerTypeCodes.Select(code => code == Constants.Application.YoungPTCCode ? Constants.Application.YouthPTCCode : code);
            var flownFlightSegments = isPartialFlown ? offerResponse.DataList.FlightSegments.Where(x => DateTimeOffset.Parse(x.ScheduledDepartureTime.LocalDateTime) < DateTimeOffset.Now).Select(x => x.SegmentId) : null;
            var offerSeg = flownFlightSegments != null ? offerResponse.DataList.OfferSegments.Where(o => flownFlightSegments.Contains(o.FlightSegmentRefId)).Select(x => x.OfferSegmentId) : Enumerable.Empty<string>();

            foreach (var service in offer.OfferItems.FirstOrDefault()?.Services)
            {
                foreach (var refId in service.ProductSegmentAssociation.OfferSegmentRefIds.Where(seg => !offerSeg.Contains(seg)))
                {
                    var priceClass = offerResponse.DataList.PriceClass.FirstOrDefault(pC => pC.Id == service.PriceClassRefIds.FirstOrDefault());
                    segments.Add(GetSegment(offerResponse.DataList.FlightSegments.FirstOrDefault(fSeg => fSeg.SegmentId == offerResponse.DataList.OfferSegments.FirstOrDefault(f => f.OfferSegmentId == refId)?.FlightSegmentRefId), priceClass?.FareBasisCode, includeTicketDesignator ? priceClass?.TicketDesigCode : null, offerResponse.DataList.OfferSegments.FirstOrDefault(oSeg => oSeg.OfferSegmentId == refId)));
                }
            }

            itinerary.Add(new BrandQueryRequestTrip()
            {
                Segments = segments,
                PassengerTypeCodes = passengerTypeCodes,
                Source = isPrivateFare ? Constants.Application.SourceSystem : null
            });

            return itinerary;
        }

        private static BrandQueryRequestSegment GetSegment(VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment fseg, string fareBasisCode, string ticketDesignator, VAA.EAPI.Domain.Virgin.Models.Products.Flight.OfferSegment offerSegment)
        {
            return new BrandQueryRequestSegment()
            {
                Carrier = fseg.MarketingCarrier,
                FlightNumber = Convert.ToInt16(fseg.MarketingFlightNumber),
                DepartureDate = Convert.ToDateTime(fseg.ScheduledDepartureTime.LocalDateTime),
                Origin = fseg.OriginAirportCode,
                Destination = fseg.DestinationAirportCode,
                Cabin = Constants.Application.FareFamilyDictionary.First(x => x.Value.Item2 == Convert.ToInt16(offerSegment.CabinType.CabinCode)).Value.Item1,
                FareBasis = fareBasisCode,
                TicketDesignator = ticketDesignator
            };
        }

        public static BrandMSCommand MapBrandMSOrderRequest(OrderDocument orderDoc)
        {
            bool isPrivateFare = orderDoc?.FareTypeRequest?.FareTypeCode == Constants.Application.PrivateFareTypeCode;
            bool includeTicketDesignator = ShouldIncludeTicketDesignator(orderDoc?.FareTypeRequest?.FareTypeCode);
            var accountCode = orderDoc.Parties.Participants.First(x => x.PartyType.Value == Participant.PartyTypeEnum.Seller).AccountCode;

            return new BrandMSCommand()
            {
                Currency = orderDoc.PointOfSale?.CurrencyCode,
                Itinerary = GetItinerariesFromOrderDoc(orderDoc, isPrivateFare, includeTicketDesignator),
                PseudoCityCode = isPrivateFare ? Constants.Application.PseudoCityCode : null,
                AccountCode = isPrivateFare ? accountCode : null
            };
        }

        public static BrandMSCommandPTC MapBrandMSPTCOrderRequest(OrderDocument orderDoc)
        {
            bool isPrivateFare = orderDoc?.FareTypeRequest?.FareTypeCode == Constants.Application.PrivateFareTypeCode;
            bool includeTicketDesignator = ShouldIncludeTicketDesignator(orderDoc?.FareTypeRequest?.FareTypeCode);
            var accountCode = orderDoc.Parties.Participants.First(x => x.PartyType.Value == Participant.PartyTypeEnum.Seller).AccountCode;

            return new BrandMSCommandPTC()
            {
                Currency = orderDoc.PointOfSale?.CurrencyCode,
                Itinerary = GetItinerariesFromOrderDoc(orderDoc, isPrivateFare, includeTicketDesignator),
                PseudoCityCode = isPrivateFare ? Constants.Application.PseudoCityCode : null,
                AccountCode = isPrivateFare ? accountCode : null
            };
        }

        private static IEnumerable<BrandQueryRequestTrip> GetItinerariesFromOrderDoc(OrderDocument orderDoc, bool isPrivateFare, bool includeTicketDesignator)
        {
            var passengerTypeCodes = (orderDoc.DataList?.CustomerDetails?.Select(c => c.ItaPtc) ?? Enumerable.Empty<string>())
                .Select(code => code == Constants.Application.YoungPTCCode ? Constants.Application.YouthPTCCode : code);
            var services = orderDoc.OrderItems?.First().Services;
            var segmentList = from orderItem in orderDoc.OrderItems.Select(x => x.Services).First()
                              from segment in orderItem.SegmentRefIds
                              let segmentDetail = orderDoc.DataList.SegmentDetails.FirstOrDefault(s => s.SegmentId == segment)
                              where segmentDetail?.Status?.DeliveryStatusCode != "Delivered"
                              select segment;

            return new List<BrandQueryRequestTrip>{new()
            {
                Segments = from seg in segmentList.Where(x => Constants.Application.FlightStatusCode.Contains(orderDoc.DataList.SegmentDetails.First(s => s.SegmentId == x).Status.Code))
                           select new BrandQueryRequestSegment()
                           {
                               Carrier = orderDoc.DataList?.SegmentDetails?.FirstOrDefault(x => x.SegmentId == seg).MarketingCarrier,
                               FlightNumber = Convert.ToInt16(orderDoc.DataList?.SegmentDetails?.FirstOrDefault(x => x.SegmentId == seg).MarketingFlightNumber),
                               DepartureDate = Convert.ToDateTime(orderDoc.DataList?.SegmentDetails?.FirstOrDefault(x => x.SegmentId == seg).ScheduledDepartureTime?.LocalDateTime),
                               Origin = orderDoc.DataList?.SegmentDetails?.FirstOrDefault(x => x.SegmentId == seg).OriginAirportCode,
                               Destination = orderDoc.DataList?.SegmentDetails?.FirstOrDefault(x => x.SegmentId == seg).DestinationAirportCode,
                               Cabin = Constants.Application.FareFamilyDictionary.First(dic => dic.Value.Item2 == Convert.ToInt16(orderDoc.DataList?.SegmentDetails?.FirstOrDefault(x => x.SegmentId == seg).CabinType?.CabinCode)).Value.Item1,
                               FareBasis = GetPriceClassForSegment(orderDoc, services, seg)?.FareBasisCode,
                               TicketDesignator = includeTicketDesignator ? GetPriceClassForSegment(orderDoc, services, seg)?.TicketDesigCode : null
                           },
                PassengerTypeCodes = passengerTypeCodes,
                Source = isPrivateFare ? Constants.Application.SourceSystem : null
            } };

        }

        private static VAA.EAPI.Domain.Virgin.Models.Common.PriceClass GetPriceClassForSegment(OrderDocument orderDoc, IEnumerable<OrderService> services, string segmentId)
        {
            var priceClassReference = services?.FirstOrDefault(service => service.SegmentRefIds?.Contains(segmentId) == true)?.PriceClassRefIds?.FirstOrDefault();
            return orderDoc.DataList?.PriceClass?.FirstOrDefault(priceClass => priceClass.Id == priceClassReference);
        }

        private static bool ShouldIncludeTicketDesignator(string fareTypeCode)
        {
            return fareTypeCode == Constants.Application.PrivateFareTypeCode || fareTypeCode == Constants.Application.FareTypeCode;
        }

        public static OrderDocument UpdateFares(OrderDocument orderMSOrderDoc, OrderDocument sabreOrderDoc)
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

                    if (matchingOrderItem.OriginalSalePrice?.Any() == true)
                    {
                        sabreItem.OriginalSalePrice = matchingOrderItem.OriginalSalePrice;
                    }
                }
            }
            return sabreOrderDoc;
        }
    }
}
