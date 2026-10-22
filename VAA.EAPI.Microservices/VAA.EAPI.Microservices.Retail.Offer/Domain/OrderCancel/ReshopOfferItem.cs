using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Offer;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel
{
    public class ReshopOfferItem
    {
        public string OfferItemId { get; set; }
        public IEnumerable<OfferitemPricing> OfferitemPricing { get; set; }
        public IEnumerable<string> ServiceBeneficiaries { get; set; }
        public IEnumerable<OfferService> Services { get; set; }
    }
}
