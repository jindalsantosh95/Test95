using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment
{
    public class FlyingClubSignUpResponse
    {
        public string CustomerId { get; set; }
        public MemberStatus MemberDetails { get; set; }
        public List<ErrorDetails> Errors { get; set; }
        public Status Status { get; set; }
    }
}
