using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange
{
    public class Offers
    {
        public string OfferId { get; set; }
        public string ProductType { get; set; }
        public IEnumerable<Price> SalePrice { get; set; }
        public IEnumerable<EAPI.Domain.Virgin.Models.Offer.OfferItem> OfferItems { get; set; }
    }
}
