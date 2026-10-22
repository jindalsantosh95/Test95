namespace VAA.EAPI.GraphQL.Domain.Operations.Models.Response
{
    public class FlightInfo
    {
        public Aircraft Aircraft { get; set; }
        public FlightNumber Marketing_Flight_Number { get; set; }
        public FlightNumber Operating_Flight_Number { get; set; }
        public IEnumerable<Leg> Legs { get; set; } = Enumerable.Empty<Leg>();
    }

}
