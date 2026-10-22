namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class DiscountCertificate
    {
        public string CertificateTypeAccountCode { get; set; }
        public bool CompanionCertificate { get; set; }
        public bool DiscountTotal { get; set; }
        public int DiscountPct { get; set; }
        public bool SecuredRate { get; set; }
        public string FareBasisTicketDesignatorCode { get; set; }
        public Price CertificateValue { get; set; }
    }
}
