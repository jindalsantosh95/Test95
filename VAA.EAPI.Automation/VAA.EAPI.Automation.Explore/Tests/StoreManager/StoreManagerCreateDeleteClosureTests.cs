using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Explore.Constants;
using VAA.EAPI.Automation.Explore.Helpers;
using VAA.EAPI.Automation.Explore.Requests.Models.RequestModels;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager;

namespace VAA.EAPI.Automation.Explore.Tests.StoreManager
{
    [Collection("Sequential")]
    public class StoreManagerCreateDeleteClosureTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;

        public StoreManagerCreateDeleteClosureTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;
            _tokenFixture.AddTokenForExploreHolidaysApplicationFromKeyVault("GraphToken");
        }

        [Theory]
        [Trait("Category", Traits.ExploreGraphqlRegressionStoreManager)]
        [Trait("Suite", Traits.ExploreMutations)]
        [InlineData("1009")]
        [InlineData("1021")]
        [InlineData("1015")]
        [InlineData("1025")]
        [InlineData("1003")]
        public async Task TC01_CreateAndDeleteClosure(string storeId)
        {
            SoftAssertHelper softAssert = new();

            string closureDateString = DateTime.Today.AddDays(10).ToString("yyyy-MM-dd");

            StoreClosureRequestInput request = new()
            {
                closureDate = closureDateString,

                closureType = Application.ClosureFullDay,

                reason = Application.ClosureTestReason
            };

            // Step 1: Create closure
            HttpResponseMessage createResponse = Requests.Endpoints.GraphQL.StoreManagerCreateClosure(storeId, request, _tokenFixture.GetToken("GraphToken"));

            // Read raw content before enforcing success
            string createContent = await createResponse.Content.ReadAsStringAsync();

            // If API returned ACTIVE_BOOKINGS_EXIST error, accept and skip further validation
            if (createContent.Contains("ACTIVE_BOOKINGS_EXIST"))
            {
                Console.WriteLine($"Closure not created for store {storeId} on {closureDateString} due to active bookings — acceptable case.");

                return; // exit test gracefully
            }

            // Otherwise enforce success and continue
            createResponse.EnsureGQLResponseSuccess();

            StoreManagerCreateClosure? createResult = JsonConvert.DeserializeObject<StoreManagerCreateClosure>(createContent);

            Assert.NotNull(createResult?.data?.storeManagerCreateClosure);

            string createdId = createResult.data.storeManagerCreateClosure.id;

            try
            {
                // Step 2: Re-check closure using full helper validations
                await StoreManagerClosuresHelper.ValidateClosures(storeId, closureDateString, closureDateString, _tokenFixture, softAssert);

                // Step 3: Delete closure
                HttpResponseMessage deleteResponse = Requests.Endpoints.GraphQL.StoreManagerDeleteClosure(storeId, createdId, _tokenFixture.GetToken("GraphToken")).EnsureGQLResponseSuccess();

                StoreManagerDeleteClosure? deleteResult = JsonConvert.DeserializeObject<StoreManagerDeleteClosure>(await deleteResponse.Content.ReadAsStringAsync());
                Assert.True(deleteResult?.data?.storeManagerDeleteClosure == true, "Delete closure failed");

                // Step 4: Re-check closure again after deletion
                await StoreManagerClosuresHelper.ValidateClosures(storeId, closureDateString, closureDateString, _tokenFixture, softAssert);

                softAssert.AssertAll();
            }
            finally
            {
                // Ensure cleanup even if assertions or validation fail.
                if (!createdId.IsNullOrWhiteSpace())
                {
                    try
                    {
                        HttpResponseMessage deleteResponse = Requests.Endpoints.GraphQL.StoreManagerDeleteClosure(storeId, createdId, _tokenFixture.GetToken("GraphToken")).EnsureGQLResponseSuccess();
                    }
                    catch (Exception cleanupException)
                    {
                        Console.WriteLine($"Cleanup failed for closure {createdId} and store {storeId}: {cleanupException.Message}");
                    }
                }
            }
        }
    }
}
