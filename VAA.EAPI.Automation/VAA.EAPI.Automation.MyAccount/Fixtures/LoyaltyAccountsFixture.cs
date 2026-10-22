using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.MyAccount.Helpers;

namespace VAA.EAPI.Automation.MyAccount.Fixtures
{
    public sealed class LoyaltyAccountsFixture
    {
        private static readonly Dictionary<string, dynamic> _accounts = new();

        public async Task CreateLoyaltyAccount(FlyingClubMemberTiers tier, int points, string dob)
        {
            if (_accounts.ContainsKey($"{dob}{points}"))
                return;

            FlyingClubMemberHelper flyingClubMemberHelper = new();
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomFirstName();
            string email = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string tierSet = tier.ToString();
            string enrollmentChannel = "WEB-MYA";
            string mediaCode = "Web";

            DateTime dateOfBirth = DateTime.Parse(dob);
            // Direct Entra enrolment rejects minors, so Book age-validation fixtures use Facade for under-18 accounts.
            string fcDetails = dateOfBirth > DateTime.UtcNow.Date.AddYears(-18)
                ? await EnrolmentHelper.CreateIndividualUnderEighteenFacadeLoyaltyAccountAsync(enrollmentChannel, firstName, lastName, email, dob)
                : (await EnrolmentHelper.CreateEntraLoyaltyAccountAsync(enrollmentChannel, mediaCode, firstName, lastName, email, dob)).MemberNumber;
            await flyingClubMemberHelper.UpgradeTheFlyingClubMemberTier(fcDetails, tier);

            bool isTransactionCreated = await flyingClubMemberHelper.AddLoyaltyPointsToTheFlyingClubAccount(fcDetails, points);
            Assert.True(isTransactionCreated, $"Failed to create a loyalty-points credit transaction for Flying Club member '{fcDetails}'.");

            long memberPointBalance = await flyingClubMemberHelper.GetPointBalanceOfFlyingClubAccount(fcDetails);
            int maxAttempts = 10;
            int attempt = 0;

            while (attempt < maxAttempts && memberPointBalance != points)
            {
                await Task.Delay(2000);
                memberPointBalance = await flyingClubMemberHelper.GetPointBalanceOfFlyingClubAccount(fcDetails);
                attempt++;
            }

            Assert.True(memberPointBalance == points, $"Flying Club member '{fcDetails}': expected {points} points after ten attempts, found {memberPointBalance}.");
            _accounts.Add($"{dob}{points}", new { fcDetails, Config.Environment, tierSet, points, firstName, lastName, email, dob });
        }

        public dynamic GetLoyaltyAccount(string dob, int points)
        {
            return _accounts.ContainsKey($"{dob}{points}") ? _accounts[$"{dob}{points}"] : null;
        }
    }
}
