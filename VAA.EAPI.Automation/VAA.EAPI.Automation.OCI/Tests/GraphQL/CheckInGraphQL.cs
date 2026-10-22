using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.OCI.Constants;
using VAA.EAPI.Automation.OCI.Helpers;
using VAA.EAPI.Automation.OCI.Requests.Endpoints;

namespace VAA.EAPI.Automation.OCI.Tests.GraphQL
{
    [Collection("CheckInGraphQLTests")]
    public class CheckInGraphQL
    {
        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        [DisplayName("Happy Path using any PNR from Storage Table within the Checkin window")]
        public void CheckInGraphQLTests_AnyPnrHappyPath_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable();

            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out string errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string partitionKey = pnrDetails["partitionKey"];
            string pnrLocator = pnrDetails["pnr"];
            string testScenario = pnrDetails["testScenario"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];
            string originCode = pnrDetails["origin"];
            int passengersCount = Convert.ToInt32(pnrDetails["passengersCount"]);

            Assert.True(!string.IsNullOrEmpty(pnrLocator) && !string.IsNullOrEmpty(originCode), "PNR and OriginCode should not be null or empty");

            string checkInGraphQLRequest = GraphQLRequestBuilder.CheckInGraphQLRequest(pnrLocator, givenNames, surname, originCode);

            HttpResponseMessage checkInGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(checkInGraphQLRequest).EnsureGQLResponseSuccess();

