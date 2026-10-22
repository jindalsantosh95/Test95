using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Microservices.Retail.Seat.Application.Query.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Domain;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability
{
    public static class SeatAvailabilityRequestMapper
    {
        public static SeatV8RetrievePassengerMapQuery MapSeatRequest(SeatAvailabilityQuery searchRequest, string segmentId, Channel channelInfo)
        {
            var segments = searchRequest.FlightSegments.Where(x => x.SegmentId == segmentId).Select(assignSegments =>
                new Segment
                {
                    DestinationAirportCode = assignSegments?.DestinationAirportCode,
                    OriginAirportCode = assignSegments?.OriginAirportCode,
                    ScheduledDepartureTime = assignSegments?.ScheduledDepartureTime,
                    ScheduledArrivalTime = assignSegments?.ScheduledArrivalTime,
                    MarketingCarrier = assignSegments?.MarketingCarrier,
                    MarketingFlightNumber = assignSegments?.MarketingFlightNumber,
                    OperatingCarrier = assignSegments?.OperatingCarrier ?? assignSegments?.MarketingCarrier,
                    OperatingFlightNumber = assignSegments?.OperatingFlightNumber ?? assignSegments?.MarketingFlightNumber,
                    SegmentNumber = assignSegments?.SegmentId,
                    ClassOfService = assignSegments?.ClassOfService,
                    FareBasis = assignSegments?.FareBasis,
                }
             );

            return new()
            {
                ChannelInfo = channelInfo,
                CurrencyCode = searchRequest.Context.CurrencyCode,
                RecordLocator = searchRequest.Context.RecordLocator,
                FlightSegments = segments,
                Customers = searchRequest.CustomerDetails
            };
        }
    }
}
