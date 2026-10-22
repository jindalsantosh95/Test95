using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Response;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models
{
    public sealed class FlightDetailResponse
    {
        public static explicit operator FlightDetailResponse(FlightDetailsX1Response source) => new()
        {
            Flights = source.FlightDetailsX1Result.FlightListX1.AsParallel()
                                                               .Select(x => (FlightDetail)x)
                                                               .ToArray()
        };

        public IEnumerable<FlightDetail> Flights { get; set; } = [];
    }
}
