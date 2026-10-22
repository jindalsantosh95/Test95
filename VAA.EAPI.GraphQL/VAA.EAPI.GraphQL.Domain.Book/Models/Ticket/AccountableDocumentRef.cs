using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Ticket
{
    public class AccountableDocumentRef
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum DocumentTypes
        {
            [EnumMember(Value = "Ticket")]
            TEnum = 0,
            [EnumMember(Value = "EMD")]
            YEnum = 1
        }
        public DocumentTypes? DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentIssuer { get; set; }
        public string IssueDate { get; set; }
        public string CustomerRefId { get; set; }
        public bool AllFlown { get; set; }
        public string FareCalculation { get; set; }
        public IEnumerable<Coupon> Coupons { get; set; }
        public string FirstNameNumber { get; set; }
        public string LastNameNumber { get; set; }

    }
}