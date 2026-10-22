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
    public class StoreManagerSlotsTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;

        public StoreManagerSlotsTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;

            _tokenFixture.AddTokenForExploreHolidaysApplicationFromKeyVault("GraphToken");
        }

        [Theory]
        [MemberData(nameof(GetActiveStoreIds))]
        [Trait("Category", Traits.ExploreGraphqlRegressionStoreManager)]
        [Trait("Suite", Traits.ExploreReadOnly)]
        public async Task TC01_StoreManagerSlots(string storeId)
        {
            SoftAssertHelper softAssert = new();

            DateTimeOffset fromUtc = new(DateTime.UtcNow.Date);

            DateTimeOffset toUtc = fromUtc.AddDays(7).AddTicks(-1);

            DateTime fromLocalDateTime = fromUtc.DateTime;

            DateTime toLocalDateTime = toUtc.DateTime;

            string from = fromUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            string to = toUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            string bearer = _tokenFixture.GetToken("GraphToken");

            HttpResponseMessage response = GraphQL.StoreManagerSlots(storeId, from, to, bearer).EnsureGQLResponseSuccess();

            string responseBody = await response.Content.ReadAsStringAsync();

            StoreManagerSlotsResponse? apiResult = JsonConvert.DeserializeObject<StoreManagerSlotsResponse>(responseBody);

            Assert.NotNull(apiResult);

            Assert.NotNull(apiResult.data);

            StoreManagerSlot[] apiSlots = apiResult.data.storeManagerSlots ?? Array.Empty<StoreManagerSlot>();

            Assert.NotEmpty(apiSlots);

            List<StoreManagerSlotDbRecord> dbSlots = StoreManagerSlotsDbHelper.GetSlots(storeId, fromUtc, toUtc);

            Assert.NotEmpty(dbSlots);

            softAssert.AreEqual(dbSlots.Count, apiSlots.Length, $"API and database slot count mismatch for store {storeId}");

            foreach (StoreManagerSlot apiSlot in apiSlots)
            {
                DateTime apiStartLocalDateTime = apiSlot.startAt.DateTime;

                DateTime apiEndLocalDateTime = apiSlot.endAt.DateTime;

                softAssert.True(!apiSlot.id.IsNullOrWhiteSpace(), $"Slot ID is empty for store {storeId}");

                softAssert.AreEqual(storeId, apiSlot.storeId, $"Store ID mismatch for slot {apiSlot.id}");

                softAssert.True(apiStartLocalDateTime < apiEndLocalDateTime, $"Invalid API time range for slot {apiSlot.id}");

                softAssert.True(apiStartLocalDateTime >= fromLocalDateTime, $"Slot {apiSlot.id} starts before the requested range");

                softAssert.True(apiEndLocalDateTime <= toLocalDateTime, $"Slot {apiSlot.id} ends after the requested range");

                softAssert.True(apiSlot.capacity > 0, $"Invalid capacity for slot {apiSlot.id}");

                softAssert.True(apiSlot.remainingCapacity >= 0, $"Invalid remaining capacity for slot {apiSlot.id}");

                softAssert.True(apiSlot.remainingCapacity <= apiSlot.capacity, $"Remaining capacity exceeds capacity for slot {apiSlot.id}");

                softAssert.True(apiSlot.status is "AVAILABLE" or "BOOKED" or "BLOCKED", $"Unexpected status for slot {apiSlot.id}: {apiSlot.status}");
            }

            foreach (StoreManagerSlotDbRecord dbSlot in dbSlots)
            {
                DateTime dbStartLocalDateTime = dbSlot.StartsAtUtc.DateTime;

                DateTime dbEndLocalDateTime = dbSlot.EndsAtUtc.DateTime;

                StoreManagerSlot? matchingApiSlot = apiSlots.FirstOrDefault(apiSlot =>
                    apiSlot.storeId == dbSlot.StoreId &&
                    apiSlot.startAt.DateTime == dbStartLocalDateTime &&
                    apiSlot.endAt.DateTime == dbEndLocalDateTime);

                softAssert.True(matchingApiSlot != null, $"Database slot {dbSlot.StoreSlotId} was not returned by the API for store {storeId}");

                if (matchingApiSlot == null)
                {
                    continue;
                }

                softAssert.AreEqual(dbSlot.StoreSlotId.ToString(), matchingApiSlot.id, $"Slot ID mismatch for store {storeId}");

                softAssert.AreEqual(dbSlot.StoreId, matchingApiSlot.storeId, $"Store ID mismatch for slot {matchingApiSlot.id}");

                softAssert.AreEqual(dbStartLocalDateTime, matchingApiSlot.startAt.DateTime, $"StartAt mismatch for slot {matchingApiSlot.id}");

                softAssert.AreEqual(dbEndLocalDateTime, matchingApiSlot.endAt.DateTime, $"EndAt mismatch for slot {matchingApiSlot.id}");

                softAssert.AreEqual(dbSlot.Capacity, matchingApiSlot.capacity, $"Capacity mismatch for slot {matchingApiSlot.id}");

                softAssert.AreEqual(dbSlot.RemainingCapacity, matchingApiSlot.remainingCapacity, $"RemainingCapacity mismatch for slot {matchingApiSlot.id}");

                softAssert.AreEqual(dbSlot.Status, matchingApiSlot.status, $"Status mismatch for slot {matchingApiSlot.id}");

                string? expectedReason = dbSlot.Reason.IsNullOrWhiteSpace() ? null : dbSlot.Reason.Trim();

                string? actualReason = matchingApiSlot.reason.IsNullOrWhiteSpace() ? null : matchingApiSlot.reason.Trim();

                softAssert.AreEqual(expectedReason, actualReason, $"Reason mismatch for slot {matchingApiSlot.id}");
            }

            for (int index = 1; index < apiSlots.Length; index++)
            {
                softAssert.True(apiSlots[index - 1].startAt.DateTime <= apiSlots[index].startAt.DateTime, $"Slots are not ordered by startAt for store {storeId}");
            }

            IEnumerable<string> duplicateSlotKeys = apiSlots
                .GroupBy(slot => $"{slot.id}|{slot.startAt.DateTime:O}|{slot.endAt.DateTime:O}")
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);

            softAssert.True(!duplicateSlotKeys.Any(), $"Duplicate slots found for store {storeId}: {string.Join(", ", duplicateSlotKeys)}");

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
