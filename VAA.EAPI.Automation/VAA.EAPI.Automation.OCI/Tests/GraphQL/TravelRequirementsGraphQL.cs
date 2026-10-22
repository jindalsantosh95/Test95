using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.OCI.Constants;
using VAA.EAPI.Automation.OCI.Helpers;
using VAA.EAPI.Automation.OCI.Requests.Endpoints;

namespace VAA.EAPI.Automation.OCI.Tests.GraphQL
{
    public class TravelRequirementsGraphQL
    {
        private readonly ITestOutputHelper _output;

        public TravelRequirementsGraphQL(ITestOutputHelper output)
        {
            _output = output;
        }

        private static bool IsRequirementStatusValid(string status)
        {
            if (status.IsNullOrEmpty())
            {
                return false;
            }

            return status.Is("REQUIRED")
                || status.Is("PROVIDED")
                || status.Is("NOT_REQUIRED");
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        [DisplayName("GetTravelRequirements + SavePassportTravelRequirements returns travel requirements and saves passport details for a valid PNR")]
        public void TravelRequirementsGraphQLTests_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCITravelRequirementsPnrs();

            string errorMessage;
            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string pnrLocator = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];

            Assert.True(!string.IsNullOrEmpty(pnrLocator) && !string.IsNullOrEmpty(givenNames) && !string.IsNullOrEmpty(surname), "PNRLocator, GivenNames and Surname should not be null or empty");

            string travelRequirementsGraphQLRequest = GraphQLRequestBuilder.GetTravelRequirementsGraphQLRequest(pnrLocator, givenNames, surname);

            HttpResponseMessage travelRequirementsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(travelRequirementsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken responseJson = JsonConvert.DeserializeObject<JToken>(travelRequirementsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            bool travelRequirementsOk = GraphQLAssertions.ValidateTravelRequirementsResponse(responseJson, pnrLocator, out ValidationResultHelper travelRequirementsValidation);
            Assert.True(travelRequirementsOk, string.Join(" | ", travelRequirementsValidation.Message));

            bool slicesOk = GraphQLAssertions.ValidateSlicesAndPassengerSliceRequirements(responseJson, out ValidationResultHelper slicesValidation);
            Assert.True(slicesOk, string.Join(" | ", slicesValidation.Message));

            string travelRequirementsId = responseJson.SelectToken("$.data.GetTravelRequirements.travelRequirementsId")?.ToString();
            Assert.False(travelRequirementsId.IsNullOrEmpty(), $"travelRequirementsId is missing from response: {responseJson}");

            string destinationCountryCode = responseJson.SelectTokens("$.data.GetTravelRequirements.slices[*].segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

            Assert.False(destinationCountryCode.IsNullOrEmpty(), $"destination.countryCode is missing from GetTravelRequirements response: {responseJson}");

            JArray passengers = responseJson.SelectToken("$.data.GetTravelRequirements.passengers") as JArray;

            Assert.NotNull(passengers);

            Assert.True(passengers.Count > 0, $"No passengers returned in GetTravelRequirements response: {responseJson}");

            foreach (JToken passenger in passengers)
            {
                string passengerId = passenger["passengerId"]?.ToString();
                string passengerGivenNames = passenger["givenNames"]?.ToString();
                string passengerSurname = passenger["surname"]?.ToString();
                string dateOfBirth = passenger["dateOfBirth"]?.ToString();

                Assert.True(!passengerId.IsNullOrEmpty() && !passengerGivenNames.IsNullOrEmpty() && !passengerSurname.IsNullOrEmpty() && !dateOfBirth.IsNullOrEmpty(),
                    $"Passenger details missing from travel requirements response: {passenger}");

                JArray passengerSliceRequirements = passenger["sliceRequirements"] as JArray;

                bool passportAlreadyProvided = passengerSliceRequirements != null
                    && passengerSliceRequirements.Count > 0
                    && passengerSliceRequirements.All(sr => sr?["passportProvided"]?.Value<bool?>() ?? false);

                if (passportAlreadyProvided)
                {
                    continue;
                }

                string passportNumber = PassportGenerator.GeneratePassport(destinationCountryCode);

                Assert.False(passportNumber.IsNullOrEmpty(), $"Passport number is missing for country {destinationCountryCode}.");

                string savePassportTravelRequirementsRequest = GraphQLRequestBuilder.SavePassportTravelRequirementsGraphQLRequest(
                    travelRequirementsId,
                    passengerId,
                    passengerGivenNames,
                    passengerSurname,
                    dateOfBirth,
                    passportNumber,
                    destinationCountryCode,
                    destinationCountryCode,
                    destinationCountryCode);

                HttpResponseMessage savePassportTravelRequirementsResponse = GraphQLEndpoint.GraphQLCheckInMobile(savePassportTravelRequirementsRequest).EnsureGQLResponseSuccess();

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportTravelRequirementsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                IEnumerable<JToken> passportProvidedTokens = savePassportResult.SelectTokens($"$.data.SavePassportTravelRequirements.passengers[?(@.passengerId == '{passengerId}')].sliceRequirements[*].passportProvided");

                Assert.True(passportProvidedTokens.Any(), $"No sliceRequirements passportProvided values returned for passengerId {passengerId}. Response: {savePassportResult}");

                bool passportProvided = passportProvidedTokens.All(t => t?.Value<bool?>() ?? false);

                Assert.True(passportProvided, $"Passport details not saved for request: {savePassportTravelRequirementsRequest}\nResponse: {savePassportResult}");
            }

            string saveEmergencyContactTravelRequirementsRequest = GraphQLRequestBuilder.SaveEmergencyContactTravelRequirementsGraphQLRequest(responseJson, travelRequirementsId);

            HttpResponseMessage saveEmergencyContactTravelRequirementsResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactTravelRequirementsRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactTravelRequirementsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactTravelRequirementsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.DoAllPassengersHaveEmergencyContact(saveEmergencyContactTravelRequirementsResult), $"Emergency contact details not saved for all passengers response: \n {saveEmergencyContactTravelRequirementsResult}");

            _output.WriteLine($"PNR used for this test: {pnrLocator}");
        }

        [Trait("Category", Traits.CheckInGraphQLRegression)]
        [Fact]
        public void TravelRequirementsGraphQLTests_ReturnDate_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.OneWay.ToString(),
                ReservationType.REVENUE.ToString(), "OneWayReturnDateScenarioRevenue", LocationType.LHR.ToString());

            string errorMessage;
            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string pnrLocator = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];

            Assert.True(!string.IsNullOrEmpty(pnrLocator) && !string.IsNullOrEmpty(givenNames) && !string.IsNullOrEmpty(surname), "PNRLocator, GivenNames and Surname should not be null or empty");

            string travelRequirementsGraphQLRequest = GraphQLRequestBuilder.GetTravelRequirementsGraphQLRequest(pnrLocator, givenNames, surname);

