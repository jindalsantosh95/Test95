using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Documents
{
    public class RetrieveDocumentLinkListRequest
    {
        public IEnumerable<DocumentLinkRequest> DocumentLinkRequests { get; set; }
    }
}
