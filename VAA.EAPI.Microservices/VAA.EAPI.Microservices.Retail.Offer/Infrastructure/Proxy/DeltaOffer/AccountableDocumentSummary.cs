using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class AccountableDocumentSummary
    {
        public Price TotalAvailableCreditAmt { get; set; }
        public Price TotalChangeFeeAmt { get; set; }
        public Price TotalCancelFeeAmt { get; set; }
        public IEnumerable<PassengerDocumentSummary> PassengerAccountableDocumentSummary { get; set; }
        public Price TotalDocumentValue { get; set; }
        public IEnumerable<Discount> DiscountsApplied { get; set; }
    }
}
