namespace VAA.EAPI.GraphQL.Domain.Account.Models.Retro
{
    public class ClaimFlightSegmentDetails
    {
        public string OperatingAirlineName { get; set; }
        public string OperatingAirlineCode { get; set; }
        public string DateOfFlight { get; set; }
        public string FlightNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public string OriginAirportName { get; set; }
        public string DestinationAirportName { get; set; }
        public string OriginAirportCountry { get; set; }
        public string DestinationAirportCountry { get; set; }
        public string VirginPoints { get; set; }
        public string TierPoints { get; set; }
        public string BonusPoints { get; set; }
        public bool IsPartialTransaction { get; set; }
    }
}
