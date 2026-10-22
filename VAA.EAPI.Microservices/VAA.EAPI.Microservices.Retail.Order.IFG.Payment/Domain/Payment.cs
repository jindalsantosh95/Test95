using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Payment;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Domain
{
    public class Payment : PaymentDetails
    {
        public string PaymentTransactionId { get; set; }
        public IEnumerable<PaymentTransactionStatus> Status { get; set; }
        public string TransactionTypeId { get; set; }
        public string PaymentApprovalCode { get; set; }
        public string Acquirer { get; set; }

        public static explicit operator Payment(JObject source)
        {
            return new Payment()
            {
                PaymentTransactionId = source["PaymentTransactionId"]?.ToString(),
                Status = source["Status"]?.Select(a => (PaymentTransactionStatus)Enum.Parse(typeof(PaymentTransactionStatus), a.ToString())),
                FormOfPaymentId = source["FormOfPaymentId"]?.ToString(),
                FormOfPayments = source["FormOfPayments"] != null ? (FormOfPayment)Enum.Parse(typeof(FormOfPayment), source["FormOfPayments"].ToString()) : null,
                Amount = source["Amount"] != null ? (CurrencyAmount)source["Amount"] : null,
                BspMethod = source["BspMethod"]?.ToString(),
                CardMethod = source["CardMethod"] != null ? (CardDetails)source["CardMethod"] : null,
                Payer = source["Payer"] != null ? (PaymentPayer)source["Payer"] : null,
                BillingAddress = source["BillingAddress"] != null ? (Address)source["BillingAddress"] : null,
                VoucherMethod = source["VoucherMethod"] != null ? (VoucherMethod)source["VoucherMethod"] : null,
                PaymentCardAuthentication = source["PaymentCardAuthentication"] != null ? (PaymentCardAuthentication)source["PaymentCardAuthentication"] : null,
                PaymentReferenceId = source["PaymentReferenceId"]?.ToString(),
                VendorPaymentTransactionId = source["VendorPaymentTransactionId"]?.ToString(),
                ParentPaymentReferenceId = source["ParentPaymentReferenceId"]?.ToString(),
                PaymentCardAuthorization = source["PaymentCardAuthorization"] != null ? (PaymentCardAuthorization)source["PaymentCardAuthorization"] : null,
                PaymentRemarks = source["PaymentRemarks"]?.Select(a => (Remark)a),
                FinancialInstitution = source["FinancialInstitution"] != null ? (FinancialInstitution)source["FinancialInstitution"] : null,
                MilesPoints = source["MilesPoints"]?.ToString(),
                MilesEquivalentPrice = source["MilesEquivalentPrice"] != null ? (CurrencyAmount)source["MilesEquivalentPrice"] : null,
                TransactionTypeId = source["TransactionTypeId"]?.ToString(),
                PaymentApprovalCode = source["PaymentApprovalCode"]?.ToString(),
                Acquirer = source["Acquirer"]?.ToString()
            };
        }
    }
}
