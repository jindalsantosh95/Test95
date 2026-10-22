using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class ContactHistoryAuditResponse
    {
        public IEnumerable<ContactHistory> ContactHistoryAudit { get; set; }
    }
}
