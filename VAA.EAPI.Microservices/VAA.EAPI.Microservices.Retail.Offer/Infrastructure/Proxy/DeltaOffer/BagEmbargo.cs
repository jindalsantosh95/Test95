namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class BagEmbargo
    {
        public string GoverningCarrierCode { get; set; }
        public BagSpecification BagSpecification { get; set; }
        public AdditionalBagParameter FiledInSeq5Code { get; set; }
        public AdditionalBagParameter FiledInSeq7Code { get; set; }
    }
}