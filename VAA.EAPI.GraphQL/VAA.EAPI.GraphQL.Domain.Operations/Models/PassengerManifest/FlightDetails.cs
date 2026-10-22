namespace VAA.EAPI.GraphQL.Domain.Operations.Models.PassengerManifest
{
    public class FlightDetails
    {
        public string InitialOrigin { get; set; }
        public string FinalDestination { get; set; }
        public int NumberOfStops { get; set; }

        [UseFiltering]
        [UseSorting]
        public ICollection<FlightLeg> Legs { get; set; } = new List<FlightLeg>();
    }
}
