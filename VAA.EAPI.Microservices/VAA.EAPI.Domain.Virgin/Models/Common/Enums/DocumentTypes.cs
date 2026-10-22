using System.Runtime.Serialization;

namespace VAA.EAPI.Domain.Virgin.Models.Common.Enums
{
    public enum DocumentTypes
    {
        [EnumMember(Value = "Passport")]
        Passport,
        [EnumMember(Value = "Visa")]
        Visa,
        [EnumMember(Value = "GreenCard")]
        GreenCard,
        [EnumMember(Value = "KnownTraveller")]
        KnownTraveller,
        [EnumMember(Value = "Redress")]
        Redress,
    }
}
