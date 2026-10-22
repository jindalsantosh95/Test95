using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models
{
    public sealed class DeltaAmenity
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? Available { get; set; }

        public static explicit operator FlightStatusAmenity?(DeltaAmenity? source) => source is null
            ? null
            : new FlightStatusAmenity
            {
                Code = source.Code,
                Name = source.Name,
                Description = source.Description,
                Available = source.Available
            };
    }
}
