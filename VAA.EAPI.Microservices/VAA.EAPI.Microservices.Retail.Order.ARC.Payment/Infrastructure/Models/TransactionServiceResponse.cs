namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models
{
    public class TransactionServiceResponse
    {
        public string RequestId { get; set; }
        public int TotalTransactionsAccepted { get; set; }
        public int TotalTransactionsRejected { get; set; }
        public int TotalTransactionsReceived { get; set; }
        public string Status { get; set; }
        public IEnumerable<object> Rejections { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
