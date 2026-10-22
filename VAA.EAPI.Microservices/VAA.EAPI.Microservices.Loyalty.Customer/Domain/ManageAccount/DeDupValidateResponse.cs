using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount
{
    public class VaEnrolmentResponseResponse
    {
        public EnrolmentResponse Value { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
}
