using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusAircraft
    {
        public static explicit operator FlightStatusAircraft?(DeltaAircraft? source) => source is null
            ? null
            : new FlightStatusAircraft
            {
                Type = source.Type,
                DeltaAircraftTypeCode = source.DeltaAircraftTypeCode,
                IndustryStandardAircraftTypeCode = source.IndustryStandardAircraftTypeCode,
                Specification = (FlightStatusAircraftSpecification?)source.Specification
            };

        public string? Type { get; set; }
        public string? DeltaAircraftTypeCode { get; set; }
        public string? IndustryStandardAircraftTypeCode { get; set; }
        public FlightStatusAircraftSpecification? Specification { get; set; }
    }
}
