/**
* The flight search results
*
* @author Gareth Williams
*/
namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class SearchResults
    {
        public PriceGrid? PriceGrid { get; set; }
        public Calendar? Calendar { get; set; }
        public Result? Result { get; set; }
        public IEnumerable<Link>? Links { get; set; }

    }
}
