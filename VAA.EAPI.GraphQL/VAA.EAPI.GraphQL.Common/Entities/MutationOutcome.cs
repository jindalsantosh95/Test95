using System.Collections.Generic;
using VAA.EAPI.GraphQL.Common.Entities.Enums;

namespace VAA.EAPI.GraphQL.Common.Entities
{
    public class MutationOutcome
    {
        public string RequestId { get; set; }
        public List<ErrorDetails> Errors { get; set; }
        public Status CommandStatus { get; set; }
    }
}
