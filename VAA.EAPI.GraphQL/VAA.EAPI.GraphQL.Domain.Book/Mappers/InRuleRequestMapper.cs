using VAA.EAPI.GraphQL.Domain.Book.Models.InRuleEngine;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Book.Mappers
{
    public class InRuleRequestMapper
    {
        public static PointsEarnCalculatorRequest MapPointsEarnCalculatorRequest(OfferResponse flightOfferResponse, CustomerDTO? customerDetails)
        {
            return new PointsEarnCalculatorRequest()
            {
                FlightSegments = flightOfferResponse.DataList.FlightSegments.Select(segment => new EarnCalculatorFlightSegment()
                {
                    ClassOfService = flightOfferResponse.Offers.First().OfferItems.First().Services.FirstOrDefault(o => o.FlightSegmentsRefIds.Contains(segment.SegmentId))?.FareDetails?.FareSegments?.First(fs => fs.FlightSegmentRefId == segment.SegmentId).BookingClass,
                    MarketingCarrier = segment.MarketingCarrier,
                    OperatingCarrier = segment.OperatingCarrier,
                    DestinationAirportCode = segment.DestinationAirportCode,
                    MarketingFlightNumber = segment.MarketingFlightNumber,
                    OperatingFlightNumber = segment.OperatingFlightNumber,
                    ScheduledDepartureTime = Common.Helper.DateTimeHelper.ParseLocalDateTime(segment.ScheduledDepartureTime.LocalDateTime).Value.ToString(),
                    OriginAirportCode = segment.OriginAirportCode,
                    SegmentDistance = segment.Legs.Sum(l => l.Distance.Distance) ?? 0,
                    OperatingBookingClass = flightOfferResponse.Offers.First().OfferItems.First().Services.FirstOrDefault(o => o.FlightSegmentsRefIds.Contains(segment.SegmentId))?.FareDetails?.FareSegments?.First(fs => fs.FlightSegmentRefId == segment.SegmentId).BookingClass,
                    MarketingBookingClass = flightOfferResponse.Offers.First().OfferItems.First().Services.FirstOrDefault(o => o.FlightSegmentsRefIds.Contains(segment.SegmentId))?.FareDetails?.FareSegments?.First(fs => fs.FlightSegmentRefId == segment.SegmentId).BookingClass
                }),
                CustomerDetails = customerDetails != null ? new List<EarnCalculatorCustomerDetails>()
                {
                    new()
                    {
                        MemberNumber = customerDetails.Customer.LoyaltyNumber,
                        TierCode = customerDetails.Customer.MemberTier,
                        MemberStatus = customerDetails.Customer.Status
                    }
                } : null
            };
        }
    }
}
