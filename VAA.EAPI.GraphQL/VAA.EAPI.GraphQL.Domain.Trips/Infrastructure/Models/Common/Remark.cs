using HotChocolate;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.Common
{
    [GraphQLName("GraphqlTripsRemark")]
    public class Remark
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum RemarksType
        {
            [EnumMember(Value = "SSR")]
            SSR = 0,
            [EnumMember(Value = "OSI")]
            OSI = 1
        }
        public RemarksType ItemType { get; set; }
        public string Code { get; set; }
        public string Text { get; set; }
    }
}
