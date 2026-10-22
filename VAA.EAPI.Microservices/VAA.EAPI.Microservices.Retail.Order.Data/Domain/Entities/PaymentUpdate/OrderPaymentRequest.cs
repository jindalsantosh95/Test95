using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.PaymentUpdate
{
    public class OrderPaymentRequest
    {

        public int ClientId { get; set; }
        public int TransactionId { get; set; }
        public string TransactionDateTime { get; set; }
        public int TransactionAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentType PaymentType { get; set; }
        public PaymentSubType PaymentSubType { get; set; }
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public string CardExpiry { get; set; }
        public PaymentStatusCode Status { get; set; }
        public string Currency { get; set; }
        public int CountryId { get; set; }
        public string AuthorisationCode { get; set; }
        public int FraudCode { get; set; }
        public string FraudStatus { get; set; }
        public string PspName { get; set; }
        public int PspId { get; set; }
    }
}
