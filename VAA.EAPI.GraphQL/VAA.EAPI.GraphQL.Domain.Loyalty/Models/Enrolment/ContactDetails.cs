using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Enrolment
{
    public class ContactDetails
    {
        public List<EmailDetail>? EmailDetails { get; set; }
        public List<PhoneDetail>? PhoneDetails { get; set; }
    }
}
