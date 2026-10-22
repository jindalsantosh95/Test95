using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment
{
    public class EnrolmentRequest
    {
        public EnrolCustomerRequest CustomerDetails { get; set; }
    }
}
