using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class AccountableDocumentInConnection
    {
        public string AccountableDocumentNum { get; set; }
        public string CouponNum { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum DocTypeEnum
        {
            [EnumMember(Value = "O")]
            O = 1,
            [EnumMember(Value = "N")]
            N = 2
        }
        public DocTypeEnum DocType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum DocumentSubCategoryCdEnum
        {
            [EnumMember(Value = "REV")]
            REV = 1,
            [EnumMember(Value = "AWD")]
            AWD = 2,
            [EnumMember(Value = "CPM")]
            CPM = 3,
            [EnumMember(Value = "MAC")]
            MAC = 4
        }
        public DocumentSubCategoryCdEnum DocumentSubCategoryCd { get; set; }
        public string DocumentCategoryCode { get; set; }
        public string DocumentConnectionTypeCode { get; set; }
        public Amount EmdAmount { get; set; }
        public string IssueDate { get; set; }
        public bool EcreditEligibility { get; set; }
        public string AssociationStatus { get; set; }
        public string ReasonForIssuanceSubCode { get; set; }
        public string ReasonForIssuanceSubCodeDesc { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public string RedeemerLoyaltyMemberAccountNum { get; set; }
        public string AccountableDocExpirationDate { get; set; }
    }
}
