using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Documents
{
    public class HolidaysDocumentLinkListResponse
    {
        public IEnumerable<HolidaysDocumentLinkItem> HolidaysDocumentLinkResponses { get; set; }
    }
}
