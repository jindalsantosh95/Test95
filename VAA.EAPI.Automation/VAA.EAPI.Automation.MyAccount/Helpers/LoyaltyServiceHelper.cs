using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.DataProvider;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;

namespace VAA.EAPI.Automation.MyAccount.Helpers
{
    public class LoyaltyServiceHelper : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;

        public LoyaltyServiceHelper(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;
        }

        public async Task<Dictionary<string, string>> GetLoyaltyAccountDetails(string tierCode, int pointsRequired, int maxAttempts = 5, string countryCode = "UK")
        {
            // Initialize a dictionary to hold loyalty account details
            Dictionary<string, string> loyaltyDetails = new();

            int attempt = 0;
            while (attempt < maxAttempts)
            {
                // Retrieve the loyalty account details
                loyaltyDetails = RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(tierCode, pointsRequired, countryCode: countryCode);

                string fcMembershipNumber = loyaltyDetails["rowKey"];

                // Call external API to get the updated points balance
                tokenFixture.AddTokenForVRedFromKeyVault("vred");
                string vredToken = tokenFixture.GetRequiredToken("vred", "VRed loyalty account lookup");
                System.Net.Http.HttpResponseMessage vRedResponseAPI = Microservice.Vred(vredToken, fcMembershipNumber).EnsureIsSuccessStatusCode();

                dynamic vRedResultAPI = JsonConvert.DeserializeObject<dynamic>(await vRedResponseAPI.Content.ReadAsStringAsync());

                // Get usable points from the response
                int vredPoints = (int)vRedResultAPI.balance.usable.Value;

                // Update the loyalty account with the new points value
                await LoyaltyAccountUpdates.UpdateLoyaltyAccountToStorageTable(fcMembershipNumber, vredPoints);

                // If the points match and the condition is met, exit the loop
                if (loyaltyDetails["points"] == vredPoints.ToString() && vredPoints >= pointsRequired)
                {
                    break;
                }

                attempt++;
            }

            return loyaltyDetails;
        }

        public async Task<Dictionary<string, string>> GetLoyaltyAccountDetailsForCoBrand(string tierCode, string lastName, int pointsRequired, int maxAttempts = 5, int minimumRequiredPoints = 0)
        {
            Dictionary<string, string> loyaltyDetails = new();

            int attempt = 0;
            while (attempt < maxAttempts)
            {
                // Retrieve the loyalty account details
                loyaltyDetails = RetrieveFromStorageTable.RetrieveCoBrandLoyaltyAccountNumberDetailsFromStorageTable(tierCode, lastName, pointsRequired);

                string fcMembershipNumber;
                if (loyaltyDetails is null
                    || !loyaltyDetails.TryGetValue("rowKey", out fcMembershipNumber)
                    || fcMembershipNumber.IsNullOrWhiteSpace())
                {
                    attempt++;
                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(attempt));
                    }
                    continue;
                }

                // Call external API to get the updated points balance
                tokenFixture.AddTokenForVRedFromKeyVault("vred");
                string vredToken = tokenFixture.GetRequiredToken("vred", "VRed co-brand loyalty account lookup");
                System.Net.Http.HttpResponseMessage vRedResponseAPI = Microservice.Vred(vredToken, fcMembershipNumber).EnsureIsSuccessStatusCode();

                dynamic vRedResultAPI = JsonConvert.DeserializeObject<dynamic>(await vRedResponseAPI.Content.ReadAsStringAsync());

                // Get usable points from the response
                int usablePoints = (int)vRedResultAPI.balance.usable.Value;

                // Update the loyalty account with the new points value
                await LoyaltyAccountUpdates.UpdateLoyaltyAccountToStorageTable(fcMembershipNumber, usablePoints);

                // Check if the points meet the conditions
                if (IsPointsConditionMet(loyaltyDetails, usablePoints, pointsRequired, minimumRequiredPoints))
                {
                    break;
                }

                attempt++;
            }

            if (loyaltyDetails is null
                || !loyaltyDetails.TryGetValue("rowKey", out string selectedMembershipNumber)
                || selectedMembershipNumber.IsNullOrWhiteSpace())
            {
                Assert.Fail(
                    $"Expected a co-brand loyalty account after {maxAttempts} attempts, but found none. "
                    + $"Tier={tierCode}, LastNameProfile={lastName}, PointsRequired={pointsRequired}, MinimumRequiredPoints={minimumRequiredPoints}.");
            }

            return loyaltyDetails ?? new Dictionary<string, string>();
        }

        private bool IsPointsConditionMet(Dictionary<string, string> loyaltyDetails, int usablePoints, int pointsRequired, int minimumRequiredPoints)
        {
            int currentPoints = int.Parse(loyaltyDetails["points"]);

            // Handle condition when there's a minimum required points threshold
            if (minimumRequiredPoints > 0)
            {
                return currentPoints == usablePoints && usablePoints <= pointsRequired && currentPoints >= minimumRequiredPoints;
            }

            // Handle condition when pointsRequired is the main condition
            return currentPoints == usablePoints && usablePoints >= pointsRequired;
        }
    }
}

