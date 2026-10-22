using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.NDC.Offer.Application.Query.SeatAvailability;
using VAA.EAPI.NDC.Offer.Domain.Model.Order;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.SeatMS;
using ProgramOwnerType = VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ.ProgramOwnerType;

namespace VAA.EAPI.NDC.Offer.Mapper.SeatAvailability
{
    public class SeatAvailabilityRqMapper
    {
        public static SeatMSQuery MapSeatAvailabilityMSRequestOfferDoc(SeatAvailabilityQuery request, OfferResponse offerDoc, bool isCreditCardAccepted, IEnumerable<FlightSegment> flightSegments)
        {
            return new SeatMSQuery
            {
                Context = new()
                {
                    CurrencyCode = offerDoc.Offers.FirstOrDefault().SalePrice.FirstOrDefault().Amount.BaseCurrency
                },
                FlightSegments = flightSegments,
                Parties = GetParties(request.SeatAvailabilityRQ.DistributionChain, offerDoc.Parties.CorporateAccount, isCreditCardAccepted),
                CustomerDetails = offerDoc.DataList.CustomerDetails
            };
        }

        public static SeatMSQuery MapSeatAvailabilityMSRequestOrderDoc(SeatAvailabilityQuery request, OrderDocument orderDocument, bool isCreditCardAccepted, IEnumerable<Segment> segmentList)
        {
            var equivalentFareType = orderDocument.OrderItems.First().PriceDetails?.FirstOrDefault(x => x.PriceComponentType == Price.PriceComponentTypes.EquivalentFareEnum)?.Amount.BaseCurrency;
            var currCode = equivalentFareType is null ? orderDocument.OrderItems.FirstOrDefault().PriceDetails.FirstOrDefault().Amount.BaseCurrency : equivalentFareType;
           
            return new SeatMSQuery
            {
                Context = new()
                {
                    CurrencyCode = currCode,
                    RecordLocator = orderDocument.BookingReference.ExternalRecordLocator,
                },
                FlightSegments = GetFlightSegments(segmentList),
                Parties = GetParties(request.SeatAvailabilityRQ.DistributionChain, null, isCreditCardAccepted),
                CustomerDetails = orderDocument.DataList.CustomerDetails
            };
        }

