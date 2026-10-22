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
    public class StoreManagerOpeningHoursTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;

        public StoreManagerOpeningHoursTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;

            _tokenFixture.AddTokenForExploreHolidaysApplicationFromKeyVault("GraphToken");
        }

        [Theory]
        [MemberData(nameof(GetActiveStoreIds))]
        [Trait("Category", Traits.ExploreGraphqlRegressionStoreManager)]
        [Trait("Suite", Traits.ExploreReadOnly)]
        public async Task TC01_StoreManagerOpeningHours(string storeId)
        {
            SoftAssertHelper softAssert = new();

            string bearer = _tokenFixture.GetToken("GraphToken");

            // Step 1: Call StoreManagerAdminStoreById to identify whether the store is closed.
            HttpResponseMessage adminResponse = GraphQL.StoreManagerAdminStoreById(storeId, bearer).EnsureGQLResponseSuccess();

            string adminBody = await adminResponse.Content.ReadAsStringAsync();

            StoreManagerAdminStoreByIdResponseRoot? adminResult = JsonConvert.DeserializeObject<StoreManagerAdminStoreByIdResponseRoot>(adminBody);

            if (adminResult?.data?.storeManagerAdminStoreById is not StoreManagerAdminStoreByIdResponse adminStore)
            {
                throw new InvalidOperationException($"StoreManagerAdminStoreById returned no store data for store {storeId}");
            }

            // Step 2: Closed stores do not have opening-hour validation performed.
            if (adminStore.isClosed)
            {
                return;
            }

            // Step 3: Call the StoreManagerOpeningHours API.
            HttpResponseMessage response = GraphQL.StoreManagerOpeningHours(storeId, bearer).EnsureGQLResponseSuccess();

            string responseBody = await response.Content.ReadAsStringAsync();

            StoreManagerOpeningHoursResponse? apiResult = JsonConvert.DeserializeObject<StoreManagerOpeningHoursResponse>(responseBody);

            if (apiResult?.data?.storeManagerOpeningHours is not StoreManagerOpeningHour[] apiHours || apiHours.Length == 0)
            {
                throw new InvalidOperationException($"API opening hours are missing for store {storeId}");
            }

            // Step 4: Read the expected weekly opening hours from StoreMaster.
            StoreMasterRecord dbStore = ExploreStoreMasterDBHelper.GetStoreById(storeId) ?? throw new InvalidOperationException($"StoreMaster record not found for store {storeId}");

            if (dbStore.WeeklyHours.Count == 0)
            {
                throw new InvalidOperationException($"Database weekly opening hours are missing for store {storeId}");
            }

            // Step 5: Validate every database opening-hour record against the API response.
            foreach (DbWeeklyHour dbHour in dbStore.WeeklyHours)
            {
                int expectedDay = DayOfWeekMapper.ToApiDayOfWeek(dbHour.day);

                StoreManagerOpeningHour? apiHour = apiHours.FirstOrDefault(hour => hour.dayOfWeek == expectedDay);

                softAssert.True(apiHour != null, $"API opening hour was not found for {dbHour.day} in store {storeId}");

                if (apiHour == null)
                {
                    continue;
                }

                // Step 6: Determine whether the database and API both represent a closed day.
                bool dbClosed = dbHour.isClosed || (dbHour.openingTime == "00:00" && dbHour.closingTime == "00:00");

                bool apiClosed = apiHour.opensAt.Is("Closed") && apiHour.closesAt.Is("Closed");

                // Step 7: Confirm that both systems agree when the day is closed.
                if (dbClosed && apiClosed)
                {
                    continue;
                }

                // Step 8: Report a mismatch when the database says closed but the API says open.
                if (dbClosed && !apiClosed)
                {
                    softAssert.True(false, $"Database says {dbHour.day} is closed but API shows opening hours for store {storeId}");

                    continue;
                }

                // Step 9: Validate opening and closing times for open days.
                softAssert.AreEqual(dbHour.openingTime, apiHour.opensAt, $"Opening time mismatch for {dbHour.day} in store {storeId}");

                softAssert.AreEqual(dbHour.closingTime, apiHour.closesAt, $"Closing time mismatch for {dbHour.day} in store {storeId}");
            }

            // Step 10: Report all validation failures for the current store.
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
