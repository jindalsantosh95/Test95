using System.Collections.Generic;
namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderCancel
{
    public class SelectedOffer
    {
        public string ShoppingRefID { get; set; }
        public string OfferRefID { get; set; }
        public List<OfferItem> OfferItems { get; set; }
    }
}
