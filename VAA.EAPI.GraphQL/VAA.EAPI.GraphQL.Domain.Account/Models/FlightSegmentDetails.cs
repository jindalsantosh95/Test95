using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class FlightSegmentDetails
    {
        public string OperatingAirlineName { get; set; }
        public LocalDate DateOfFlight { get; set; }
        public LocalTime ScheduledDepartureTime { get; set; }
        public string OriginAirportName { get; set; }
        public string OriginAirportCountry { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportName { get; set; }
        public string DestinationAirportCountry { get; set; }
        public string DestinationAirportCode { get; set; }
        public string FlightNumber { get; set; }
        public string OperatingAirlineCode { get; set; }
        public int NumberOfDaysToDeparture { get; set; }
        public string BookingReference { get; set; }
        public bool ShowClaimsBanner { get; set; }
        public string VirginPoints { get; set; }
        public string TierPoints { get; set; }
    }
}
