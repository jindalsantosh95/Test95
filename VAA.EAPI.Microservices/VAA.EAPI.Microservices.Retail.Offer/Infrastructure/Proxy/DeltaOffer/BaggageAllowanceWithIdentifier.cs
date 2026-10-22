namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class BaggageAllowanceWithIdentifier
    {
        public string BaggageAllowanceId { get; set; }
        public string BaggageAllowanceTypeCode { get; set; }
        public FreeBaggageAllowance FreeBaggageAllowance { get; set; }
    }
}
