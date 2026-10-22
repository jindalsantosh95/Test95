using System;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;


namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.MemberAccountDetails
{
    public class MemberAccountResponseMapping
    {

        public static MemberAccountResponse ResponseMapping(MemberAccountResponse documentData, MemberAccountReferenceData referenceData)
        {
            if (documentData == null) throw new ArgumentNullException(nameof(documentData));
            if (referenceData == null) throw new ArgumentNullException(nameof(referenceData));
            MemberAccountResponse responses = new MemberAccountResponse();

            LoyaltyMemberAccount loyaltyMemberAccount = new LoyaltyMemberAccount()
            {
                CustomerId = documentData.loyaltyMemberAccount?.CustomerId,
                ByDate = documentData.loyaltyMemberAccount?.ByDate,
                RetainPointTotal = string.IsNullOrEmpty(documentData.loyaltyMemberAccount.RetainPointTotal) ? "0" : documentData.loyaltyMemberAccount.RetainPointTotal,
                RollingPointTotal = string.IsNullOrEmpty(documentData.loyaltyMemberAccount.RollingPointTotal) ? "0" : documentData.loyaltyMemberAccount.RollingPointTotal,
                TotalVsflownMiles = string.IsNullOrEmpty(documentData.loyaltyMemberAccount?.TotalVsflownMiles) ? "0" : documentData.loyaltyMemberAccount?.TotalVsflownMiles,
                TierStartDate = documentData.loyaltyMemberAccount?.TierStartDate,
                TierEndDate = documentData.loyaltyMemberAccount?.TierEndDate,
                NumberofYearsAtGold = string.IsNullOrEmpty(documentData.loyaltyMemberAccount.NumberofYearsAtGold) ? "0" : documentData.loyaltyMemberAccount.NumberofYearsAtGold,
                TotalVSTierPointsInCurrentYear = string.IsNullOrEmpty(documentData.loyaltyMemberAccount.TotalVSTierPointsInCurrentYear) ? "0" : documentData.loyaltyMemberAccount.TotalVSTierPointsInCurrentYear,
                TotalVSTierPointsSinceBecomingGold = string.IsNullOrEmpty(documentData.loyaltyMemberAccount.TotalVSTierPointsSinceBecomingGold) ? "0" : documentData.loyaltyMemberAccount.TotalVSTierPointsSinceBecomingGold,
                TierPointsForRetainToGold = referenceData.TierPointsForRetainToGold > 0 ? referenceData.TierPointsForRetainToGold : 0,
                TierPointsForRetainToSilver = referenceData.TierPointsForRetainToSilver > 0 ? referenceData.TierPointsForRetainToSilver : 0,
                TierPointsForRedToSilver = referenceData.TierPointsForRedToSilver > 0 ? referenceData.TierPointsForRedToSilver : 0,
                TierPointsForSilverToGold = referenceData.TierPointsForSilverToGold > 0 ? referenceData.TierPointsForSilverToGold : 0,
                WingsStartDate = documentData.loyaltyMemberAccount?.WingsStartDate,
                TotalTierPointsSinceBecomingWings = string.IsNullOrEmpty(documentData.loyaltyMemberAccount.TotalTierPointsSinceBecomingWings) ? "0" : documentData.loyaltyMemberAccount.TotalTierPointsSinceBecomingWings,
                TotalVSTierPointsSinceBecomingWings = string.IsNullOrEmpty(documentData.loyaltyMemberAccount.TotalVSTierPointsSinceBecomingWings) ? "0" : documentData.loyaltyMemberAccount.TotalVSTierPointsSinceBecomingWings,
                GoldStartDate = documentData.loyaltyMemberAccount?.GoldStartDate,
                RetainCoreTierPoints = string.IsNullOrEmpty(documentData.loyaltyMemberAccount.RetainCoreTierPoints) ? "0" : documentData.loyaltyMemberAccount.RetainCoreTierPoints,
                RollingCoreTierPoints = string.IsNullOrEmpty(documentData.loyaltyMemberAccount.RollingCoreTierPoints) ? "0" : documentData.loyaltyMemberAccount.RollingCoreTierPoints,
                CoreTierPointsSinceBecomingWings = string.IsNullOrEmpty(documentData.loyaltyMemberAccount.CoreTierPointsSinceBecomingWings) ? "0" : documentData.loyaltyMemberAccount.CoreTierPointsSinceBecomingWings,
            };
            responses.loyaltyMemberAccount = loyaltyMemberAccount;
            return responses;
        }
    }
}
