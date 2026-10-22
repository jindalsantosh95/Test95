using System.Collections.Concurrent;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.UI.Helpers;

namespace VAA.EAPI.Automation.Core.UI.Xunit.Fixtures
{
    public sealed class B2CTokenFixture
    {
        private static readonly object @lock = new();
        private static readonly ConcurrentDictionary<string, string> cache = new();

        public string AddB2CToken(string tokenName, string tierCode, int points)
        {
            if (!cache.TryGetValue(tokenName, out string token) || !token.IsJwtTokenValid())
                lock (@lock)
                    if (!cache.TryGetValue(tokenName, out token) || !token.IsJwtTokenValid())
                    {
                        //Retrieving loyalty account details from storage table
                        Dictionary<string, string> loyaltyDetails = RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(tierCode, points);

                        string fcMembershipNumber = loyaltyDetails["rowKey"];

                        // Generate b2c token
                        token = B2CTokenHelper.GenerateLoginB2CToken(fcMembershipNumber, "Virgin123");

                        cache.TryAdd(tokenName, token);
                    }

            return cache[tokenName];
        }

        public string GetB2CToken(string name) => cache.TryGetValue(name, out string value) ? value : string.Empty;
    }
}