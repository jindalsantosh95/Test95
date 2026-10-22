using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Documents
{
    public class HolidaysDocumentListResponse
    {
        public IEnumerable<Document> Documents { get; set; }

        public static explicit operator HolidaysDocumentListResponse(JObject source)
        {
            if (source == null) return null;
            if (source["data"] == null || !source["data"].HasValues) return null;

            return new()
            {
                Documents = source["data"].Select(x => (Document)x)
            };
        }
    }
}
