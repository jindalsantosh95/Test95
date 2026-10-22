using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Constants
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentTypeEnum
    {
        [EnumMember(Value = "CARD")]
        Card = 0,
        [EnumMember(Value = "CASH")]
        Cash = 1
    }
}
