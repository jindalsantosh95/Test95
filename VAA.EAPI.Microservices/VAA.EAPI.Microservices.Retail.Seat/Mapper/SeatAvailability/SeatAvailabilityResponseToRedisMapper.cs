using System;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability
{
    public static class SeatAvailabilityResponseToRedisMapper
    {
        public static SeatAvailabilityDocument RedisCacheMapper(SeatAvailabilityResponse seatAvailabilityResponse, string docId, string docType, string channel)
        {
            return new SeatAvailabilityDocument
            {
                FlightSegmentSeatMap = seatAvailabilityResponse.FlightSegmentSeatMap.Select(flightSegmentSeatMap => new FlightSegmentSeatMap
                {
                    FlightSegment = flightSegmentSeatMap.FlightSegment,
                    Seatmap = flightSegmentSeatMap.Seatmap,
                    Catalogue = flightSegmentSeatMap.Catalogue
                }),
                SeatOfferId = docId,
                Id = docId,
                PartitionKey = docId,
                CreatedBy = channel,
                CreatedOn = DateTime.UtcNow.ToString(Constants.Application.IsoDateTimeFormat),
                DocType = docType,
                Pos = seatAvailabilityResponse.Pos,
                Parties = seatAvailabilityResponse.Parties,
                OrderId = seatAvailabilityResponse.OrderId,
                OfferExpirationTimeLimit = seatAvailabilityResponse.OfferExpirationTimeLimit
            };
        }
    }
}
