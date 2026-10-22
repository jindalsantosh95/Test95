using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.Activity
{
    public class ManageAccountResponse<T>
    {
        public HouseHoldMemberType MemberType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PreferredName { get; set; }
        public DateTime DateofBirth { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public string CountryCode { get; set; }
        public IEnumerable<T> Address { get; set; }
        public bool IsVAAMarketingPreferred { get; set; }
        public string HeadOfGroupMemberNumber { get; set; }
    }
}
