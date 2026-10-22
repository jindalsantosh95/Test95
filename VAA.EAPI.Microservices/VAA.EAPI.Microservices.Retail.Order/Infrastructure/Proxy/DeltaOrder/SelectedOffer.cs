using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class SelectedOffer
    {
        public string OfferId { get; set; }
        public IEnumerable<SelectedOfferItem> SelectedOfferItems { get; set; }
        public TotalOfferAmt TotalOfferAmt { get; set; }
        public IEnumerable<string> FormOfPaymentId { get; set; }
        public OrderCriteria OrderCriteria { get; set; }
        public IEnumerable<Remark> Remarks { get; set; }
    }
}
