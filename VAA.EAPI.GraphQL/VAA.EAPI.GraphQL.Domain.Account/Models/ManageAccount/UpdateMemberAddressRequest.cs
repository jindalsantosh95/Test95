using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount
{
    public class UpdateMemberAddressRequest
    {
        public HouseHoldMemberType MemberType { get; set; }
        public AccountAddressDetail AccountAddressDetail { get; set; }
    }
}
