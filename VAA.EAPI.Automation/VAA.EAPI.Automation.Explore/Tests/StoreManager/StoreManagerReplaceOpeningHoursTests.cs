using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Explore.Constants;
using VAA.EAPI.Automation.Explore.Helpers;
using VAA.EAPI.Automation.Explore.Requests.Models.RequestModels;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager;
using GraphQL = VAA.EAPI.Automation.Explore.Requests.Endpoints.GraphQL;

namespace VAA.EAPI.Automation.Explore.Tests.StoreManager
{
    [Collection("Sequential")]
    public class StoreManagerReplaceOpeningHoursTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;

        public StoreManagerReplaceOpeningHoursTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;
            _tokenFixture.AddTokenForExploreHolidaysApplicationFromKeyVault("GraphToken");
        }

        [Theory]
        [MemberData(nameof(GetRandomStores))]
        [Trait("Category", Traits.ExploreGraphqlRegressionStoreManager)]
        [Trait("Suite", Traits.ExploreMutations)]
        public async Task TC01_ReplaceOpeningHours(string storeId)
        {
            SoftAssertHelper softAssert = new();

            Random random = Random.Shared;

            // Step 1: Query current opening hours
            HttpResponseMessage queryResponse = GraphQL.StoreManagerOpeningHours(storeId, _tokenFixture.GetToken("GraphToken")).EnsureGQLResponseSuccess();

            StoreManagerOpeningHoursResponse? currentResult = JsonConvert.DeserializeObject<StoreManagerOpeningHoursResponse>(await queryResponse.Content.ReadAsStringAsync());

            Assert.NotNull(currentResult?.data?.storeManagerOpeningHours);

            StoreManagerOpeningHour[] currentHours = currentResult.data.storeManagerOpeningHours ?? Array.Empty<StoreManagerOpeningHour>();

            // Step 2: Build dynamic mutation request
            StoreManagerOpeningHoursReplaceRequestInput request = new StoreManagerOpeningHoursReplaceRequestInput
            {
                openingHours = currentHours.Select(h =>
                {
                    // Handle "Closed" case
                    if (h.opensAt.Equals("Closed", StringComparison.OrdinalIgnoreCase) ||
                        h.closesAt.Equals("Closed", StringComparison.OrdinalIgnoreCase))
                    {
                        return new StoreManagerOpeningHourReplace
                        {
                            dayOfWeek = h.dayOfWeek,
                            opensAt = "09:00",
                            closesAt = "17:00",
                            closeAfterOpen = true
                        };
                    }

                    // Parse times
                    TimeSpan open = TimeSpan.Parse(h.opensAt);

                    TimeSpan close = TimeSpan.Parse(h.closesAt);

                    int strategy = random.Next(0, 3);

                    TimeSpan newStart = open;

                    TimeSpan newEnd = close;

                    switch (strategy)
                    {
                        case 0: // Move opening earlier by 1h, closing later by 1h
                            newStart = open.Subtract(TimeSpan.FromHours(1));
                            newEnd = close.Add(TimeSpan.FromHours(1));
                            break;

                        case 1: // Move opening earlier by 30m, keep closing same
                            newStart = open.Subtract(TimeSpan.FromMinutes(30));
                            newEnd = close;
                            break;

                        case 2: // Keep opening same, extend closing by 2h
                            newStart = open;
                            newEnd = close.Add(TimeSpan.FromHours(2));
                            break;
                    }

                    // Ensure valid window (open >= 00:00, end > start)
                    if (newStart < TimeSpan.Zero)
                        newStart = open;
                    if (newEnd >= TimeSpan.FromHours(24))
                        newEnd = close;   // clamp anything >= 24h back to the close
                    if (newEnd <= newStart)
                        newEnd = close;

                    return new StoreManagerOpeningHourReplace
                    {
                        dayOfWeek = h.dayOfWeek,
                        opensAt = newStart.ToString(@"hh\:mm"),
                        closesAt = newEnd.ToString(@"hh\:mm"),
                        closeAfterOpen = true
                    };
                }).ToList()
            };

            try
            {
                // Step 3: Mutation call
                HttpResponseMessage mutationResponse = GraphQL.StoreManagerReplaceOpeningHours(storeId, request, _tokenFixture.GetToken("GraphToken")).EnsureGQLResponseSuccess();

                StoreManagerReplaceOpeningHoursResponse? mutationResult = JsonConvert.DeserializeObject<StoreManagerReplaceOpeningHoursResponse>(await mutationResponse.Content.ReadAsStringAsync());

                Assert.NotNull(mutationResult?.data?.storeManagerReplaceOpeningHours);

                // Step 4: Query again to validate updates
                HttpResponseMessage updatedQueryResponse = GraphQL.StoreManagerOpeningHours(storeId, _tokenFixture.GetToken("GraphToken")).EnsureGQLResponseSuccess();

                StoreManagerOpeningHoursResponse? updatedResult = JsonConvert.DeserializeObject<StoreManagerOpeningHoursResponse>(await updatedQueryResponse.Content.ReadAsStringAsync());

                Assert.NotNull(updatedResult?.data?.storeManagerOpeningHours);

                foreach (StoreManagerOpeningHourReplace expected in request.openingHours)
                {
                    StoreManagerOpeningHour? updatedHour = updatedResult.data.storeManagerOpeningHours?.FirstOrDefault(h => h.dayOfWeek == expected.dayOfWeek);

                    softAssert.True(updatedHour != null, $"Updated response missing day {expected.dayOfWeek}");

                    if (updatedHour != null)
                    {
                        softAssert.AreEqual(expected.opensAt, updatedHour.opensAt, $"OpensAt mismatch for day {expected.dayOfWeek}");

                        softAssert.AreEqual(expected.closesAt, updatedHour.closesAt, $"ClosesAt mismatch for day {expected.dayOfWeek}");
                    }
                }

                softAssert.AssertAll();
            }
            finally
            {
                // Restore original opening hours so regression is isolated
                StoreManagerOpeningHoursReplaceRequestInput restoreRequest = new StoreManagerOpeningHoursReplaceRequestInput
                {
                    openingHours = currentHours.Select(h => new StoreManagerOpeningHourReplace
                    {
                        dayOfWeek = h.dayOfWeek,
                        opensAt = h.opensAt,
                        closesAt = h.closesAt,
                        closeAfterOpen = true
                    }).ToList()
                };

                try
                {
                    HttpResponseMessage restoreResponse = GraphQL.StoreManagerReplaceOpeningHours(storeId, restoreRequest, _tokenFixture.GetToken("GraphToken"));

                    restoreResponse.EnsureGQLResponseSuccess();
                }
                catch (Exception restoreException)
                {
                    Console.WriteLine($"Restore failed for store {storeId}: {restoreException.Message}");
                }
            }
        }

        public static IEnumerable<object[]> GetRandomStores()
        {
            List<string> storeIds;
            try
            {
                storeIds = ExploreStoreMasterDBHelper.GetAllActiveStoreIds();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB fetch failed, fallback list. {ex.GetType().Name}: {ex.Message}");

                storeIds = new() { "1003", "1025", "1030", "1017", "1011", "1009", "1028" };
            }

            if (storeIds.Count == 0)
                storeIds = new() { "1001" };

            Random random = Random.Shared;

            List<string> selectedStores = storeIds.OrderBy(_ => random.Next()).Take(7).ToList();

            foreach (string storeId in selectedStores)
            {
                yield return new object[] { storeId };
            }
        }
    }
}
