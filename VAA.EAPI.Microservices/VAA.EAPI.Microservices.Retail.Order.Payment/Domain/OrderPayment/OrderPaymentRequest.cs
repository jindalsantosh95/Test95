using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment
{
    public class OrderPaymentRequest
    {
        [OpenApiProperty(Description = "Client ID from Cell Point")]
        public int ClientId { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "Payment transaction ID")]
        public int TransactionId { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "Payment transaction Date and Time")]
        [DataType(DataType.DateTime)]
        public string TransactionDateTime { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "Payment transaction amount")]
        public int TransactionAmount { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The type of method used for payment")]
        public PaymentMethod PaymentMethod { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The type of payment")]
        public PaymentType PaymentType { get; set; }

        [OpenApiProperty(Description = "The card scheme/network of the transaction eg. Visa / Mastercard")]
        public PaymentSubType? PaymentSubType { get; set; }

        [OpenApiProperty(Description = "The card scheme/network of the transaction eg. Visa / Mastercard")]
        public string CardName { get; set; }

        [OpenApiProperty(Description = "Masked Card Number (411234******4113)")]
        public string CardNumber { get; set; }

        [OpenApiProperty(Description = "Expiry date of card, in MM-YY format")]
        public string CardExpiry { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "Status of transaction returned by Cell Point")]
        public PaymentStatusCode Status { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The currency of the transaction")]
        public string Currency { get; set; }

        [OpenApiProperty(Description = "Country ID from Cell Point")]
        public int CountryId { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "Authorisation code from the acquirer")]
        public string AuthorisationCode { get; set; }

        [OpenApiProperty(Description = "Fraud code from Accertify")]
        public int FraudCode { get; set; }

        [OpenApiProperty(Description = "Fraud status from Accertify")]
        public string FraudStatus { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "Name of PSP used to process transaction")]
        public string PspName { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "ID of PSP used to process transaction")]
        public int PspId { get; set; }
    }
}
