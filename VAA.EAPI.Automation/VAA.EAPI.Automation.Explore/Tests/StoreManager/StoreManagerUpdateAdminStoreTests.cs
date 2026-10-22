using System.Text.RegularExpressions;
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
    public class StoreManagerUpdateAdminStoreTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;

        public StoreManagerUpdateAdminStoreTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;

            _tokenFixture.AddTokenForExploreHolidaysApplicationFromKeyVault("GraphToken");
        }

        [Theory]
        [MemberData(nameof(GetActiveStoreIds))]
        [Trait("Category", Traits.ExploreGraphqlRegressionStoreManager)]
        [Trait("Suite", Traits.ExploreMutations)]
        public async Task TC01_StoreManagerUpdateAdminStore(string storeId)
        {
            SoftAssertHelper softAssert = new();

            string bearer = _tokenFixture.GetToken("GraphToken");

            StoreMasterRecord originalStore = ExploreStoreMasterDBHelper.GetStoreById(storeId) ?? throw new InvalidOperationException($"StoreMaster record not found for store {storeId}");

            string runToken = DateTime.UtcNow.ToString("MMddHHmmssfff");

            StoreRequestInput updateRequest = BuildUpdateRequest(storeId, originalStore, runToken);

            bool mutationApplied = false;

            try
            {
                // Step 1: Confirm every editable field is different from the original database value.
                ValidateChangedFields(softAssert, storeId, originalStore, updateRequest);

                // Step 2: Confirm the original values can be sent back through the API for restoration.
                ValidateRestorationValues(softAssert, storeId, originalStore);

                // Step 3: Stop before mutation if the generated or restoration values are invalid.
                softAssert.AssertAll();

                // Step 4: Call the StoreManagerUpdateAdminStore mutation.
                HttpResponseMessage mutationResponse = GraphQL.StoreManagerUpdateAdminStore(storeId, updateRequest, bearer).EnsureGQLResponseSuccess();

                mutationApplied = true;

                // Step 5: Deserialize the mutation response.
                string mutationBody = await mutationResponse.Content.ReadAsStringAsync();

                StoreManagerUpdateAdminStoreResponse? mutationResult = JsonConvert.DeserializeObject<StoreManagerUpdateAdminStoreResponse>(mutationBody);

                if (mutationResult?.data?.storeManagerUpdateAdminStore is not StoreManagerUpdatedStore mutationStore)
                {
                    throw new InvalidOperationException($"Mutation returned no store data for store {storeId}");
                }

                // Step 6: Validate the direct mutation response against the request.
                ValidateMutationResponse(softAssert, storeId, updateRequest, mutationStore);

                // Step 7: Call StoreManagerAdminStoreById after the mutation.
                HttpResponseMessage queryResponse = GraphQL.StoreManagerAdminStoreById(storeId, bearer).EnsureGQLResponseSuccess();

                // Step 8: Deserialize the query response.
                string queryBody = await queryResponse.Content.ReadAsStringAsync();

                StoreManagerAdminStoreByIdResponseRoot? queryResult = JsonConvert.DeserializeObject<StoreManagerAdminStoreByIdResponseRoot>(queryBody);

                if (queryResult?.data?.storeManagerAdminStoreById is not StoreManagerAdminStoreByIdResponse updatedApiStore)
                {
                    throw new InvalidOperationException($"StoreManagerAdminStoreById returned no store data for store {storeId}");
                }

                // Step 9: Validate the updated API response against the request.
                ValidateQueryResponse(softAssert, storeId, updateRequest, updatedApiStore);

                // Step 10: Read the updated StoreMaster record from the database.
                StoreMasterRecord updatedDbStore = ExploreStoreMasterDBHelper.GetStoreById(storeId) ?? throw new InvalidOperationException($"Updated StoreMaster record not found for store {storeId}");

                // Step 11: Validate the updated database record against the request.
                ValidateDatabaseRecord(softAssert, storeId, updateRequest, updatedDbStore);
            }
            finally
            {
                if (mutationApplied)
                {
                    try
                    {
                        // Step 12: Build the restoration request using the original database values.
                        StoreRequestInput restoreRequest = BuildRestoreRequest(originalStore);

                        // Step 13: Restore the original values through the mutation API.
                        HttpResponseMessage restoreMutationResponse = GraphQL.StoreManagerUpdateAdminStore(storeId, restoreRequest, bearer).EnsureGQLResponseSuccess();

                        // Step 14: Deserialize the restoration mutation response.
                        string restoreMutationBody = await restoreMutationResponse.Content.ReadAsStringAsync();

                        StoreManagerUpdateAdminStoreResponse? restoreMutationResult = JsonConvert.DeserializeObject<StoreManagerUpdateAdminStoreResponse>(restoreMutationBody);

                        if (restoreMutationResult?.data?.storeManagerUpdateAdminStore is not StoreManagerUpdatedStore restoredMutationStore)
                        {
                            throw new InvalidOperationException($"Restoration mutation returned no store data for store {storeId}");
                        }

                        // Step 15: Validate the restoration mutation response.
                        ValidateMutationResponse(softAssert, storeId, restoreRequest, restoredMutationStore);

                        // Step 16: Query the store again after restoration.
                        HttpResponseMessage restoreQueryResponse = GraphQL.StoreManagerAdminStoreById(storeId, bearer).EnsureGQLResponseSuccess();

                        // Step 17: Deserialize the restored API response.
                        string restoreQueryBody = await restoreQueryResponse.Content.ReadAsStringAsync();

                        StoreManagerAdminStoreByIdResponseRoot? restoreQueryResult = JsonConvert.DeserializeObject<StoreManagerAdminStoreByIdResponseRoot>(restoreQueryBody);

                        if (restoreQueryResult?.data?.storeManagerAdminStoreById is not StoreManagerAdminStoreByIdResponse restoredApiStore)
                        {
                            throw new InvalidOperationException($"Restoration query returned no store data for store {storeId}");
                        }

                        // Step 18: Validate that the original values were restored in the API.
                        ValidateQueryResponse(softAssert, storeId, restoreRequest, restoredApiStore);

                        // Step 19: Read the restored StoreMaster record from the database.
                        StoreMasterRecord restoredDbStore = ExploreStoreMasterDBHelper.GetStoreById(storeId) ?? throw new InvalidOperationException($"Restored StoreMaster record not found for store {storeId}");

                        // Step 20: Validate that the original values were restored in the database.
                        ValidateDatabaseRecord(softAssert, storeId, restoreRequest, restoredDbStore);
                    }
                    catch (Exception restorationException)
                    {
                        softAssert.True(false, $"Restoration failed for store {storeId}: {restorationException.Message}");
                    }
                }
            }

            // Step 21: Report all API, database, mutation, and restoration validation failures.
            softAssert.AssertAll();
        }

        // Returns every active store from StoreMaster for this mutation test.
        public static IEnumerable<object[]> GetActiveStoreIds()
        {
            // Read every active store dynamically from the database.
            List<string> activeStoreIds = ExploreStoreMasterDBHelper.GetAllActiveStoreIds();

            // Stop the test if no active stores are available.
            if (activeStoreIds.Count == 0)
            {
                throw new InvalidOperationException("No active stores were found in StoreMaster.");
            }

            // Create one test case for every active store.
            foreach (string storeId in activeStoreIds)
            {
                yield return new object[] { storeId };
            }
        }

        // Builds unique changed values for every editable field.
        private static StoreRequestInput BuildUpdateRequest(string storeId, StoreMasterRecord originalStore, string runToken)
        {
            List<string> services = new() { "Kids area", "Aircraft seats" };

            if (originalStore.Services.SequenceEqual(services, StringComparer.OrdinalIgnoreCase))
            {
                services = new List<string> { "Welcome drinks and snacks", "Secret cinema" };
            }

            string telephone = $"0{runToken[^6..]}{storeId}";

            if (NormalizeTelephone(originalStore.Phone).Is(NormalizeTelephone(telephone)))
            {
                telephone = $"0{runToken[^5..]}9{storeId}";
            }

            return new StoreRequestInput
            {
                name = $"Automation Store {storeId} {runToken}",
                addressLine1 = $"Automation Address {storeId} {runToken}",
                addressLine2 = $"Automation Centre {storeId} {runToken}",
                cityTown = $"Automation City {storeId} {runToken}",
                countyRegion = $"Automation County {storeId} {runToken}",
                postalCode = BuildPostalCode(storeId, runToken),
                telephone = telephone,
                email = $"automation.{storeId}.{runToken}@fly.virgin.com",
                services = services,
                isClosed = originalStore.MarkedAsClosed
            };
        }

        // Builds the restoration request using API-compatible formatting.
        private static StoreRequestInput BuildRestoreRequest(StoreMasterRecord store)
        {
            return new StoreRequestInput
            {
                name = store.Name,
                addressLine1 = store.AddressLine1,
                addressLine2 = store.AddressLine2,
                cityTown = store.City,
                countyRegion = store.County,
                postalCode = store.PostalCode,
                telephone = NormalizeTelephone(store.Phone),
                email = store.EmailAddress.Trim(),
                services = store.Services.ToList(),
                isClosed = store.MarkedAsClosed
            };
        }

        // Creates a store-specific postcode for the test request.
        private static string BuildPostalCode(string storeId, string runToken)
        {
            int outwardNumber = int.Parse(runToken[^2..]);

            if (outwardNumber == 0)
            {
                outwardNumber = 99;
            }

            return $"RM{outwardNumber:00} {storeId[^1]}ZP";
        }

        // Confirms every editable request field differs from the original database value.
        private static void ValidateChangedFields(SoftAssertHelper softAssert, string storeId, StoreMasterRecord originalStore, StoreRequestInput request)
        {
            softAssert.True(!request.name.Is(originalStore.Name), $"Name was not changed for store {storeId}");

            softAssert.True(!request.addressLine1.Is(originalStore.AddressLine1), $"AddressLine1 was not changed for store {storeId}");

            softAssert.True(!request.addressLine2.Is(originalStore.AddressLine2), $"AddressLine2 was not changed for store {storeId}");

            softAssert.True(!request.cityTown.Is(originalStore.City), $"CityTown was not changed for store {storeId}");

            softAssert.True(!request.countyRegion.Is(originalStore.County), $"CountyRegion was not changed for store {storeId}");

            softAssert.True(!request.postalCode.Is(originalStore.PostalCode), $"PostalCode was not changed for store {storeId}");

            softAssert.True(!NormalizeTelephone(request.telephone).Is(NormalizeTelephone(originalStore.Phone)), $"Telephone was not changed for store {storeId}");

            softAssert.True(!request.email.Is(originalStore.EmailAddress), $"Email was not changed for store {storeId}");

            softAssert.True(!request.services.SequenceEqual(originalStore.Services, StringComparer.OrdinalIgnoreCase), $"Services were not changed for store {storeId}");
        }

        // Confirms that original values can be accepted by the API during restoration.
        private static void ValidateRestorationValues(SoftAssertHelper softAssert, string storeId, StoreMasterRecord originalStore)
        {
            string restoreEmail = originalStore.EmailAddress.Trim();

            string restoreTelephone = NormalizeTelephone(originalStore.Phone);

            bool validEmail = Regex.IsMatch(restoreEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

            bool validTelephone = !restoreTelephone.IsNullOrWhiteSpace() && !restoreTelephone.Any(char.IsWhiteSpace);

            softAssert.True(validEmail, $"Original email cannot be accepted by the API for restoration for store {storeId}: {restoreEmail}");

            softAssert.True(validTelephone, $"Original telephone cannot be accepted by the API for restoration for store {storeId}: {originalStore.Phone}");
        }

        // Validates the direct mutation response against the expected request.
        private static void ValidateMutationResponse(SoftAssertHelper softAssert, string storeId, StoreRequestInput expected, StoreManagerUpdatedStore actual)
        {
            softAssert.AreEqual(storeId, actual.id, $"Mutation API id mismatch for store {storeId}");

            softAssert.AreEqual(expected.name, actual.name, $"Mutation API name mismatch for store {storeId}");

            softAssert.AreEqual(expected.addressLine1, actual.addressLine1, $"Mutation API addressLine1 mismatch for store {storeId}");

            softAssert.AreEqual(expected.addressLine2, actual.addressLine2, $"Mutation API addressLine2 mismatch for store {storeId}");

            softAssert.AreEqual(expected.cityTown, actual.cityTown, $"Mutation API cityTown mismatch for store {storeId}");

            softAssert.AreEqual(expected.countyRegion, actual.countyRegion, $"Mutation API countyRegion mismatch for store {storeId}");

            softAssert.AreEqual(expected.postalCode, actual.postalCode, $"Mutation API postalCode mismatch for store {storeId}");

            softAssert.AreEqual(NormalizeTelephone(expected.telephone), NormalizeTelephone(actual.telephone), $"Mutation API telephone mismatch for store {storeId}");

            softAssert.AreEqual(expected.email, actual.email, $"Mutation API email mismatch for store {storeId}");

            softAssert.AreEqual(expected.isClosed, actual.isClosed, $"Mutation API isClosed mismatch for store {storeId}");

            List<string> actualServices = actual.services?.ToList() ?? new List<string>();

            softAssert.True(expected.services.SequenceEqual(actualServices, StringComparer.OrdinalIgnoreCase), $"Mutation API services mismatch for store {storeId}");
        }

        // Validates the StoreManagerAdminStoreById query response against the expected request.
        private static void ValidateQueryResponse(SoftAssertHelper softAssert, string storeId, StoreRequestInput expected, StoreManagerAdminStoreByIdResponse actual)
        {
            softAssert.AreEqual(storeId, actual.id, $"Query API id mismatch for store {storeId}");

            softAssert.AreEqual(expected.name, actual.name, $"Query API name mismatch for store {storeId}");

            softAssert.AreEqual(expected.addressLine1, actual.addressLine1, $"Query API addressLine1 mismatch for store {storeId}");

            softAssert.AreEqual(expected.addressLine2, actual.addressLine2, $"Query API addressLine2 mismatch for store {storeId}");

            softAssert.AreEqual(expected.cityTown, actual.cityTown, $"Query API cityTown mismatch for store {storeId}");

            softAssert.AreEqual(expected.countyRegion, actual.countyRegion, $"Query API countyRegion mismatch for store {storeId}");

            softAssert.AreEqual(expected.postalCode, actual.postalCode, $"Query API postalCode mismatch for store {storeId}");

            softAssert.AreEqual(NormalizeTelephone(expected.telephone), NormalizeTelephone(actual.telephone), $"Query API telephone mismatch for store {storeId}");

            softAssert.AreEqual(expected.email, actual.email, $"Query API email mismatch for store {storeId}");

            softAssert.AreEqual(expected.isClosed, actual.isClosed, $"Query API isClosed mismatch for store {storeId}");

            List<string> actualServices = actual.services?.Select(service => service?.ToString() ?? string.Empty).ToList() ?? new List<string>();

            softAssert.True(expected.services.SequenceEqual(actualServices, StringComparer.OrdinalIgnoreCase), $"Query API services mismatch for store {storeId}");
        }

        // Validates the StoreMaster database record against the expected request.
        private static void ValidateDatabaseRecord(SoftAssertHelper softAssert, string storeId, StoreRequestInput expected, StoreMasterRecord actual)
        {
            softAssert.AreEqual(storeId, actual.StoreId, $"Database StoreId mismatch for store {storeId}");

            softAssert.AreEqual(expected.name, actual.Name, $"Database Name mismatch for store {storeId}");

            softAssert.AreEqual(expected.addressLine1, actual.AddressLine1, $"Database AddressLine1 mismatch for store {storeId}");

            softAssert.AreEqual(expected.addressLine2, actual.AddressLine2, $"Database AddressLine2 mismatch for store {storeId}");

            softAssert.AreEqual(expected.cityTown, actual.City, $"Database City mismatch for store {storeId}");

            softAssert.AreEqual(expected.countyRegion, actual.County, $"Database County mismatch for store {storeId}");

            softAssert.AreEqual(expected.postalCode, actual.PostalCode, $"Database PostalCode mismatch for store {storeId}");

            softAssert.AreEqual(NormalizeTelephone(expected.telephone), NormalizeTelephone(actual.Phone), $"Database telephone mismatch for store {storeId}");

            softAssert.AreEqual(expected.email, actual.EmailAddress, $"Database EmailAddress mismatch for store {storeId}");

            softAssert.AreEqual(expected.isClosed, actual.MarkedAsClosed, $"Database MarkedAsClosed mismatch for store {storeId}");

            softAssert.True(expected.services.SequenceEqual(actual.Services, StringComparer.OrdinalIgnoreCase), $"Database services mismatch for store {storeId}");
        }

        // Removes spaces before comparing telephone numbers.
        private static string NormalizeTelephone(string? telephone)
        {
            return telephone.IsNullOrWhiteSpace() ? string.Empty : new string(telephone.Where(character => !char.IsWhiteSpace(character)).ToArray());
        }
    }
}
