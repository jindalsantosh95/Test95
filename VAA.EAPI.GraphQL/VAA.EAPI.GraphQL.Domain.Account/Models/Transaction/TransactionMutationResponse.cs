using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.Transaction
{
    public class TransactionMutationResponse
    {
        public string VaaTransactionId { get; set; }
        public string OrderId { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
}
