namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class FopHistory
    {
        public string CurrencyCode { get; set; }
        public string LoyaltyMemberId { get; set; }
        public string MiscellaneousFopText { get; set; }
        public string PaymentCardNum { get; set; }
        public string PaymentDocumentActivityTypeCode { get; set; }
        public Price PaymentDocumentCollectionAmt { get; set; }
        public string PaymentDocumentFopTypeCode { get; set; }
        public string PaymentDocumentIssueLocalDate { get; set; }
        public string PaymentDocumentNum { get; set; }
        public string PaymentDocumentSequenceNum { get; set; }
        public string PaymentDocumentServiceFeeTypeCode { get; set; }
        public string PaymentDocumentTypeCode { get; set; }
        public string PaymentRedeemedMileCnt { get; set; }
        public string PaymentRedeemerLoyaltyMemberId { get; set; }
        public string PaymentRemarkText { get; set; }
        public string PaymentSequenceNum { get; set; }
        public string PaymentTransactionTypeCode { get; set; }
        public string PaymentCardTypeCode { get; set; }
    }
}
