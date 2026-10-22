namespace VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS
{
    public class PaymentDocument
    {
        public string TransactionId { get; set; }
        public string ApprovalCode { get; set; }
        public decimal? Amount { get; set; }
        public string OrderID { get; set; }
        public string TransactionStatus { get; set; }
        public string MaskedCardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string CardType { get; set; }
        public string PaymentCommitedDateTime { get; set; }
        public string TrackId { get; set; }
        public int HttpStatus { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string PaymentStatus { get; set; }
        public AmountAuthorized AmountAuthorized { get; set; }
        public string FormOfPayment { get; set; }
    }
}
