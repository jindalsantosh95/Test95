using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Domain.CustomerProfile
{
    public class CustomerProfileAuditHistoryResponse
    {
        public IEnumerable<CustomerProfileAudit> CustomerProfileHistoryAudit { get; set; }

        public CustomerProfileAuditHistoryResponse(IEnumerable<CustomerProfileAudit> customerProfileHistoryDocuments)
        {
            CustomerProfileHistoryAudit = customerProfileHistoryDocuments;
        }
    }
}