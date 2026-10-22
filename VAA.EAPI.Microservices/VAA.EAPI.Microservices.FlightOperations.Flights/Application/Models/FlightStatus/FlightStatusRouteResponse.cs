using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusRouteResponse
    {
        public static explicit operator FlightStatusRouteResponse(DeltaRouteSearchResponse source)
        {
            ArgumentNullException.ThrowIfNull(source);

            return new FlightStatusRouteResponse
            {
                DepartureAirport = (FlightStatusAirport?)source.DepartureAirport,
                ArrivalAirport = (FlightStatusAirport?)source.ArrivalAirport,
                DepartureDate = source.DepartureDate,
                Routes = source.Routes?
                    .Select(route => (FlightStatusRoute?)route)
                    .Where(HasAnyLeg)
                    .ToArray(),
                RouteSummary = (FlightStatusRouteSummary?)source.RouteSummary
            };
        }

        public FlightStatusAirport? DepartureAirport { get; set; }
        public FlightStatusAirport? ArrivalAirport { get; set; }
        public string? DepartureDate { get; set; }
        public IEnumerable<FlightStatusRoute?>? Routes { get; set; }
        public FlightStatusRouteSummary? RouteSummary { get; set; }

        private static bool HasAnyLeg(FlightStatusRoute? route) =>
            route?.Segments?.Any(segment =>
                segment?.Legs?.Any(leg => leg is not null) == true ||
                segment?.LegsWithChangeOfGauge?.Any(leg => leg is not null) == true) == true;
    }
}
