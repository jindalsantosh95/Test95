namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class FormOfPayment
    {
        public string FopAmt { get; set; }
        public string FopFreeText { get; set; }
        public string FopIndicationCode { get; set; }
        public string FopSequenceNum { get; set; }
        public string FopTransactionText { get; set; }
        public string FopTypeCode { get; set; }
        public bool OriginalFop { get; set; }
    }
}
