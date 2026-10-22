using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Seat.Domain.OrderChange
{
    public class Payment
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum FormOfPayment
        {
            [EnumMember(Value = "Card")]
            CardEnum = 0,
            [EnumMember(Value = "Cash")]
            CashEnum = 1,
            [EnumMember(Value = "Voucher")]
            VoucherEnum = 2,
            [EnumMember(Value = "BSP")]
            BSPEnum = 3
        }
        public FormOfPayment? FormOfPayments { get; set; }
        public CurrencyAmount Amount { get; set; }
        public string BspMethod { get; set; }
        public CardDetails CardMethod { get; set; }
        public PaymentPayer Payer { get; set; }
        public VoucherDetails VoucherMethod { get; set; }
    }
}
