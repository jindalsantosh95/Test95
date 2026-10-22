using System.Collections.Generic;
using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction
{
    public class CreateTransactionMutationResponse
    {
        public string TransactionId { get; set; }
        public string TransactionStatus { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
}
