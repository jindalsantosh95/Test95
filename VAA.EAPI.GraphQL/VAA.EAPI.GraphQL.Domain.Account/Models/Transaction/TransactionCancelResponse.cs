using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.Transaction
{
    public class TransactionCancelResponse
    {
        public string Message { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
}
