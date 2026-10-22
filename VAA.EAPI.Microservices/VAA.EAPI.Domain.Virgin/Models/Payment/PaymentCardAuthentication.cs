using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Domain.Virgin.Models.Payment
{
    public class PaymentCardAuthentication
    {
        [OpenApiProperty(Description = "Electronic Commerce Indicator is part of the 2 data elements that indicate the transaction was processed electronically.")]
        public string Eci { get; set; }
        [OpenApiProperty(Description = "Cardholder Authentication Verification Value (CAVV) is the value generated from algorithm for payment card authentication.")]
        public string Cavv { get; set; }
        [OpenApiProperty(Description = "XID is the transaction identifier resulting from authentication processing.")]
        public string Xid { get; set; }
        [OpenApiProperty(Description = "Server Transaction Identifier is the universally unique transaction identifier assigned by the 3DS Server to identify a single transaction.")]
        public string ServerTransactionId { get; set; }
        [OpenApiProperty(Description = "DS Transaction Identifier is the unique transaction identifier assigned by the Directory Server (DS) to identify a single transaction.")]
        public string DsTransactionId { get; set; }
        [OpenApiProperty(Description = "DS Authentication Version from the DS authentication.")]
        public string DsAuthenticationVersionText { get; set; }
        [OpenApiProperty(Description = "Validation Status Text represents the status of the validation in the response message.")]
        public string ValidationStatusText { get; set; }


        public static explicit operator PaymentCardAuthentication(JObject source)
        {
            return new PaymentCardAuthentication()
            {
                Eci = source["Eci"]?.ToString(),
                Cavv = source["Cavv"]?.ToString(),
                Xid = source["Xid"]?.ToString(),
                ServerTransactionId = source["ServerTransactionId"]?.ToString(),
                DsTransactionId = source["DsTransactionId"]?.ToString(),
                DsAuthenticationVersionText = source["DsAuthenticationVersionText"]?.ToString(),
                ValidationStatusText = source["ValidationStatusText"]?.ToString()
            };
        }
    }
}
