using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class DlOrderRequest
    {
        public PointOfSale PointOfSale { get; set; }
        public IEnumerable<SelectedOffer> SelectedOffers { get; set; }
        public IEnumerable<FormsOfPayment> FormsOfPayment { get; set; }
        public IEnumerable<Customer> Customers { get; set; }
    }
}
