using DeltaPaxInfo = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    public class LoyaltyProgramAccount
    {
        public LoyaltyProgram LoyaltyProgram { get; set; }
        public string Number { get; set; }
        public string TierLevelCode { get; set; }
        public string TierLevelName { get; set; }
        public string TierFullLevelName { get; set; }
        public string TierLevelPriority { get; set; }
        public bool ThreeSixtyMember { get; set; }

        public static explicit operator LoyaltyProgramAccount(DeltaPaxInfo.LoyaltyProgramAccount source)
        {
            if (source is null) return null;

            return new LoyaltyProgramAccount()
            {
                LoyaltyProgram = (LoyaltyProgram)source.LoyaltyProgram,
                Number = source.Number,
                TierLevelCode = source.TierLevelCode,
                TierLevelName = source.TierLevelName,
                TierFullLevelName = source.TierFullLevelName,
                TierLevelPriority = source.TierLevelPriority,
                ThreeSixtyMember = source.ThreeSixtyMember
            };
        }
    }
}