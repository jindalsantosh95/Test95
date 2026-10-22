using System;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.MemberAccount
{
    public class AuditEventRequest
    {
        public CustomerProfileAuditDocument CustomerProfileAuditDocument { get; set; }
    }

    public class CustomerProfileAuditDocument
    {
        public string? SourceIdentifier { get; set; }
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Status { get; set; }
        public string CustomerProfileUpdatedBy { get; set; }
        public string CustomerProfileUpdatedOn { get; set; }
    }

}
