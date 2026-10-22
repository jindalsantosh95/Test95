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
    public class StoreManagerAdminStoreByIdTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;
        public StoreManagerAdminStoreByIdTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;
            _tokenFixture.AddTokenForExploreHolidaysApplicationFromKeyVault("GraphToken");
        }

        [Theory]
        [Trait("Category", Traits.ExploreGraphqlRegressionStoreManager)]
        [Trait("Suite", Traits.ExploreReadOnly)]
        [InlineData("1034")]
        [InlineData("1009")]
        [InlineData("1021")]
        [InlineData("1002")]
        [InlineData("1031")]
        [InlineData("1023")]
        public async Task TC01_StoreManagerAdminStoreById(string storeId)
        {
            HttpResponseMessage response = GraphQL.StoreManagerAdminStoreById(storeId, _tokenFixture.GetToken("GraphToken")).EnsureGQLResponseSuccess();

            StoreManagerAdminStoreByIdResponseRoot? apiResult = JsonConvert.DeserializeObject<StoreManagerAdminStoreByIdResponseRoot>(await response.Content.ReadAsStringAsync());

            Assert.NotNull(apiResult);

            Assert.NotNull(apiResult.data);

            StoreManagerAdminStoreByIdResponse? apiStore = apiResult.data.storeManagerAdminStoreById;

            Assert.NotNull(apiStore);

            StoreMasterRecord? dbStore = ExploreStoreMasterDBHelper.GetStoreById(storeId);

            SoftAssertHelper softAssert = new();

            Assert.NotNull(dbStore);

            StoreMasterRecord dbStoreNotNull = dbStore!;

            softAssert.AreEqual(dbStoreNotNull.StoreId, apiStore.id);

            softAssert.AreEqual(dbStoreNotNull.Name, apiStore.name);

            softAssert.AreEqual(dbStoreNotNull.City, apiStore.cityTown);

            softAssert.AreEqual(dbStoreNotNull.County, apiStore.countyRegion);

            softAssert.AreEqual(dbStoreNotNull.PostalCode, apiStore.postalCode);

            softAssert.AreEqual(dbStoreNotNull.Phone, apiStore.telephone);

            softAssert.AreEqual(dbStoreNotNull.AddressLine1, apiStore.addressLine1);

            softAssert.AreEqual(dbStoreNotNull.AddressLine2, apiStore.addressLine2);

            softAssert.AssertAll();
        }
    }
}
