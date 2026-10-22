namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class CardDetails
    {
        public string CardType { get; set; }
        public string CardNumber { get; set; }
        public bool? IsTokenised { get; set; }
        public string IssueDate { get; set; }
        public string ExpiryDate { get; set; }
        public string CardholderName { get; set; }
        public string CVV { get; set; }
        public string IssueNumber { get; set; }
        public Address BillingAddress { get; set; }
        public string MaskedCardNumber { get; set; }
        public string PaymentAuthorisationCode { get; set; }
        public string PaymentTransactionNumber { get; set; }
        public string PartyPaymentApprovalCode { get; set; }
        public string CustomerPaymentRef { get; set; }
        public Customer Payer { get; set; }


    }
}
