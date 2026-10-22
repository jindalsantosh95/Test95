using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Explore.Constants;
using VAA.EAPI.Automation.Explore.Helpers;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.AppointmentBooker;
using GraphQL = VAA.EAPI.Automation.Explore.Requests.Endpoints.GraphQL;

namespace VAA.EAPI.Automation.Explore.Tests.AppointmentBooker
{
    public class RetrieveAllNearByStoresTests
    {
        [Theory]
        [MemberData(nameof(GetActiveStoreSearchData))]
        [Trait("Category", Traits.ExploreGraphqlRegressionAppointmentBooker)]
        [Trait("Suite", Traits.ExploreReadOnly)]
        public async Task TC01_RetrieveAllNearByStores(string expectedStoreId, string searchParameter)
        {
            // Confirm the database postal code is available before calling the API.
            Assert.False(searchParameter.IsNullOrWhiteSpace(), $"Postal code is missing for store {expectedStoreId}");

            // Call the RetrieveAllNearByStores API using the store postal code.
            HttpResponseMessage response = GraphQL.RetrieveAllNearByStores(searchParameter).EnsureGQLResponseSuccess();

            // Deserialize the API response.
            string responseBody = await response.Content.ReadAsStringAsync();

            RetrieveAllNearByStoresResponse? apiResult = JsonConvert.DeserializeObject<RetrieveAllNearByStoresResponse>(responseBody);

            if (apiResult?.data?.RetrieveAllNearByStores?.stores is not Store[] apiStores)
            {
                throw new InvalidOperationException($"Nearby stores response is empty for store {expectedStoreId} and search value {searchParameter}");
            }

            // Validate that the response contains stores.
            RetrieveAllNearByStoresHelper.ValidateStoreList(apiStores);

            // Validate the nearest-store rule.
            RetrieveAllNearByStoresHelper.ValidateIsClosestRule(apiStores);

            // Confirm that the target store is returned for its own postal-code search.
            bool targetStoreReturned = apiStores.Any(apiStore => apiStore.storeId.Is(expectedStoreId));

            Assert.True(targetStoreReturned, $"Store {expectedStoreId} was not returned for postal code {searchParameter}");

            // Validate every API store returned by the search against StoreMaster.
            foreach (Store apiStore in apiStores)
            {
                StoreMasterRecord? dbStore = ExploreStoreMasterDBHelper.GetStoreById(apiStore.storeId);

                Assert.NotNull(dbStore);

                // Validate basic store fields.
                RetrieveAllNearByStoresHelper.ValidateStoreBasicFields(apiStore, dbStore);

                // Validate complete store details.
                RetrieveAllNearByStoresHelper.ValidateFullDetails(apiStore, dbStore);
            }
        }

        // Returns every active store together with its database postal code.
        public static IEnumerable<object[]> GetActiveStoreSearchData()
        {
            // Read every active store dynamically from StoreMaster.
            List<string> activeStoreIds = ExploreStoreMasterDBHelper.GetAllActiveStoreIds();

            // Stop the test if no active stores are available.
            if (activeStoreIds.Count == 0)
            {
                throw new InvalidOperationException("No active stores were found in StoreMaster");
            }

            // Create one test case for every active store.
            foreach (string storeId in activeStoreIds)
            {
                StoreMasterRecord? dbStore = ExploreStoreMasterDBHelper.GetStoreById(storeId);

                if (dbStore == null)
                {
                    throw new InvalidOperationException($"StoreMaster record not found for store {storeId}");
                }

                yield return new object[] { storeId, dbStore.PostalCode };
            }
        }
    }
}
