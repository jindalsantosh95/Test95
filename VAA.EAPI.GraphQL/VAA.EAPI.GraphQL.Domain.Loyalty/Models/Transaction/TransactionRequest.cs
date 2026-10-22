namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction
{
    public class TransactionRequest
    {
        public Transaction TransactionDocument { get; set; }
        public string ProductName { get; set; }
        public string PartnerName { get; set; }
    }
}
