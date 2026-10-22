using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment
{
    public class VirginAccountDetails
    {
        public AccountAddressDetail AddressDetails { get; set; }
        public string Gender { get; set; }
        public IEnumerable<AccountPreference> Preferences { get; set; }
        public bool TermsAndConditionsAccepted { get; set; }
        public string EnrolmentChannel { get; set; }
        public string MediaCode { get; set; }
    }
}
