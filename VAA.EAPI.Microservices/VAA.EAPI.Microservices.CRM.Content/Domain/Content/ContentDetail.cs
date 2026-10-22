using System.Text.Json.Serialization;
using VAA.EAPI.Microservices.CRM.Content.Domain.Content.Enums;

namespace VAA.EAPI.Microservices.CRM.Content.Domain.Content
{
    public class ContentDetail
    {
        public string? ContentFragmentName { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContentType? ContentType { get; set; }
        public string? ContentId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BrandType? BrandCode { get; set; }
        public string? SupplierName { get; set; }
        public string? SippCode { get; set; }
    }
}
