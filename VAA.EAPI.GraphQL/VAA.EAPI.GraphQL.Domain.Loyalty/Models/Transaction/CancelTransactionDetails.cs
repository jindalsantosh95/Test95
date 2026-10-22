namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction
{
    public class CancelTransactionDetails
    {
        public string TransactionId { get; set; }
        public string TransctionType { get; set; }
        public string TransactionStatus { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
    }
}
