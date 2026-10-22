using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Enum
{
    public enum SourceType
    {
        [EnumMember(Value = "AEM")]
        AEM = 0,
        [EnumMember(Value = "VHOLS")]
        VHOLS = 1
    }
}