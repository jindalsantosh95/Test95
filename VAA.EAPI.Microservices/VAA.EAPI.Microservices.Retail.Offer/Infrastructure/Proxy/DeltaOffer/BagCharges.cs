namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class BagCharges
    {
        public string BagSequenceNum { get; set; }
        public BagSpecification BagSpecification { get; set; }
        public string GoverningCarrierCode { get; set; }
        public bool SpecialItem { get; set; }
        public int ExcessFirstNum { get; set; }
        public int ExcessLastNum { get; set; }
        public AdditionalBagParameter FiledInSeq5Code { get; set; }
        public AdditionalBagParameter FiledInSeq7Code { get; set; }
    }
}