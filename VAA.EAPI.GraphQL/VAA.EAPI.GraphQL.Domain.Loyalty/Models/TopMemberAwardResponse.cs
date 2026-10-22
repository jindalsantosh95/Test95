using System.Collections.Generic;
using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class TopMemberAwardResponse
    {
        public string RequestId { get; set; }
        public Award Award { get; set; }
        public List<ErrorDetails> Errors { get; set; }
        public int CommandStatus { get; set; }
    }
}
