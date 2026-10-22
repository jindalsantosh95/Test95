using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager;
using ApiClosure = VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager.StoreManagerClosure;
using DbClosure = VAA.EAPI.Automation.Explore.Helpers.StoreManagerClosure;

namespace VAA.EAPI.Automation.Explore.Helpers
{
    public static class StoreManagerClosuresHelper
    {
        public static async Task ValidateClosures(string storeId, string from, string to, TokenFixture tokenFixture, SoftAssertHelper softAssert)
        {
            // API CALL
            HttpResponseMessage response = Requests.Endpoints.GraphQL.StoreManagerClosures(storeId, from, to, tokenFixture.GetToken("GraphToken")).EnsureGQLResponseSuccess();

            string content = await response.Content.ReadAsStringAsync();

            StoreManagerClosuresResponse? apiResult = JsonConvert.DeserializeObject<StoreManagerClosuresResponse>(content);

            Assert.NotNull(apiResult?.data);

            List<ApiClosure> apiClosures = apiResult.data.storeManagerClosures ?? new List<ApiClosure>();

            HashSet<string> seenDates = new();

            DateTime fromDate = DateTime.Parse(from);

            DateTime toDate = DateTime.Parse(to);

            // API VALIDATION
            foreach (ApiClosure closure in apiClosures)
            {
                softAssert.False(string.IsNullOrEmpty(closure.id), "Closure ID is missing");

                softAssert.False(string.IsNullOrEmpty(closure.closureDate), "Closure date is missing");

                softAssert.False(string.IsNullOrEmpty(closure.reason), "Closure reason is missing");

                // Validate closureDate format
                bool validDate = DateTime.TryParse(closure.closureDate, out DateTime parsedDate);

                softAssert.True(validDate, $"Invalid closureDate format: {closure.closureDate}");

                if (validDate)
                {
                    softAssert.True(parsedDate >= fromDate, $"Closure date {parsedDate:yyyy-MM-dd} is before requested fromDate {from}");

                    softAssert.True(parsedDate <= toDate, $"Closure date {parsedDate:yyyy-MM-dd} is after requested toDate {to}");

                    softAssert.True(parsedDate >= DateTime.Today, $"Closure date {parsedDate:yyyy-MM-dd} is in the past");
                }

                softAssert.True(seenDates.Add(closure.closureDate), $"Duplicate closureDate found: {closure.closureDate}");

                string[] invalidReasons = { "N/A", "None", "Unknown", "" };

                softAssert.False(invalidReasons.Contains(closure.reason.Trim()), $"Invalid closure reason: {closure.reason}");
            }

            // DB CALL
            List<DbClosure> dbClosures = ExploreStoreMasterDBHelper.GetClosuresByStoreId(storeId, from, to);

            // DB vs API VALIDATION
            foreach (DbClosure db in dbClosures)
            {
                ApiClosure? api = apiClosures.FirstOrDefault(c => c.id == db.ClosureId);

                softAssert.True(api != null, $"API is missing closure with ID {db.ClosureId} when compared to DB for the dates from: {from} to: {to}");

                if (api != null)
                {
                    string dbDate = DateTime.Parse(db.ClosureDate).ToString("yyyy-MM-dd");

                    softAssert.AreEqual(dbDate, api.closureDate, $"Mismatch in closureDate for ClosureId {db.ClosureId}");

                    softAssert.AreEqual(db.ClosureType, api.closureType, $"Mismatch in closureType for ClosureId {db.ClosureId}");

                    softAssert.AreEqual(db.Note, api.reason, $"Mismatch in reason/Note for ClosureId {db.ClosureId}");
                }
            }
        }
    }
}
