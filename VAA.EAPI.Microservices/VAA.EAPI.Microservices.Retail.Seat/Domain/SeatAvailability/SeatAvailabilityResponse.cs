using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability
{
    public class SeatAvailabilityResponse
    {
        public Parties Parties { get; set; }
        public Pos Pos { get; set; }
        public string SeatOfferId { get; set; }
        public string OfferExpirationTimeLimit { get; set; }
        public List<FlightSegmentSeatMap> FlightSegmentSeatMap { get; set; }
        public string OrderId { get; set; }

        public static explicit operator SeatAvailabilityResponse(SeatAvailabilityDocument seatAvailabilityDoc)
        {
            return new SeatAvailabilityResponse()
            {
                Pos = seatAvailabilityDoc.Pos,
                Parties = seatAvailabilityDoc.Parties,
                SeatOfferId = seatAvailabilityDoc.SeatOfferId,
                OfferExpirationTimeLimit = seatAvailabilityDoc.OfferExpirationTimeLimit,
                FlightSegmentSeatMap = (List<FlightSegmentSeatMap>)seatAvailabilityDoc.FlightSegmentSeatMap,
                OrderId = seatAvailabilityDoc.OrderId
            };
        }
    }
}
