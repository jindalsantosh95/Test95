using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS
{
    public class ItemsForDeletion
    {
        public string OrderItemId { get; set; }
        public IEnumerable<VAA.EAPI.Domain.Virgin.Models.Offer.OfferItem> CancelOfferItems { get; set; }
    }
}
