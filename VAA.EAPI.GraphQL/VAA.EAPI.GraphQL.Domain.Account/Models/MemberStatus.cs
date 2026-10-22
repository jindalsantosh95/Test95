using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class MemberStatus
    {
        public string MemberNumber { get; set; }

        public string FlyingClubCardNumber { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum StatusEnum
        {
            Active = 0,
            Expired = 1,
            Frozen = 2,
            Inactive = 3,
            Merged = 4,
            Deceased = 5,
            Suspended = 6,
            Cancelled = 7,
            [EnumMember(Value = "Active Pending")]
            ActivePending = 8,
            Fraud = 9,
            [EnumMember(Value = "Pending Cancellation")]
            PendingCancellation = 10,
            [EnumMember(Value = "Route Deal")]
            RouteDeal = 11
        }

        public StatusEnum? Status { get; set; }

    }
}
