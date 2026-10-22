/**
* The flight search price grid price
*
* @author Gareth Williams
*/
namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class PriceGridPrice
    {

        public Price? price { get; set; }

        public bool? minPrice { get; set; }

    }
}
