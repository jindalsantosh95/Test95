using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange
{
    public class ItemsForAddition
    {
        public string OfferRefID { get; set; }
        public List<OfferItem> OfferItems { get; set; }
    }
}
