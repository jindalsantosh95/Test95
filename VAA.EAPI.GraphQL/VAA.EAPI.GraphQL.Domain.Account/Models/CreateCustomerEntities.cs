using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;

namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class CreateCustomerEntities
    {
        public string FcNumber { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public AccountContactDetails ContactDetails { get; set; }
    }
}
