using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.MyAccount.Helpers
{
    public class LoyaltyAccountTestCases
    {
        public static IEnumerable<object[]> LoyaltyAccounts =>
               GetTierPoints();

        public static IEnumerable<object[]> VacAccounts =>
               GetVacAccounts();

        private static IEnumerable<object[]> GetTierPoints()
        {
            yield return new object[] { FlyingClubMemberTiers.Red, 1000000, false };
            yield return new object[] { FlyingClubMemberTiers.Silver, 10000000, false };
            yield return new object[] { FlyingClubMemberTiers.Gold, 20000000, false };
            yield return new object[] { FlyingClubMemberTiers.Red, 1000000, true };
            yield return new object[] { FlyingClubMemberTiers.Silver, 10000000, true };
            yield return new object[] { FlyingClubMemberTiers.Gold, 20000000, true };
            yield return new object[] { FlyingClubMemberTiers.Red, 2000000, false };
            yield return new object[] { FlyingClubMemberTiers.Silver, 20000000, false };
            yield return new object[] { FlyingClubMemberTiers.Gold, 30000000, false };
            yield return new object[] { FlyingClubMemberTiers.Red, 2000000, true };
            yield return new object[] { FlyingClubMemberTiers.Silver, 20000000, true };
        }

        private static IEnumerable<object[]> GetVacAccounts()
        {
            yield return new object[] { FlyingClubMemberTiers.VAC, 0, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 1, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 2, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 3, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 4, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 5, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 6, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 7, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 8, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 9, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 10, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 11, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 12, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 13, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 14, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 15, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 16, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 17, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 18, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 19, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 20, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 21, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 22, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 23, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 24, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 25, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 26, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 27, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 28, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 29, false };
            yield return new object[] { FlyingClubMemberTiers.VAC, 30, false };
        }
    }
}
