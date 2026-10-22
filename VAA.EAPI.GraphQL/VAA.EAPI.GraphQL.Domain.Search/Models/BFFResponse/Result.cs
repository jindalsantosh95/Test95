/**
* The flight search results
*
* @author Gareth Williams
*/
namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class Result
    {
        public Slices Slices { get; set; }
        public Criteria Criteria { get; set; }
        public Slice Slice { get; set; }
        public IEnumerable<Link>? Links { get; set; }
        public FlightSortLinks FlightSortLinks { get; set; }
        public FilterCollection FlightSearchFilters { get; set; }
        public TripSummary TripSummary { get; set; }
        public Guid BasketId { get; set; }
    }
}
