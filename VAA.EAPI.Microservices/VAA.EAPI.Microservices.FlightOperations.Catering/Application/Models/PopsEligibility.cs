using System;

namespace VAA.EAPI.Microservices.FlightOperations.Catering.Application.Models
{
    public class PopsEligibility
    {
        public int MinBookingHour { get; set; }
        public int MaxBookingHour { get; set; }
        public DateTime DepartureDateUTC { get; set; }
        public DateTime? OfferCreatedDate { get; set; }
    }
}
