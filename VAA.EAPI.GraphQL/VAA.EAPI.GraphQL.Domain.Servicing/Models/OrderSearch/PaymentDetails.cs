using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.OrderSearch
{
    public class PaymentDetails
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum FormOfPayments
        {
            Card,
            Cash,
            Voucher,
            BSP
        }
        public FormOfPayments? FormOfPayment { get; set; }
        public Amount? Amount { get; set; }
        public string? BSPMethod { get; set; }
        public CardMethod? CardMethod { get; set; }
        public Payer? Payer { get; set; }
        public VoucherMethod? VoucherMethod { get; set; }
    }
}
