using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PreferenceTypes
    {
        [EnumMember(Value = "VAAMarketingPermissions")]
        VAAMarketingPermissions = 0,
        [EnumMember(Value = "VAPartnersMarketingPermissions")]
        VAPartnersMarketingPermissions = 1,
        [EnumMember(Value = "EmailCommunication")]
        EmailCommunication = 2,
        [EnumMember(Value = "PostCommunication")]
        PostCommunication = 3,
        [EnumMember(Value = "FaxCommunication")]
        FaxCommunication = 4,
        [EnumMember(Value = "FlyingClubNews")]
        FlyingClubNews = 5,
        [EnumMember(Value = "Language")]
        Language = 6,
        [EnumMember(Value = "PhoneCommunication")]
        PhoneCommunication = 7,
        [EnumMember(Value = "DoNotSMS")]
        DoNotSMS = 8,
        [EnumMember(Value = "DoNotEmail")]
        DoNotEmail = 9,
        [EnumMember(Value = "DoNotPost")]
        DoNotPost = 10,
        [EnumMember(Value = "DoNotCall")]
        DoNotCall = 11,
        [EnumMember(Value = "DoNotFax")]
        DoNotFax = 12,
        [EnumMember(Value = "Communication")]
        Communication = 13,
        [EnumMember(Value = "DoNotMail")]
        DoNotMail = 14
    }

}
