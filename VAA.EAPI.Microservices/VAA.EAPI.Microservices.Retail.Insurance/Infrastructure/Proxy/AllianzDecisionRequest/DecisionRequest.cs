using VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteRequest;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzDecisionRequest
{
    public class AllianzDecisionRequest
    {
        public Credentials Credentials { get; set; }
        public string BookingId { get; set; }
        public string BusinessUnit { get; set; }
        public string CurrencyType { get; set; }
        public string Cxid { get; set; }
        public string ResponseId { get; set; }
        public string ResponseType { get; set; }
        public IEnumerable<Detail> Details { get; set; }
    }
}
