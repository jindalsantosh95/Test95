using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.FlexiDateSearch
{
    public class PriceItems
    {
        public string Ptc { get; set; }
        public IEnumerable<Price> SalePrice { get; set; }

    }
}
