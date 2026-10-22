using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.Customer;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Payment
{
    public class PaymentDetails
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
            BSPEnum = 3,
            [EnumMember(Value = "ApplePay")]
            ApplePay = 4,
            [EnumMember(Value = "PayPal")]
            PayPal = 5,
            [EnumMember(Value = "GooglePay")]
            GooglePay = 6,
            [EnumMember(Value = "MilesPoints")]
            MilesPoint = 7,
            [EnumMember(Value = "MilesPlusMoneyFull")]
            MoneyPlusMilesFull = 8,
            [EnumMember(Value = "MilesPlusMoneyPartial")]
            MoneyPlusMilesPartial = 9
        }
        public string FormOfPaymentId { get; set; }
        public FormOfPayment? FormOfPayments { get; set; }
        public CurrencyAmount Amount { get; set; }
        public string BspMethod { get; set; }
        public CardDetails CardMethod { get; set; }
        public PaymentPayer Payer { get; set; }
        public Address BillingAddress { get; set; }
        public VoucherMethod VoucherMethod { get; set; }
        public PaymentCardAuthentication PaymentCardAuthentication { get; set; }
        public string PaymentReferenceId { get; set; }
        public string VendorPaymentTransactionId { get; set; }
        public string ParentPaymentReferenceId { get; set; }
        public PaymentCardAuthorization PaymentCardAuthorization { get; set; }
        public IEnumerable<Remark> PaymentRemarks { get; set; }
        public FinancialInstitution FinancialInstitution { get; set; }
        public string MilesPoints { get; set; }
        public CurrencyAmount MilesEquivalentPrice { get; set; }
    }
}
