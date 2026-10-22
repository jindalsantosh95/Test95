using System.Text.Json.Serialization;
using VAA.EAPI.Microservices.CRM.Content.Domain.Content.Enums;

namespace VAA.EAPI.Microservices.CRM.Content.Domain.Content
{
    public class ContentRequest
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SourceType? SourceType { get; set; }
        public ContentDetail Detail { get; set; } = new ContentDetail();

    }
}
