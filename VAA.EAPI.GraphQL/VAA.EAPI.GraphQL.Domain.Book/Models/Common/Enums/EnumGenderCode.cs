using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Common.Enums
{
    public enum EnumGenderCode
    {
        [EnumMember(Value = "Male")]
        Male = 1,
        [EnumMember(Value = "Female")]
        Female = 2,
        [EnumMember(Value = "Undisclosed")]
        Undisclosed = 3,
        [EnumMember(Value = "Unspecified")]
        Unspecified = 4
    }
}
