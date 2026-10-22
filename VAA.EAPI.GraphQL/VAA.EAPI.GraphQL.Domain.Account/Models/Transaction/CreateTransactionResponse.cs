using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.Transaction
{
    public class CreateTransactionResponse
    {
        public string Value { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
}
