using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Explore.Constants;
using VAA.EAPI.Automation.Explore.Helpers;

namespace VAA.EAPI.Automation.Explore.Tests.StoreManager
{
    [Collection("Sequential")]
    public class StoreManagerClosuresTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;

        public StoreManagerClosuresTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;

            _tokenFixture.AddTokenForExploreHolidaysApplicationFromKeyVault("GraphToken");
        }

        [Theory]
        [MemberData(nameof(GetActiveStoreIds))]
        [Trait("Category", Traits.ExploreGraphqlRegressionStoreManager)]
        [Trait("Suite", Traits.ExploreReadOnly)]
        public async Task TC01_StoreManagerClosures(string storeId)
        {
            SoftAssertHelper softAssert = new();

            // Build a dynamic date range from today to one year ahead.
            DateTime fromDate = DateTime.Today;

            DateTime toDate = fromDate.AddYears(1);

            string from = fromDate.ToString("yyyy-MM-dd");

            string to = toDate.ToString("yyyy-MM-dd");

            // Validate closures through the API and database.
            await StoreManagerClosuresHelper.ValidateClosures(storeId, from, to, _tokenFixture, softAssert);

            // Report all validation failures for the current store.
            softAssert.AssertAll();
        }

        // Returns every active store from StoreMaster as an individual test case.
        public static IEnumerable<object[]> GetActiveStoreIds()
        {
            List<string> activeStoreIds = ExploreStoreMasterDBHelper.GetAllActiveStoreIds();

            if (activeStoreIds.Count == 0)
            {
                throw new InvalidOperationException("No active stores were found in StoreMaster");
            }

            foreach (string storeId in activeStoreIds)
            {
                yield return new object[] { storeId };
            }
        }
    }
}
