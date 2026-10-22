using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.CRM.Content.Domain.Content.Enums
{
    public enum SourceType
    {
        [EnumMember(Value = "AEM")]
        AEM = 0,
        [EnumMember(Value = "VHOLS")]
        VHOLS = 1
    }

}
