using System.Collections.Generic;
using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.MembersProfile
{
    public class MemberPreferenceUpdateResponse
    {
        public bool PreferenceValue { get; set; }
        public IEnumerable<ErrorDetails> ErrorDetails { get; set; }
    }
}
