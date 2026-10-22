/**
* The flight search price grid
*/
namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class PriceGrid
    {
        public IEnumerable<DateOnly>? Returning { get; set; }
        public IEnumerable<PriceGridDeparture>? Departures { get; set; }
        public IEnumerable<Criteria>? Criteria { get; set; }
    }
}
