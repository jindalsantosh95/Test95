namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Insurance
{
    public class PurchaseResponse
    {
        public Status Status { get; set; }
        public string TransactionID { get; set; }
        public Purchases Purchases { get; set; }
    }
}