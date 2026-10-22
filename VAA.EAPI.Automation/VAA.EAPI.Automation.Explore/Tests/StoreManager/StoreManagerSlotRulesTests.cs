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
    public class StoreManagerSlotRulesTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;
        public StoreManagerSlotRulesTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;
            _tokenFixture.AddTokenForExploreHolidaysApplicationFromKeyVault("GraphToken");
        }

        [Theory]
        [MemberData(nameof(GetActiveStoreIds))]
        [Trait("Category", Traits.ExploreGraphqlRegressionStoreManager)]
        [Trait("Suite", Traits.ExploreReadOnly)]
        public async Task TC01_StoreManagerSlotRules_DBValidation(string storeId)
        {
            SoftAssertHelper softAssert = new();

            // Step 1: Call AdminStoreById API
            HttpResponseMessage adminResponse = GraphQL.StoreManagerAdminStoreById(storeId, _tokenFixture.GetToken("GraphToken")).EnsureGQLResponseSuccess();

            StoreManagerAdminStoreByIdResponseRoot? adminResult = JsonConvert.DeserializeObject<StoreManagerAdminStoreByIdResponseRoot>(await adminResponse.Content.ReadAsStringAsync());

            Assert.NotNull(adminResult);

            Assert.NotNull(adminResult.data);

            Assert.NotNull(adminResult.data.storeManagerAdminStoreById);

            bool isClosed = adminResult.data.storeManagerAdminStoreById.isClosed;

            // Step 2: If store is closed, stop test execution here 
            if (isClosed)
            {
                softAssert.True(isClosed, $"Store {storeId} is closed, skipping slot rules validation.");

                softAssert.AssertAll();

                return;
            }

            // Step 3: Continue with SlotRules validation if store is open
            HttpResponseMessage response = GraphQL.StoreManagerSlotRules(storeId, _tokenFixture.GetToken("GraphToken")).EnsureGQLResponseSuccess();

            StoreManagerSlotRulesResponse? apiResult = JsonConvert.DeserializeObject<StoreManagerSlotRulesResponse>(await response.Content.ReadAsStringAsync());

            Assert.NotNull(apiResult);

            Assert.NotNull(apiResult.data);

            StoreManagerSlotRule[] apiRules = apiResult.data.storeManagerSlotRules ?? Array.Empty<StoreManagerSlotRule>();

            Assert.True(apiRules.Length > 0, $"No slot rules returned by API for store {storeId}");

            // DB FETCH
            List<StoreManagerSlotRulesHelper> dbRules = ExploreStoreMasterDBHelper.GetActiveSlotRulesByStoreId(storeId);

            Assert.True(dbRules.Count > 0, $"No slot rules found in DB for store {storeId}");

            softAssert.AreEqual(dbRules.Count, apiRules.Length, $"API and database slot-rule count mismatch for store {storeId}.");

            foreach (StoreManagerSlotRulesHelper dbRule in dbRules)
            {
                StoreManagerSlotRule? apiRule = apiRules.FirstOrDefault(r => r.id == dbRule.SlotRuleId);

                softAssert.True(apiRule != null, $"API missing slot rule {dbRule.SlotRuleId} for store {storeId}");

                if (apiRule == null)
                {
                    continue; // skip this iteration
                }

                // Normalize time formats before comparison
                softAssert.True(!string.IsNullOrWhiteSpace(apiRule.startTime), $"StartTime is null/empty for rule {apiRule.id}");

                softAssert.True(!string.IsNullOrWhiteSpace(apiRule.endTime), $"EndTime is null/empty for rule {apiRule.id}");

                if (string.IsNullOrWhiteSpace(apiRule.startTime) || string.IsNullOrWhiteSpace(apiRule.endTime))
                {
                    continue;
                }

                string apiStart = TimeSpan.Parse(apiRule.startTime!).ToString(@"hh\:mm\:ss");

                string apiEnd = TimeSpan.Parse(apiRule.endTime!).ToString(@"hh\:mm\:ss");

                softAssert.AreEqual(dbRule.DayOfWeek, apiRule.dayOfWeek, $"DayOfWeek mismatch for rule {apiRule.id}");

                softAssert.AreEqual(dbRule.StartTime, apiStart, $"StartTime mismatch for rule {apiRule.id}");

                softAssert.AreEqual(dbRule.EndTime, apiEnd, $"EndTime mismatch for rule {apiRule.id}");

                softAssert.AreEqual(dbRule.SlotDurationMinutes, apiRule.slotDurationMinutes, $"SlotDuration mismatch for rule {apiRule.id}");

                softAssert.AreEqual(dbRule.SlotCapacity, apiRule.slotCapacity, $"SlotCapacity mismatch for rule {apiRule.id}");

                // BASIC VALIDATIONS
                TimeSpan start = TimeSpan.Parse(apiRule.startTime!);

                TimeSpan end = TimeSpan.Parse(apiRule.endTime!);

                softAssert.True(start < end, $"StartTime must be earlier than EndTime for rule {apiRule.id}");

                softAssert.True(apiRule.slotCapacity > 0, $"Slot capacity should be > 0 for store {storeId}, rule {apiRule.id}");

                softAssert.True(apiRule.dayOfWeek >= 0 && apiRule.dayOfWeek <= 6, $"DayOfWeek out of range for rule {apiRule.id}");
            }
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
