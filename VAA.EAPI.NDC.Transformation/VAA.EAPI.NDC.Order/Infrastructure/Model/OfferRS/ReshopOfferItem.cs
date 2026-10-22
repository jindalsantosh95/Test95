using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Offer;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS
{
    public class ReshopOfferItem
    {
        public string OfferItemId { get; set; }
        public IEnumerable<OfferitemPricing> OfferitemPricing { get; set; }
        public IEnumerable<string> ServiceBeneficiaries { get; set; }
        public IEnumerable<OfferService> Services { get; set; }
    }
}
