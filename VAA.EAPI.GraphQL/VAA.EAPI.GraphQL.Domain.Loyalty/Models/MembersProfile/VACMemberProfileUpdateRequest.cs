
using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.MembersProfile
{
    public class VACMemberProfileUpdateRequest
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string EmailId { get; set; }
        public string PhoneNumber { get; set; }
        public string CountryCode { get; set; }
        public string LastUpdatedBy { get; set; }
        public string Channel { get; set; }
        public IEnumerable<MemberPreference>? Preferences { get; set; }
        public string TierCode { get; set; }
    }
}
