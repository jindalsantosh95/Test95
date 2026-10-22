using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;

namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class FcCustomerResponse : CustomerCustMS
    {
        public FlyingClubMember FlyingClubProfileDetails { get; set; }
        public FcCustomerResponse() { }

        public FcCustomerResponse(CustomerCustMS response, FlyingClubMember fCResponse)
        {
            CustomerId = response.CustomerId;
            FirstName = response.FirstName;
            LastName = response.LastName;
            Gender = response.Gender;
            Salutation = response.Salutation;
            Pronouns = response.Pronouns;
            Nationality = response.Nationality;
            LoyaltyNumber = response.LoyaltyNumber;
            DateOfBirth = response.DateOfBirth;
            AddressDetails = response.AddressDetails;
            ContactDetails = response.ContactDetails;
            MemberAccountType = response.MemberAccountType;
            HeadOfGroupMemberNumber = response.HeadOfGroupMemberNumber;
            Status = response.Status;
            TermsAndConditionsAccepted = response.TermsAndConditionsAccepted;
            FlyingClubProfileDetails = fCResponse;
        }
    }
}
