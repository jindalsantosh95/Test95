using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferPriceRQ
{
    public class OfferPriceRQ
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public RepriceQueries RepriceQueries { get; set; }
        public PricedOffer PricedOffer { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
    }
}
