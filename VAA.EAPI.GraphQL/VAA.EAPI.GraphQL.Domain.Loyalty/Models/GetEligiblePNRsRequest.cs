using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Enrolment;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class GetEligiblePNRsRequest
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public CustomerDetails CustomerDetails { get; set; }
    }
}
