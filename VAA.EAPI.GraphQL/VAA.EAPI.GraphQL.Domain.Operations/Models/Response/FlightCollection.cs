namespace VAA.EAPI.GraphQL.Domain.Operations.Models.Response
{
    public class FlightCollection
    {
        public IEnumerable<FlightInfo> Flights { get; set; } = Enumerable.Empty<FlightInfo>();

        public FlightCollection() { }

        public FlightCollection(string json)
        {
            Flights = Common.Serialisation.Json.Deserialize<FlightCollection>(json).Flights;
        }
    }
}
