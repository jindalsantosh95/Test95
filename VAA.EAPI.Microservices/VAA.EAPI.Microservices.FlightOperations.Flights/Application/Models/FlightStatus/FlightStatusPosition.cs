namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusPosition
    {
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? AltitudeFeet { get; set; }
        public decimal? SpeedKnots { get; set; }
        public decimal? HeadingDegrees { get; set; }
        public string? LastUpdated { get; set; }
    }
}