            HttpResponseMessage travelRequirementsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(travelRequirementsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken responseJson = JsonConvert.DeserializeObject<JToken>(travelRequirementsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            bool travelRequirementsOk = GraphQLAssertions.ValidateTravelRequirementsResponse(responseJson, pnrLocator, out ValidationResultHelper travelRequirementsValidation);
            Assert.True(travelRequirementsOk, string.Join(" | ", travelRequirementsValidation.Message));

            bool slicesOk = GraphQLAssertions.ValidateSlicesAndPassengerSliceRequirements(responseJson, out ValidationResultHelper slicesValidation);
            Assert.True(slicesOk, string.Join(" | ", slicesValidation.Message));

            string travelRequirementsId = responseJson.SelectToken("$.data.GetTravelRequirements.travelRequirementsId")?.ToString();
            Assert.False(travelRequirementsId.IsNullOrEmpty(), $"travelRequirementsId is missing from response: {responseJson}");

            string destinationCountryCode = PassportCountryCode.GBR.ToString();

            JArray passengers = responseJson.SelectToken("$.data.GetTravelRequirements.passengers") as JArray;

            Assert.NotNull(passengers);

            Assert.True(passengers.Count > 0, $"No passengers returned in GetTravelRequirements response: {responseJson}");

            foreach (JToken passenger in passengers)
            {
                string passengerId = passenger["passengerId"]?.ToString();
                string passengerGivenNames = passenger["givenNames"]?.ToString();
                string passengerSurname = passenger["surname"]?.ToString();
                string dateOfBirth = passenger["dateOfBirth"]?.ToString();

                Assert.True(!passengerId.IsNullOrEmpty() && !passengerGivenNames.IsNullOrEmpty() && !passengerSurname.IsNullOrEmpty() && !dateOfBirth.IsNullOrEmpty(),
                    $"Passenger details missing from travel requirements response: {passenger}");

                JArray passengerSliceRequirements = passenger["sliceRequirements"] as JArray;

                bool passportAlreadyProvided = passengerSliceRequirements != null
                    && passengerSliceRequirements.Count > 0
                    && passengerSliceRequirements.All(sr => sr?["passportProvided"]?.Value<bool?>() ?? false);

                if (passportAlreadyProvided)
                {
                    continue;
                }

                string passportNumber = PassportGenerator.GeneratePassport(PassportCountryCode.GBR.ToString());

                Assert.False(passportNumber.IsNullOrEmpty(), $"Passport number is missing for country {destinationCountryCode}.");

                string savePassportTravelRequirementsRequest = GraphQLRequestBuilder.SavePassportTravelRequirementsGraphQLRequest(
                    travelRequirementsId,
                    passengerId,
                    passengerGivenNames,
                    passengerSurname,
                    dateOfBirth,
                    passportNumber,
                    destinationCountryCode,
                    destinationCountryCode,
                    destinationCountryCode);

                HttpResponseMessage savePassportTravelRequirementsResponse = GraphQLEndpoint.GraphQLCheckInMobile(savePassportTravelRequirementsRequest).EnsureGQLResponseSuccess();

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportTravelRequirementsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                IEnumerable<JToken> passportProvidedTokens = savePassportResult.SelectTokens($"$.data.SavePassportTravelRequirements.passengers[?(@.passengerId == '{passengerId}')].sliceRequirements[*].passportProvided");

                Assert.True(passportProvidedTokens.Any(), $"No sliceRequirements passportProvided values returned for passengerId {passengerId}. Response: {savePassportResult}");

                bool passportProvided = passportProvidedTokens.All(t => t?.Value<bool?>() ?? false);

                Assert.True(passportProvided, $"Passport details not saved for request: {savePassportTravelRequirementsRequest}\nResponse: {savePassportResult}");

                IEnumerable<JToken> returnDateStatusTokens = savePassportResult.SelectTokens($"$..passengers[?(@.passengerId == '{passengerId}')].sliceRequirements[*].returnDateStatus");

                Assert.True(returnDateStatusTokens.Any(), $"No sliceRequirements returnDateStatus values returned for passengerId {passengerId}. Response: {savePassportResult}");

                Assert.True(returnDateStatusTokens.All(t => IsRequirementStatusValid(t?.ToString())), $"Return date status is not returned as expected: {savePassportResult}");

                IEnumerable<JToken> secondaryDocumentStatusTokens = savePassportResult.SelectTokens($"$..passengers[?(@.passengerId == '{passengerId}')].sliceRequirements[*].secondaryDocumentStatus");

                Assert.True(secondaryDocumentStatusTokens.Any(), $"No sliceRequirements secondaryDocumentStatus values returned for passengerId {passengerId}. Response: {savePassportResult}");

                Assert.True(secondaryDocumentStatusTokens.All(t => IsRequirementStatusValid(t?.ToString())), $"Secondary document status is not returned as expected: {savePassportResult}");

                string travelRequirementsSaveReturnDateRequest = GraphQLRequestBuilder.TravelRequirementsSaveReturnDateGraphQLRequest(passengerId, travelRequirementsId, DataGenerator.GetRandomDateWithinNextMonth());

                HttpResponseMessage travelRequirementsSaveReturnDateResponse = GraphQLEndpoint.GraphQLCheckInMobile(travelRequirementsSaveReturnDateRequest).EnsureGQLResponseSuccess();

                JToken travelRequirementsSaveReturnDateResult = JsonConvert.DeserializeObject<JToken>(travelRequirementsSaveReturnDateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                IEnumerable<JToken> savedReturnDateStatusTokens = travelRequirementsSaveReturnDateResult.SelectTokens($"$..passengers[?(@.passengerId == '{passengerId}')].sliceRequirements[*].returnDateStatus");

                Assert.True(savedReturnDateStatusTokens.Any(), $"No sliceRequirements returnDateStatus values returned for passengerId {passengerId}. Response: {travelRequirementsSaveReturnDateResult}");

                Assert.True(savedReturnDateStatusTokens.All(t => t != null && t.ToString().Is("PROVIDED")), $"Return date for travelRequirementsId: {travelRequirementsId} and passengerId: {passengerId} not saved. Response: {travelRequirementsSaveReturnDateResult}");

                IEnumerable<JToken> savedSecondaryDocumentStatusTokens = travelRequirementsSaveReturnDateResult.SelectTokens($"$..passengers[?(@.passengerId == '{passengerId}')].sliceRequirements[*].secondaryDocumentStatus");

                Assert.True(savedSecondaryDocumentStatusTokens.Any(), $"No sliceRequirements secondaryDocumentStatus values returned for passengerId {passengerId}. Response: {travelRequirementsSaveReturnDateResult}");

                Assert.True(savedSecondaryDocumentStatusTokens.All(t => IsRequirementStatusValid(t?.ToString())), $"Secondary document status is not returned as expected: {travelRequirementsSaveReturnDateResult}");
            }

            _output.WriteLine($"PNR used for this test: {pnrLocator}");
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void TravelRequirementsGraphQLTests_ReturnOnwardTicketRequired()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCITravelRequirementsPnrs(
                testScenarioName: "OneWayReturnDateScenarioRevenue", origin: LocationType.LHR.ToString(), destination: LocationType.BGI.ToString());

            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out string errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string pnrLocator = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];

            Assert.True(!pnrLocator.IsNullOrEmpty() && !givenNames.IsNullOrEmpty() && !surname.IsNullOrEmpty(),
                "PNR locator, given names, and surname should not be null or empty.");

            string travelRequirementsGraphQLRequest = GraphQLRequestBuilder.GetTravelRequirementsGraphQLRequest(pnrLocator, givenNames, surname);
            HttpResponseMessage travelRequirementsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(travelRequirementsGraphQLRequest).EnsureGQLResponseSuccess();
            JToken travelRequirementsResult = JsonConvert.DeserializeObject<JToken>(travelRequirementsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string travelRequirementsId = travelRequirementsResult.SelectToken("$.data.GetTravelRequirements.travelRequirementsId")?.ToString();
            Assert.False(travelRequirementsId.IsNullOrEmpty(), $"travelRequirementsId is missing from response: {travelRequirementsResult}");

            JToken passenger = travelRequirementsResult.SelectToken("$.data.GetTravelRequirements.passengers[0]");
            Assert.NotNull(passenger);

            string passengerId = passenger["passengerId"]?.ToString();
            string passengerGivenNames = passenger["givenNames"]?.ToString();
            string passengerSurname = passenger["surname"]?.ToString();
            string dateOfBirth = passenger["dateOfBirth"]?.ToString();

            Assert.True(!passengerId.IsNullOrEmpty() && !passengerGivenNames.IsNullOrEmpty() && !passengerSurname.IsNullOrEmpty() && !dateOfBirth.IsNullOrEmpty(),
                $"Passenger details are missing from travel requirements response: {passenger}");

            string britishPassportCountryCode = PassportCountryCode.GBR.ToString();
            string passportNumber = PassportGenerator.GeneratePassport(britishPassportCountryCode);

            string savePassportRequest = GraphQLRequestBuilder.SavePassportTravelRequirementsGraphQLRequest(
                travelRequirementsId,
                passengerId,
                passengerGivenNames,
                passengerSurname,
                dateOfBirth,
                passportNumber,
                britishPassportCountryCode,
                britishPassportCountryCode,
                britishPassportCountryCode);

            HttpResponseMessage savePassportResponse = GraphQLEndpoint.GraphQLCheckInMobile(savePassportRequest).EnsureGQLResponseSuccess();
            JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            IEnumerable<JToken> passportProvidedTokens = savePassportResult.SelectTokens(
                $"$.data.SavePassportTravelRequirements.passengers[?(@.passengerId == '{passengerId}')].sliceRequirements[*].passportProvided");
            IEnumerable<JToken> unhappyPassportTokens = savePassportResult.SelectTokens(
                $"$.data.SavePassportTravelRequirements.passengers[?(@.passengerId == '{passengerId}')].sliceRequirements[*].unhappyPassport");
            IEnumerable<JToken> travelRequirementErrors = savePassportResult.SelectTokens(
                $"$.data.SavePassportTravelRequirements.passengers[?(@.passengerId == '{passengerId}')].sliceRequirements[*].travelRequirementErrors[*]");

            Assert.True(passportProvidedTokens.Any(), $"No passportProvided values returned for passengerId {passengerId}. Response: {savePassportResult}");
            Assert.True(passportProvidedTokens.All(token => token?.Value<bool?>() == true),
                $"passportProvided should remain true when a return or onward ticket is required. Response: {savePassportResult}");

            Assert.True(unhappyPassportTokens.Any(), $"No unhappyPassport values returned for passengerId {passengerId}. Response: {savePassportResult}");
            Assert.True(unhappyPassportTokens.All(token => token?.Value<bool?>() == false),
                $"unhappyPassport should be false when a return or onward ticket is required. Response: {savePassportResult}");

            Assert.Contains(travelRequirementErrors, error =>
                error?["code"]?.ToString().Is("travelrequirement_030_011") == true);

            _output.WriteLine($"PNR used for this test: {pnrLocator}");
        }

        [Theory]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        [InlineData(true)]
        [InlineData(false)]
        public void TravelRequirementsGraphQLTests_EmergencyContacts_Success(bool applyToAllPassengers)
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCITravelRequirementsPnrs();

            string errorMessage;
            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string pnrLocator = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];

            Assert.True(!string.IsNullOrEmpty(pnrLocator) && !string.IsNullOrEmpty(givenNames) && !string.IsNullOrEmpty(surname), "PNRLocator, GivenNames and Surname should not be null or empty");

            string travelRequirementsGraphQLRequest = GraphQLRequestBuilder.GetTravelRequirementsGraphQLRequest(pnrLocator, givenNames, surname);

            HttpResponseMessage travelRequirementsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(travelRequirementsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken responseJson = JsonConvert.DeserializeObject<JToken>(travelRequirementsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            bool travelRequirementsOk = GraphQLAssertions.ValidateTravelRequirementsResponse(responseJson, pnrLocator, out ValidationResultHelper travelRequirementsValidation);
            Assert.True(travelRequirementsOk, string.Join(" | ", travelRequirementsValidation.Message));

            bool slicesOk = GraphQLAssertions.ValidateSlicesAndPassengerSliceRequirements(responseJson, out ValidationResultHelper slicesValidation);
            Assert.True(slicesOk, string.Join(" | ", slicesValidation.Message));

            string travelRequirementsId = responseJson.SelectToken("$.data.GetTravelRequirements.travelRequirementsId")?.ToString();
            Assert.False(travelRequirementsId.IsNullOrEmpty(), $"travelRequirementsId is missing from response: {responseJson}");

            string destinationCountryCode = responseJson.SelectTokens("$.data.GetTravelRequirements.slices[*].segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

            Assert.False(destinationCountryCode.IsNullOrEmpty(), $"destination.countryCode is missing from GetTravelRequirements response: {responseJson}");

            JArray passengers = responseJson.SelectToken("$.data.GetTravelRequirements.passengers") as JArray;

            Assert.NotNull(passengers);

            Assert.True(passengers.Count > 0, $"No passengers returned in GetTravelRequirements response: {responseJson}");

            foreach (JToken passenger in passengers)
            {
                string passengerId = passenger["passengerId"]?.ToString();
                string passengerGivenNames = passenger["givenNames"]?.ToString();
                string passengerSurname = passenger["surname"]?.ToString();
                string dateOfBirth = passenger["dateOfBirth"]?.ToString();

                Assert.True(!passengerId.IsNullOrEmpty() && !passengerGivenNames.IsNullOrEmpty() && !passengerSurname.IsNullOrEmpty() && !dateOfBirth.IsNullOrEmpty(),
                    $"Passenger details missing from travel requirements response: {passenger}");

                JArray passengerSliceRequirements = passenger["sliceRequirements"] as JArray;

                bool passportAlreadyProvided = passengerSliceRequirements != null
                    && passengerSliceRequirements.Count > 0
                    && passengerSliceRequirements.All(sr => sr?["passportProvided"]?.Value<bool?>() ?? false);

                if (passportAlreadyProvided)
                {
                    continue;
                }

                string passportNumber = PassportGenerator.GeneratePassport(destinationCountryCode);

                Assert.False(passportNumber.IsNullOrEmpty(), $"Passport number is missing for country {destinationCountryCode}.");

                string savePassportTravelRequirementsRequest = GraphQLRequestBuilder.SavePassportTravelRequirementsGraphQLRequest(
                    travelRequirementsId,
                    passengerId,
                    passengerGivenNames,
                    passengerSurname,
                    dateOfBirth,
                    passportNumber,
                    destinationCountryCode,
                    destinationCountryCode,
                    destinationCountryCode);

                HttpResponseMessage savePassportTravelRequirementsResponse = GraphQLEndpoint.GraphQLCheckInMobile(savePassportTravelRequirementsRequest).EnsureGQLResponseSuccess();

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportTravelRequirementsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                IEnumerable<JToken> passportProvidedTokens = savePassportResult.SelectTokens($"$.data.SavePassportTravelRequirements.passengers[?(@.passengerId == '{passengerId}')].sliceRequirements[*].passportProvided");

                Assert.True(passportProvidedTokens.Any(), $"No sliceRequirements passportProvided values returned for passengerId {passengerId}. Response: {savePassportResult}");

                bool passportProvided = passportProvidedTokens.All(t => t?.Value<bool?>() ?? false);

                Assert.True(passportProvided, $"Passport details not saved for request: {savePassportTravelRequirementsRequest}\nResponse: {savePassportResult}");
            }

            string saveEmergencyContactTravelRequirementsRequest = GraphQLRequestBuilder.SaveEmergencyContactTravelRequirementsGraphQLRequest(responseJson, travelRequirementsId, applyToAllPassengers);

            HttpResponseMessage saveEmergencyContactTravelRequirementsResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactTravelRequirementsRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactTravelRequirementsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactTravelRequirementsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.DoAllPassengersHaveEmergencyContact(saveEmergencyContactTravelRequirementsResult), $"Emergency contact details not saved for all passengers response: \n {saveEmergencyContactTravelRequirementsResult}");

            _output.WriteLine($"PNR used for this test: {pnrLocator}");
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void TravelRequirementsGraphQLTests_ContactTracing_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(origin: LocationType.LHR.ToString(), destination: "USA");

            string errorMessage;
            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string pnrLocator = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];

            Assert.True(!string.IsNullOrEmpty(pnrLocator) && !string.IsNullOrEmpty(givenNames) && !string.IsNullOrEmpty(surname), "PNRLocator, GivenNames and Surname should not be null or empty");

            string travelRequirementsGraphQLRequest = GraphQLRequestBuilder.GetTravelRequirementsGraphQLRequest(pnrLocator, givenNames, surname);

            HttpResponseMessage travelRequirementsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(travelRequirementsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken responseJson = JsonConvert.DeserializeObject<JToken>(travelRequirementsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            bool travelRequirementsOk = GraphQLAssertions.ValidateTravelRequirementsResponse(responseJson, pnrLocator, out ValidationResultHelper travelRequirementsValidation);

            Assert.True(travelRequirementsOk, string.Join(" | ", travelRequirementsValidation.Message));

            bool slicesOk = GraphQLAssertions.ValidateSlicesAndPassengerSliceRequirements(responseJson, out ValidationResultHelper slicesValidation);

            Assert.True(slicesOk, string.Join(" | ", slicesValidation.Message));

            string travelRequirementsId = responseJson.SelectToken("$.data.GetTravelRequirements.travelRequirementsId")?.ToString();

            Assert.False(travelRequirementsId.IsNullOrEmpty(), $"travelRequirementsId is missing from response: {responseJson}");

            string destinationCountryCode = PassportCountryCode.GBR.ToString();

            JArray passengers = responseJson.SelectToken("$.data.GetTravelRequirements.passengers") as JArray;

            Assert.NotNull(passengers);

            Assert.True(passengers.Count > 0, $"No passengers returned in GetTravelRequirements response: {responseJson}");

            foreach (JToken passenger in passengers)
            {
                string passengerId = passenger["passengerId"]?.ToString();
                string passengerGivenNames = passenger["givenNames"]?.ToString();
                string passengerSurname = passenger["surname"]?.ToString();
                string dateOfBirth = passenger["dateOfBirth"]?.ToString();

                Assert.True(!passengerId.IsNullOrEmpty() && !passengerGivenNames.IsNullOrEmpty() && !passengerSurname.IsNullOrEmpty() && !dateOfBirth.IsNullOrEmpty(),
                    $"Passenger details missing from travel requirements response: {passenger}");

                string passportNumber = PassportGenerator.GeneratePassport(destinationCountryCode);

                Assert.False(passportNumber.IsNullOrEmpty(), $"Passport number is missing for country {destinationCountryCode}.");

                string savePassportTravelRequirementsRequest = GraphQLRequestBuilder.SavePassportTravelRequirementsGraphQLRequest(
                    travelRequirementsId,
                    passengerId,
                    passengerGivenNames,
                    passengerSurname,
                    dateOfBirth,
                    passportNumber,
                    destinationCountryCode,
                    destinationCountryCode,
                    destinationCountryCode);

                HttpResponseMessage savePassportTravelRequirementsResponse = GraphQLEndpoint.GraphQLCheckInMobile(savePassportTravelRequirementsRequest).EnsureGQLResponseSuccess();

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportTravelRequirementsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                IEnumerable<JToken> passportProvidedTokens = savePassportResult.SelectTokens($"$.data.SavePassportTravelRequirements.passengers[?(@.passengerId == '{passengerId}')].sliceRequirements[*].passportProvided");

                Assert.True(passportProvidedTokens.Any(), $"No sliceRequirements passportProvided values returned for passengerId {passengerId}. Response: {savePassportResult}");

                bool passportProvided = passportProvidedTokens.All(t => t?.Value<bool?>() ?? false);

                Assert.True(passportProvided, $"Passport details not saved for request: {savePassportTravelRequirementsRequest}\nResponse: {savePassportResult}");
            }

            string saveEmergencyContactTravelRequirementsRequest = GraphQLRequestBuilder.SaveEmergencyContactTravelRequirementsGraphQLRequest(responseJson, travelRequirementsId);

            HttpResponseMessage saveEmergencyContactTravelRequirementsResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactTravelRequirementsRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactTravelRequirementsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactTravelRequirementsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.DoAllPassengersHaveEmergencyContact(saveEmergencyContactTravelRequirementsResult), $"Emergency contact details not saved for all passengers response: \n {saveEmergencyContactTravelRequirementsResult}");

            IEnumerable<JToken> contactTracingStatuses = saveEmergencyContactTravelRequirementsResult.SelectTokens("$.data.SaveEmergencyContactTravelRequirements.passengers[*].sliceRequirements[*].contactTracing");

            List<string> contactTracingStatusValues = contactTracingStatuses
                             .Select(t => t?.ToString()).Where(s => !string.IsNullOrEmpty(s))
                             .ToList();

            if (contactTracingStatusValues.Any() && contactTracingStatusValues.All(s => s == CheckInStatus.COMPLETE.ToString()))
                return;

            Assert.True(contactTracingStatusValues.Any(s => s == CheckInStatus.TO_COMPLETE.ToString()), $"No passengers have contactTracing status TO_COMPLETE. \nResponse: {saveEmergencyContactTravelRequirementsResult}");

            string travelRequirementsSaveContactTracingRequest = GraphQLRequestBuilder.TravelRequirementsSaveContactTracingGraphQLRequest(travelRequirementsId, saveEmergencyContactTravelRequirementsResult);

            JToken travelRequirementsSaveContactTracingRequestJson = JToken.Parse(travelRequirementsSaveContactTracingRequest);

            string sliceId = travelRequirementsSaveContactTracingRequestJson.SelectToken("$.variables.sliceId")?.ToString();

            List<string> passengerIds = travelRequirementsSaveContactTracingRequestJson
                .SelectTokens("$.variables.contactTracing[*].passengerId").Select(t => t?.ToString()).Where(id => !string.IsNullOrEmpty(id)).ToList();

            Assert.False(sliceId.IsNullOrEmpty(), $"Contact tracing request sliceId is missing. Request: {travelRequirementsSaveContactTracingRequestJson}");

            Assert.True(passengerIds.Any(), $"No passengerIds found in contact tracing request. Request: {travelRequirementsSaveContactTracingRequestJson}");

            HttpResponseMessage travelRequirementsSaveContactTracingResponse = GraphQLEndpoint.GraphQLCheckInMobile(travelRequirementsSaveContactTracingRequest).EnsureGQLResponseSuccess();

            JToken travelRequirementsSaveContactTracingResult = JsonConvert.DeserializeObject<JToken>(travelRequirementsSaveContactTracingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            foreach (string passengerId in passengerIds)
            {
                IEnumerable<JToken> statusTokens = travelRequirementsSaveContactTracingResult.SelectTokens(
                    $"$.data.SaveContactTracingTravelRequirements.passengers[?(@.passengerId == '{passengerId}')].sliceRequirements[?(@.sliceId == '{sliceId}')].contactTracing");

                string contactTracingStatus = statusTokens.Select(t => t?.ToString()).SingleOrDefault() ?? string.Empty;

                Assert.True(contactTracingStatus == CheckInStatus.COMPLETE.ToString(),
                    $"Contact tracing status not COMPLETE for passengerId {passengerId} sliceId {sliceId}. Response: {travelRequirementsSaveContactTracingResult}");
            }

            _output.WriteLine($"PNR used for this test: {pnrLocator}");
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void TravelRequirementsGraphQLTests_ContactTracing_NeededLater_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCITravelRequirementsPnrs(testScenarioName: "OneWayDirectFlightScenarioRevenue", origin: LocationType.LHR.ToString(), destination: "USA");

            string errorMessage;
            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string pnrLocator = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];

            Assert.True(!string.IsNullOrEmpty(pnrLocator) && !string.IsNullOrEmpty(givenNames) && !string.IsNullOrEmpty(surname), "PNRLocator, GivenNames and Surname should not be null or empty");

            string travelRequirementsGraphQLRequest = GraphQLRequestBuilder.GetTravelRequirementsGraphQLRequest(pnrLocator, givenNames, surname);

            HttpResponseMessage travelRequirementsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(travelRequirementsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken responseJson = JsonConvert.DeserializeObject<JToken>(travelRequirementsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            bool travelRequirementsOk = GraphQLAssertions.ValidateTravelRequirementsResponse(responseJson, pnrLocator, out ValidationResultHelper travelRequirementsValidation);

            Assert.True(travelRequirementsOk, string.Join(" | ", travelRequirementsValidation.Message));

            bool slicesOk = GraphQLAssertions.ValidateSlicesAndPassengerSliceRequirements(responseJson, out ValidationResultHelper slicesValidation);

            Assert.True(slicesOk, string.Join(" | ", slicesValidation.Message));

            string travelRequirementsId = responseJson.SelectToken("$.data.GetTravelRequirements.travelRequirementsId")?.ToString();

            Assert.False(travelRequirementsId.IsNullOrEmpty(), $"travelRequirementsId is missing from response: {responseJson}");

            string destinationCountryCode = PassportCountryCode.GBR.ToString();

            JArray passengers = responseJson.SelectToken("$.data.GetTravelRequirements.passengers") as JArray;

            Assert.NotNull(passengers);

            Assert.True(passengers.Count > 0, $"No passengers returned in GetTravelRequirements response: {responseJson}");

            foreach (JToken passenger in passengers)
            {
                string passengerId = passenger["passengerId"]?.ToString();
                string passengerGivenNames = passenger["givenNames"]?.ToString();
                string passengerSurname = passenger["surname"]?.ToString();
                string dateOfBirth = passenger["dateOfBirth"]?.ToString();

                Assert.True(!passengerId.IsNullOrEmpty() && !passengerGivenNames.IsNullOrEmpty() && !passengerSurname.IsNullOrEmpty() && !dateOfBirth.IsNullOrEmpty(),
                    $"Passenger details missing from travel requirements response: {passenger}");

                string passportNumber = PassportGenerator.GeneratePassport(destinationCountryCode);

                Assert.False(passportNumber.IsNullOrEmpty(), $"Passport number is missing for country {destinationCountryCode}.");

                string savePassportTravelRequirementsRequest = GraphQLRequestBuilder.SavePassportTravelRequirementsGraphQLRequest(
                    travelRequirementsId,
                    passengerId,
                    passengerGivenNames,
                    passengerSurname,
                    dateOfBirth,
                    passportNumber,
                    destinationCountryCode,
                    destinationCountryCode,
                    destinationCountryCode);

                HttpResponseMessage savePassportTravelRequirementsResponse = GraphQLEndpoint.GraphQLCheckInMobile(savePassportTravelRequirementsRequest).EnsureGQLResponseSuccess();

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportTravelRequirementsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                IEnumerable<JToken> passportProvidedTokens = savePassportResult.SelectTokens($"$.data.SavePassportTravelRequirements.passengers[?(@.passengerId == '{passengerId}')].sliceRequirements[*].passportProvided");

                Assert.True(passportProvidedTokens.Any(), $"No sliceRequirements passportProvided values returned for passengerId {passengerId}. Response: {savePassportResult}");

                bool passportProvided = passportProvidedTokens.All(t => t?.Value<bool?>() ?? false);

                Assert.True(passportProvided, $"Passport details not saved for request: {savePassportTravelRequirementsRequest}\nResponse: {savePassportResult}");
            }

            string saveEmergencyContactTravelRequirementsRequest = GraphQLRequestBuilder.SaveEmergencyContactTravelRequirementsGraphQLRequest(responseJson, travelRequirementsId);

            HttpResponseMessage saveEmergencyContactTravelRequirementsResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactTravelRequirementsRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactTravelRequirementsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactTravelRequirementsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.DoAllPassengersHaveEmergencyContact(saveEmergencyContactTravelRequirementsResult), $"Emergency contact details not saved for all passengers response: \n {saveEmergencyContactTravelRequirementsResult}");

            IEnumerable<JToken> contactTracingStatuses = saveEmergencyContactTravelRequirementsResult.SelectTokens("$.data.SaveEmergencyContactTravelRequirements.passengers[*].sliceRequirements[*].contactTracing");

            Assert.True(contactTracingStatuses.Any(t => t?.ToString() == CheckInStatus.NEEDED_LATER.ToString()), $"ContactTracing status is needed later. \nResponse: {saveEmergencyContactTravelRequirementsResult}");

            _output.WriteLine($"PNR used for this test: {pnrLocator}");
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void TravelRequirementsGraphQLTests_SaveSecondaryDocument_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCITravelRequirementsPnrs(origin: LocationType.LHR.ToString(), destination: "GOVTFORMSROUTES", cutOffWindowTime: null);

            string errorMessage;
            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string pnrLocator = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];

            Assert.True(!string.IsNullOrEmpty(pnrLocator) && !string.IsNullOrEmpty(givenNames) && !string.IsNullOrEmpty(surname), "PNRLocator, GivenNames and Surname should not be null or empty");

            string travelRequirementsGraphQLRequest = GraphQLRequestBuilder.GetTravelRequirementsGraphQLRequest(pnrLocator, givenNames, surname);

            HttpResponseMessage travelRequirementsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(travelRequirementsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken responseJson = JsonConvert.DeserializeObject<JToken>(travelRequirementsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            bool travelRequirementsOk = GraphQLAssertions.ValidateTravelRequirementsResponse(responseJson, pnrLocator, out ValidationResultHelper travelRequirementsValidation);

            Assert.True(travelRequirementsOk, string.Join(" | ", travelRequirementsValidation.Message));

            bool slicesOk = GraphQLAssertions.ValidateSlicesAndPassengerSliceRequirements(responseJson, out ValidationResultHelper slicesValidation);

            Assert.True(slicesOk, string.Join(" | ", slicesValidation.Message));

            string travelRequirementsId = responseJson.SelectToken("$.data.GetTravelRequirements.travelRequirementsId")?.ToString();
            Assert.False(travelRequirementsId.IsNullOrEmpty(), $"travelRequirementsId is missing from response: {responseJson}");

            JArray passengers = responseJson.SelectToken("$.data.GetTravelRequirements.passengers") as JArray;

            Assert.NotNull(passengers);

            Assert.True(passengers.Count > 0, $"No passengers returned in GetTravelRequirements response: {responseJson}");

            bool secondaryDocumentSaved = false;
            JToken lastRefreshedResponseJson = responseJson;

            foreach (JToken passenger in passengers)
            {
                string passengerId = passenger["passengerId"]?.ToString();
                string passengerGivenNames = passenger["givenNames"]?.ToString();
                string passengerSurname = passenger["surname"]?.ToString();
                string dateOfBirth = passenger["dateOfBirth"]?.ToString();

                Assert.True(!passengerId.IsNullOrEmpty() && !passengerGivenNames.IsNullOrEmpty() && !passengerSurname.IsNullOrEmpty() && !dateOfBirth.IsNullOrEmpty(),
                    $"Passenger details missing from travel requirements response: {passenger}");

                // Ensure a passport is provided for this passenger before attempting to save a secondary document for the same passenger.
                JArray passengerSliceRequirements = passenger["sliceRequirements"] as JArray;

                bool passportAlreadyProvided = passengerSliceRequirements != null
                    && passengerSliceRequirements.Count > 0
                    && passengerSliceRequirements.All(sr => sr?["passportProvided"]?.Value<bool?>() ?? false);

                if (!passportAlreadyProvided)
                {
                    string passportCountryCode = PassportCountryCode.GBR.ToString();
                    string passportNumber = PassportGenerator.GeneratePassport(passportCountryCode);

                    Assert.False(passportNumber.IsNullOrEmpty(), $"Passport number is missing for country {passportCountryCode}.");

                    string savePassportTravelRequirementsRequest = GraphQLRequestBuilder.SavePassportTravelRequirementsGraphQLRequest(
                        travelRequirementsId, passengerId, passengerGivenNames,
                        passengerSurname, dateOfBirth, passportNumber,
                        passportCountryCode, passportCountryCode, passportCountryCode);

                    HttpResponseMessage savePassportTravelRequirementsResponse = GraphQLEndpoint.GraphQLCheckInMobile(savePassportTravelRequirementsRequest).EnsureGQLResponseSuccess();

                    JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportTravelRequirementsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                    IEnumerable<JToken> passportProvidedTokens = savePassportResult.SelectTokens(
                        $"$.data.SavePassportTravelRequirements.passengers[?(@.passengerId == '{passengerId}')].sliceRequirements[*].passportProvided");

                    Assert.True(passportProvidedTokens.Any(), $"No sliceRequirements passportProvided values returned for passengerId {passengerId}. Response: {savePassportResult}");

                    bool passportProvided = passportProvidedTokens.All(t => t?.Value<bool?>() ?? false);

                    Assert.True(passportProvided, $"Passport details not saved for request: {savePassportTravelRequirementsRequest}\nResponse: {savePassportResult}");
                }

                string refreshedTravelRequirementsRequest = GraphQLRequestBuilder.GetTravelRequirementsGraphQLRequest(pnrLocator, givenNames, surname);

                HttpResponseMessage refreshedTravelRequirementsResponse = GraphQLEndpoint.GraphQLCheckInMobile(refreshedTravelRequirementsRequest).EnsureGQLResponseSuccess();

                JToken refreshedResponseJson = JsonConvert.DeserializeObject<JToken>(refreshedTravelRequirementsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                lastRefreshedResponseJson = refreshedResponseJson;

                IEnumerable<JToken> refreshedPassengerTokens = refreshedResponseJson.SelectTokens($"$.data.GetTravelRequirements.passengers[?(@.passengerId == '{passengerId}')]");

                JToken refreshedPassenger = refreshedPassengerTokens.FirstOrDefault();

                if (refreshedPassenger == null)
                {
                    continue;
                }

                JArray sliceRequirements = refreshedPassenger["sliceRequirements"] as JArray;

                if (sliceRequirements == null || sliceRequirements.Count == 0)
                {
                    continue;
                }

                foreach (JToken sliceRequirement in sliceRequirements)
                {
                    string sliceId = sliceRequirement["sliceId"]?.ToString();

                    if (sliceId.IsNullOrEmpty())
                    {
                        continue;
                    }

                    string secondaryDocumentStatusBeforeSave = sliceRequirement["secondaryDocumentStatus"]?.ToString();

                    if (secondaryDocumentStatusBeforeSave.IsNullOrEmpty() || !secondaryDocumentStatusBeforeSave.Is("Required"))
                    {
                        continue;
                    }

                    JArray secondaryDocuments = sliceRequirement["secondaryDocuments"] as JArray;

                    if (secondaryDocuments == null || secondaryDocuments.Count == 0)
                    {
                        continue;
                    }

                    foreach (JToken secondaryDocument in secondaryDocuments)
                    {
                        string countryCode = secondaryDocument["countryCode"]?.ToString();
                        JArray applicationOptions = secondaryDocument["secondaryDocumentApplicationOptions"] as JArray;

                        if (applicationOptions == null || applicationOptions.Count == 0)
                        {
                            continue;
                        }

                        JToken? secondaryDocumentApplicationOption = applicationOptions.FirstOrDefault(option =>
                            option?["secondaryDocumentId"]?.ToString().IsNullOrEmpty() == false);

                        string? secondaryDocumentId = secondaryDocumentApplicationOption?["secondaryDocumentId"]?.ToString();

                        if (secondaryDocumentId.IsNullOrEmpty())
                        {
                            continue;
                        }

                        if (countryCode.IsNullOrEmpty())
                        {
                            countryCode = PassportCountryCode.GBR.ToString();
                        }

                        string saveSecondaryDocumentTravelRequirementsRequest = GraphQLRequestBuilder.SaveSecondaryDocumentTravelRequirementsGraphQLRequest(
                            travelRequirementsId, passengerId, sliceId, secondaryDocumentId, countryCode);

                        HttpResponseMessage saveSecondaryDocumentTravelRequirementsResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveSecondaryDocumentTravelRequirementsRequest).EnsureGQLResponseSuccess();

                        JToken saveSecondaryDocumentResult = JsonConvert.DeserializeObject<JToken>(saveSecondaryDocumentTravelRequirementsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                        IEnumerable<JToken> secondaryDocumentStatusTokens = saveSecondaryDocumentResult.SelectTokens(
                            $"$.data.SaveSecondaryDocumentTravelRequirements.passengers[?(@.passengerId == '{passengerId}')].sliceRequirements[?(@.sliceId == '{sliceId}')].secondaryDocumentStatus");

                        Assert.True(secondaryDocumentStatusTokens.Any(),
                            $"No secondaryDocumentStatus returned for passengerId {passengerId}, sliceId {sliceId}. Response: {saveSecondaryDocumentResult}");

                        string secondaryDocumentStatus = secondaryDocumentStatusTokens.First().ToString();

                        Assert.False(secondaryDocumentStatus.IsNullOrEmpty(), $"secondaryDocumentStatus is missing in response: {saveSecondaryDocumentResult}");

                        Assert.False(secondaryDocumentStatus.Is(CheckInStatus.REQUIRED.ToString()),
                            $"Secondary document was not saved for request: {saveSecondaryDocumentTravelRequirementsRequest}\nResponse: {saveSecondaryDocumentResult}");

                        secondaryDocumentSaved = true;
                        break;
                    }

                    if (secondaryDocumentSaved)
                    {
                        break;
                    }
                }

                if (secondaryDocumentSaved)
                {
                    break;
                }
            }

            Assert.True(secondaryDocumentSaved, $"No secondary document options were found to save for travelRequirementsId {travelRequirementsId}. Last refreshed response: {lastRefreshedResponseJson}");

            _output.WriteLine($"PNR used for this test: {pnrLocator}");
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void TravelRequirementsGraphQLTests_GovtForms_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCITravelRequirementsPnrs(origin: LocationType.LHR.ToString(), destination: "GOVTFORMSROUTES", cutOffWindowTime: null);

            string errorMessage;
            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string pnrLocator = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];

            Assert.True(!string.IsNullOrEmpty(pnrLocator) && !string.IsNullOrEmpty(givenNames) && !string.IsNullOrEmpty(surname), "PNRLocator, GivenNames and Surname should not be null or empty");

            string travelRequirementsGraphQLRequest = GraphQLRequestBuilder.GetTravelRequirementsGraphQLRequest(pnrLocator, givenNames, surname);

            HttpResponseMessage travelRequirementsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(travelRequirementsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken responseJson = JsonConvert.DeserializeObject<JToken>(travelRequirementsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            bool travelRequirementsOk = GraphQLAssertions.ValidateTravelRequirementsResponse(responseJson, pnrLocator, out ValidationResultHelper travelRequirementsValidation);

            Assert.True(travelRequirementsOk, string.Join(" | ", travelRequirementsValidation.Message));

            bool slicesOk = GraphQLAssertions.ValidateSlicesAndPassengerSliceRequirements(responseJson, out ValidationResultHelper slicesValidation);

            Assert.True(slicesOk, string.Join(" | ", slicesValidation.Message));

            string travelRequirementsId = responseJson.SelectToken("$.data.GetTravelRequirements.travelRequirementsId")?.ToString();

            Assert.False(travelRequirementsId.IsNullOrEmpty(), $"travelRequirementsId is missing from response: {responseJson}");

            string destinationCountryCode = PassportCountryCode.GBR.ToString();

            JArray passengers = responseJson.SelectToken("$.data.GetTravelRequirements.passengers") as JArray;

            Assert.NotNull(passengers);

            Assert.True(passengers.Count > 0, $"No passengers returned in GetTravelRequirements response: {responseJson}");

            bool governmentFormSubmitted = false;

            foreach (JToken passenger in passengers)
            {
                string passengerId = passenger["passengerId"]?.ToString();
                string passengerGivenNames = passenger["givenNames"]?.ToString();
                string passengerSurname = passenger["surname"]?.ToString();
                string dateOfBirth = passenger["dateOfBirth"]?.ToString();

                Assert.True(!passengerId.IsNullOrEmpty() && !passengerGivenNames.IsNullOrEmpty() && !passengerSurname.IsNullOrEmpty() && !dateOfBirth.IsNullOrEmpty(),
                    $"Passenger details missing from travel requirements response: {passenger}");

                string passportNumber = PassportGenerator.GeneratePassport(destinationCountryCode);

                Assert.False(passportNumber.IsNullOrEmpty(), $"Passport number is missing for country {destinationCountryCode}.");

                string savePassportTravelRequirementsRequest = GraphQLRequestBuilder.SavePassportTravelRequirementsGraphQLRequest(travelRequirementsId, passengerId, passengerGivenNames,
                    passengerSurname, dateOfBirth, passportNumber, destinationCountryCode, destinationCountryCode, destinationCountryCode);

                HttpResponseMessage savePassportTravelRequirementsResponse = GraphQLEndpoint.GraphQLCheckInMobile(savePassportTravelRequirementsRequest).EnsureGQLResponseSuccess();

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportTravelRequirementsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                IEnumerable<JToken> passportProvidedTokens = savePassportResult.SelectTokens($"$.data.SavePassportTravelRequirements.passengers[?(@.passengerId == '{passengerId}')].sliceRequirements[*].passportProvided");

                Assert.True(passportProvidedTokens.Any(), $"No sliceRequirements passportProvided values returned for passengerId {passengerId}. Response: {savePassportResult}");

                bool passportProvided = passportProvidedTokens.All(t => t?.Value<bool?>() ?? false);

                Assert.True(passportProvided, $"Passport details not saved for request: {savePassportTravelRequirementsRequest}\nResponse: {savePassportResult}");

                string saveGovernmentFormTravelRequirementsRequest;
                string sliceId;
                string governmentFormId;

                bool canBuildGovernmentFormRequest = GraphQLRequestBuilder.TryBuildSaveGovernmentFormTravelRequirementsGraphQLRequestFromPassportResponse(
                    savePassportResult, travelRequirementsId, passengerId, out saveGovernmentFormTravelRequirementsRequest, out sliceId, out governmentFormId);

                if (!canBuildGovernmentFormRequest)
                {
                    continue;
                }

                HttpResponseMessage saveGovernmentFormTravelRequirementsResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveGovernmentFormTravelRequirementsRequest).EnsureGQLResponseSuccess();

                JToken saveGovernmentFormTravelRequirementsResult = JsonConvert.DeserializeObject<JToken>(saveGovernmentFormTravelRequirementsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                IEnumerable<JToken> savedGovernmentFormStatuses = saveGovernmentFormTravelRequirementsResult.SelectTokens(
                    $"$.data.SaveGovernmentFormTravelRequirements.passengers[?(@.passengerId == '{passengerId}')].sliceRequirements[?(@.sliceId == '{sliceId}')].governmentForms[?(@.id == '{governmentFormId}')].status");

                Assert.True(savedGovernmentFormStatuses.Any(), $"No governmentForms status returned for passengerId {passengerId}, sliceId {sliceId}, governmentFormId {governmentFormId}. Response: {saveGovernmentFormTravelRequirementsResult}");

                string savedGovernmentFormStatus = savedGovernmentFormStatuses.First().ToString();

                Assert.False(savedGovernmentFormStatus.IsNullOrEmpty(), $"GovernmentForms status is missing after SaveGovernmentFormTravelRequirements. Response: {saveGovernmentFormTravelRequirementsResult}");

                Assert.NotEqual(CheckInStatus.REQUIRED.ToString(), savedGovernmentFormStatus);

                governmentFormSubmitted = true;
            }

            Assert.True(governmentFormSubmitted,
                $"No government form was available to submit for travelRequirementsId {travelRequirementsId}. Response: {responseJson}");

            _output.WriteLine($"PNR used for this test: {pnrLocator}");
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void TravelRequirementsGraphQLTests_WithVendorAndGovernmentOptions_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCITravelRequirementsPnrs(
                testScenarioName: "OneWayDirectFlightScenarioRevenue",
                origin: LocationType.LHR.ToString(),
                destination: "USA");

            string errorMessage;

            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string pnrLocator = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];

            JToken responseJson = GetTravelRequirementsAfterEnsuringPassportProvided(pnrLocator, givenNames, surname, PassportCountryCode.GBR.ToString());

            List<JToken> secondaryDocuments = GetAllSecondaryDocuments(responseJson);

            JToken? usEsta = secondaryDocuments.FirstOrDefault(d =>
                d?["displayName"]?.ToString().Equals("ESTA", StringComparison.OrdinalIgnoreCase) == true &&
                d?["countryCode"]?.ToString().Equals("US", StringComparison.OrdinalIgnoreCase) == true);

            Assert.NotNull(usEsta);

            Assert.Equal("ETA", usEsta!["documentType"]?.ToString());

            JArray? options = usEsta!["secondaryDocumentApplicationOptions"] as JArray;

            Assert.NotNull(options);

            Assert.Equal(2, options!.Count);

            (JToken vendor, JToken government) = AssertHasVendorAndGovernmentOptions(options);

            Assert.True(vendor["secondaryDocumentId"]?.ToString().IsNullOrEmpty() == false, $"VENDOR secondaryDocumentId missing. Requirement: {usEsta}");

            Assert.Equal("https://esta.cbp.dhs.gov/", government["url"]?.ToString());

            string? governmentMessage = government["message"]?.ToString();

            Assert.False(governmentMessage.IsNullOrWhiteSpace(), $"GOVERNMENT message missing. Requirement: {usEsta}");

            Assert.Contains("ESTA", governmentMessage!, StringComparison.OrdinalIgnoreCase);

            _output.WriteLine($"PNR used for this test: {pnrLocator}");
        }

        private static JToken GetTravelRequirementsAfterEnsuringPassportProvided(string pnrLocator, string givenNames, string surname, string passportCountryCode)
        {
            string travelRequirementsGraphQLRequest = GraphQLRequestBuilder.GetTravelRequirementsGraphQLRequest(pnrLocator, givenNames, surname);

            HttpResponseMessage travelRequirementsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(travelRequirementsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken responseJson = JsonConvert.DeserializeObject<JToken>(travelRequirementsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string travelRequirementsId = responseJson.SelectToken("$.data.GetTravelRequirements.travelRequirementsId")?.ToString();

            Assert.False(travelRequirementsId.IsNullOrEmpty(), $"travelRequirementsId is missing from response: {responseJson}");

            JArray? passengers = responseJson.SelectToken("$.data.GetTravelRequirements.passengers") as JArray;

            Assert.NotNull(passengers);

            Assert.True(passengers.Count > 0, $"No passengers returned in GetTravelRequirements response: {responseJson}");

            foreach (JToken passenger in passengers)
            {
                string? passengerId = passenger["passengerId"]?.ToString();
                string? passengerGivenNames = passenger["givenNames"]?.ToString();
                string? passengerSurname = passenger["surname"]?.ToString();
                string? dateOfBirth = passenger["dateOfBirth"]?.ToString();

                Assert.True(!passengerId.IsNullOrEmpty() && !passengerGivenNames.IsNullOrEmpty() && !passengerSurname.IsNullOrEmpty() && !dateOfBirth.IsNullOrEmpty(),
                    $"Passenger details missing from travel requirements response: {passenger}");

                JArray? passengerSliceRequirements = passenger["sliceRequirements"] as JArray;

                bool passportAlreadyProvided = passengerSliceRequirements != null
                    && passengerSliceRequirements.Count > 0
                    && passengerSliceRequirements.All(sr => sr?["passportProvided"]?.Value<bool?>() ?? false);

                if (passportAlreadyProvided)
                {
                    continue;
                }

                string passportNumber = PassportGenerator.GeneratePassport(passportCountryCode);

                Assert.False(passportNumber.IsNullOrEmpty(), $"Passport number is missing for country {passportCountryCode}.");

                string savePassportTravelRequirementsRequest = GraphQLRequestBuilder.SavePassportTravelRequirementsGraphQLRequest(
                    travelRequirementsId,
                    passengerId!,
                    passengerGivenNames!,
                    passengerSurname!,
                    dateOfBirth!,
                    passportNumber,
                    passportCountryCode,
                    passportCountryCode,
                    passportCountryCode);

                HttpResponseMessage savePassportTravelRequirementsResponse = GraphQLEndpoint.GraphQLCheckInMobile(savePassportTravelRequirementsRequest).EnsureGQLResponseSuccess();

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportTravelRequirementsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                IEnumerable<JToken> passportProvidedTokens = savePassportResult.SelectTokens(
                    $"$.data.SavePassportTravelRequirements.passengers[?(@.passengerId == '{passengerId}')].sliceRequirements[*].passportProvided");

                Assert.True(passportProvidedTokens.Any(), $"No sliceRequirements passportProvided values returned for passengerId {passengerId}. Response: {savePassportResult}");

                Assert.True(passportProvidedTokens.All(t => t?.Value<bool?>() ?? false), $"Passport details not saved. Request: {savePassportTravelRequirementsRequest}\nResponse: {savePassportResult}");
            }

            // Refresh once passport details are available.
            string refreshedRequest = GraphQLRequestBuilder.GetTravelRequirementsGraphQLRequest(pnrLocator, givenNames, surname);

            HttpResponseMessage refreshedResponse = GraphQLEndpoint.GraphQLCheckInMobile(refreshedRequest).EnsureGQLResponseSuccess();

            return JsonConvert.DeserializeObject<JToken>(refreshedResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        }

        private static List<JToken> GetAllSecondaryDocuments(JToken responseJson)
        {
            return responseJson.SelectTokens("$.data.GetTravelRequirements.passengers[*].sliceRequirements[*].secondaryDocuments[*]")
                .Where(t => t != null)
                .ToList();
        }

        private static (JToken vendor, JToken government) AssertHasVendorAndGovernmentOptions(JArray options)
        {
            List<JToken> vendorOptions = options.Where(o => o?["applicationType"]?.ToString().Equals("VENDOR", StringComparison.OrdinalIgnoreCase) == true).ToList();

            List<JToken> governmentOptions = options.Where(o => o?["applicationType"]?.ToString().Equals("GOVERNMENT", StringComparison.OrdinalIgnoreCase) == true).ToList();

            Assert.Single(vendorOptions);

            Assert.Single(governmentOptions);

            return (vendorOptions[0], governmentOptions[0]);
        }
    }
}
