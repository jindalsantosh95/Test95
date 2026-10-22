using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Documents
{
    public class Document
    {
        public string LeadPassengerName { get; set; }
        public DateOnly? BookingDate { get; set; }
        public DateOnly? DepartureDate { get; set; }
        public int DocumentId { get; set; }
        public int DocumentStoreId { get; set; }
        public string DocumentType { get; set; }
        public IEnumerable<DocumentGroup> DocumentGroups { get; set; }
        public string DocumentName { get; set; }
        public string Description { get; set; }
        public string DispatchStatus { get; set; }
        public string DispatchMethods { get; set; }
        public string DocumentCategory { get; set; }
        public string DocumentSubCategory { get; set; }
        public bool QualityChecked { get; set; }
        public bool DeletedFromStorage { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public IEnumerable<DispatchDetail> DispatchDetails { get; set; }
        public string DispatchSummary { get; set; }
        public bool QualityCheckedInfo { get; set; }
        public int BookingId { get; set; }
        public string BookingStatus { get; set; }
        public string OptionStatus { get; set; }
        public int? ItemNumber { get; set; }
        public string ProductCode { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public Supplier Supplier { get; set; }

        public static explicit operator Document(JToken source)
        {
            if (source == null) return null;

            return new()
            {
                LeadPassengerName = source["leadPassengerName"]?.Value<string>(),
                BookingDate = DateOnly.TryParse(source["bookingDate"]?.Value<string>(), out var bookingDate) ? bookingDate : null,
                DepartureDate = DateOnly.TryParse(source["departureDate"]?.Value<string>(), out var departureDate) ? departureDate : null,
                DocumentId = source["docId"]?.Value<int>() ?? 0,
                DocumentStoreId = source["docStoreId"]?.Value<int>() ?? 0,
                DocumentType = source["docType"]?.Value<string>(),
                DocumentGroups = source["docGroups"]?.Select(x => (DocumentGroup)x) ?? [],
                DocumentName = source["docName"]?.Value<string>(),
                Description = source["description"]?.Value<string>(),
                DispatchStatus = source["dispatchStatus"]?.Value<string>(),
                DispatchMethods = source["dispatchMethods"]?.Value<string>(),
                DocumentCategory = source["docCategory"]?.Value<string>(),
                DocumentSubCategory = source["docSubCategory"]?.Value<string>(),
                QualityChecked = source["qualityChecked"]?.Value<bool>() ?? false,
                DeletedFromStorage = source["deletedFromStorage"]?.Value<bool>() ?? false,
                CreatedDateTime = DateTime.TryParse(source["createdDateTime"]?.Value<string>(), out var createdDateTime) ? createdDateTime : null,
                DispatchDetails = source["dispatchDetails"]?.Select(x => (DispatchDetail)x) ?? [],
                DispatchSummary = source["dispatchSummary"]?.Value<string>(),
                QualityCheckedInfo = source["qualityCheckedInfo"]?["qualityChecked"]?.Value<bool>() ?? false,
                BookingId = source["bookingId"]?.Value<int>() ?? 0,
                BookingStatus = source["bookingStatus"]?.Value<string>(),
                OptionStatus = source["optionStatus"]?.Value<string>(),
                ItemNumber = source["itemNo"]?.Value<int?>(),
                ProductCode = source["productCode"]?.Value<string>(),
                SupplierCode = source["supplierCode"]?.Value<string>(),
                SupplierName = source["supplierName"]?.Value<string>(),
                Supplier = source["supplier"] != null ? (Supplier)(JObject)source["supplier"] : null
            };
        }
    }
}
