/**
* The flight search price grid departures
*
* @author Gareth Williams
*/
namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class PriceGridDeparture
    {

        public DateOnly departing { get; set; }

        public IEnumerable<PriceGridPrice?> prices { get; set; }
    }
}
