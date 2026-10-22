namespace VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction
{
    public class UpdateTransactionPnrDetails
    {
        public string TransactionId { get; set; }
        public string BookingReference { get; set; }
        public string TransactionType { get; set; }
    }
}
