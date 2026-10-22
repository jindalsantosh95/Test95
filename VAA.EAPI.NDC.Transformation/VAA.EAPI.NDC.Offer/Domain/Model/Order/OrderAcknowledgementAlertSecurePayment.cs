namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class OrderAcknowledgementAlertSecurePayment
    {
        public string PAREQ { get; set; }
        public byte[] MerchantData { get; set; }
        public Link _3DSecureURL { get; set; }
        public string OriginalTransactionID { get; set; }
        public string EnrollmentStatus { get; set; }

    }
}