        public static IEnumerable<FlightSegment> GetFlightSegments(IEnumerable<Segment> segmentList)
        {
            return segmentList.Select(seg => new FlightSegment
            {
                SegmentID = seg.SegmentId.Replace("SEG","0"),
                MarketingCarrier = seg.MarketingCarrier,
                OperatingCarrier = seg.OperatingCarrier,
                OperatingFlightNumber=seg.OperatingFlightNumber,
                MarketingFlightNumber = seg.MarketingFlightNumber,
                OriginAirportCode = seg.OriginAirportCode,
                DestinationAirportCode = seg.DestinationAirportCode,
                ScheduledArrivalTime = new()
                {
                    LocalDateTime = DateTime.ParseExact(seg?.ScheduledArrivalTime?.LocalDateTime, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                },

                ScheduledDepartureTime = new()
                {
                    LocalDateTime = DateTime.ParseExact(seg.ScheduledDepartureTime?.LocalDateTime, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                },
                FareFamily = seg.FareFamily,
                FlightDuration = TimeSpan.Parse(seg.FlightDuration)
            });
        }

        public static Parties GetParties(DistributionChainLinkType[] distributionChains, CorporateAccount corporateAccount, bool isCreditCardAccepted)
        {
            var sortedDistributionChain = distributionChains.OrderBy(w => Convert.ToInt32(w?.Ordinal)).ToArray();
            if (sortedDistributionChain.Length == 1)
            {
                IEnumerable<Participant> participantList = new[] {
                new Participant()
                {
                     AgencyID = sortedDistributionChain?.FirstOrDefault()?.ParticipatingOrg?.OrgID,
                     Name = sortedDistributionChain?.FirstOrDefault()?.ParticipatingOrg?.Name,
                     PartyType = (Participant.PartyTypeEnum?)Enum.Parse(typeof(Participant.PartyTypeEnum), sortedDistributionChain?.FirstOrDefault().OrgRole.ToString(), true),
                     Ordinal = sortedDistributionChain?.FirstOrDefault()?.Ordinal,
                     IsCreditCardAccepted = isCreditCardAccepted
                }
                };
                var corporateAc = new CorporateAccount()
                {
                    AccountName = corporateAccount?.AccountName,
                    AgencyID = corporateAccount?.AgencyID,
                    CorporateCode = corporateAccount?.CorporateCode
                };
                return new Parties()
                {
                    Participants = participantList,
                    CorporateAccount = corporateAc
                };
            }
            else if (sortedDistributionChain.Length > 1)
            {
                IEnumerable<Participant> participantList = new[] {
                new Participant()
                {
                           AgencyID = sortedDistributionChain?.Last()?.ParticipatingOrg?.OrgID,
                            Name = sortedDistributionChain?.Last()?.ParticipatingOrg?.Name,
                            PartyType = (Participant.PartyTypeEnum?)Enum.Parse(typeof(Participant.PartyTypeEnum), sortedDistributionChain?.Last().OrgRole.ToString(), true),
                            Ordinal = sortedDistributionChain?.Last()?.Ordinal,
                            IsCreditCardAccepted= isCreditCardAccepted
                }
                ,new Participant(){
                            AgencyID = sortedDistributionChain?.FirstOrDefault()?.ParticipatingOrg?.OrgID,
                            Name = sortedDistributionChain?.FirstOrDefault()?.ParticipatingOrg?.Name,
                            PartyType = (Participant.PartyTypeEnum?)Enum.Parse(typeof(Participant.PartyTypeEnum), sortedDistributionChain?.FirstOrDefault().OrgRole.ToString(), true),
                            Ordinal = sortedDistributionChain?.FirstOrDefault()?.Ordinal,
                            IsCreditCardAccepted= isCreditCardAccepted
                        }
                };
                var corporateAc = new CorporateAccount()
                {
                    AccountName = corporateAccount?.AccountName,
                    AgencyID = corporateAccount?.AgencyID,
                    CorporateCode = corporateAccount?.CorporateCode
                };
                return new Parties()
                {
                    Participants = participantList,
                    CorporateAccount = corporateAc
                };
            }
            return null;
        }

        public static SeatMSQuery MapSeatAvailabilityMSRequestGDS(SeatAvailabilityQuery request, Air4OrderSearchMSResponse orderSearchResponse, IEnumerable<Segment> segmentList, bool isCreditCardAccepted)
        {
            return new SeatMSQuery
            {
                Context = new()
                {
                    CurrencyCode = orderSearchResponse.OrderSearchResponse.OrderItems.FirstOrDefault().PriceDetails?.FirstOrDefault()?.Amount?.BaseCurrency,
                    RecordLocator = orderSearchResponse.OrderSearchResponse.BookingReferences.RLOC
                },
                FlightSegments = GetGDSFlightSegments(orderSearchResponse, segmentList),
                OrderId = orderSearchResponse.OrderSearchResponse.OrderNumber.OrderId,
                Parties = GetParties(request.SeatAvailabilityRQ.DistributionChain, null, isCreditCardAccepted)
            };
        }

        public static IEnumerable<FlightSegment> GetGDSFlightSegments(Air4OrderSearchMSResponse orderSearchResponse, IEnumerable<Segment> segmentList)
        {
            return segmentList.Select(seg =>
                    new FlightSegment
                    {
                        SegmentID = seg.SegmentId,
                        MarketingCarrier = seg.MarketingCarrier,
                        MarketingFlightNumber = seg.MarketingFlightNumber,
                        OriginAirportCode = seg.OriginAirportCode,
                        DestinationAirportCode = seg.DestinationAirportCode,
                        ScheduledArrivalTime = new() { LocalDateTime = DateTime.ParseExact(seg?.ScheduledArrivalTime?.LocalDateTime, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture) },
                        ScheduledDepartureTime = new() { LocalDateTime = DateTime.ParseExact(seg.ScheduledDepartureTime?.LocalDateTime, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture) },
                        FareFamily = orderSearchResponse.OrderSearchResponse.OrderItems.First().Services.FirstOrDefault(s => s.ProductSegmentAssociation.FlightSegmentIds.First() == seg.SegmentId)?.ProductOrder?.ProductName,
                        FlightDuration = seg.FlightDuration != null ? TimeSpan.Parse(seg.FlightDuration) : default
                    }
                );
        }
    }
}
