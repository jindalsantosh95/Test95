using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.TestDataConfig.PaymentCards;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.OCI.Constants;
using VAA.EAPI.Automation.OCI.Helpers;
using VAA.EAPI.Automation.OCI.Requests.Endpoints;
using MyAccountGraphQL = VAA.EAPI.Automation.MyAccount.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.OCI.Tests.GraphQL
{
    [Collection("BaggageGraphQLTests")]
    public class CheckInBaggageGraphQL : IClassFixture<MemoryStoreFixture>
    {
        private TestConfig _testConfig;
        public static readonly string BagsPaymentCardUsdCurrency = Vault.Default.GetSecretValue("Bags-PaymentCard-USD");
        public static readonly string BagsPaymentCardAllCurrency = Vault.Default.GetSecretValue("Bags-PaymentCard-AllCurrencies");
        private readonly MemoryStoreFixture memoryStoreFixture;
        public static string env = Config.Environment;

        public CheckInBaggageGraphQL(MemoryStoreFixture memoryStoreFixture)
        {
            _testConfig = new TestConfig();
            this.memoryStoreFixture = memoryStoreFixture;
            this.memoryStoreFixture.SetOnce("MYA-Password", () => Vault.Default.GetSecretValue("MYA-Password"));
        }

        [Theory]
        [InlineData(BaggageSelectionType.FreePlusPaid, LocationType.LHR, 4)]
        [InlineData(BaggageSelectionType.PaidOnly, LocationType.LHR, 5)]
        [InlineData(BaggageSelectionType.FreePlusPaid, LocationType.JFK, 6)]
        [InlineData(BaggageSelectionType.PaidOnly, LocationType.JFK, 7)]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void BaggageGraphQLTests_PaidBaggage_Success(BaggageSelectionType baggageSelectionType, LocationType origin, int baggageCount)
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(origin: origin.ToString(), airline: FlightType.VS.ToString());

            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out string errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string partitionKey = pnrDetails["partitionKey"];
            string testScenario = pnrDetails["testScenario"];
            string pnrLocator = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];
            string originCode = pnrDetails["origin"];
            string currencyCode = pnrDetails["currency"];
            int passengersCount = Convert.ToInt32(pnrDetails["passengersCount"]);

            Assert.True(!string.IsNullOrEmpty(pnrLocator) && !string.IsNullOrEmpty(originCode), "PNR and OriginCode should not be null or empty");

            string checkInGraphQLRequest = GraphQLRequestBuilder.CheckInGraphQLRequest(pnrLocator, givenNames, surname, originCode);

            HttpResponseMessage checkInGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(checkInGraphQLRequest).EnsureGQLResponseSuccess();

            JToken checkInResult = JsonConvert.DeserializeObject<JToken>(checkInGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            bool isCheckInAvailable = checkInResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

            Assert.True(isCheckInAvailable, $"Check-in is not available for request : {checkInGraphQLRequest} and \n response is : {checkInResult.ToString()}");

            // Extract check-in ID
            string checkInId = checkInResult?.SelectToken("$..checkInId")?.ToString();

            Assert.True(!checkInId.IsNullOrEmpty(), $"Check-in response is invalid : {checkInResult} for PNR : {pnrLocator}");

            // Getting passenger IDs as a list
            List<string> selectedPassengerIds = OCIHelper.GetPassengersListFromCheckInResponse(checkInGraphQLResponse);

            int checkInPassengerCount = selectedPassengerIds.Count;

            Assert.True(passengersCount == checkInPassengerCount, $"Passenger count mismatch. Expected: {passengersCount}, Actual: {checkInPassengerCount}, \n Check-in Response:{checkInResult.ToString()}");

            if (checkInPassengerCount > 1)
            {
                string selectPassengersGraphQLRequest = GraphQLRequestBuilder.SelectPassengersGraphQLRequest(checkInId, selectedPassengerIds);

                HttpResponseMessage selectPassengersGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(selectPassengersGraphQLRequest).EnsureGQLResponseSuccess();

                JToken selectPassengersResult = JsonConvert.DeserializeObject<JToken>(selectPassengersGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                checkInId = selectPassengersResult?.SelectToken("$..checkInId")?.ToString();
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

            JToken savePassportResult = null;

            foreach (JToken passenger in passengers)
            {
                string passengerGivenNames = passenger["givenNames"]?.ToString();
                string passengerSurname = passenger["surname"]?.ToString();
                string passengerId = passenger["passengerId"]?.ToString();
                string dateOfBirth = passenger["dateOfBirth"]?.ToString();

                // Generate passport number based on destination country
                string passportNumber = PassportGenerator.GeneratePassport(destinationCountryCode);

                Assert.False(string.IsNullOrEmpty(passportNumber), $"Passport number is missing for country {destinationCountryCode}.");

                string savePassportGraphQLRequest = GraphQLRequestBuilder.SavePassportGraphQLRequest(checkInId, passengerId, passengerGivenNames, passengerSurname, dateOfBirth, passportNumber, destinationCountryCode, destinationCountryCode, destinationCountryCode);

                HttpResponseMessage savePassportGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(savePassportGraphQLRequest).EnsureGQLResponseSuccess();

                savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            checkInId = savePassportResult?.SelectToken("$..checkInId")?.ToString();

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            checkInId = saveEmergencyContactsResult?.SelectToken("$..checkInId")?.ToString();

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            checkInId = saveContactTracingResult?.SelectToken("$..checkInId")?.ToString();

            string getBaggageRequest = GraphQLRequestBuilder.GetBaggageGraphQLRequest(checkInId: checkInId, hidePaidBaggage: false);

            HttpResponseMessage getBaggageResponse = GraphQLEndpoint.GraphQLCheckInMobile(getBaggageRequest).EnsureGQLResponseSuccess();

            JToken getBaggageResult = JsonConvert.DeserializeObject<JToken>(getBaggageResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            bool canChangeBags = getBaggageResult?.SelectToken("$.data.BaggageAllowance.passengers[0].canChangeBags")?.Value<bool>() ?? false;

            Assert.True(canChangeBags, $"Expected to be able to change bags, but either they were already added or the delta did not return any offers.\nResult: {getBaggageResult}");

            checkInId = getBaggageResult?.SelectToken("$..checkInId")?.ToString();

            string saveBaggageRequest = GraphQLRequestBuilder.SaveBaggageGraphQLRequest(getBaggageResponse, checkInId, baggageSelectionType, baggageCount);

            JToken saveBaggageRequestJson = JsonConvert.DeserializeObject<JToken>(saveBaggageRequest)!;

            bool hasPaidSelectionForRequestedCount = saveBaggageRequestJson
                .SelectTokens("$.variables.request.selectedBaggageOffer[?(@.totalPrice > 0)]")
                .Any(o => o["baggageCount"]?.Value<int>() == baggageCount);

            Assert.True(hasPaidSelectionForRequestedCount, $"SaveBaggage request did not include a paid baggage selection with baggageCount={baggageCount}. Request: {saveBaggageRequest}");

            HttpResponseMessage saveBaggageResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveBaggageRequest).EnsureGQLResponseSuccess();

            JToken saveBaggageResult = JsonConvert.DeserializeObject<JToken>(saveBaggageResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            checkInId = saveBaggageResult?.SelectToken("$..checkInId")?.ToString();

            string paymentEligibilityCheckRequest = GraphQLRequestBuilder.PaymentEligibilityCheckGraphQLRequest(checkInId);

            HttpResponseMessage paymentEligibilityCheckResponse = GraphQLEndpoint.GraphQLCheckInMobile(paymentEligibilityCheckRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQLAssertions.ValidatePaymentEligibilityCheckResponse(paymentEligibilityCheckResponse, baggageSelectionType), $"Payment eligibility check response is invalid: \n{paymentEligibilityCheckResponse.ReadResponseAsString()}");

            JToken paymentEligibilityCheckResult = JsonConvert.DeserializeObject<JToken>(paymentEligibilityCheckResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string paymentReferenceId = paymentEligibilityCheckResult?.SelectToken("$..paymentReferenceId")?.ToString();

            string hppLinkUrl = paymentEligibilityCheckResult?.SelectToken("$..hPPLink")?.ToString();

            using NavigateToHostedPaymentPage enterHppCardDetails = new NavigateToHostedPaymentPage(hppLinkUrl);

            _testConfig = new TestConfig();

            string cardNumber = currencyCode == "USD" ? BagsPaymentCardUsdCurrency : BagsPaymentCardAllCurrency;

            _testConfig.Book.Payment = new Payment
            {
                CardDetails = new CardDetails
                {
                    CardNumber = cardNumber
                }
            };

            enterHppCardDetails.SubmitHppCardDetails(_testConfig);

            string confirmBaggageGraphQLRequest = GraphQLRequestBuilder.ConfirmBaggageGraphQLRequest(checkInId, paymentReferenceId);

            HttpResponseMessage confirmBaggageResponse = GraphQLEndpoint.GraphQLCheckInMobile(confirmBaggageGraphQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQLAssertions.ValidateTotalPriceAndCurrencyForSelectedBaggage(saveBaggageRequest, confirmBaggageResponse), $"Confirm baggage response is invalid: \n{confirmBaggageResponse.ReadResponseAsString()} for baggage request: \n{saveBaggageRequest}");

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            IEnumerable<JToken> qrCodes = generateBoardingPassResult.SelectTokens("$..boardingPasses[*].qrCode");

            string qrCode = qrCodes.FirstOrDefault()?.ToString();

            Assert.True(!qrCode.IsNullOrEmpty(), $"Test Scenario: {testScenario} \n Boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            IEnumerable<JToken> retrieveBoardingPassQRCodes = retrieveBoardingPassResult.SelectTokens("$..boardingPasses[*].qrCode");

            string retrieveBoardingPassQRCode = retrieveBoardingPassQRCodes.FirstOrDefault()?.ToString();

            Assert.True(!retrieveBoardingPassQRCode.IsNullOrEmpty(), $"Test Scenario: {testScenario} \n Boarding pass not retuned for the request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus, baggageSelectionType.ToString());
        }

        [Theory]
        [InlineData(CabinType.UpperClass)]
        [InlineData(CabinType.EconomyClassic)]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void BaggageGraphQLTests_FreeBagsOnly_Success(CabinType cabinType)
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(cabinType: cabinType.GetEnumDescription(), airline: FlightType.VS.ToString());

            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out string errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string partitionKey = pnrDetails["partitionKey"];
            string testScenario = pnrDetails["testScenario"];
            string pnrLocator = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];
            string originCode = pnrDetails["origin"];
            int passengersCount = Convert.ToInt32(pnrDetails["passengersCount"]);

            BaggageSelectionType baggageSelectionType = BaggageSelectionType.FreeOnly;

            Assert.True(!string.IsNullOrEmpty(pnrLocator) && !string.IsNullOrEmpty(originCode), "PNR and OriginCode should not be null or empty");

            string checkInGraphQLRequest = GraphQLRequestBuilder.CheckInGraphQLRequest(pnrLocator, givenNames, surname, originCode);

            HttpResponseMessage checkInGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(checkInGraphQLRequest).EnsureGQLResponseSuccess();

            JToken checkInResult = JsonConvert.DeserializeObject<JToken>(checkInGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            bool isCheckInAvailable = checkInResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

            Assert.True(isCheckInAvailable, $"Check-in is not available for request : {checkInGraphQLRequest} and \n response is : {checkInResult.ToString()}");

            // Extract check-in ID
            string checkInId = checkInResult?.SelectToken("$..checkInId")?.ToString();

            Assert.True(!checkInId.IsNullOrEmpty(), $"Check-in response is invalid : {checkInResult} for PNR : {pnrLocator}");

            // Getting passenger IDs as a list
            List<string> selectedPassengerIds = OCIHelper.GetPassengersListFromCheckInResponse(checkInGraphQLResponse);

            int checkInPassengerCount = selectedPassengerIds.Count;

            Assert.True(passengersCount == checkInPassengerCount, $"Passenger count mismatch. Expected: {passengersCount}, Actual: {checkInPassengerCount}, \n Check-in Response:{checkInResult.ToString()}");

            if (checkInPassengerCount > 1)
            {
                string selectPassengersGraphQLRequest = GraphQLRequestBuilder.SelectPassengersGraphQLRequest(checkInId, selectedPassengerIds);

                HttpResponseMessage selectPassengersGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(selectPassengersGraphQLRequest).EnsureGQLResponseSuccess();

                JToken selectPassengersResult = JsonConvert.DeserializeObject<JToken>(selectPassengersGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                checkInId = selectPassengersResult?.SelectToken("$..checkInId")?.ToString();
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

            JToken savePassportResult = null;

            foreach (JToken passenger in passengers)
            {
                string passengerGivenNames = passenger["givenNames"]?.ToString();
                string passengerSurname = passenger["surname"]?.ToString();
                string passengerId = passenger["passengerId"]?.ToString();
                string dateOfBirth = passenger["dateOfBirth"]?.ToString();

                // Generate passport number based on destination country
                string passportNumber = PassportGenerator.GeneratePassport(destinationCountryCode);

                Assert.False(string.IsNullOrEmpty(passportNumber), $"Passport number is missing for country {destinationCountryCode}.");

                string savePassportGraphQLRequest = GraphQLRequestBuilder.SavePassportGraphQLRequest(checkInId, passengerId, passengerGivenNames, passengerSurname, dateOfBirth, passportNumber, destinationCountryCode, destinationCountryCode, destinationCountryCode);

                HttpResponseMessage savePassportGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(savePassportGraphQLRequest).EnsureGQLResponseSuccess();

                savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            checkInId = savePassportResult?.SelectToken("$..checkInId")?.ToString();

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            checkInId = saveEmergencyContactsResult?.SelectToken("$..checkInId")?.ToString();

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            checkInId = saveContactTracingResult?.SelectToken("$..checkInId")?.ToString();

            string getBaggageRequest = GraphQLRequestBuilder.GetBaggageGraphQLRequest(checkInId: checkInId, hidePaidBaggage: true);

            HttpResponseMessage getBaggageResponse = GraphQLEndpoint.GraphQLCheckInMobile(getBaggageRequest).EnsureGQLResponseSuccess();

            JToken getBaggageResult = JsonConvert.DeserializeObject<JToken>(getBaggageResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.CheckBaggageAdvisoryMessageExistsForEachPassenger(getBaggageResult), $"Get baggage expected response not returned => {getBaggageResult}");

            checkInId = getBaggageResult?.SelectToken("$..checkInId")?.ToString();

            string saveBaggageRequest = GraphQLRequestBuilder.SaveBaggageGraphQLRequest(getBaggageResponse, checkInId, baggageSelectionType);

            HttpResponseMessage saveBaggageResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveBaggageRequest).EnsureGQLResponseSuccess();

            JToken saveBaggageResult = JsonConvert.DeserializeObject<JToken>(saveBaggageResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            checkInId = saveBaggageResult?.SelectToken("$..checkInId")?.ToString();

            string paymentEligibilityCheckRequest = GraphQLRequestBuilder.PaymentEligibilityCheckGraphQLRequest(checkInId);

            HttpResponseMessage paymentEligibilityCheckResponse = GraphQLEndpoint.GraphQLCheckInMobile(paymentEligibilityCheckRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQLAssertions.ValidatePaymentEligibilityCheckResponse(paymentEligibilityCheckResponse, baggageSelectionType), $"Payment eligibility check response is invalid: \n{paymentEligibilityCheckResponse.ReadResponseAsString()}");

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            IEnumerable<JToken> qrCodes = generateBoardingPassResult.SelectTokens("$..boardingPasses[*].qrCode");

            string qrCode = qrCodes.FirstOrDefault()?.ToString();

            Assert.True(!qrCode.IsNullOrEmpty(), $"Test Scenario: {testScenario} \n Boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            IEnumerable<JToken> retrieveBoardingPassQRCodes = retrieveBoardingPassResult.SelectTokens("$..boardingPasses[*].qrCode");

            string retrieveBoardingPassQRCode = retrieveBoardingPassQRCodes.FirstOrDefault()?.ToString();

            Assert.True(!retrieveBoardingPassQRCode.IsNullOrEmpty(), $"Test Scenario: {testScenario} \n Boarding pass not retuned for the request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus, baggageSelectionType.ToString());
        }

        [Theory]
        [InlineData(BaggageSelectionType.FreePlusPaid, LocationType.MLE)]
        [InlineData(BaggageSelectionType.PaidOnly, LocationType.DEL)]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void BaggageGraphQLTests_UnsupportedCurrencies_Success(BaggageSelectionType baggageSelectionType, LocationType origin)
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(origin: origin.ToString(), airline: FlightType.VS.ToString());

            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out string errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string partitionKey = pnrDetails["partitionKey"];
            string testScenario = pnrDetails["testScenario"];
            string pnrLocator = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];
            string originCode = pnrDetails["origin"];
            int passengersCount = Convert.ToInt32(pnrDetails["passengersCount"]);

            Assert.True(!string.IsNullOrEmpty(pnrLocator) && !string.IsNullOrEmpty(originCode), "PNR and OriginCode should not be null or empty");

            string checkInGraphQLRequest = GraphQLRequestBuilder.CheckInGraphQLRequest(pnrLocator, givenNames, surname, originCode);

            HttpResponseMessage checkInGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(checkInGraphQLRequest).EnsureGQLResponseSuccess();

            JToken checkInResult = JsonConvert.DeserializeObject<JToken>(checkInGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            bool isCheckInAvailable = checkInResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

            Assert.True(isCheckInAvailable, $"Check-in is not available for request : {checkInGraphQLRequest} and \n response is : {checkInResult.ToString()}");

            // Extract check-in ID
            string checkInId = checkInResult?.SelectToken("$..checkInId")?.ToString();

            Assert.True(!checkInId.IsNullOrEmpty(), $"Check-in response is invalid : {checkInResult} for PNR : {pnrLocator}");

            // Getting passenger IDs as a list
            List<string> selectedPassengerIds = OCIHelper.GetPassengersListFromCheckInResponse(checkInGraphQLResponse);

            int checkInPassengerCount = selectedPassengerIds.Count;

            Assert.True(passengersCount == checkInPassengerCount, $"Passenger count mismatch. Expected: {passengersCount}, Actual: {checkInPassengerCount}, \n Check-in Response:{checkInResult.ToString()}");

            if (checkInPassengerCount > 1)
            {
                string selectPassengersGraphQLRequest = GraphQLRequestBuilder.SelectPassengersGraphQLRequest(checkInId, selectedPassengerIds);

                HttpResponseMessage selectPassengersGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(selectPassengersGraphQLRequest).EnsureGQLResponseSuccess();

                JToken selectPassengersResult = JsonConvert.DeserializeObject<JToken>(selectPassengersGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                checkInId = selectPassengersResult?.SelectToken("$..checkInId")?.ToString();
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

            JToken savePassportResult = null;

            foreach (JToken passenger in passengers)
            {
                string passengerGivenNames = passenger["givenNames"]?.ToString();
                string passengerSurname = passenger["surname"]?.ToString();
                string passengerId = passenger["passengerId"]?.ToString();
                string dateOfBirth = passenger["dateOfBirth"]?.ToString();

                // Generate passport number based on destination country
                string passportNumber = PassportGenerator.GeneratePassport(destinationCountryCode);

                Assert.False(string.IsNullOrEmpty(passportNumber), $"Passport number is missing for country {destinationCountryCode}.");

                string savePassportGraphQLRequest = GraphQLRequestBuilder.SavePassportGraphQLRequest(checkInId, passengerId, passengerGivenNames, passengerSurname, dateOfBirth, passportNumber, destinationCountryCode, destinationCountryCode, destinationCountryCode);

                HttpResponseMessage savePassportGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(savePassportGraphQLRequest).EnsureGQLResponseSuccess();

                savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            checkInId = savePassportResult?.SelectToken("$..checkInId")?.ToString();

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            checkInId = saveEmergencyContactsResult?.SelectToken("$..checkInId")?.ToString();

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            checkInId = saveContactTracingResult?.SelectToken("$..checkInId")?.ToString();

            string getBaggageRequest = GraphQLRequestBuilder.GetBaggageGraphQLRequest(checkInId: checkInId, hidePaidBaggage: false);

            HttpResponseMessage getBaggageResponse = GraphQLEndpoint.GraphQLCheckInMobile(getBaggageRequest).EnsureGQLResponseSuccess();

            JToken getBaggageResult = JsonConvert.DeserializeObject<JToken>(getBaggageResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.CheckBaggageAdvisoryMessageExistsForEachPassenger(getBaggageResult), $"Get baggage expected response not returned => {getBaggageResult}");

            bool hasEmptyBaggageOffers = getBaggageResult.SelectTokens("$.data.BaggageAllowance.passengers[*].baggageOffers").Any(baggageOffer => baggageOffer is JArray arr && arr.Count == 0);

            if (!hasEmptyBaggageOffers)
            {
                checkInId = getBaggageResult?.SelectToken("$..checkInId")?.ToString();

                string saveBaggageRequest = GraphQLRequestBuilder.SaveBaggageGraphQLRequest(getBaggageResponse, checkInId, baggageSelectionType);

                HttpResponseMessage saveBaggageResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveBaggageRequest).EnsureGQLResponseSuccess();

                JToken saveBaggageResult = JsonConvert.DeserializeObject<JToken>(saveBaggageResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                checkInId = saveBaggageResult?.SelectToken("$..checkInId")?.ToString();

                string paymentEligibilityCheckRequest = GraphQLRequestBuilder.PaymentEligibilityCheckGraphQLRequest(checkInId);

                HttpResponseMessage paymentEligibilityCheckResponse = GraphQLEndpoint.GraphQLCheckInMobile(paymentEligibilityCheckRequest).EnsureGQLResponseSuccess();

                Assert.True(GraphQLAssertions.ValidatePaymentEligibilityCheckResponse(paymentEligibilityCheckResponse, BaggageSelectionType.FreeOnly), $"Payment eligibility check response is invalid: \n{paymentEligibilityCheckResponse.ReadResponseAsString()}");
            }

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            IEnumerable<JToken> qrCodes = generateBoardingPassResult.SelectTokens("$..boardingPasses[*].qrCode");

            string qrCode = qrCodes.FirstOrDefault()?.ToString();

            Assert.True(!qrCode.IsNullOrEmpty(), $"Test Scenario: {testScenario} \n Boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            IEnumerable<JToken> retrieveBoardingPassQRCodes = retrieveBoardingPassResult.SelectTokens("$..boardingPasses[*].qrCode");

            string retrieveBoardingPassQRCode = retrieveBoardingPassQRCodes.FirstOrDefault()?.ToString();

            Assert.True(!retrieveBoardingPassQRCode.IsNullOrEmpty(), $"Test Scenario: {testScenario} \n Boarding pass not retuned for the request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus, "RESTRICTED_CURRENCY_BAGS");
        }

        [Fact]
        public void BaggageGraphQLTests_PaidBaggage_LoggedInUser_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(testScenarioName: "OneWayScenarioRedemption");

            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out string errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string partitionKey = pnrDetails["partitionKey"];
            string testScenario = pnrDetails["testScenario"];
            string pnrLocator = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];
            string originCode = pnrDetails["origin"];
            string currencyCode = pnrDetails["currency"];
            string flyingClubNumber = pnrDetails["flyingClubNumber"];
            int passengersCount = Convert.ToInt32(pnrDetails["passengersCount"]);

            Assert.True(!string.IsNullOrEmpty(pnrLocator) && !string.IsNullOrEmpty(originCode), "PNR and OriginCode should not be null or empty");

            string checkInGraphQLRequest = GraphQLRequestBuilder.CheckInGraphQLRequest(pnrLocator, givenNames, surname, originCode);

            HttpResponseMessage checkInGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(checkInGraphQLRequest).EnsureGQLResponseSuccess();

            JToken checkInResult = JsonConvert.DeserializeObject<JToken>(checkInGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            bool isCheckInAvailable = checkInResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

            Assert.True(isCheckInAvailable, $"Check-in is not available for request : {checkInGraphQLRequest} and \n response is : {checkInResult.ToString()}");

            // Extract check-in ID
            string checkInId = checkInResult?.SelectToken("$..checkInId")?.ToString();

            Assert.True(!checkInId.IsNullOrEmpty(), $"Check-in response is invalid : {checkInResult} for PNR : {pnrLocator}");

            // Getting passenger IDs as a list
            List<string> selectedPassengerIds = OCIHelper.GetPassengersListFromCheckInResponse(checkInGraphQLResponse);

            int checkInPassengerCount = selectedPassengerIds.Count;

            Assert.True(passengersCount == checkInPassengerCount, $"Passenger count mismatch. Expected: {passengersCount}, Actual: {checkInPassengerCount}, \n Check-in Response:{checkInResult.ToString()}");

            if (checkInPassengerCount > 1)
            {
                string selectPassengersGraphQLRequest = GraphQLRequestBuilder.SelectPassengersGraphQLRequest(checkInId, selectedPassengerIds);

                HttpResponseMessage selectPassengersGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(selectPassengersGraphQLRequest).EnsureGQLResponseSuccess();

                JToken selectPassengersResult = JsonConvert.DeserializeObject<JToken>(selectPassengersGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                checkInId = selectPassengersResult?.SelectToken("$..checkInId")?.ToString();
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

            JToken savePassportResult = null;

            foreach (JToken passenger in passengers)
            {
                string passengerGivenNames = passenger["givenNames"]?.ToString();
                string passengerSurname = passenger["surname"]?.ToString();
                string passengerId = passenger["passengerId"]?.ToString();
                string dateOfBirth = passenger["dateOfBirth"]?.ToString();

                // Generate passport number based on destination country
                string passportNumber = PassportGenerator.GeneratePassport(destinationCountryCode);

                Assert.False(string.IsNullOrEmpty(passportNumber), $"Passport number is missing for country {destinationCountryCode}.");

                string savePassportGraphQLRequest = GraphQLRequestBuilder.SavePassportGraphQLRequest(checkInId, passengerId, passengerGivenNames, passengerSurname, dateOfBirth, passportNumber, destinationCountryCode, destinationCountryCode, destinationCountryCode);

                HttpResponseMessage savePassportGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(savePassportGraphQLRequest).EnsureGQLResponseSuccess();

                savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            checkInId = savePassportResult?.SelectToken("$..checkInId")?.ToString();

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            checkInId = saveEmergencyContactsResult?.SelectToken("$..checkInId")?.ToString();

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            checkInId = saveContactTracingResult?.SelectToken("$..checkInId")?.ToString();

            string password = memoryStoreFixture.Get<string>("MYA-Password");

            HttpResponseMessage accountMemberSignInResponse = MyAccountGraphQL.AccountMemberSignIn(env, "", flyingClubNumber, password).EnsureGQLResponseSuccess();

            JToken accountMemberSignInResult = JsonConvert.DeserializeObject<JToken>(accountMemberSignInResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string idToken = accountMemberSignInResult?["data"]?["accountMemberSignIn"]?["idToken"]?.ToString() ?? string.Empty;

            Assert.False(idToken.IsNullOrEmpty(), $"idToken was empty for flyingClubNumber {flyingClubNumber}. Sign-in response: {accountMemberSignInResult}");

            string getBaggageRequest = GraphQLRequestBuilder.GetBaggageGraphQLRequest(checkInId: checkInId, hidePaidBaggage: false);

            HttpResponseMessage getBaggageResponse = GraphQLEndpoint.GraphQLCheckInMobile(getBaggageRequest, idToken).EnsureGQLResponseSuccess();

            JToken getBaggageResult = JsonConvert.DeserializeObject<JToken>(getBaggageResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string emailAddress = getBaggageResult?["data"]?["BaggageAllowance"]?["emailAddress"]?.ToString() ?? string.Empty;

            Assert.False(emailAddress.IsNullOrEmpty(), $"Email address was missing in get baggage response: {getBaggageResult}");

            bool canChangeBags = getBaggageResult?.SelectToken("$.data.BaggageAllowance.passengers[0].canChangeBags")?.Value<bool>() ?? false;

            Assert.True(canChangeBags, $"Expected to be able to change bags, but either they were already added or the delta did not return any offers.\nResult: {getBaggageResult}");

            checkInId = getBaggageResult?.SelectToken("$..checkInId")?.ToString();

            string saveBaggageRequest = GraphQLRequestBuilder.SaveBaggageGraphQLRequest(getBaggageResponse, checkInId, BaggageSelectionType.PaidOnly);

            HttpResponseMessage saveBaggageResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveBaggageRequest).EnsureGQLResponseSuccess();

            JToken saveBaggageResult = JsonConvert.DeserializeObject<JToken>(saveBaggageResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            checkInId = saveBaggageResult?.SelectToken("$..checkInId")?.ToString();

            string paymentEligibilityCheckRequest = GraphQLRequestBuilder.PaymentEligibilityCheckGraphQLRequest(checkInId);

            HttpResponseMessage paymentEligibilityCheckResponse = GraphQLEndpoint.GraphQLCheckInMobile(paymentEligibilityCheckRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQLAssertions.ValidatePaymentEligibilityCheckResponse(paymentEligibilityCheckResponse, BaggageSelectionType.PaidOnly), $"Payment eligibility check response is invalid: \n{paymentEligibilityCheckResponse.ReadResponseAsString()}");

            JToken paymentEligibilityCheckResult = JsonConvert.DeserializeObject<JToken>(paymentEligibilityCheckResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string paymentReferenceId = paymentEligibilityCheckResult?.SelectToken("$..paymentReferenceId")?.ToString();

            string hppLinkUrl = paymentEligibilityCheckResult?.SelectToken("$..hPPLink")?.ToString();

            using NavigateToHostedPaymentPage enterHppCardDetails = new NavigateToHostedPaymentPage(hppLinkUrl);

            _testConfig = new TestConfig();

            string cardNumber = currencyCode == "USD" ? BagsPaymentCardUsdCurrency : BagsPaymentCardAllCurrency;

            _testConfig.Book.Payment = new Payment
            {
                CardDetails = new CardDetails
                {
                    CardNumber = cardNumber
                }
            };

            enterHppCardDetails.SubmitHppCardDetails(_testConfig);

            string confirmBaggageGraphQLRequest = GraphQLRequestBuilder.ConfirmBaggageGraphQLRequest(checkInId, paymentReferenceId);

            HttpResponseMessage confirmBaggageResponse = GraphQLEndpoint.GraphQLCheckInMobile(confirmBaggageGraphQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQLAssertions.ValidateTotalPriceAndCurrencyForSelectedBaggage(saveBaggageRequest, confirmBaggageResponse), $"Confirm baggage response is invalid: \n{confirmBaggageResponse.ReadResponseAsString()} for baggage request: \n{saveBaggageRequest}");

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            IEnumerable<JToken> qrCodes = generateBoardingPassResult.SelectTokens("$..boardingPasses[*].qrCode");

            string qrCode = qrCodes.FirstOrDefault()?.ToString();

            Assert.True(!qrCode.IsNullOrEmpty(), $"Test Scenario: {testScenario} \n Boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            IEnumerable<JToken> retrieveBoardingPassQRCodes = retrieveBoardingPassResult.SelectTokens("$..boardingPasses[*].qrCode");

            string retrieveBoardingPassQRCode = retrieveBoardingPassQRCodes.FirstOrDefault()?.ToString();

            Assert.True(!retrieveBoardingPassQRCode.IsNullOrEmpty(), $"Test Scenario: {testScenario} \n Boarding pass not retuned for the request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus, BaggageSelectionType.PaidOnly.ToString());
        }

        [Fact]
        public void BaggageGraphQLTests_PaidBaggage_LocalRun_Success()
        {
            string partitionKey = "";
            string testScenario = "";
            string pnrLocator = "";
            string givenNames = "";
            string surname = "";
            string originCode = "";
            string currencyCode = "";
            int passengersCount = 0;

            Assert.True(!string.IsNullOrEmpty(pnrLocator) && !string.IsNullOrEmpty(originCode), "PNR and OriginCode should not be null or empty");

            string checkInGraphQLRequest = GraphQLRequestBuilder.CheckInGraphQLRequest(pnrLocator, givenNames, surname, originCode);

            HttpResponseMessage checkInGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(checkInGraphQLRequest).EnsureGQLResponseSuccess();

            JToken checkInResult = JsonConvert.DeserializeObject<JToken>(checkInGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            bool isCheckInAvailable = checkInResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

            Assert.True(isCheckInAvailable, $"Check-in is not available for request : {checkInGraphQLRequest} and \n response is : {checkInResult.ToString()}");

            // Extract check-in ID
            string checkInId = checkInResult?.SelectToken("$..checkInId")?.ToString();

            Assert.True(!checkInId.IsNullOrEmpty(), $"Check-in response is invalid : {checkInResult} for PNR : {pnrLocator}");

            // Getting passenger IDs as a list
            List<string> selectedPassengerIds = OCIHelper.GetPassengersListFromCheckInResponse(checkInGraphQLResponse);

            int checkInPassengerCount = selectedPassengerIds.Count;

            Assert.True(passengersCount == checkInPassengerCount, $"Passenger count mismatch. Expected: {passengersCount}, Actual: {checkInPassengerCount}, \n Check-in Response:{checkInResult.ToString()}");

            if (checkInPassengerCount > 1)
            {
                string selectPassengersGraphQLRequest = GraphQLRequestBuilder.SelectPassengersGraphQLRequest(checkInId, selectedPassengerIds);

                HttpResponseMessage selectPassengersGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(selectPassengersGraphQLRequest).EnsureGQLResponseSuccess();

                JToken selectPassengersResult = JsonConvert.DeserializeObject<JToken>(selectPassengersGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                checkInId = selectPassengersResult?.SelectToken("$..checkInId")?.ToString();
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

            JToken savePassportResult = null;

            foreach (JToken passenger in passengers)
            {
                string passengerGivenNames = passenger["givenNames"]?.ToString();
                string passengerSurname = passenger["surname"]?.ToString();
                string passengerId = passenger["passengerId"]?.ToString();
                string dateOfBirth = passenger["dateOfBirth"]?.ToString();

                // Generate passport number based on destination country
                string passportNumber = PassportGenerator.GeneratePassport(destinationCountryCode);

                Assert.False(string.IsNullOrEmpty(passportNumber), $"Passport number is missing for country {destinationCountryCode}.");

                string savePassportGraphQLRequest = GraphQLRequestBuilder.SavePassportGraphQLRequest(checkInId, passengerId, passengerGivenNames, passengerSurname, dateOfBirth, passportNumber, destinationCountryCode, destinationCountryCode, destinationCountryCode);

                HttpResponseMessage savePassportGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(savePassportGraphQLRequest).EnsureGQLResponseSuccess();

                savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            checkInId = savePassportResult?.SelectToken("$..checkInId")?.ToString();

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            checkInId = saveEmergencyContactsResult?.SelectToken("$..checkInId")?.ToString();

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            checkInId = saveContactTracingResult?.SelectToken("$..checkInId")?.ToString();

            string getBaggageRequest = GraphQLRequestBuilder.GetBaggageGraphQLRequest(checkInId: checkInId, hidePaidBaggage: false);

            HttpResponseMessage getBaggageResponse = GraphQLEndpoint.GraphQLCheckInMobile(getBaggageRequest).EnsureGQLResponseSuccess();

            JToken getBaggageResult = JsonConvert.DeserializeObject<JToken>(getBaggageResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            checkInId = getBaggageResult?.SelectToken("$..checkInId")?.ToString();

            string saveBaggageRequest = GraphQLRequestBuilder.SaveBaggageGraphQLRequest(getBaggageResponse, checkInId, BaggageSelectionType.PaidOnly);

            HttpResponseMessage saveBaggageResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveBaggageRequest).EnsureGQLResponseSuccess();

            JToken saveBaggageResult = JsonConvert.DeserializeObject<JToken>(saveBaggageResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            checkInId = saveBaggageResult?.SelectToken("$..checkInId")?.ToString();

            string paymentEligibilityCheckRequest = GraphQLRequestBuilder.PaymentEligibilityCheckGraphQLRequest(checkInId);

            HttpResponseMessage paymentEligibilityCheckResponse = GraphQLEndpoint.GraphQLCheckInMobile(paymentEligibilityCheckRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQLAssertions.ValidatePaymentEligibilityCheckResponse(paymentEligibilityCheckResponse, BaggageSelectionType.PaidOnly), $"Payment eligibility check response is invalid: \n{paymentEligibilityCheckResponse.ReadResponseAsString()}");

            JToken paymentEligibilityCheckResult = JsonConvert.DeserializeObject<JToken>(paymentEligibilityCheckResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string paymentReferenceId = paymentEligibilityCheckResult?.SelectToken("$..paymentReferenceId")?.ToString();

            string hppLinkUrl = paymentEligibilityCheckResult?.SelectToken("$..hPPLink")?.ToString();

            using NavigateToHostedPaymentPage enterHppCardDetails = new NavigateToHostedPaymentPage(hppLinkUrl);

            _testConfig = new TestConfig();

            string cardNumber = currencyCode == "USD" ? BagsPaymentCardUsdCurrency : BagsPaymentCardAllCurrency;

            _testConfig.Book.Payment = new Payment
            {
                CardDetails = new CardDetails
                {
                    CardNumber = cardNumber
                }
            };

            enterHppCardDetails.SubmitHppCardDetails(_testConfig);

            string confirmBaggageGraphQLRequest = GraphQLRequestBuilder.ConfirmBaggageGraphQLRequest(checkInId, paymentReferenceId);

            HttpResponseMessage confirmBaggageResponse = GraphQLEndpoint.GraphQLCheckInMobile(confirmBaggageGraphQLRequest).EnsureGQLResponseSuccess();

            Assert.True(GraphQLAssertions.ValidateTotalPriceAndCurrencyForSelectedBaggage(saveBaggageRequest, confirmBaggageResponse), $"Confirm baggage response is invalid: \n{confirmBaggageResponse.ReadResponseAsString()} for baggage request: \n{saveBaggageRequest}");

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            IEnumerable<JToken> qrCodes = generateBoardingPassResult.SelectTokens("$..boardingPasses[*].qrCode");

            string qrCode = qrCodes.FirstOrDefault()?.ToString();

            Assert.True(!qrCode.IsNullOrEmpty(), $"Test Scenario: {testScenario} \n Boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            IEnumerable<JToken> retrieveBoardingPassQRCodes = retrieveBoardingPassResult.SelectTokens("$..boardingPasses[*].qrCode");

            string retrieveBoardingPassQRCode = retrieveBoardingPassQRCodes.FirstOrDefault()?.ToString();

            Assert.True(!retrieveBoardingPassQRCode.IsNullOrEmpty(), $"Test Scenario: {testScenario} \n Boarding pass not retuned for the request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus, "PaidBags");
        }
    }
}
