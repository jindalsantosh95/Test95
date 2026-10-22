using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SeatMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Mapper.SeatMS
{
    public static class RetrieveSeatMapRequestMapper
    {
        public static SeatMSRetrieveSeatMapQuery MapSeatAvailabilityMSRequestOrderDoc(OrderDocument orderDocument, IATA_OrderCreateRQ request, string rloc)
        {
            return new()
            {
                Context = new()
                {
                    CurrencyCode = orderDocument.OrderItems.FirstOrDefault().PriceDetails.FirstOrDefault().Amount.BaseCurrency,
                    RecordLocator = rloc,
                },
                FlightSegments = GetFlightSegments(orderDocument.DataList.SegmentDetails),
                Parties = GetParties(request.DistributionChain , null),
                CustomerDetails = orderDocument.DataList.CustomerDetails
            };
        }

        public static IEnumerable<FlightSegment> GetFlightSegments(IEnumerable<EAPI.Domain.Virgin.Models.Products.Flight.Segment> segmentList)
        {
            return segmentList.Select(seg => new FlightSegment
            {
                SegmentID = seg.SegmentId.Replace("SEG", "0"),
                MarketingCarrier = seg.MarketingCarrier,
                OperatingCarrier = seg.OperatingCarrier,
                OperatingFlightNumber = seg.OperatingFlightNumber,
                MarketingFlightNumber = seg.MarketingFlightNumber,
                OriginAirportCode = seg.OriginAirportCode,
                DestinationAirportCode = seg.DestinationAirportCode,
                ScheduledArrivalTime = new()
                {
                    LocalDateTime = seg.ScheduledArrivalTime.LocalDateTime
                },

                ScheduledDepartureTime = new()
                {
                    LocalDateTime = seg.ScheduledDepartureTime.LocalDateTime
                },
                FareFamily = seg.FareFamily,
                FlightDuration = TimeSpan.Parse(seg.FlightDuration),
                ClassOfService = seg.ClassOfService
            });
        }

        public static Parties GetParties(DistributionChainLinkType[] distributionChains, CorporateAccount corporateAccount)
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
                     Ordinal = sortedDistributionChain?.FirstOrDefault()?.Ordinal
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
                            Ordinal = sortedDistributionChain?.Last()?.Ordinal
                }
                ,new Participant(){
                            AgencyID = sortedDistributionChain?.FirstOrDefault()?.ParticipatingOrg?.OrgID,
                            Name = sortedDistributionChain?.FirstOrDefault()?.ParticipatingOrg?.Name,
                            PartyType = (Participant.PartyTypeEnum?)Enum.Parse(typeof(Participant.PartyTypeEnum), sortedDistributionChain?.FirstOrDefault().OrgRole.ToString(), true),
                            Ordinal = sortedDistributionChain?.FirstOrDefault()?.Ordinal
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

    }
}
