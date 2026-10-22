using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Offer;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS
{
    public class ItemsForAddition
    {
        public string OfferRefID { get; set; }
        public IEnumerable<OfferItem> OfferItems { get; set; }
    }
}
