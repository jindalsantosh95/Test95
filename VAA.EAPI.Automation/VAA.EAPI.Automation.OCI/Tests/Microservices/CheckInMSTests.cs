using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.Requests;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.OCI.Constants;
using VAA.EAPI.Automation.OCI.Requests.Endpoints;

namespace VAA.EAPI.Automation.OCI.Tests.Microservices
{
    public class CheckInMSTests
    {
        public static string ocpSubKey = Vault.Default.GetSecretValue("AVATEUNAFNCHECKINMICROSERV-Subscription-Automation-Key");

        [Theory]
        [InlineData(ResponseCode.OK)]
        [InlineData(ResponseCode.InternalServerError)]
        [InlineData(ResponseCode.Unauthorized)]
        [InlineData(ResponseCode.UnprocessableEntity)]
        public async Task OCICheckInAPIsSchemaValidation(ResponseCode expectedResponseCode)
        {
            // Retrieve schema definition
            string schemaResponse = Endpoints.GetSchema(Application.OCIMSEndPoint, ocpSubKey).EnsureIsSuccessStatusCode().ReadResponseAsString();

            // Prepare request data
            string journeyType = JourneyType.OneWay.ToString();
            string reservationType = ReservationType.REVENUE.ToString();

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable
                .RetrieveOCIPNRDetailsFromStorageTable(journeyType, reservationType);

            // Validate fetched PNR details
            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out string errorMsg) ? $"Test case failed: {errorMsg}" : string.Empty);

