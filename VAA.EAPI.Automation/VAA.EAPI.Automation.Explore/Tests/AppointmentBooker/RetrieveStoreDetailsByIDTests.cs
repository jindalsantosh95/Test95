using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Explore.Constants;
using VAA.EAPI.Automation.Explore.Helpers;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.AppointmentBooker;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager;
using GraphQL = VAA.EAPI.Automation.Explore.Requests.Endpoints.GraphQL;

namespace VAA.EAPI.Automation.Explore.Tests.AppointmentBooker
{
    public class RetrieveStoreDetailsByIDTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;
        public RetrieveStoreDetailsByIDTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;
            _tokenFixture.AddTokenForExploreHolidaysApplicationFromKeyVault("GraphToken");
        }

        [Theory]
        [Trait("Category", Traits.ExploreGraphqlRegressionAppointmentBooker)]
        [Trait("Suite", Traits.ExploreReadOnly)]
        [InlineData("1009", "B904RB")]           //PostalCode
        [InlineData("1028", "AL13HL")]           //PostalCode
        [InlineData("1034", "Merseyside")]       //County
        [InlineData("1015", "Basildon")]         //City
        [InlineData("1005", "High Wycombe")]     //City
        [InlineData("1022", "Cardiff")]          //City
        [InlineData("1016", "Nottinghamshire")]  //County
        [InlineData("1011", "Hampshire")]        //Country
        public async Task TC01_RetrieveStoreDetailsByID(string storeId, string origin)
        {
            SoftAssertHelper softAssert = new();

            // Step 1: Call AdminStoreById API
            HttpResponseMessage adminResponse = GraphQL.StoreManagerAdminStoreById(storeId, _tokenFixture.GetToken("GraphToken")).EnsureGQLResponseSuccess();

            StoreManagerAdminStoreByIdResponseRoot? adminResult = JsonConvert.DeserializeObject<StoreManagerAdminStoreByIdResponseRoot>(await adminResponse.Content.ReadAsStringAsync());

            Assert.NotNull(adminResult);

            Assert.NotNull(adminResult.data);

            Assert.NotNull(adminResult.data.storeManagerAdminStoreById);

            bool isClosed = adminResult.data.storeManagerAdminStoreById.isClosed;

            // Step 2: If store is closed, stop here as no further validation required
            if (isClosed)
            {
                softAssert.True(isClosed, $"Store {storeId} is closed, skipping store details validation.");

                softAssert.AssertAll();

                return;
            }

            // API CALL
            HttpResponseMessage response = GraphQL.RetrieveStoreDetailsByID(storeId, origin).EnsureGQLResponseSuccess();

            RetrieveAllNearByStoresResponse apiResult = JsonConvert.DeserializeObject<RetrieveAllNearByStoresResponse>(await response.Content.ReadAsStringAsync());

            RetrieveStoreDetailsById apiStore = apiResult.data.RetrieveStoreDetailsByID;

            Assert.True(apiStore != null, "API store object is null");

            // DB CALL
            StoreMasterRecord dbStore = ExploreStoreMasterDBHelper.GetStoreById(storeId);

            Assert.True(dbStore != null, "DB store record is null");
           
            // BASIC STORE FIELDS
            softAssert.AreEqual(dbStore.StoreId, apiStore.storeId, "StoreId mismatch");

            softAssert.AreEqual(dbStore.Name, apiStore.name, "Name mismatch");

            softAssert.AreEqual(dbStore.Label, apiStore.locationLabel, "Label mismatch");

            // BOOLEAN CHECK
            softAssert.True(apiStore.isClosest, "isClosest should be true");

            // FULL DETAILS
            softAssert.AreEqual(dbStore.Phone, apiStore.fullDetails.details.phone, "Phone mismatch");

            softAssert.AreEqual(dbStore.EmailAddress, apiStore.fullDetails.details.emailAddress, "Email mismatch");

            softAssert.AreEqual(dbStore.AddressLine1, apiStore.fullDetails.details.address.addressLine1, "AddressLine1 mismatch");

            softAssert.AreEqual(dbStore.AddressLine2, apiStore.fullDetails.details.address.addressLine2, "AddressLine2 mismatch");

            softAssert.AreEqual(dbStore.City, apiStore.fullDetails.details.address.city, "City mismatch");

            softAssert.AreEqual(dbStore.County, apiStore.fullDetails.details.address.county, "County mismatch");

            softAssert.AreEqual(dbStore.CountryCode, apiStore.fullDetails.details.address.countryCode, "CountryCode mismatch");

            softAssert.AreEqual(dbStore.PostalCode, apiStore.fullDetails.details.address.postalCode, "PostalCode mismatch");

            // WEEKLY HOURS          
            RetrieveStoreDetailsByIDHelper.ValidateWeeklyHours(apiStore.fullDetails.openingHours.weeklyHours,dbStore.WeeklyHours);
            // SERVICES           
            RetrieveStoreDetailsByIDHelper.ValidateServices(apiStore.fullDetails.services.inStoreFacilities,dbStore);

            softAssert.AssertAll();       
        }        
    }
}
