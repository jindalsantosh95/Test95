using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Offer;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMSReshopCancel
{
    public class ReshopOfferItem
    {
        public string OfferItemId { get; set; }
        public IEnumerable<OfferItemPricing> OfferitemPricing { get; set; }
        public IEnumerable<OfferService> Services { get; set; }
    }
}
