using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Documents
{
    public class HolidaysDocumentDownloadResponse
    {
        public string DownloadLink { get; set; }
        public int? DocumentId { get; set; }
        public IEnumerable<VAA.EAPI.Domain.Entities.Common.ErrorDetails> Errors { get; set; }

        public static explicit operator HolidaysDocumentDownloadResponse((JObject source, int? docId) data)
        {
            var (source, docId) = data;

            if (source == null) return null;
            if (source["data"] == null || !source["data"].HasValues) return null;

            return new()
            {
                DownloadLink = source["data"][0]?["link"]?.Value<string>(),
                DocumentId = docId
            };
        }
    }
}
