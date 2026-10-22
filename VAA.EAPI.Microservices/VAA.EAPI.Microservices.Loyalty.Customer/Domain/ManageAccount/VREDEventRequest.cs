using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount
{
    public class VREDEventRequest
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string MemberStatus { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string DateOfBirth { get; set; }
        public string Channel { get; set; }
        public string CreateUpdateTimestamp { get; set; }
    }
}
