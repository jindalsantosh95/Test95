using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.VoucherMonitoring
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum VoucherTxnProcessingStatus
    {
        [EnumMember(Value = "NotSet")]
        NotSet,
        [EnumMember(Value = "Success")]
        Success,
        [EnumMember(Value = "PartialSuccess")]
        PartialSuccess,
        [EnumMember(Value = "Failed")]
        Failed
    }
}
