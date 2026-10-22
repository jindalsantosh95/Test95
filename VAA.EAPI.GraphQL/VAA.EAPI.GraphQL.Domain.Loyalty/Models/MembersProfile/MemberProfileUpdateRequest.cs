
namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.MembersProfile
{
    public class MemberProfileUpdateRequest
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

        public static explicit operator MemberProfileUpdateRequest(VACMemberProfileUpdateRequest memberProfileUpdateRequest)
        {
            return new()
            {
                CustomerId = memberProfileUpdateRequest.CustomerId,
                MemberNumber = memberProfileUpdateRequest.MemberNumber,
                FirstName = memberProfileUpdateRequest.FirstName,
                LastName = memberProfileUpdateRequest.LastName,
                DateOfBirth = memberProfileUpdateRequest.DateOfBirth,
                EmailId = memberProfileUpdateRequest.EmailId,
                PhoneNumber = memberProfileUpdateRequest.PhoneNumber,
                CountryCode = memberProfileUpdateRequest.CountryCode,
                LastUpdatedBy = memberProfileUpdateRequest.LastUpdatedBy,
                Channel = memberProfileUpdateRequest.Channel
            };
        }
    }
}
