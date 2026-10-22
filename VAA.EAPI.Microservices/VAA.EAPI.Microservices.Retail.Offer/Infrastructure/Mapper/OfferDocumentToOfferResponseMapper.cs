using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Microservices.Retail.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Mapper
{
    public class OfferDocumentToOfferResponseMapper
    {
        public static OfferResponse Map(OfferDocument offerDetails)
        {
            DataList filteredDataList = new()
            {
                CustomerDetails = new List<Customer>(),
                OfferSegments = new List<OfferSegment>(),
                Journey = new List<Journey>(),
                Penalty = new List<Penalty>(),
                ProductComponents = new List<ProductComponent>(),
                BaggageAllowances = new List<BaggageAllowances>(),
                FlightSegments = new List<Segment>(),
                OriginDestinationAssociations = new List<OriginDestinationAssociation>(),
                PriceClass = new List<PriceClass>()
            };

            foreach (var offerItem in offerDetails.FlightOfferResponse.Offers.SelectMany(x => x.OfferItems))
            {
                foreach (var service in offerItem.Services ?? Enumerable.Empty<OfferService>())
                {
                    FilterOfferSegments(offerDetails, service, filteredDataList);
                    FilterJourney(offerDetails, service, filteredDataList);
                    FilterPaneltyInfo(offerDetails, service, filteredDataList);
                    FilterPriceClassInfo(offerDetails, service, filteredDataList);
                    FilterBaggageInfo(offerDetails, service, filteredDataList);
                    FilterProductComponents(offerDetails, filteredDataList);
                    FilterOnDInfo(offerDetails, service, filteredDataList);
                }
            }

            filteredDataList.CustomerDetails = offerDetails?.FlightOfferResponse?.DataList?.CustomerDetails;
            offerDetails.FlightOfferResponse.DataList = filteredDataList;

            FormatOfferExpiryLimitDateTime(offerDetails);
            FormatTicketingTLDateTime(offerDetails);

            return new()
            {
                DataList = offerDetails.FlightOfferResponse.DataList,
                Offers = offerDetails.FlightOfferResponse.Offers.ToList(),
                ShoppingResponseId = offerDetails.ShoppingResponseId,
                Pos = offerDetails.FlightOfferResponse.Pos,
                Parties = offerDetails.FlightOfferResponse.Parties,
                OrderId = offerDetails.OrderId,
                DocType = offerDetails.DocType,
                FareTypeRequest = new FareTypeRequest()
                {
                    FareTypeCode = offerDetails?.FareTypeRequest?.FareTypeCode,
                    FareSubTypeCode = offerDetails?.FareTypeRequest?.FareSubTypeCode
                },
                TotalOffers = offerDetails.FlightOfferResponse.Offers?.Count() ?? 0,
                MetaPartnerId = offerDetails.MetaPartnerId,
                FormOfPayments = offerDetails?.FormOfPayments
            };
        }

        private static void FormatOfferExpiryLimitDateTime(OfferDocument offerDetails)
        {
            if (DateTime.TryParseExact(offerDetails.FlightOfferResponse.Offers.First().OfferExpirationTimeLimit, Constants.Application.CosmosReaderDateTimeFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime offerExpirationTimeLimit))
            {
                offerDetails.FlightOfferResponse.Offers.First().OfferExpirationTimeLimit = offerExpirationTimeLimit.ToString(Constants.Application.ZuluDateTimeFormat);
            }
        }

        private static void FormatTicketingTLDateTime(OfferDocument offerDetails)
        {
            if (DateTime.TryParseExact(offerDetails.FlightOfferResponse.Offers.First().TicketingTimeLimit, Constants.Application.CosmosReaderDateTimeFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime ticketingTimeLimit))
            {
                offerDetails.FlightOfferResponse.Offers.First().TicketingTimeLimit = ticketingTimeLimit.ToString(Constants.Application.ZuluDateTimeFormat);
            }
        }

        private static void FilterOnDInfo(OfferDocument offerDetails, OfferService service, DataList filteredDataList)
        {
            if (offerDetails.FlightOfferResponse.DataList.OriginDestinationAssociations != null)
            {
                foreach (var ond in offerDetails.FlightOfferResponse.DataList.OriginDestinationAssociations)
                {
                    foreach (var jrid in service.JourneyRefIds)
                    {
                        if (ond.JourneyRefIds.Any(jid => jid == jrid))
                        {
                            ond.JourneyRefIds = service.JourneyRefIds.ToList();
                            filteredDataList.OriginDestinationAssociations.Add(ond);
                        }
                    }
                }

                filteredDataList.OriginDestinationAssociations = filteredDataList.OriginDestinationAssociations.DistinctBy(d => d.OdLabel).OrderBy(x => x.OdLabel).ToList();
            }
            else
            {
                filteredDataList.OriginDestinationAssociations = null;
            }
        }

        private static void FilterProductComponents(OfferDocument offerDetails, DataList filteredDataList)
        {
            if (offerDetails.FlightOfferResponse.DataList.ProductComponents != null)
            {
                foreach (var pcm in offerDetails.FlightOfferResponse.DataList.ProductComponents)
                    filteredDataList.ProductComponents.Add(pcm);

                filteredDataList.ProductComponents = filteredDataList.ProductComponents.OrderBy(x => x.Id).ToList();
            }
            else
            {
                filteredDataList.ProductComponents = null;
            }
        }

        private static void FilterBaggageInfo(OfferDocument offerDetails, OfferService service, DataList filteredDataList)
        {
            if (offerDetails.FlightOfferResponse.DataList.BaggageAllowances != null)
            {
                foreach (var ba in offerDetails.FlightOfferResponse.DataList.BaggageAllowances)
                    if (service.BaggageRefIds != null)
                        foreach (var barid in service.BaggageRefIds)
                            if (barid == ba.BaggageAllowanceId && filteredDataList.BaggageAllowances.All(x => x.BaggageAllowanceId != barid))
                                filteredDataList.BaggageAllowances.Add(ba);

                filteredDataList.BaggageAllowances = filteredDataList.BaggageAllowances.OrderBy(x => x.BaggageAllowanceId).ToList();
            }
            else
            {
                filteredDataList.BaggageAllowances = null;
            }
        }

        private static void FilterPriceClassInfo(OfferDocument offerDetails, OfferService service, DataList filteredDataList)
        {
            if (offerDetails.FlightOfferResponse.DataList.PriceClass != null)
            {
                foreach (var pc in offerDetails.FlightOfferResponse.DataList.PriceClass)
                    foreach (var pcrid in service.PriceClassRefIds)
                        if (pcrid == pc.Id && filteredDataList.PriceClass.All(x => x.Id != pcrid))
                            filteredDataList.PriceClass.Add(pc);

                filteredDataList.PriceClass = filteredDataList.PriceClass.OrderBy(x => x.Id).ToList();
            }
            else
            {
                filteredDataList.PriceClass = null;
            }
        }

        private static void FilterPaneltyInfo(OfferDocument offerDetails, OfferService service, DataList filteredDataList)
        {
            if (offerDetails.FlightOfferResponse.DataList.Penalty != null)
            {
                foreach (var p in offerDetails.FlightOfferResponse.DataList.Penalty)
                {
                    if (service.PenaltyRef?.ChangePenaltyRefIds != null)
                    {
                        foreach (var prid in service.PenaltyRef.ChangePenaltyRefIds)
                        {
                            if (prid == p.Id &&
                            filteredDataList != null &&
                            filteredDataList.Penalty != null && filteredDataList.Penalty.All(x => x.Id != prid))
                            {
                                filteredDataList.Penalty.Add(p);
                            }
                        }
                    }
                    if (service.PenaltyRef?.CancellationPenaltyRefIds != null)
                    {
                        foreach (var prid in service.PenaltyRef.CancellationPenaltyRefIds)
                        {
                            if (prid == p.Id && filteredDataList != null && filteredDataList.Penalty != null && filteredDataList.Penalty.All(x => x.Id != prid))
                            {
                                filteredDataList.Penalty.Add(p);
                            }
                        }
                    }
                }

                if (service.PenaltyRef != null)
                    filteredDataList.Penalty = filteredDataList?.Penalty?.OrderBy(x => x.Id).ToList();
                else
                    filteredDataList.Penalty = null;
            }
            else
            {
                filteredDataList.Penalty = null;
            }
        }

        private static void FilterJourney(OfferDocument offerDetails, OfferService service, DataList filteredDataList)
        {
            if (offerDetails.FlightOfferResponse.DataList.Journey != null)
            {
                foreach (var j in offerDetails.FlightOfferResponse.DataList.Journey)
                    foreach (var jrid in service.JourneyRefIds)
                        if (jrid == j.JourneyId && filteredDataList.Journey.All(x => x.JourneyId != jrid))
                            filteredDataList.Journey.Add(j);

                filteredDataList.Journey = filteredDataList.Journey.OrderBy(x => x.JourneyId).ToList();
            }
            else
            {
                filteredDataList.Journey = null;
            }
        }

        private static void FilterOfferSegments(OfferDocument offerDetails, OfferService service, DataList filteredDataList)
        {
            if (offerDetails.FlightOfferResponse.DataList.OfferSegments != null)
            {
                foreach (var os in offerDetails.FlightOfferResponse.DataList.OfferSegments)
                    foreach (var i in service.ProductSegmentAssociation.OfferSegmentRefIds)
                    {
                        if (i != os.OfferSegmentId || filteredDataList.OfferSegments.Any(x => x.OfferSegmentId == i))
                            continue;

                        filteredDataList.OfferSegments.Add(os);
                        filteredDataList.FlightSegments.Add(offerDetails.FlightOfferResponse.DataList.FlightSegments.FirstOrDefault(fs => fs.SegmentId == os.FlightSegmentRefId));
                    }

                filteredDataList.OfferSegments = filteredDataList.OfferSegments.OrderBy(x => x.OfferSegmentId).ToList();
                filteredDataList.FlightSegments = filteredDataList.FlightSegments.OrderBy(x => x.SegmentId).ToList();
            }
            else
            {
                if (offerDetails.FlightOfferResponse.DataList.Journey != null)
                {
                    foreach (var ji in service.JourneyRefIds)
                        foreach (var id in offerDetails.FlightOfferResponse.DataList.Journey.FirstOrDefault(jour => jour.JourneyId == ji)?.FlightSegmentRefIds)
                            if (!filteredDataList.FlightSegments.Any(fSeg => fSeg.SegmentId == id))
                                filteredDataList.FlightSegments.Add(offerDetails.FlightOfferResponse.DataList.FlightSegments.FirstOrDefault(fs => fs.SegmentId == id));
                }
            }
        }
    }
}
