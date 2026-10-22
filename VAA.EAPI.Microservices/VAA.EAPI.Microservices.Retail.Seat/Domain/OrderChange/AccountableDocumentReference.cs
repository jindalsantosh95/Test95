using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Seat.Domain.OrderChange
{
    public class AccountableDocumentReference
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum DocumentTypes
        {
            [EnumMember(Value = "T")]
            TEnum = 0,
            [EnumMember(Value = "Y")]
            YEnum = 1,
            [EnumMember(Value = "INF")]
            INFEnum = 2
        }
        [OpenApiProperty(Description = "Document type code. example T- Ticket , Y - EMD-S (Standalone),INF- Infant ticket")]
        public DocumentTypes? DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string CheckDigit { get; set; }
        public string CouponNumber { get; set; }
        public string CouponStatus { get; set; }
        public string SegmentRefId { get; set; }
        public string DocumentIssuer { get; set; }
        public DateTime? IssueDate { get; set; }
        public string CustomerRefId { get; set; }
        public string CurrentSeatAssignment { get; set; }
    }
}
