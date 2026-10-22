using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.AimsSoap;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models
{
    public sealed class RouteDetail
    {
        public static explicit operator RouteDetail(AssosiatedCrewRouteInfo source) => new()
        {
            Date = DateOnly.ParseExact(source.RouteDate, Constants.Application.DateOnlyIncomingFormats),
            Name = source.RouteName
        };

        public DateOnly Date { get; set; }

        public string Name { get; set; }
    }
}
