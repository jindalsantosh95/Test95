using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro
{
    public class RetroFlightDetails
    {
        public string OperatedAirlineCode { get; set; }
        public string OperatedFlightNumber { get; set; }
        public string OperatedbookingClass { get; set; }
        public string MarketingAirlineCode { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string MarketingBookingClass { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public DateTime? FlightActivityDate { get; set; }
        public string MarketedCabin { get; set; }
    }
}
