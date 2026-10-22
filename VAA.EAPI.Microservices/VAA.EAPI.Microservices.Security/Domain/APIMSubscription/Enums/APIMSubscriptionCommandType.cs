using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Security.Domain.APIMSubscription.Enums
{
    public enum APIMSubscriptionCommandType
    {
        [EnumMember(Value = "VAA.Security.APIM.CreateSubscription")]
        CreateSubscription,
        [EnumMember(Value = "VAA.Security.APIM.UpdateSubscription")]
        UpdateSubscription,
        [EnumMember(Value = "VAA.Security.APIM.DeleteSubscription")]
        DeleteSubscription,
    }
}
