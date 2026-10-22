using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMSReshopCancel
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ReshoppingActionType
    {
        [EnumMember(Value = "Refund")]
        Refund = 1,
        [EnumMember(Value = "Reusable")]
        Reusable = 2,
        [EnumMember(Value = "Void")]
        Void = 3,
    }
}
