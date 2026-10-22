using System.Collections.Generic;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Entities.Enums;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction
{
    public class VRedTransactionResponse
    {
        public Status Status { get; set; }
        public string Value { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
}
