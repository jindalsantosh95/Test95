namespace VAA.EAPI.Automation.Trips.Helpers
{
    public sealed class ScvHistoricTripTestData
    {
        public required string OrderId { get; init; }
        public required string Pnr { get; init; }
        public required string FlightDate { get; init; }
        public required string GivenName { get; init; }
        public required string Surname { get; init; }
        public required string MarketingFlightNumber { get; init; }
        public required string OriginAirportCode { get; init; }
        public required string DestinationAirportCode { get; init; }
    }
}
