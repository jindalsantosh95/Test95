namespace VAA.EAPI.NDC.Order.Infrastructure.Model
{
    public class TransactionIdStatus
    {
        public string TransactionId {  get; set; }
        public bool PaymentStatus {  get; set; }
        public decimal? Amount { get; set; }
    }
}
