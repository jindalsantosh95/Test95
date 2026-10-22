namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class SliceFlightsAndFares
    {
        public SliceFlight Flight { get; set; }
        public IEnumerable<SliceFare> Fares { get; set; }
    }
}
