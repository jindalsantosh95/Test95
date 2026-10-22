using System.Collections.Generic;
using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberMerge
{
    public class MemberMergeResponse
    {
        public string MemberNumber { get; set; }
        public string MergeToMemberNumber { get; set; }
        public string Value { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
}
