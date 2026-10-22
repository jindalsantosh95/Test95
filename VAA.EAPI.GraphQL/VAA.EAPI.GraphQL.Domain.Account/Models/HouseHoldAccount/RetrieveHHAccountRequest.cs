using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount
{
    public class RetrieveHHAccountRequest
    {
        public HouseHoldMemberType MemberType { get; set; }
        public string HouseHoldFcNumber { get; set; }
    }
}
