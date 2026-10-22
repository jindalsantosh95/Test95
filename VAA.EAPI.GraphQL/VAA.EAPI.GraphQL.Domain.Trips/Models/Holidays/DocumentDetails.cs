using HotChocolate;
using Newtonsoft.Json;
using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    [GraphQLName("HolidayDocumentDetails")]
    public class DocumentDetails
    {
        public string? Name { get; set; }
        public string? BookingReference { get; set; }
        public string? DocumentId { get; set; }
        public string? DocumentType { get; set; }
        public string? DocumentToken { get; set; }
        [GraphQLIgnore]
        [JsonIgnore]
        public string? DocumentUrl { get; set; }
        public int? Status { get; set; }
        public IEnumerable<ErrorDetails>? Errors { get; set; }
    }
}
