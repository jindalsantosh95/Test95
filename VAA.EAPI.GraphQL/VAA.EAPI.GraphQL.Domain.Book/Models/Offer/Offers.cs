using VAA.EAPI.GraphQL.Domain.Book.Models.Common;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Offer
{
    public class Offers
    {
        public string OfferId { get; set; }
        public string ProductType { get; set; }
        public IEnumerable<Price> SalePrice { get; set; }
        public IEnumerable<OfferItem> OfferItems { get; set; }
    }
}
