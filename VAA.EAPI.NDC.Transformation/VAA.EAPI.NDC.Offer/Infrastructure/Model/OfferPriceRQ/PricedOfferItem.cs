using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferPriceRQ
{
    public class PricedOfferItem
    {
        public string OfferItemId { get; set; }
        public Product Product { get; set; }
        public IEnumerable<string> ServiceBeneficiaries { get; set; }
    }
}
