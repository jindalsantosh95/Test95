using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.Customer;


namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS.Payment
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
            MoneyPlusMilesPartial = 9
        }
        public string FormOfPaymentId { get; set; }
        public FormOfPayment? FormOfPayments { get; set; }
        public CurrencyAmount Amount { get; set; }
        public string BspMethod { get; set; }
        public CardDetails CardMethod { get; set; }
        public PaymentPayer Payer { get; set; }
        public string PaymentReferenceId { get; set; }
        public string ParentPaymentReferenceId { get; set; }
        public PaymentCardAuthorization PaymentCardAuthorization { get; set; }
        public IEnumerable<Remark> PaymentRemarks { get; set; }
        public FinancialInstitution FinancialInstitution { get; set; }

        public static explicit operator PaymentDetails((PaymentMS.PaymentDeails.PaymentDetails source, string paymentReferenceId, Basket basket) input)
        {
            var (source, paymentReferenceId, basketSeats) = input;
            if (input.source == null) return null;

            return new PaymentDetails
            {
                FormOfPaymentId = input.source.FormOfPaymentId,
                FormOfPayments = (FormOfPayment)input.source.FormOfPayments,
                Amount = (CurrencyAmount)(basketSeats),
                CardMethod = (CardDetails)(input.source.CardMethod, input.source.Payer, input.source.BillingAddress),
                Payer = (PaymentPayer)(input.source, basketSeats?.ContactDetails),
                PaymentReferenceId = paymentReferenceId,
                FinancialInstitution = (FinancialInstitution)input.source.FinancialInstitution,
                PaymentRemarks = input.source.PaymentRemarks?.Select(r => (Remark)r),
                PaymentCardAuthorization = (PaymentCardAuthorization)input.source.PaymentCardAuthorization
            };
        }
    }
}