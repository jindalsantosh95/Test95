using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.MembersProfile
{
    public class MemberPreferenceUpdateRequest
    {
        public string FcNumber { get; set; }
        public string CustomerId { get; set; }
        public IEnumerable<MemberPreference> Preference { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string TierCode { get; set; }
        public string LastUpdatedBy { get; set; }

        public static explicit operator MemberPreferenceUpdateRequest(VACMemberProfileUpdateRequest request)
        {
            return new()
            {
                FcNumber = request.MemberNumber,
                CustomerId = request.CustomerId,
                Preference = request.Preferences,
                EmailAddress = request.EmailId,
                FirstName = request.FirstName,
                TierCode = request.TierCode,
                LastUpdatedBy = request.LastUpdatedBy,
            };
        }
    }
}
