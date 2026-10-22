using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SSRToPNR
{
    public class MemberDetailsToAddSSR : TableEntity
    {
        public string AmperityId { get; set; }
        public string MemberNumber { get; set; }
        public string PNR { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Pos { get; set; }        
        public string OrderId { get; set; }
        public string Response { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Statuses
        {
            [EnumMember(Value = "Ingested")]
            Ingested = 0,
            [EnumMember(Value = "Failed")]
            Failed = 2,
            [EnumMember(Value = "Processed")]
            Processed = 1,

        }
    }
}
