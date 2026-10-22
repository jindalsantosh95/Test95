using VAA.EAPI.GraphQL.Domain.FlightSearch.Models;

namespace VAA.EAPI.GraphQL.Domain.Search.Models
{
    public class FlightCollection
    {
        public IEnumerable<Flight> Flights { get; set; } = Enumerable.Empty<Flight>();

        public FlightCollection() { }

        public FlightCollection(string json)
        {
            this.Flights = VAA.EAPI.GraphQL.Common.Serialisation.Json.DeserializeObject<FlightCollection>(json).Flights;
        }
    }
}
