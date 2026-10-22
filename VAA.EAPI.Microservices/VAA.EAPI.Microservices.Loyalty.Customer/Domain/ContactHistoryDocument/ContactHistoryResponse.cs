using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.ContactHistoryDocument
{
    public class ContactHistoryResponse
    {
        public IEnumerable<ContactHistory> ContactHistoryAudit { get; set; }

        public ContactHistoryResponse(IEnumerable<ContactHistory> contactHistoryDocuments)
        {
            ContactHistoryAudit = contactHistoryDocuments;
        }
    }
}
