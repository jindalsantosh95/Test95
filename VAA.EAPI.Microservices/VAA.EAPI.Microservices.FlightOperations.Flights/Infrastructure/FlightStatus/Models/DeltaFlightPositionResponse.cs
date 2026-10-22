using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models
{
    public sealed class DeltaFlightPositionResponse
    {
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? AltitudeFeet { get; set; }
        public decimal? SpeedKnots { get; set; }
        public decimal? HeadingDegrees { get; set; }
        public string? LastUpdated { get; set; }

        public static explicit operator FlightStatusPosition?(DeltaFlightPositionResponse? source)
        {
            if (source is null ||
                source.Latitude is null &&
                source.Longitude is null &&
                source.AltitudeFeet is null &&
                source.SpeedKnots is null &&
                source.HeadingDegrees is null &&
                source.LastUpdated is null)
            {
                return null;
            }

            return new FlightStatusPosition
            {
                Latitude = source.Latitude,
                Longitude = source.Longitude,
                AltitudeFeet = source.AltitudeFeet,
                SpeedKnots = source.SpeedKnots,
                HeadingDegrees = source.HeadingDegrees,
                LastUpdated = source.LastUpdated
            };
        }
    }
}
