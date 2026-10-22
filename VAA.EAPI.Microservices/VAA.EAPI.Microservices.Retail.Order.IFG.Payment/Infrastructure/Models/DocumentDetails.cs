using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models
{
    public class DocumentDetails
    {
        public IEnumerable<Itinerary> Itinerary { get; set; }
        public RefundDetails RefundDetails { get; set; }
        public IEnumerable<ExchangeDetails> ExchangeDetails { get; set; }
        public IEnumerable<EmdDetails> EmdDetails { get; set; }
    }
}
