using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NodaTime;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Constants;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.BoardingPass
{
    public class BoardingPass
    {
        public string PnrLocator { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string PassengerNIN { get; set; }
        public string PassengerCustomerId { get; set; }
        public string PassengerId { get; set; }
        public string SeatNumber { get; set; }
        public string Zone { get; set; }
        public string Terminal { get; set; }
        public string OperatingAirlineName { get; set; }
        public string MarketingAirlineName { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string DestinationAirportCode { get; set; }
        public string OriginAirportCode { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public string QrCode { get; set; }
        public string QrCodeData { get; set; }
        public string DesktopBoardingPassBase64 { get; set; }
        public string VerificationStatusMessage { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TemplateCode TemplateCode { get; set; }
        public string CabinName { get; set; }
        public string Gate { get; set; }
        public LocalTime? BoardingTimeLocalTime { get; set; }
        public bool ShowSkyPriority { get; set; }
        public bool ShowTsaPre { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public FlightStatusCode FlightStatusCode { get; set; }
        public string InfantName { get; set; }
        public bool CheckedIn { get; set; }
    }
}