            string pnrLocator = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];
            string originCode = pnrDetails["origin"];

            Assert.True(!string.IsNullOrEmpty(pnrLocator) && !string.IsNullOrEmpty(givenNames) && !string.IsNullOrEmpty(surname) && !string.IsNullOrEmpty(originCode), "PNRLocator, GivenNames, Surname and Origin should not be null or empty");

            // Build request based on the expected response scenario
            string checkInRequest = expectedResponseCode switch
            {
                ResponseCode.Unauthorized => MicroserviceRequestBuilder.CheckInMSRequest(pnrLocator, givenNames, surname, originCode),
                ResponseCode.InternalServerError => MicroserviceRequestBuilder.CheckInMSRequest(pnrLocator, givenNames, surname, "123"),
                ResponseCode.UnprocessableEntity => MicroserviceRequestBuilder.CheckInMSRequest("pnrLocator", "givenNames", "surname", "originCode"),
                ResponseCode.OK => MicroserviceRequestBuilder.CheckInMSRequest(pnrLocator, givenNames, surname, originCode),
                _ => throw new ArgumentException($"Unsupported response code: {expectedResponseCode}")
            };

            // Send request
            string authKey = expectedResponseCode == ResponseCode.Unauthorized ? "InvalidOcpSubKey" : ocpSubKey;
            HttpResponseMessage response = MicroserviceEndpoint.CheckIn(checkInRequest, authKey);

            if (expectedResponseCode == ResponseCode.OK)
            {
                response.EnsureIsSuccessStatusCode();
            }

            // Read and validate response
            string actualResponseCode = ((int)response.StatusCode).ToString();
            string responseContent = await response.Content.ReadAsStringAsync();
            string schemaName = "/CheckIn";

            SchemaValidator.JsonSchemaValidator(responseContent, schemaResponse, schemaName, actualResponseCode);
        }

        [Theory]
        [Trait("Category", Traits.CheckInMicroserviceCiCd)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public void CheckInServiceTests(JourneyType journeyType, ReservationType reservationType)
        {
            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(journeyTypeString, reservationTypeString);

            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out string errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string pnrLocator = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];
            string originCode = pnrDetails["origin"];

            Assert.True(!string.IsNullOrEmpty(pnrLocator) && !string.IsNullOrEmpty(givenNames) && !string.IsNullOrEmpty(surname) && !string.IsNullOrEmpty(originCode), "PNRLocator, GivenNames, Surname and Origin should not be null or empty");

            string checkInRequest = MicroserviceRequestBuilder.CheckInMSRequest(pnrLocator, givenNames, surname, originCode);

            // calling post check in end point
            HttpResponseMessage checkInResponse = MicroserviceEndpoint.CheckIn(checkInRequest, ocpSubKey).EnsureIsSuccessStatusCode();

            JToken checkInResult = JsonConvert.DeserializeObject<JToken>(checkInResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            // Extract TravelPartyId and RequirementsId
            string travelPartyId = checkInResult?.SelectToken("$.travelPartyId")?.ToString();

            string requirementsId = checkInResult?.SelectToken("$.requirementsId")?.ToString();

            Assert.True(!travelPartyId.IsNullOrEmpty() && !requirementsId.IsNullOrEmpty(), $"Check-in response is invalid : {checkInResult} for PNR : {pnrLocator} ");

            // Extract passenger IDs as a list
            IEnumerable<JToken> passengerTokens = checkInResult?.SelectTokens("$..passengerId");

            List<string> selectedPassengerIds = passengerTokens?.Select(p => p.ToString()).ToList() ?? new List<string>();

            if (selectedPassengerIds.Count > 1)
            {
                string selectPassengersRequest = MicroserviceRequestBuilder.SelectPassengersMSRequest(travelPartyId, givenNames, surname, pnrLocator, originCode, selectedPassengerIds);

                // calling post select passengers end point
                HttpResponseMessage selectPassengersResponse = MicroserviceEndpoint.SelectPassengers(selectPassengersRequest, ocpSubKey).EnsureIsSuccessStatusCode();
            }

            string jsonResponse = checkInResponse.ReadResponseAsString();
            JObject root = JObject.Parse(jsonResponse);
            JArray passengers = (JArray)root["passengers"];

            foreach (JToken passenger in passengers)
            {
                string passengerGivenNames = passenger["givenNames"]?.ToString();
                string passengerSurname = passenger["surname"]?.ToString();
                string passengerId = passenger["passengerId"]?.ToString();
                string dateOfBirth = passenger["dateOfBirth"]?.ToString();

                string savePassportRequest = MicroserviceRequestBuilder.SavePassportMSRequest(travelPartyId, requirementsId, passengerId, passengerGivenNames, passengerSurname, dateOfBirth);

                // calling post save passport end point
                HttpResponseMessage savePassportResponse = MicroserviceEndpoint.SavePassport(savePassportRequest, ocpSubKey).EnsureIsSuccessStatusCode();

                dynamic savePassportResult = JsonConvert.DeserializeObject<dynamic>(savePassportResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            }

            string saveEmergencyContactRequest = MicroserviceRequestBuilder.SaveEmergencyContactMSRequest(checkInResponse, travelPartyId, requirementsId);

            //calling emergency contact end point
            HttpResponseMessage saveEmergencyContactResponse = MicroserviceEndpoint.SaveEmergencyContact(saveEmergencyContactRequest, ocpSubKey).EnsureIsSuccessStatusCode();

            dynamic saveEmergencyContactResult = JsonConvert.DeserializeObject<dynamic>(saveEmergencyContactResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string contactTracingRequest = MicroserviceRequestBuilder.SaveContactTracingMSRequest(travelPartyId, requirementsId, selectedPassengerIds);

            //calling contract tarcing end point
            HttpResponseMessage saveContactTracingResponse = MicroserviceEndpoint.SaveContactTracing(contactTracingRequest, ocpSubKey).EnsureIsSuccessStatusCode();

            dynamic saveContactTracingResult = JsonConvert.DeserializeObject<dynamic>(saveContactTracingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        }

        [Fact]
        [Trait("Category", Traits.CheckInMicroserviceCiCd)]
        public void CheckInService_WithMockRequestAndResponse_ShouldSucceed()
        {
            //Check-in request creation
            string checkInRequest = MicroserviceRequestBuilder.CheckInMSRequest(MockUpData.pnrLocator, MockUpData.givenNames, MockUpData.surname, MockUpData.originCode, MockUpData.eTicketNumber);

            // calling post check in end point
            HttpResponseMessage checkInResponse = MicroserviceEndpoint.CheckIn(checkInRequest, ocpSubKey).EnsureIsSuccessStatusCode();

            JToken checkInResult = JsonConvert.DeserializeObject<JToken>(checkInResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string pnrLocatorInResponse = checkInResult?.SelectToken("$.reference.pnrLocator")?.ToString();

            Assert.True(MockUpData.pnrLocator == pnrLocatorInResponse, "PNR locator not matched");

            // Extract TravelPartyId and RequirementsId
            string travelPartyId = checkInResult?.SelectToken("$.travelPartyId")?.ToString();

            string requirementsId = checkInResult?.SelectToken("$.requirementsId")?.ToString();

            Assert.True(!travelPartyId.IsNullOrEmpty() && !requirementsId.IsNullOrEmpty(), $"Check-in response is invalid : {checkInResult} for PNR : {MockUpData.pnrLocator} ");

            // Extract passenger IDs as a list
            IEnumerable<JToken> passengerTokens = checkInResult?.SelectTokens("$..passengerId");

            List<string> selectedPassengerIds = passengerTokens?.Select(p => p.ToString()).ToList() ?? new List<string>();

            string selectPassengersRequest = MicroserviceRequestBuilder.SelectPassengersMSRequest(travelPartyId, MockUpData.givenNames, MockUpData.surname, MockUpData.pnrLocator, MockUpData.originCode, selectedPassengerIds);

            // calling post select passengers end point
            HttpResponseMessage selectPassengersResponse = MicroserviceEndpoint.SelectPassengers(selectPassengersRequest, ocpSubKey).EnsureIsSuccessStatusCode();

            string jsonResponse = checkInResponse.ReadResponseAsString();
            JObject root = JObject.Parse(jsonResponse);
            JArray passengers = (JArray)root["passengers"];

            foreach (JToken passenger in passengers)
            {
                string passengerGivenNames = passenger["givenNames"]?.ToString();
                string passengerSurname = passenger["surname"]?.ToString();
                string passengerId = passenger["passengerId"]?.ToString();
                string dateOfBirth = passenger["dateOfBirth"]?.ToString();

                string savePassportRequest = MicroserviceRequestBuilder.SavePassportMSRequest(travelPartyId, requirementsId, passengerId, passengerGivenNames, passengerSurname, dateOfBirth);

                // calling post save passport end point
                HttpResponseMessage savePassportResponse = MicroserviceEndpoint.SavePassport(savePassportRequest, ocpSubKey).EnsureIsSuccessStatusCode();

                dynamic savePassportResult = JsonConvert.DeserializeObject<dynamic>(savePassportResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            }

            string saveEmergencyContactRequest = MicroserviceRequestBuilder.SaveEmergencyContactMSRequest(checkInResponse, travelPartyId, requirementsId);

            //calling emergency contact end point
            HttpResponseMessage saveEmergencyContactResponse = MicroserviceEndpoint.SaveEmergencyContact(saveEmergencyContactRequest, ocpSubKey).EnsureIsSuccessStatusCode();

            dynamic saveEmergencyContactResult = JsonConvert.DeserializeObject<dynamic>(saveEmergencyContactResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string contactTracingRequest = MicroserviceRequestBuilder.SaveContactTracingMSRequest(travelPartyId, requirementsId, selectedPassengerIds);

            //calling contract tarcing end point
            HttpResponseMessage saveContactTracingResponse = MicroserviceEndpoint.SaveContactTracing(contactTracingRequest, ocpSubKey).EnsureIsSuccessStatusCode();

            dynamic saveContactTracingResult = JsonConvert.DeserializeObject<dynamic>(saveContactTracingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        }
    }
}
