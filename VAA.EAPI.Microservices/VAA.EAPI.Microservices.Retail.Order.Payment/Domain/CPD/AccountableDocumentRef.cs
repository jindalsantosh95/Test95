using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using VAA.EAPI.Domain.Virgin.Models.Ticket;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD
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
        public bool Primary { get; set; }
        public string DocumentIssuer { get; set; }
        public string IssueDate { get; set; }
        public string CustomerRefId { get; set; }
        public bool AllFlown { get; set; }
        public string FareCalculation { get; set; }
        public IEnumerable<Coupon> Coupons { get; set; }
        public ReasonForIssuance ReasonForIssuance { get; set; }
        public OriginalTicketDocInfo OriginalTicketDocInfo { get; set; }

        public static explicit operator AccountableDocumentRef(JObject source)
        {
            return new AccountableDocumentRef()
            {
                DocumentType = source["DocumentType"] != null ? (DocumentTypes)Enum.Parse(typeof(DocumentTypes), source["DocumentType"].ToString()) : null,
                DocumentNumber = source["DocumentNumber"]?.ToString(),
                Primary = source["Primary"] != null ? (bool)source["Primary"] : false,
                DocumentIssuer = source["DocumentIssuer"]?.ToString(),
                IssueDate = source["IssueDate"]?.ToString(),
                CustomerRefId = source["CustomerRefId"]?.ToString(),
                AllFlown = source["AllFlown"] != null ? (bool)source["AllFlown"] : false,
                FareCalculation = source["FareCalculation"]?.ToString(),
                Coupons = source["Coupons"]?.Select(a => (Coupon)a),
                ReasonForIssuance = source["ReasonForIssuance"] != null ? (ReasonForIssuance)source["ReasonForIssuance"] : null,
                OriginalTicketDocInfo = source["OriginalTicketDocInfo"] != null ? (OriginalTicketDocInfo)source["OriginalTicketDocInfo"] : null
            };
        }
    }
}