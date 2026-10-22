using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.Sustain
{
    [GraphQLName("CabinTypeSustain")]
    public enum CabinType
    {
        [EnumMember(Value = "Economy")]
        Economy,
        [EnumMember(Value = "Business")]
        Business,
        [EnumMember(Value = "Premium")]
        Premium,
        [EnumMember(Value = "First")]
        First
    }
}
