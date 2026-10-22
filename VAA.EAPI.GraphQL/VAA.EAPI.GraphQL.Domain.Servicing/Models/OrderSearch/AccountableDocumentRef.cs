using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.OrderSearch
{
    public class AccountableDocumentRef
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum DocumentTypes
        {
            Ticket,
            EMD
        }
        public DocumentTypes? DocumentType { get; set; }
        public string? DocumentNumber { get; set; }
        public string? DocumentIssuer { get; set; }
        public string? IssueDate { get; set; }
        public string? CustomerRefId { get; set; }
        public string? FareCalculation { get; set; }
        public List<Coupon>? Coupons { get; set; }
    }
}