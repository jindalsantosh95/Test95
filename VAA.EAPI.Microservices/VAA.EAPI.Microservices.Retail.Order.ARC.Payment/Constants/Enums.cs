using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Constants
{
    public class Enums
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ProductType
        {
            [EnumMember(Value = "Ticket")]
            Ticket = 100,
            [EnumMember(Value = "Seat")]
            Seat = 200
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ARCRequestType
        {
            [EnumMember(Value = "Rfc")]
            Rfc,
            [EnumMember(Value = "NonRfc")]
            NonRfc,
            [EnumMember(Value = "EvenExchange")]
            EvenExchange,
            [EnumMember(Value = "NameCorrection")]
            NameCorrection,
           [EnumMember(Value = "AddCollect")]
            AddCollect
        }
    }
}
