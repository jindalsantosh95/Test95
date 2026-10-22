namespace VAA.EAPI.Microservices.Retail.Order.Domain.Order
{
    public class InsuranceDetails
    {
        public string BusinessUnit { get; set; }
        public string TransactionID { get; set; }
        public Purchase Purchase { get; set; }
    }
}