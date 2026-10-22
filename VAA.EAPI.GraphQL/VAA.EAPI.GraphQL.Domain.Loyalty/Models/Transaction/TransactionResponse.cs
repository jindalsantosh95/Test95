namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction
{
    public class TransactionResponse : MutationProviderResponseBase
    {
        public Transaction Transaction { get; set; }
    }
}
