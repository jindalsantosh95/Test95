using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;


namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CustomerDetails
{
    public class MemberStatus
    {
        public string MemberNumber { get; set; }

        public string FlyingClubCardNumber { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum StatusEnum
        {
            [EnumMember(Value = "Active")]
            Active = 0,
            [EnumMember(Value = "Expired")]
            Expired = 1,
            [EnumMember(Value = "Frozen")]
            Frozen = 2,
            [EnumMember(Value = "Inactive")]
            Inactive = 3,
            [EnumMember(Value = "Merged")]
            Merged = 4,
            [EnumMember(Value = "Deceased")]
            Deceased = 5,
            [EnumMember(Value = "Suspended")]
            Suspended = 6,
            [EnumMember(Value = "Cancelled")]
            Cancelled = 7,
            [EnumMember(Value = "Active Pending")]
            ActivePending = 8,
            [EnumMember(Value = "Fraud")]
            Fraud = 9,
            [EnumMember(Value = "Pending Cancellation")]
            PendingCancellation = 10,
            [EnumMember(Value = "Route Deal")]
            RouteDeal = 11
        }

        public StatusEnum? Status { get; set; }


    }
}
