using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.FlyingClubMember
{
    public class MemberAttribute
    {
        public string AttributeId { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string AttributeName { get; set; }
        public string Value { get; set; }
        public string MemberNumber { get; set; }
        public string SSAMemberNumber { get; set; }
    }
}