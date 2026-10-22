using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Airship
{
    public class FlightSemantic
    {
        public string AirlineCode { get; set; }
        public string FlightNumber { get; set; }
        public string OriginalBoardingDate { get; set; }
        public string OriginalDepartureDate { get; set; }
        public string OriginalArrivalDate { get; set; }
        public string DepartureAirportCode { get; set; }
        public string DepartureCityName { get; set; }
        public string DepartureLocationTimeZone { get; set; }
        public IEnumerable<string> DepartureLocationSecurityPrograms { get; set; }
        public string DestinationAirportCode { get; set; }
        public string DestinationCityName { get; set; }
        public string DestinationLocationTimeZone { get; set; }
        public string DepartureGate { get; set; }
        public string DepartureTerminal { get; set; }
    }
}
