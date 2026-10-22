using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Documents
{
    public class HolidaysPendingDocumentListResponse
    {
        public IEnumerable<PendingDocument> PendingDocuments { get; set; }

        public static explicit operator HolidaysPendingDocumentListResponse(JObject source)
        {
            if (source == null) return null;
            if (source["data"] == null || !source["data"].HasValues) return null;

            return new()
            {
                PendingDocuments = source["data"].Select(x => (PendingDocument)x)
            };
        }
    }

    public class PendingDocument
    {
        public string Format { get; set; }
        public string DocumentType { get; set; }
        public string DispatchMethods { get; set; }
        public string Template { get; set; }
        public IEnumerable<PendingDocumentAttribute> Attributes { get; set; }
        public DateTime? DispatchDateTime { get; set; }
        public string Description { get; set; }

        public static explicit operator PendingDocument(JToken source)
        {
            if (source == null) return null;

            return new()
            {
                Format = source["format"]?.Value<string>(),
                DocumentType = source["docType"]?.Value<string>(),
                DispatchMethods = source["dispatchMethods"]?.Value<string>(),
                Template = source["template"]?.Value<string>(),
                Attributes = source["attributes"]?.Select(x => (PendingDocumentAttribute)x) ?? [],
                DispatchDateTime = DateTime.TryParse(source["dispatchDateTime"]?.Value<string>(), out var dispatchDateTime)
                                    ? dispatchDateTime : null,
                Description = source["description"]?.Value<string>()
            };
        }
    }

    public class PendingDocumentAttribute
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public static explicit operator PendingDocumentAttribute(JToken source)
        {
            if (source == null) return null;

            return new()
            {
                Name = source["name"]?.Value<string>(),
                Value = source["value"]?.Value<string>()
            };
        }
    }
}
