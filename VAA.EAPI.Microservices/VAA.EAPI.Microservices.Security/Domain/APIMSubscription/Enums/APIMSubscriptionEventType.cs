using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Security.Domain.APIMSubscription.Enums
{
    public enum APIMSubscriptionEventType
    {
        [EnumMember(Value = "VAA.Security.APIM.CreateSubscription")]
        CreateAPIMSubscription,
        [EnumMember(Value = "VAA.Security.APIM.UpdateSubscription")]
        UpdateAPIMSubscription,
        [EnumMember(Value = "VAA.Security.APIM.DeleteSubscription")]
        DeleteAPIMSubscription,
    }
}
