using System.Text.Json.Serialization;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Enum;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Content
{
    public class ContentRequest
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SourceType? SourceType { get; set; }
        public ContentDetail Detail { get; set; } = new ContentDetail();

    }
}
