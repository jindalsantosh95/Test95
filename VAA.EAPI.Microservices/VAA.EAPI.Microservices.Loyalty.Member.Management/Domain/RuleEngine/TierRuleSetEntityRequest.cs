namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.RuleEngine
{
    public class TierRuleSetEntityRequest
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string MillionMilerIndicator { get; set; }
        public string LifeTimeGoldIndicator { get; set; }
        public string CenturionFlag { get; set; }
        public string CompanionFlightawardflag { get; set; }
        public string DowngradeProhibitFlag { get; set; }
        public string UpgradeRewardflag { get; set; }
        public string ExpiredFlag { get; set; }
        public string LoungePassflag { get; set; }
        public string S2GFlag { get; set; }
        public string S2GFTGFlag { get; set; }
        public string S2SFTGFlag { get; set; }
        public string S2SFlag { get; set; }
        public string VSFlownMiles { get; set; }
        public string WingsMember { get; set; }
        public string VSRollingTierPoints { get; set; }
        public string VSFlightInPrevious12Months { get; set; }
        public string GoldCounter { get; set; }
        public string VALifetimePointsTotal { get; set; }
        public string RetainPointsTotal { get; set; }
        public string VSJCDISectorCount { get; set; }
        public string FastTrackId { get; set; }
        public string VATotalFastTrackTierPts { get; set; }
        public string RollingPointsTotal { get; set; }
        public string InstGoldFirstVSFlight { get; set; }
        public string VAInstantGoldId { get; set; }
        public string InstSilverFirstVSFlight { get; set; }
        public string InstantSilverId { get; set; }
        public string ReferSSAGoldId { get; set; }
        public string TierApprovalStatus { get; set; }
        public string FirstVSFlightActivity { get; set; }
        public string CurrentStatus { get; set; }
        public string MemberType { get; set; }
        public string CurrentTier { get; set; }
        public string TierName { get; set; }
        public string CabinClass { get; set; }
        public string SsaGoldTierName { get; set; }
        public string OriginCountryCode { get; set; }
        public string DestinationCountryCode { get; set; }

        public static explicit operator TierRuleSetEntityRequest(TierRuleSetEntityRequestSource source)
        {
            if (source == null)
            {
                throw new System.ArgumentNullException(nameof(source));
            }

            return new TierRuleSetEntityRequest
            {
                CustomerId = source.Member.CustomerId,
                MemberNumber = source.Member.LoyaltyMemberNumber,
                MillionMilerIndicator = source.IsMillionMilerAwardEligible.ToString(),
                VSFlownMiles = source.VSFlownMiles.ToString(System.Globalization.CultureInfo.InvariantCulture),
                WingsMember = source.IsWingsMember.ToString(),
                RollingPointsTotal = source.TotalTierPoints.ToString(System.Globalization.CultureInfo.InvariantCulture),
                VSRollingTierPoints = source.VSTierPoints.ToString(System.Globalization.CultureInfo.InvariantCulture),
                VSFlightInPrevious12Months = source.HasQualifyingVSFlight.ToString(),
                CurrentStatus = source.FlyingClubMember?.Status,
                MemberType = source.FlyingClubMember?.MemberType,
                CurrentTier = source.FlyingClubMember?.TierCode,
                TierName = source.FlyingClubMember?.TierCode
            };
        }
    }

    public class TierRuleSetEntityRequestSource
    {
        public VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.WingsGoldMemberAssessment.GoldMemberDetails Member { get; set; }
        public VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember.FlyingClubMember FlyingClubMember { get; set; }
        public decimal VSFlownMiles { get; set; }
        public decimal TotalTierPoints { get; set; }
        public decimal VSTierPoints { get; set; }
        public bool IsMillionMilerAwardEligible { get; set; }
        public bool IsWingsMember { get; set; }
        public bool HasQualifyingVSFlight { get; set; }
    }
}
