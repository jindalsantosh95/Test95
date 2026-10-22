using System;

namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class IATAPayload
    {
        public string EchoToken { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Target { get; set; }
        public string IATAVersion { get; set; }
        public string TransactionId { get; set; }
        public string SequenceNumber { get; set; }
        public bool? AsyncAllowed { get; set; }
        public string CorrelationId { get; set; }

    }
}
