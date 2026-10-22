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
    public class CheckInGraphQLValidations
    {
        [Theory]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        [InlineData("LHR", "DEL", "GBR")]
        [InlineData("LHR", "USA", "IND")]
        [InlineData("LHR", "USA", "GBR")]
        public void CheckInGraphQLTests_DirectFlights_DocumentAdvisory(string origin, string destination, string passportCountryCode)
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.OneWay.ToString(), ReservationType.REVENUE.ToString(), "OneWayDirectFlightScenarioRevenue", origin: origin, destination: destination);

            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out string errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string pnrLocator = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];
            string originCode = pnrDetails["origin"];
            string destinationCode = pnrDetails["destination"];
            int passengersCount = Convert.ToInt32(pnrDetails["passengersCount"]);
            string testScenario = pnrDetails["testScenario"];

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

                //Validations as per passport country code.
                if (passportCountryCode.Is("GBR") && destination.Is("DEL"))
                {
                    bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                    Assert.True(isCheckInAvailableAfterPassportSubmission, $"Save passport operation failed | SCENARIO => {testScenario}  | REQUEST => {savePassportGraphQLRequest} | RESPONSE => {savePassportResult}");

                    string returnDateStatus = savePassportResult.SelectToken($"$..passengers[?(@.passengerId == '{passengerId}')].requirements.returnDateStatus")?.ToString();

                    string secondaryDocumentStatus = savePassportResult.SelectToken($"$..passengers[?(@.passengerId == '{passengerId}')].requirements.secondaryDocumentStatus")?.ToString();

                    Assert.False(returnDateStatus.IsNullOrEmpty(), $"ReturnDateStatus was not returned as expected. It was returned as {returnDateStatus}");

                    Assert.False(secondaryDocumentStatus.IsNullOrEmpty(), $"Secondary document status was not returned as expected. It was returned as {secondaryDocumentStatus}");

                    Assert.True(returnDateStatus.Is("Required"), $"Returned Date Status was not returned as expected. It was returned as {returnDateStatus}");

                    Assert.True(secondaryDocumentStatus.Is("Required"), $"Secondary document status was not returned as expected. It was returned as {secondaryDocumentStatus}");

                    string saveReturnDateGraphQLRequest = GraphQLRequestBuilder.SaveReturnDateGraphQLRequest(checkInId, passengerId, returnDate);

                    HttpResponseMessage saveReturnDateGraphQLResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveReturnDateGraphQLRequest).EnsureGQLResponseSuccess();

                    JToken saveReturnDateResult = JsonConvert.DeserializeObject<JToken>(saveReturnDateGraphQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                    string onlineCheckInStatusCode = saveReturnDateResult.SelectToken("$..onlineCheckInStatus.code").ToString();

                    Assert.True(onlineCheckInStatusCode.Is("SecondaryDocNonETARequirement"), $"The response of save return date is not returned as expected : \n {saveReturnDateResult}");
                }
                else if (passportCountryCode.Is("IND") && destination.Is("USA"))
                {
                    bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                    Assert.False(isCheckInAvailableAfterPassportSubmission, $"Check-in is not available for request : {savePassportGraphQLRequest} and \n response is : {savePassportResult.ToString()}");

                    bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                    Assert.True(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
                }
                else if (passportCountryCode.Is("GBR") && destination.Is("USA"))
                {
                    bool isCheckInAvailableAfterPassportSubmission = savePassportResult.SelectToken("$..onlineCheckInStatus.available").Value<bool>();

                    Assert.False(isCheckInAvailableAfterPassportSubmission, $"Check-in is not available for request : {savePassportGraphQLRequest} and \n response is : {savePassportResult.ToString()}");

                    bool passportProvided = savePassportResult.SelectTokens($"$.data.SavePassport.passengers[?(@.passengerId =~ /{passengerId}/)].requirements.passportProvided").FirstOrDefault()?.Value<bool>() ?? false;

                    Assert.False(passportProvided, $"Passport details not saved for request : {savePassportGraphQLRequest} \n response : {savePassportResult}");
                }
            }
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void CheckInGraphQLTests_NonSeamlessCheckin_Advisory()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.OneWay.ToString(), ReservationType.REVENUE.ToString(), "OneWayNonSeamlessScenarioRevenue");

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

            string onlineCheckInStatusCode = checkInResult.SelectToken("$..onlineCheckInStatus.code").ToString();

            Assert.False(isCheckInAvailable, $"Check-in should not be available for non-seamless check-in request : {checkInGraphQLRequest} and \n response is : {checkInResult.ToString()}");

            Assert.True(onlineCheckInStatusCode == "redirectJVPartner", $"Expected status code not returned for request : {checkInGraphQLRequest} and \n response is : {checkInResult.ToString()}");
        }

        [Fact]
        [Trait("Category", Traits.CheckInGraphQLRegression)]
        public void CheckInGraphQLTests_ESTAB_DocumentAdvisory()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.OneWay.ToString(), ReservationType.REVENUE.ToString(), "OneWayESTAScenarioRevenue", LocationType.LHR.ToString(), LocationType.JFK.ToString(), "ESTAB");

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

                Assert.False(isCheckInAvailableAfterPassportSubmission, $"ESTAB expected response not returned for request : {savePassportGraphQLRequest} and \n response is : {savePassportResult.ToString()}");
            }
        }
    }
}
