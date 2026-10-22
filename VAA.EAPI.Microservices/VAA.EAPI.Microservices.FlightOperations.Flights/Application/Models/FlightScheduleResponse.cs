using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Response;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models
{
    public sealed class FlightScheduleResponse
    {
        public static explicit operator FlightScheduleResponse(FlightDetailsForPeriodResponse source) => new()
        {
            Flights = source.FlightDetailsForPeriodResult.FlightList.AsParallel()
                                                                    .Select(x => (FlightDetail)x)
                                                                    .ToArray()
        };

        public IEnumerable<FlightDetail> Flights { get; set; } = [];
    }
}
