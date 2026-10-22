using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums
{
    public enum StatusEnum
    {
        /// <summary>
        /// Enum ActiveEnum for Active
        /// </summary>
        [EnumMember(Value = "Active")]
        Active = 0,
        /// <summary>
        /// Enum ExpiredEnum for Expired
        /// </summary>
        [EnumMember(Value = "Expired")]
        Expired = 1,
        /// <summary>
        /// Enum FrozenEnum for Frozen
        /// </summary>
        [EnumMember(Value = "Frozen")]
        Frozen = 2,
        /// <summary>
        /// Enum InactiveEnum for Inactive
        /// </summary>
        [EnumMember(Value = "Inactive")]
        Inactive = 3,
        /// <summary>
        /// Enum MergedEnum for Merged
        /// </summary>
        [EnumMember(Value = "Merged")]
        Merged = 4,
        /// <summary>
        /// Enum DeceasedEnum for Deceased
        /// </summary>
        [EnumMember(Value = "Deceased")]
        Deceased = 5,
        /// <summary>
        /// Enum SuspendedEnum for Suspended
        /// </summary>
        [EnumMember(Value = "Suspended")]
        Suspended = 6,
        /// <summary>
        /// Enum CancelledEnum for Cancelled
        /// </summary>
        [EnumMember(Value = "Cancelled")]
        Cancelled = 7,
        /// <summary>
        /// Enum ActivePendingEnum for Active Pending
        /// </summary>
        [EnumMember(Value = "Active Pending")]
        ActivePending = 8,
        /// <summary>
        /// Enum FraudEnum for Fraud
        /// </summary>
        [EnumMember(Value = "Fraud")]
        Fraud = 9,
        /// <summary>
        /// Enum PendingCancellationEnum for Pending Cancellation
        /// </summary>
        [EnumMember(Value = "Pending Cancellation")]
        PendingCancellation = 10,
        /// <summary>
        /// Enum RouteDealEnum for Route Deal
        /// </summary>
        [EnumMember(Value = "Route Deal")]
        RouteDeal = 11
    }
}
