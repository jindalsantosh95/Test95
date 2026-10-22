using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Documents
{
    public class DocumentLinkResponseStatus
    {
        public int HttpStatusCode { get; set; }
        public IEnumerable<DocumentLinkError> Errors { get; set; }
    }
}
