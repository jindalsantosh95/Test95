using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class Criteria
    {
        public Airport origin { get; set; }

        public Airport destination { get; set; }

        public DateOnly departing { get; set; }

        public Criteria() { }
        public Criteria(SearchOriginDestination ond, IEnumerable<AirportInfo> airportInfo)
        {
            origin = (Airport)(ond.Origin, airportInfo);
            destination = (Airport)(ond.Destination, airportInfo);
            departing = DateOnly.Parse(ond.DepartureDate);
        }
    }
}
