using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CustomerDetails;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Domain
{
    public class EnrolmentResponse
    {
        public string CustomerId { get; set; }
        public MemberStatus Memberdetails { get; set; }

    }
}
