using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain
{
    public class PricedOfferItem
    {
        public string OfferItemId { get; set; }
        public Product Product { get; set; }
        public List<string> ServiceBeneficiaries { get; set; }
    }
}
