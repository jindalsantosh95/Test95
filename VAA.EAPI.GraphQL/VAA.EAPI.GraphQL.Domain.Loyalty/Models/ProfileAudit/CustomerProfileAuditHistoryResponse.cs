using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.ProfileAudit
{
    public class CustomerProfileAuditHistoryResponse
    {
        public IEnumerable<CustomerProfileAudit> CustomerProfileHistoryAudit { get; set; }
    }
}