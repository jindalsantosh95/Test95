using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment
{
    public partial class EnrolmentResponse
    {
        public string CustomerId { get; set; }
        public MemberStatus Memberdetails { get; set; }

    }
}
