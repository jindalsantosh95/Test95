namespace VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions.CreateTransaction
{
    public class TransactionRequest
    {
        public Transaction TransactionDocument { get; set; }
        public string ProductName { get; set; }
        public string PartnerName { get; set; }
    }
}
