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
    public class StoreManagerStoreByIdTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;
        public StoreManagerStoreByIdTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;
            _tokenFixture.AddTokenForExploreHolidaysApplicationFromKeyVault("GraphToken");
        }

        [Theory]
        [Trait("Category", Traits.ExploreGraphqlRegressionStoreManager)]
        [Trait("Suite", Traits.ExploreReadOnly)]
        [InlineData("1030")]
        [InlineData("1003")]
        [InlineData("1018")]
        [InlineData("1031")]
        [InlineData("1011")]
        [InlineData("1022")]
        public async Task TC01_StoreManagerStoreById(string storeId)
        {
            SoftAssertHelper softAssert = new();

            // API CALL
            HttpResponseMessage response = GraphQL.StoreManagerStoreById(storeId, _tokenFixture.GetToken("GraphToken")).EnsureGQLResponseSuccess();

            StoreManagerStoreByIdResponse? apiResult = JsonConvert.DeserializeObject<StoreManagerStoreByIdResponse>(await response.Content.ReadAsStringAsync());

            Assert.NotNull(apiResult);

            Assert.NotNull(apiResult.data);

            StoreManagerStoreById? apiStore = apiResult.data.storeManagerStoreById;

            Assert.NotNull(apiStore);

            Assert.True(apiStore != null, "API store object is null");

            // DB CALL
            StoreMasterRecord? dbStore = ExploreStoreMasterDBHelper.GetStoreById(storeId);

            Assert.True(dbStore != null, "DB store record is null");

            // BASIC STORE FIELDS
            softAssert.AreEqual(dbStore.StoreId, apiStore.id, "StoreId mismatch");

            softAssert.AreEqual(dbStore.Name, apiStore.name, "Name mismatch");

            softAssert.AreEqual(dbStore.Label, apiStore.label, "Label mismatch");

            softAssert.AreEqual(dbStore.Phone, apiStore.telephone, "Phone mismatch");

            softAssert.AreEqual(dbStore.AddressLine1, apiStore.addressLine1, "AddressLine1 mismatch");

            softAssert.AreEqual(dbStore.AddressLine2, apiStore.addressLine2, "AddressLine2 mismatch");

            softAssert.AreEqual(dbStore.City, apiStore.cityTown, "City mismatch");

            softAssert.AreEqual(dbStore.County, apiStore.countyRegion, "County mismatch");

            softAssert.AreEqual(dbStore.PostalCode, apiStore.postalCode, "PostalCode mismatch");

            // SERVICES validation inline
            List<string> expectedFacilities = new();

            if (dbStore.HasKidsArea)
                expectedFacilities.Add(Application.ServiceKids);

            if (dbStore.HasWelcomeDrinksAndSnacks)
                expectedFacilities.Add(Application.ServiceFood);

            if (dbStore.HasAircraftSeats)
                expectedFacilities.Add(Application.ServiceAircraft);

            if (dbStore.HasSecretCinema)
                expectedFacilities.Add(Application.ServiceCinema);

            // If DB expects services, API must not be null
            if (expectedFacilities.Any())
            {
                softAssert.True(apiStore.services != null, "API services array is null when DB expects services");

                string[] apiServices = apiStore.services ?? Array.Empty<string>();

                softAssert.AreEqual(expectedFacilities.Count, apiServices.Length, "Services count mismatch");

                foreach (string facility in expectedFacilities)
                {
                    softAssert.True(Application.ContainsService(apiServices, facility), $"Missing service: {facility}");
                }
            }
            else
            {
                // If DB expects no services, API should be null or empty
                if (apiStore.services != null)
                    softAssert.AreEqual(0, apiStore.services.Length, "API services should be empty when DB expects none");
            }
            softAssert.AssertAll();
        }
    }
}
