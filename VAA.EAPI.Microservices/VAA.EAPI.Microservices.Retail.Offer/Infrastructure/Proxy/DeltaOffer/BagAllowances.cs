namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class BagAllowances
    {
        public int AllowedBagCnt { get; set; }
        public int MaximumAllowedBagCnt { get; set; }
        public string GoverningCarrierCode { get; set; }
        public BagSpecification BagSpecification { get; set; }
        public bool CarryOnIndicator { get; set; }
        public AdditionalBagParameter FiledInSeq5Code { get; set; }
        public AdditionalBagParameter FiledInSeq7Code { get; set; }
    }
}
