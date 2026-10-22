using HotChocolate;

using VAA.EAPI.GraphQL.Domain.Account.Models.Activity;
using VAA.EAPI.GraphQL.Domain.Account.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.ManagedYoungFlyer
{
    public sealed class ManagedYoungFlyerAccount
    {
        public bool HoHManaged { get; init; }
        public string CustomerId { get; init; }
        public string FcNumber { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public DateTime? DateOfBirth { get; init; }
        [GraphQLName("fcJoinDate")]
        public DateTime? FCJoinDate { get; init; }
        public int? TierPoints { get; init; }
        public string TierCode { get; init; }
        public DateTime? DateTierPointsReset { get; init; }
        public DateTime? UpgradeByDate { get; init; }
        public int? TierPointsNeedToReachNextTier { get; init; }
        public int? RetainTierPoints { get; init; }
        public int? TierPointsNeedToRetain { get; init; }
        public int? RollingTierPoints { get; init; }
        public string Status { get; init; }
        public string EmailAddress { get; init; }
        internal int ActivitiesCount { get; init; }
        internal CustomerCustMS Customer { get; init; }
        internal FcMemberDetails MemberDetails { get; init; }
        public ActivityResponse ActivityResponse { get; set; }
        public MemberCustomAttributesResponseAccount TenureResponse { get; set; }
    }
}
