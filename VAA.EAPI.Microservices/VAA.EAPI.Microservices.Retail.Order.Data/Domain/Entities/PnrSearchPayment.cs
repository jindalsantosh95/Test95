namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities
{
    public class PnrSearchPayment
    {
        public string PaymentId { get; set; }
        public int ClientId { get; set; }
        public string ChannelId { get; set; }
        public int TransactionId { get; set; }
        public string TransactionDateTime { get; set; }
        public string TransactionType { get; set; }
        public int TransactionAmount { get; set; }
        public string Status { get; set; }
        public string Currency { get; set; }
        public int CountryId { get; set; }
        public string AuthorisationCode { get; set; }
        public string RefundReason { get; set; }

    }
}
