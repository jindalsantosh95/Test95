using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount
{
    public class UpdateAddressRequest
    {
        public AccountAddressDetail NewAddressDetails { get; set; }
        public IEnumerable<string> CustomerIds { get; set; }
    }
}
