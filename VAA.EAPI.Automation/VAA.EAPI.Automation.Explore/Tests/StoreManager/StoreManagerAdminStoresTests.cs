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
    public class StoreManagerAdminStoresTests: IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;
        public StoreManagerAdminStoresTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;
            _tokenFixture.AddTokenForExploreHolidaysApplicationFromKeyVault("GraphToken");
        }

        [Fact]
        [Trait("Category", Traits.ExploreGraphqlRegressionStoreManager)]
        [Trait("Suite", Traits.ExploreReadOnly)]
        public async Task TC01_StoreManagerAdminStores()
        {
            // API Call
            HttpResponseMessage response = GraphQL.StoreManagerAdminStores(_tokenFixture.GetToken("GraphToken")).EnsureGQLResponseSuccess();

            StoreManagerAdminStoresResponse? apiResult = JsonConvert.DeserializeObject<StoreManagerAdminStoresResponse>(await response.Content.ReadAsStringAsync());

            Assert.NotNull(apiResult);

            Assert.NotNull(apiResult.data);

            StoreManagerAdminStore[]? apiStores = apiResult.data.storeManagerAdminStores;

            Assert.NotNull(apiStores);

            Assert.NotEmpty(apiStores);

            // Loop through each store returned by API
            foreach (StoreManagerAdminStore apiStore in apiStores)
            {
                StoreMasterRecord? dbStore = ExploreStoreMasterDBHelper.GetStoreById(apiStore.id);

                Assert.NotNull(dbStore);

                StoreMasterRecord dbStoreNotNull = dbStore!;

                SoftAssertHelper softAssert = new();

                softAssert.AreEqual(dbStoreNotNull.StoreId, apiStore.id, "StoreId mismatch");

                softAssert.AreEqual(dbStoreNotNull.Name, apiStore.name, "Name mismatch");

                softAssert.AreEqual(dbStoreNotNull.City, apiStore.cityTown, "City mismatch");

                softAssert.AreEqual(dbStoreNotNull.County, apiStore.countyRegion, "County mismatch");

                softAssert.AreEqual(dbStoreNotNull.PostalCode, apiStore.postalCode, "PostalCode mismatch");

                softAssert.AreEqual(dbStoreNotNull.Phone, apiStore.telephone, "Telephone mismatch");

                softAssert.AreEqual(dbStoreNotNull.AddressLine1, apiStore.addressLine1, "AddressLine1 mismatch");

                softAssert.AreEqual(dbStoreNotNull.AddressLine2, apiStore.addressLine2, "AddressLine2 mismatch");

                softAssert.AssertAll();
            }
        }
    }
}