            JToken checkInResult = JsonConvert.DeserializeObject<JToken>(checkInGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            bool isCheckInAvailable = checkInResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

            Assert.True(isCheckInAvailable, $"Test Scenario: {testScenario} \n Check-in is not available for request : {checkInGraphQLRequest} and \n response is : {checkInResult.ToString()}");

            // Extract check-in ID
            string checkInId = checkInResult?.SelectToken("$..checkInId")?.ToString();

            Assert.True(!checkInId.IsNullOrEmpty(), $"Test Scenario: {testScenario} \n Check-in response is invalid : {checkInResult} for PNR : {pnrLocator}");

            // Getting passenger IDs as a list
            List<string> selectedPassengerIds = OCIHelper.GetPassengersListFromCheckInResponse(checkInGraphQLResponse);

            int checkInPassengerCount = selectedPassengerIds.Count;

            Assert.True(passengersCount == checkInPassengerCount, $"Test Scenario: {testScenario} \n Passenger count mismatch. Expected: {passengersCount}, Actual: {checkInPassengerCount}, \n Check-in Response:{checkInResult.ToString()}");

            if (checkInPassengerCount > 1)
            {
                string selectPassengersGraphQLRequest = GraphQLRequestBuilder.SelectPassengersGraphQLRequest(checkInId, selectedPassengerIds);

                HttpResponseMessage selectPassengersGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(selectPassengersGraphQLRequest).EnsureGQLResponseSuccess();
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

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

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Test Scenario: {testScenario} \n Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Test Scenario: {testScenario} \n Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(generateBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(retrieveBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus);
        }

        [Theory]
        [Trait("Category", GlobalTraits.MobileAppsSanityTests)]
        [Trait("Category", Traits.CheckInGraphQLCiCd)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.OneWay, ReservationType.AWARD)]
        public void CheckInGraphQLTests_Success_DataSet(JourneyType journeyType, ReservationType reservationType)
        {
            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(journeyTypeString, reservationTypeString);

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
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

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

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Test Scenario: {testScenario} \n Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(generateBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(retrieveBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus);
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void CheckInGraphQLTests_ReturnDateRequiredFlights_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.OneWay.ToString(), ReservationType.REVENUE.ToString(), "OneWayReturnDateScenarioRevenue", LocationType.LHR.ToString(), LocationType.BGI.ToString());

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
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string passportCountryCode = PassportCountryCode.GBR.ToString();

            foreach (JToken passenger in passengers)
            {
                string passengerGivenNames = passenger["givenNames"]?.ToString();
                string passengerSurname = passenger["surname"]?.ToString();
                string passengerId = passenger["passengerId"]?.ToString();
                string dateOfBirth = passenger["dateOfBirth"]?.ToString();
                string returnDate = DataGenerator.GetRandomDateWithinNextMonth();

                // Generate passport number based on destination country
                string passportNumber = PassportGenerator.GeneratePassport(passportCountryCode);

                Assert.False(string.IsNullOrEmpty(passportNumber), $"Passport number is missing for country {passportCountryCode}.");

                string savePassportGraphQLRequest = GraphQLRequestBuilder.SavePassportGraphQLRequest(checkInId, passengerId, passengerGivenNames, passengerSurname, dateOfBirth, passportNumber, passportCountryCode, passportCountryCode, passportCountryCode);

                HttpResponseMessage savePassportGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(savePassportGraphQLRequest).EnsureGQLResponseSuccess();

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");

                string returnDateStatus = savePassportResult.SelectToken($"$..passengers[?(@.passengerId =~ /{passengerId}/)].requirements.returnDateStatus").ToString();

                Assert.True(returnDateStatus == "REQUIRED" || returnDateStatus == "PROVIDED", $"Return date status is not returned as expected: {savePassportResult}");

                string saveReturnDateGraphQLRequest = GraphQLRequestBuilder.SaveReturnDateGraphQLRequest(checkInId, passengerId, returnDate);

                HttpResponseMessage saveReturnDateGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveReturnDateGraphQLRequest).EnsureGQLResponseSuccess();

                JToken saveReturnDateResult = JsonConvert.DeserializeObject<JToken>(saveReturnDateGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                string savedReturnDateStatus = saveReturnDateResult.SelectToken($"$..passengers[?(@.passengerId =~ /{passengerId}/)].requirements.returnDateStatus").ToString();

                Assert.True(savedReturnDateStatus == "PROVIDED", $"Return date for checkin id : {checkInId} and passenger : {passengerId} not saved ");
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Test Scenario: {testScenario} \n Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(generateBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(retrieveBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus);
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void CheckInGraphQLTests_MulticityFlights_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.MultiCity.ToString(), ReservationType.REVENUE.ToString());

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
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

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

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Test Scenario: {testScenario} \n Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(generateBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(retrieveBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus);
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void CheckInGraphQLTests_SameDayReturnFlights_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.Return.ToString(), ReservationType.REVENUE.ToString(), "ReturnSameDayScenarioRevenue");

            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out string errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string partitionKey = pnrDetails["partitionKey"];
            string testScenario = pnrDetails["testScenario"];
            string pnrLocator = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];
            string originCode = pnrDetails["origin"];
            string destinationCode = pnrDetails["destination"];
            int passengersCount = Convert.ToInt32(pnrDetails["passengersCount"]);

            Assert.True(!string.IsNullOrEmpty(pnrLocator) && !string.IsNullOrEmpty(originCode) && !string.IsNullOrEmpty(destinationCode), "PNR and OriginCode, DestinationCode should not be null or empty");

            List<string> journeyStartCodes = new List<string> { originCode, destinationCode };

            string checkInStatus = "";

            foreach (string journeyStartCode in journeyStartCodes)
            {
                string checkInGraphQLRequest = GraphQLRequestBuilder.CheckInGraphQLRequest(pnrLocator, givenNames, surname, journeyStartCode);

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
                }

                JToken passengers = checkInResult?.SelectToken("$..passengers");

                string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

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

                    JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                    bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                    Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                    bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                    Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
                }

                string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

                HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

                JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

                HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

                JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

                HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

                JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Test Scenario: {testScenario} \n Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

                Assert.True(GraphQLAssertions.ValidateQrCodes(generateBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {generateBoardingPassResult}");

                string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, journeyStartCode, givenNames, surname);

                HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

                JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

                Assert.True(GraphQLAssertions.ValidateQrCodes(retrieveBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {retrieveBoardingPassResult}");

                Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

                checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

                Assert.True(checkInStatus == "COMPLETED", $"Check-in status is not completed :: {checkInStatus}");
            }

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus);
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void CheckInGraphQLTests_DirectFlights_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.OneWay.ToString(), ReservationType.REVENUE.ToString(), "OneWayDirectFlightScenarioRevenue");

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
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

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

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Test Scenario: {testScenario} \n Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(generateBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(retrieveBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus);
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void CheckInGraphQLTests_DirectWithStopFlights_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.OneWay.ToString(), ReservationType.REVENUE.ToString(), "OneWayDirectWithStopScenarioRevenue");

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
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

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

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Test Scenario: {testScenario} \n Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(generateBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(retrieveBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus);
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void CheckInGraphQLTests_OneStopFlightsOrThroughCheckIn_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.OneWay.ToString(), ReservationType.REVENUE.ToString(), "OneWayWithOneStopScenarioRevenue");

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
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

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

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Test Scenario: {testScenario} \n Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(generateBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(retrieveBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus);
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void CheckInGraphQLTests_TwoStopFlights_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.OneWay.ToString(), ReservationType.REVENUE.ToString(), "OneWayWithTwoStopScenarioRevenue");

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
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

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

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Test Scenario: {testScenario} \n Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(generateBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(retrieveBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus);
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void CheckInGraphQLTests_JVPartnerFlights_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.Return.ToString(), ReservationType.REVENUE.ToString(), "ReturnJVPartnersScenarioRevenue");

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

            // Extract passenger IDs as a list
            List<string> selectedPassengerIds = checkInResult?.SelectTokens("$..passengerId").Where(t => t.Type != JTokenType.Null).Select(t => t.ToString()).ToList();

            int checkInPassengerCount = selectedPassengerIds.Count;

            Assert.True(passengersCount == checkInPassengerCount, $"Passenger count mismatch. Expected: {passengersCount}, Actual: {checkInPassengerCount}, \n Check-in Response:{checkInResult.ToString()}");

            if (checkInPassengerCount > 1)
            {
                string selectPassengersGraphQLRequest = GraphQLRequestBuilder.SelectPassengersGraphQLRequest(checkInId, selectedPassengerIds);

                HttpResponseMessage selectPassengersGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(selectPassengersGraphQLRequest).EnsureGQLResponseSuccess();
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

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

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Test Scenario: {testScenario} \n Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(generateBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(retrieveBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus);
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void CheckInGraphQLTests_InterlinePartnerFlights_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.Return.ToString(), ReservationType.REVENUE.ToString(), "ReturnInterlinePartnersScenarioRevenue");

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
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

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

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Test Scenario: {testScenario} \n Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(generateBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(retrieveBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus);
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void CheckInGraphQLTests_CodeshareFlights_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.Return.ToString(), ReservationType.REVENUE.ToString(), "ReturnCodesharePartnersScenarioRevenue");

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
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

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

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Test Scenario: {testScenario} \n Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(generateBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(retrieveBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus);
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void CheckInGraphQLTests_LongLayoverFlights_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.OneWay.ToString(), ReservationType.REVENUE.ToString(), "OneWayLongLayoverFlightScenarioRevenue");

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
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

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

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Test Scenario: {testScenario} \n Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(generateBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(retrieveBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus);
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void CheckInGraphQLTests_GermanPassport_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.OneWay.ToString(), ReservationType.REVENUE.ToString());

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
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

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

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                string issuingCountry = savePassportResult.SelectToken($"$..passengers[?(@.passengerId =~ /{passengerId}/)].documents[*].issuingCountry.code").ToString();

                string nationality = savePassportResult.SelectToken($"$..passengers[?(@.passengerId =~ /{passengerId}/)].documents[*].nationality.code").ToString();

                string countryOfResidence = savePassportResult.SelectToken($"$..passengers[?(@.passengerId =~ /{passengerId}/)].documents[*].countryOfResidence.code").ToString();

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");

                Assert.True(issuingCountry.Equals(destinationCountryCode) && nationality.Equals(destinationCountryCode) && countryOfResidence.Equals(destinationCountryCode), $"PNR Locator: {pnrLocator} expected code not returned for issuingCountry : {issuingCountry}, nationality: {nationality} and countryOfResidence : {countryOfResidence}");
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Test Scenario: {testScenario} \n Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(generateBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(retrieveBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus);
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void CheckInGraphQLTests_PassportScanned_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable();

            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out string errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string partitionKey = pnrDetails["partitionKey"];
            string pnrLocator = pnrDetails["pnr"];
            string testScenario = pnrDetails["testScenario"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];
            string originCode = pnrDetails["origin"];
            int passengersCount = Convert.ToInt32(pnrDetails["passengersCount"]);

            Assert.True(!string.IsNullOrEmpty(pnrLocator) && !string.IsNullOrEmpty(originCode), "PNR and OriginCode should not be null or empty");

            string checkInGraphQLRequest = GraphQLRequestBuilder.CheckInGraphQLRequest(pnrLocator, givenNames, surname, originCode);

            HttpResponseMessage checkInGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(checkInGraphQLRequest).EnsureGQLResponseSuccess();

            JToken checkInResult = JsonConvert.DeserializeObject<JToken>(checkInGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            bool isCheckInAvailable = checkInResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

            Assert.True(isCheckInAvailable, $"Test Scenario: {testScenario} \n Check-in is not available for request : {checkInGraphQLRequest} and \n response is : {checkInResult.ToString()}");

            // Extract check-in ID
            string checkInId = checkInResult?.SelectToken("$..checkInId")?.ToString();

            Assert.True(!checkInId.IsNullOrEmpty(), $"Test Scenario: {testScenario} \n Check-in response is invalid : {checkInResult} for PNR : {pnrLocator}");

            // Getting passenger IDs as a list
            List<string> selectedPassengerIds = OCIHelper.GetPassengersListFromCheckInResponse(checkInGraphQLResponse);

            int checkInPassengerCount = selectedPassengerIds.Count;

            Assert.True(passengersCount == checkInPassengerCount, $"Test Scenario: {testScenario} \n Passenger count mismatch. Expected: {passengersCount}, Actual: {checkInPassengerCount}, \n Check-in Response:{checkInResult.ToString()}");

            if (checkInPassengerCount > 1)
            {
                string selectPassengersGraphQLRequest = GraphQLRequestBuilder.SelectPassengersGraphQLRequest(checkInId, selectedPassengerIds);

                HttpResponseMessage selectPassengersGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(selectPassengersGraphQLRequest).EnsureGQLResponseSuccess();
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

            foreach (JToken passenger in passengers)
            {
                string passengerGivenNames = passenger["givenNames"]?.ToString();
                string passengerSurname = passenger["surname"]?.ToString();
                string passengerId = passenger["passengerId"]?.ToString();
                string dateOfBirth = passenger["dateOfBirth"]?.ToString();

                // Generate passport number based on destination country
                string passportNumber = PassportGenerator.GeneratePassport(destinationCountryCode);

                Assert.False(string.IsNullOrEmpty(passportNumber), $"Passport number is missing for country {destinationCountryCode}.");

                string savePassportGraphQLRequest = GraphQLRequestBuilder.SavePassportGraphQLRequest(checkInId, passengerId, passengerGivenNames, passengerSurname, dateOfBirth, passportNumber, destinationCountryCode, destinationCountryCode, destinationCountryCode, isScanned: true);

                HttpResponseMessage savePassportGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(savePassportGraphQLRequest).EnsureGQLResponseSuccess();

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                bool passportVerified = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportVerified").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided && passportVerified, $"Test Scenario: {testScenario} \n Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Test Scenario: {testScenario} \n Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(generateBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(retrieveBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus);
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void CheckInGraphQLTests_DayOfTravel_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable();

            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out string errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string partitionKey = pnrDetails["partitionKey"];
            string pnrLocator = pnrDetails["pnr"];
            string testScenario = pnrDetails["testScenario"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];
            string originCode = pnrDetails["origin"];
            int passengersCount = Convert.ToInt32(pnrDetails["passengersCount"]);

            Assert.True(!string.IsNullOrEmpty(pnrLocator) && !string.IsNullOrEmpty(originCode), "PNR and OriginCode should not be null or empty");

            string checkInGraphQLRequest = GraphQLRequestBuilder.CheckInGraphQLRequest(pnrLocator, givenNames, surname, originCode);

            HttpResponseMessage checkInGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(checkInGraphQLRequest).EnsureGQLResponseSuccess();

            JToken checkInResult = JsonConvert.DeserializeObject<JToken>(checkInGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            bool isCheckInAvailable = checkInResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

            Assert.True(isCheckInAvailable, $"Test Scenario: {testScenario} \n Check-in is not available for request : {checkInGraphQLRequest} and \n response is : {checkInResult.ToString()}");

            // Extract check-in ID
            string checkInId = checkInResult?.SelectToken("$..checkInId")?.ToString();

            Assert.True(!checkInId.IsNullOrEmpty(), $"Test Scenario: {testScenario} \n Check-in response is invalid : {checkInResult} for PNR : {pnrLocator}");

            // Getting passenger IDs as a list
            List<string> selectedPassengerIds = OCIHelper.GetPassengersListFromCheckInResponse(checkInGraphQLResponse);

            int checkInPassengerCount = selectedPassengerIds.Count;

            Assert.True(passengersCount == checkInPassengerCount, $"Test Scenario: {testScenario} \n Passenger count mismatch. Expected: {passengersCount}, Actual: {checkInPassengerCount}, \n Check-in Response:{checkInResult.ToString()}");

            if (checkInPassengerCount > 1)
            {
                string selectPassengersGraphQLRequest = GraphQLRequestBuilder.SelectPassengersGraphQLRequest(checkInId, selectedPassengerIds);

                HttpResponseMessage selectPassengersGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(selectPassengersGraphQLRequest).EnsureGQLResponseSuccess();
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

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

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Test Scenario: {testScenario} \n Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Test Scenario: {testScenario} \n Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(generateBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator: pnrLocator, originCode: originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(retrieveBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus);
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void CheckInGraphQLTests_SeamlessCheckin_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.OneWay.ToString(), ReservationType.REVENUE.ToString(), "OneWaySeamlessScenarioRevenue");

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
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

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

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Test Scenario: {testScenario} \n Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(generateBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(retrieveBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus);
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void CheckInGraphQLTests_SkyPriority_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.OneWay.ToString(), ReservationType.REVENUE.ToString(), "OneWaySkyPriorityScenarioRevenue");

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
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

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

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Test Scenario: {testScenario} \n Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(generateBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {generateBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateShowSkyPriority(generateBoardingPassResult), $"Boarding pass sky priority status not returned correctly for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(retrieveBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus);
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void CheckInGraphQLTests_Loyalty_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.OneWay.ToString(), ReservationType.AWARD.ToString(), "OneWayScenarioRedemption");

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
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

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

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Test Scenario: {testScenario} \n Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(generateBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateShowLoyaltyStatusFirstPassenger(retrieveBoardingPassResult), $"Loyalty status not returned correctly in boarding pass for request: {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(retrieveBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus);
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void CheckInGraphQLTests_ESTAA_Success()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.OneWay.ToString(), ReservationType.REVENUE.ToString(), "OneWayESTAScenarioRevenue", LocationType.LHR.ToString(), LocationType.JFK.ToString(), "ESTAA");

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
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string passportCountryCode = PassportCountryCode.GBR.ToString();

            foreach (JToken passenger in passengers)
            {
                string passengerGivenNames = passenger["givenNames"]?.ToString();
                string passengerSurname = passenger["surname"]?.ToString();
                string passengerId = passenger["passengerId"]?.ToString();
                string dateOfBirth = passenger["dateOfBirth"]?.ToString();
                string returnDate = DataGenerator.GetRandomDateWithinNextMonth();

                // Generate passport number based on destination country
                string passportNumber = PassportGenerator.GeneratePassport(passportCountryCode);

                Assert.False(string.IsNullOrEmpty(passportNumber), $"Passport number is missing for country {passportCountryCode}.");

                string savePassportGraphQLRequest = GraphQLRequestBuilder.SavePassportGraphQLRequest(checkInId, passengerId, passengerGivenNames, passengerSurname, dateOfBirth, passportNumber, passportCountryCode, passportCountryCode, passportCountryCode);

                HttpResponseMessage savePassportGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(savePassportGraphQLRequest).EnsureGQLResponseSuccess();

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");

                string returnDateStatus = savePassportResult.SelectToken($"$..passengers[?(@.passengerId =~ /{passengerId}/)].requirements.returnDateStatus").ToString();

                Assert.True(returnDateStatus == "REQUIRED" || returnDateStatus == "PROVIDED", $"Return date status is not returned as expected: {savePassportResult}");

                string saveReturnDateGraphQLRequest = GraphQLRequestBuilder.SaveReturnDateGraphQLRequest(checkInId, passengerId, returnDate);

                HttpResponseMessage saveReturnDateGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveReturnDateGraphQLRequest).EnsureGQLResponseSuccess();

                JToken saveReturnDateResult = JsonConvert.DeserializeObject<JToken>(saveReturnDateGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                string savedReturnDateStatus = saveReturnDateResult.SelectToken($"$..passengers[?(@.passengerId =~ /{passengerId}/)].requirements.returnDateStatus").ToString();

                Assert.True(savedReturnDateStatus == "PROVIDED", $"Return date for checkin id : {checkInId} and passenger : {passengerId} not saved ");
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Test Scenario: {testScenario} \n Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(generateBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.ValidateQrCodes(retrieveBoardingPassResult), $"One or more returned boarding passes do not contain a QR code in response: {retrieveBoardingPassResult}");

            Assert.True(GraphQLAssertions.CheckAirshipLinksExists(retrieveBoardingPassResult), $"Test Scenario: {testScenario} \n Airship links not generated for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();

            //Updating CheckInStatus in azure table.
            WriteToAzureStorage.UpdatePNRCheckInStatusToAzureStorageTable(partitionKey, pnrLocator, checkInStatus);
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLCiCd)]
        public void CheckInGraphQLTests_WithMockRequestAndResponse_Success()
        {
            string checkInGraphQLRequest = GraphQLRequestBuilder.CheckInGraphQLRequest(MockUpData.pnrLocator, MockUpData.givenNames, MockUpData.surname, MockUpData.originCode);

            HttpResponseMessage checkInGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(checkInGraphQLRequest).EnsureGQLResponseSuccess();

            JToken checkInResult = JsonConvert.DeserializeObject<JToken>(checkInGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            // Extract check-in ID
            string checkInId = checkInResult?.SelectToken("$..checkInId")?.ToString();

            Assert.True(!checkInId.IsNullOrEmpty(), $"Check-in response is invalid : {checkInResult} for PNR : {MockUpData.pnrLocator}");

            // Getting passenger IDs as a list
            List<string> selectedPassengerIds = OCIHelper.GetPassengersListFromCheckInResponse(checkInGraphQLResponse);

            if (selectedPassengerIds.Count > 1)
            {
                string selectPassengersGraphQLRequest = GraphQLRequestBuilder.SelectPassengersGraphQLRequest(checkInId, selectedPassengerIds);

                HttpResponseMessage selectPassengersGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(selectPassengersGraphQLRequest).EnsureGQLResponseSuccess();
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectToken("$..currentSlice.segments[0].destination.countryCode").ToString();

            foreach (JToken passenger in passengers)
            {
                string passengerGivenNames = passenger["givenNames"]?.ToString();
                string passengerSurname = passenger["surname"]?.ToString();
                string passengerId = passenger["passengerId"]?.ToString();
                string dateOfBirth = passenger["dateOfBirth"]?.ToString();

                // Retrieve passport number
                string passportNumber = PassportGenerator.GeneratePassport(destinationCountryCode);

                Assert.False(string.IsNullOrEmpty(passportNumber), $"Passport number is missing for country {destinationCountryCode}.");

                string savePassportGraphQLRequest = GraphQLRequestBuilder.SavePassportGraphQLRequest(checkInId, passengerId, passengerGivenNames, passengerSurname, dateOfBirth, passportNumber, destinationCountryCode, destinationCountryCode, destinationCountryCode);

                HttpResponseMessage savePassportGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(savePassportGraphQLRequest).EnsureGQLResponseSuccess();

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            }

            //Extract passenger IDs as list for those return date is required.
            string[] passengerIdsReturnDateRequired = checkInResult?.SelectTokens("$..passengers[?(@.requirements.returnDate == 'REQUIRED')].passengerId").Select(x => x.ToString()).ToArray();

            if (passengerIdsReturnDateRequired != null && passengerIdsReturnDateRequired.Length > 0)
            {
                foreach (string passengerId in passengerIdsReturnDateRequired)
                {
                    string returnDate = DataGenerator.GetRandomDateWithinNextMonth();

                    string saveReturnDateGraphQLRequest = GraphQLRequestBuilder.SaveReturnDateGraphQLRequest(checkInId, passengerId, returnDate);

                    HttpResponseMessage saveReturnDateGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveReturnDateGraphQLRequest).EnsureGQLResponseSuccess();

                    JToken saveReturnDateResult = JsonConvert.DeserializeObject<JToken>(saveReturnDateGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                    bool saveReturnDateAcceptedStatus = saveReturnDateResult.SelectToken("$..acceptedStatus").Value<bool>();

                    Assert.True(saveReturnDateAcceptedStatus, $"Return date for checkin id : {checkInId} and passenger : {passengerId} not saved ");
                }
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        }

        [Fact]
        public void CheckInGraphQLTests_LocalRun_Success()
        {
            // Add the values below when performing check-in for any ad-hoc PNR.
            string pnrLocator = "";
            string givenNames = "";
            string surname = "";
            string originCode = "";
            int passengersCount = 0;

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
            }

            JToken passengers = checkInResult?.SelectToken("$..passengers");

            string destinationCountryCode = checkInResult?.SelectTokens("$..currentSlice.segments[*].destination.countryCode")?.LastOrDefault()?.ToString();

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

                JToken savePassportResult = JsonConvert.DeserializeObject<JToken>(savePassportGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                Assert.True(isCheckInAvailableAfterPassportSubmission, $"Check-in is not available for request : {savePassportGraphQLRequest} and \n response is : {savePassportResult.ToString()}");

                bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
            }

            string saveEmergencyContactsGraphQLRequest = GraphQLRequestBuilder.SaveEmergencyContactsGraphQLRequest(checkInResult, checkInId);

            HttpResponseMessage saveEmergencyContactsGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveEmergencyContactsGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveEmergencyContactsResult = JsonConvert.DeserializeObject<JToken>(saveEmergencyContactsGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string saveContactTracingGraphQLRequest = GraphQLRequestBuilder.SaveContactTracingGraphQLRequest(checkInId, selectedPassengerIds);

            HttpResponseMessage saveContactTracingGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveContactTracingGraphQLRequest).EnsureGQLResponseSuccess();

            JToken saveContactTracingResult = JsonConvert.DeserializeObject<JToken>(saveContactTracingGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string generateBoardingPassGraphQLRequest = GraphQLRequestBuilder.GenerateBoardingPassGraphQLRequest(checkInId);

            HttpResponseMessage generateBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(generateBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken generateBoardingPassResult = JsonConvert.DeserializeObject<JToken>(generateBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(generateBoardingPassResult), $"Desktop boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            IEnumerable<JToken> qrCodes = generateBoardingPassResult.SelectTokens("$..boardingPasses[*].qrCode");

            string qrCode = qrCodes.FirstOrDefault()?.ToString();

            Assert.True(!qrCode.IsNullOrEmpty(), $"Boarding pass not generated for request : {generateBoardingPassGraphQLRequest} and \n response : {generateBoardingPassResult}");

            string retrieveBoardingPassGraphQLRequest = GraphQLRequestBuilder.RetrieveBoardingPassGraphQLRequest(pnrLocator, originCode, givenNames, surname);

            HttpResponseMessage retrieveBoardingPassGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(retrieveBoardingPassGraphQLRequest).EnsureGQLResponseSuccess();

            JToken retrieveBoardingPassResult = JsonConvert.DeserializeObject<JToken>(retrieveBoardingPassGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(GraphQLAssertions.ValidateDesktopBoardingPassBase64(retrieveBoardingPassResult), $"Desktop boarding pass not returned for request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            IEnumerable<JToken> retrieveBoardingPassQRCodes = retrieveBoardingPassResult.SelectTokens("$..boardingPasses[*].qrCode");

            string retrieveBoardingPassQRCode = retrieveBoardingPassQRCodes.FirstOrDefault()?.ToString();

            Assert.True(!retrieveBoardingPassQRCode.IsNullOrEmpty(), $"Boarding pass not retuned for the request : {retrieveBoardingPassGraphQLRequest} and \n response : {retrieveBoardingPassResult}");

            string checkInStatus = retrieveBoardingPassResult?.SelectToken("$..checkInStatus")?.ToString();
        }
    }
}
