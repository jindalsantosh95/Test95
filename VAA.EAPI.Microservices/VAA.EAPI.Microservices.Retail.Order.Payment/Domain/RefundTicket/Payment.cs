namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.RefundTicket
{
    public class Payment
    {
        public string PaymentId { get; set; }
        public int ClientId { get; set; }
        public string ChannelId { get; set; }
        public int TransactionId { get; set; }
        public string TransactionDateTime { get; set; }
        public string TransactionType { get; set; }
        public int TransactionAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentType { get; set; }
        public string PaymentSubType { get; set; }
        public string CardNumber { get; set; }
        public string CardExpiry { get; set; }
        public string Status { get; set; }
        public string Currency { get; set; }
        public int CountryId { get; set; }
        public string AuthorisationCode { get; set; }
        public int FraudCode { get; set; }
        public string FraudStatus { get; set; }
        public string PspName { get; set; }
        public int PspId { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdatedDateTime { get; set; }
    }
}
