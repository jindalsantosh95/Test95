using Newtonsoft.Json.Linq;
using System;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Holidays
{
    public class PaymentHistory
    {
        public decimal? CurrencyAmount { get; set; }
        public DateTime? RealisedDate { get; set; }
        public string PaymentCurrency { get; set; }
        public string PaymentMethodCode { get; set; }
        public bool Realised { get; set; }
        public DateTime? ReceiptDate { get; set; }
        public PaymentTransactionType TransactionType { get; set; }

        public static explicit operator PaymentHistory(JToken pastPaymentDetails)
        {
            if (pastPaymentDetails == null) return null;

            var payOrRefund = pastPaymentDetails["paymentInfo"]?["payOrRefund"]?.Value<bool?>();
            var amount = pastPaymentDetails["paymentInfo"]?["amount"]?.Value<decimal?>() ?? 0m;
            var paymentMethod = pastPaymentDetails["uniqueId"]?["receiptType"]?.Value<string>();

            return new()
            {
                CurrencyAmount = paymentMethod != Constants.Application.CreditCardPaymentMethod && amount < 0 ? -amount : amount,
                RealisedDate = pastPaymentDetails["paymentInfo"]?["realisedDate"]?.Value<DateTime?>(),
                PaymentCurrency = pastPaymentDetails["paymentInfo"]?["currencyCode"]?.Value<string>() ?? pastPaymentDetails["paymentDetails"]?["exchangeCurrency"]?.Value<string>(),
                PaymentMethodCode = paymentMethod,
                Realised = pastPaymentDetails["paymentInfo"]?["realised"]?.Value<bool>() ?? false,
                ReceiptDate = pastPaymentDetails["paymentInfo"]?["receiptDate"]?.Value<DateTime?>(),
                TransactionType = DetermineTransactionType(payOrRefund, amount, paymentMethod)
            };
        }

        private static PaymentTransactionType DetermineTransactionType(bool? payOrRefund, decimal amount, string? paymentMethod)
        {
            // TravelBox provides negative entries for refunds/corrections. Prioritise those rules over payOrRefund.
            if (amount < 0)
            {
                return paymentMethod == Constants.Application.BankTransferPaymentMethod ? PaymentTransactionType.Refund : PaymentTransactionType.Correction;
            }
            
            if (paymentMethod == Constants.Application.CreditCardPaymentMethod && payOrRefund == false)
            {
                return PaymentTransactionType.Refund;
            }

            return payOrRefund == true ? PaymentTransactionType.Pay : PaymentTransactionType.Refund;
        }
    }
}
