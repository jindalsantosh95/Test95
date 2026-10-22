using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class RepriceQuote
    {
        public IEnumerable<CurrencyExchange> CurrencyExchange { get; set; }
        public IEnumerable<EventWaiver> EventWaivers { get; set; }
        public string RepriceQuoteTypeCode { get; set; }
        public string RefundPaymentTypeCode { get; set; }
        public string ReissueActionTypeCode { get; set; }
        public string ReissueInformationNoteText { get; set; }
        public string RepriceTagText { get; set; }
        public string ReshopTripSummaryId { get; set; }
    }
}
