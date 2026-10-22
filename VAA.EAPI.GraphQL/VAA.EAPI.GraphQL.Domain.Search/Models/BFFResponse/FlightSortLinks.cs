namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class FlightSortLinks
    {
        public string SelectedSort { get; set; }

        public IEnumerable<string> AvailableSortFields { get; set; }

        public IEnumerable<SortLink> Links { get; set; }
    }
}
