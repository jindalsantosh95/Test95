using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Runtime.Serialization;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.Domain.Virgin.Models.Payment
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
            BSP = 3,
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
            MoneyPlusMilesPartial = 9,
            [EnumMember(Value = "EasyPay")]
            EasyPay = 10,
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
        public bool PaymentStatus { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Status
        {
            [EnumMember(Value = "SUCCESSFUL")]
            SUCCESSFUL,
            [EnumMember(Value = "FAILED")]
            FAILED,
            [EnumMember(Value = "PENDING")]
            PENDING
        }
        public Status? PaymentProcessingStatus { get; set; }
        public string VendorPaymentTransactionId { get; set; }
        public string ParentPaymentReferenceId { get; set; }
        public PaymentCardAuthorization PaymentCardAuthorization { get; set; }
        public IEnumerable<Remark> PaymentRemarks { get; set; }
        public FinancialInstitution FinancialInstitution { get; set; }
        public string MilesPoints { get; set; }
        public CurrencyAmount MilesEquivalentPrice { get; set; }
        public string PaymentCommitmentDateTime { get; set; }
        public string TrackId { get; set; }
        public IEnumerable<string> PaxRefIds { get; set; }
        public string SequenceId { get; set; }
        public string TransactionType { get; set; }
        public string ApprovalCode { get; set; }
    }
}
