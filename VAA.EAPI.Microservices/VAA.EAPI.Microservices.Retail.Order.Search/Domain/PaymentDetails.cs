using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class PaymentDetails
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum FormOfPayments
        {
            [EnumMember(Value = "Card")]
            CardEnum = 0,
            [EnumMember(Value = "Cash")]
            CashEnum = 1,
            [EnumMember(Value = "Voucher")]
            VoucherEnum = 2,
            [EnumMember(Value = "BSP")]
            BSPEnum = 3,
            [EnumMember(Value = "OfflinePayment")]
            OfflinePaymentEnum = 4,
        }
        [OpenApiProperty(Description = "Component type. for example Tax,Basefare etc.")]
        public FormOfPayments? FormOfPayment { get; set; }
        public Amount Amount { get; set; }
        public string BSPMethod { get; set; }
        public CardMethod CardMethod { get; set; }
        public Payer Payer { get; set; }
        public VoucherMethod VoucherMethod { get; set; }
        public string PaymentID { get; set; }
        public string PaymentReferenceID { get; set; }
    }
}
