using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Seat.Domain.OrderChange
{

    public class CardDetails
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum CardTypes
        {
            [EnumMember(Value = "Visa")]
            VI = 0,
            [EnumMember(Value = "MasterCard")]
            CA = 1,
            [EnumMember(Value = "AmericanExpress")]
            AX = 2,
            [EnumMember(Value = "UATP")]
            TP = 3,
            [EnumMember(Value = "Diners")]
            DC = 4,
            [EnumMember(Value = "Discover")]
            DS = 5,
            [EnumMember(Value = "Maestro")]
            TO = 6
        }
        public CardTypes? CardType { get; set; }
        public string CardNumber { get; set; }
        [OpenApiProperty(Description = "Is the card number provided already tokenised. This means that the airline has key to de-tokenise in order to take payment.")]
        public bool IsTokenised { get; set; }
        public DateTime? IssueDate { get; set; }
        public string ExpiryDate { get; set; }
        public string CardholderName { get; set; }
        public string Cvv { get; set; }
        public Address BillingAddress { get; set; }
        public PaymentCardAuthentication AuthenticationData { get; set; }
    }
}
