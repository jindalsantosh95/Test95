using System.Collections.Generic;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Entities.Enums;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class MutationProviderResponseBase
    {
        public string RequestId { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
        public Status CommandStatus { get; set; }
    }
}
