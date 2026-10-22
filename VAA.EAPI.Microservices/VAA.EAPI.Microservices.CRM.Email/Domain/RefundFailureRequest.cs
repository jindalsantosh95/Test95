namespace VAA.EAPI.Microservices.CRM.Email.Domain
{
    public class RefundFailureRequest
    {
        public string TransactionId { get; set; }
        public string TransactionDateTime { get; set; }
        public string TransactionAmount { get; set; }
        public string Currency { get; set; }
        public string PaymentMethod { get; set; }
        public string CardNumber { get; set; }
        public string Channel { get; set; }
        public string RefundRequestDateTime { get; set; }
        public string PNR { get; set; }
        public string OrderReference { get; set; }
        public string ReasonForCPDFailure { get; set; }
    }
}
