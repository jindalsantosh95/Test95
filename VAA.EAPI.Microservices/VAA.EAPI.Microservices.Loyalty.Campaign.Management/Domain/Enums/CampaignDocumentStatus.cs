using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums
{
    public enum CampaignDocumentStatus
    {
        [EnumMember(Value = "Targeted")]
        DocumentStatusTargeted,
        [EnumMember(Value = "Registered")]
        DocumentStatusRegistered,
    }
}
