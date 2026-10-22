using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange
{
    public class ItemsForDeletion
    {
        public string OrderItemId { get; set; }
        public IEnumerable<OfferItem> CancelOfferItems { get; set; }
    }
}
