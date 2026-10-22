using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Explore.Constants;
using VAA.EAPI.Automation.Explore.Helpers;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager;
using GraphQL = VAA.EAPI.Automation.Explore.Requests.Endpoints.GraphQL;

namespace VAA.EAPI.Automation.Explore.Tests.StoreManager
{
    public class StoreManagerAvailabilityRangeTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;

        public StoreManagerAvailabilityRangeTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;

            _tokenFixture.AddTokenForExploreHolidaysApplicationFromKeyVault("GraphToken");
        }

        [Theory]
        [MemberData(nameof(GetActiveStoreIds))]
        [Trait("Category", Traits.ExploreGraphqlRegressionStoreManager)]
        [Trait("Suite", Traits.ExploreReadOnly)]
        public async Task TC01_StoreManagerAvailabilityRange(string storeId)
        {
            SoftAssertHelper softAssert = new();

            DateTime fromDate = DateTime.UtcNow.Date;

            DateTime toDate = fromDate.AddDays(7);

            string from = fromDate.ToString("yyyy-MM-dd");

            string to = toDate.ToString("yyyy-MM-dd");

            string bearer = _tokenFixture.GetToken("GraphToken");

            HttpResponseMessage response = GraphQL.StoreManagerAvailabilityRange(storeId, from, to, bearer).EnsureGQLResponseSuccess();

            string responseBody = await response.Content.ReadAsStringAsync();

            StoreManagerAvailabilityRangeResponse? apiResult = JsonConvert.DeserializeObject<StoreManagerAvailabilityRangeResponse>(responseBody);

            Assert.NotNull(apiResult);

            StoreManagerAvailabilityRangeItem[] apiWindows = apiResult.data?.storeManagerAvailabilityRange ?? Array.Empty<StoreManagerAvailabilityRangeItem>();

            Assert.NotEmpty(apiWindows);

            List<StoreManagerAvailabilityRangeDbRecord> dbWindows = StoreManagerAvailabilityRangeDbHelper.GetAvailabilityRange(storeId, fromDate, toDate);

            Assert.NotEmpty(dbWindows);

            softAssert.AreEqual(dbWindows.Count, apiWindows.Length, $"API and database availability count mismatch for store {storeId}");

            foreach (StoreManagerAvailabilityRangeItem apiWindow in apiWindows)
            {
                DateTime apiStartLocalDateTime = apiWindow.startAt.DateTime;

                DateTime apiEndLocalDateTime = apiWindow.endAt.DateTime;

                softAssert.True(!apiWindow.id.IsNullOrWhiteSpace(), $"Availability window ID is empty for store {storeId}");

                softAssert.AreEqual(storeId, apiWindow.storeId, $"Store ID mismatch for availability window {apiWindow.id}");

                softAssert.True(apiStartLocalDateTime < apiEndLocalDateTime, $"Invalid availability time range for window {apiWindow.id}");

                softAssert.True(apiStartLocalDateTime.Date >= fromDate.Date, $"Availability window {apiWindow.id} starts before the requested date range");

                softAssert.True(apiEndLocalDateTime.Date <= toDate.Date, $"Availability window {apiWindow.id} ends after the requested date range");

                softAssert.True(apiWindow.capacity > 0, $"Invalid capacity for availability window {apiWindow.id}");

                softAssert.True(apiWindow.remainingCapacity >= 0, $"Invalid remaining capacity for availability window {apiWindow.id}");

                softAssert.True(apiWindow.remainingCapacity <= apiWindow.capacity, $"Remaining capacity exceeds capacity for availability window {apiWindow.id}");
            }

            foreach (StoreManagerAvailabilityRangeDbRecord dbWindow in dbWindows)
            {
                DateTime dbStartLocalDateTime = dbWindow.StartsAtUtc.DateTime;

                DateTime dbEndLocalDateTime = dbWindow.EndsAtUtc.DateTime;

                StoreManagerAvailabilityRangeItem? matchingApiWindow = apiWindows.FirstOrDefault(apiWindow =>
                    apiWindow.storeId == dbWindow.StoreId &&
                    apiWindow.startAt.DateTime == dbStartLocalDateTime &&
                    apiWindow.endAt.DateTime == dbEndLocalDateTime);

                softAssert.True(matchingApiWindow != null, $"Database availability window {dbWindow.StoreSlotId} was not returned by the API for store {storeId}");

                if (matchingApiWindow == null)
                {
                    continue;
                }

                softAssert.AreEqual(dbWindow.StoreSlotId.ToString(), matchingApiWindow.id, $"Availability window ID mismatch for store {storeId}");

                softAssert.AreEqual(dbWindow.StoreId, matchingApiWindow.storeId, $"Store ID mismatch for availability window {matchingApiWindow.id}");

                softAssert.AreEqual(dbStartLocalDateTime, matchingApiWindow.startAt.DateTime, $"StartAt mismatch for availability window {matchingApiWindow.id}");

                softAssert.AreEqual(dbEndLocalDateTime, matchingApiWindow.endAt.DateTime, $"EndAt mismatch for availability window {matchingApiWindow.id}");

                softAssert.AreEqual(dbWindow.Capacity, matchingApiWindow.capacity, $"Capacity mismatch for availability window {matchingApiWindow.id}");

                softAssert.AreEqual(dbWindow.RemainingCapacity, matchingApiWindow.remainingCapacity, $"RemainingCapacity mismatch for availability window {matchingApiWindow.id}");
            }

            for (int index = 1; index < apiWindows.Length; index++)
            {
                softAssert.True(apiWindows[index - 1].startAt.DateTime <= apiWindows[index].startAt.DateTime, $"Availability windows are not ordered by startAt for store {storeId}");
            }

            IEnumerable<string> duplicateWindowKeys = apiWindows
                .GroupBy(window => $"{window.id}|{window.startAt.DateTime:O}|{window.endAt.DateTime:O}")
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);

            softAssert.True(!duplicateWindowKeys.Any(), $"Duplicate availability windows found for store {storeId}: {string.Join(", ", duplicateWindowKeys)}");

            softAssert.AssertAll();
        }

        public static IEnumerable<object[]> GetActiveStoreIds()
        {
            List<string> storeIds = ExploreStoreMasterDBHelper.GetActiveStoresWithSlotRulesAndSlots();

            Assert.NotEmpty(storeIds);

            foreach (string storeId in storeIds)
            {
                yield return new object[] { storeId };
            }
        }
    }
}
