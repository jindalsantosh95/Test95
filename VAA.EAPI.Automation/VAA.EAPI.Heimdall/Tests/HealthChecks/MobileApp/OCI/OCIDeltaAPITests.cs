using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Delta.Endpoint;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.OCI.Helpers;
using VAA.EAPI.Automation.OCI.Tests.Delta;

namespace VAA.EAPI.Automation.Heimdall.Tests.HealthChecks.MobileApp.OCI
{
    public class OCIDeltaAPITests : IClassFixture<TokenFixture>
    {
        public static string clientId = Vault.Default.GetSecretValue("PCCOciWebClientId");
        public static string trmVSDigitalTestId = Vault.Default.GetSecretValue("X-TRRM-VSDigital-Test");

        private readonly TokenFixture tokenFixture;
        private static string deltaAWSTRMAPIToken;

        public OCIDeltaAPITests(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;
            this.tokenFixture.AddTokenForDeltaAwsTRMAPIFromKeyVault("awsDelta");
            deltaAWSTRMAPIToken = tokenFixture.GetToken("awsDelta");
        }

        [Fact]
        [Trait("Category", GlobalTraits.OCIHealthCheck)]
        [Trait("Tag", GlobalTraits.OCIDeltaServices)]
        public void CheckInDeltaAPIsE2EFlowTest()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.OneWay.ToString(), ReservationType.REVENUE.ToString());

            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out var errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string recordLocator = pnrDetails["pnr"];
            string originCode = pnrDetails["origin"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];
            int passengersCount = Convert.ToInt32(pnrDetails["passengersCount"]);

            Assert.True(!string.IsNullOrEmpty(recordLocator) && !string.IsNullOrEmpty(originCode), "PNR and Origin should not be null or empty");

            var travelPartiesRequest = DeltaMicroserviceRequestBuilder.TravelPartiesRequest(recordLocator, originCode);

            // calling post travel party end point
            HttpResponseMessage travelPartiesResponse = PCCOciWeb.TravelParties(deltaAWSTRMAPIToken, travelPartiesRequest, clientId).EnsureIsSuccessStatusCode();

            JToken travelPartiesResult = JsonConvert.DeserializeObject<JToken>(travelPartiesResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string travelPartyId = travelPartiesResult.SelectToken("$.id")?.ToString();

            Assert.True(!string.IsNullOrEmpty(travelPartyId), $"Travel party ID not exists in response: {travelPartiesResult.ToString()}");

            if (passengersCount > 1)
            {
                var selectPassengersRequest = DeltaMicroserviceRequestBuilder.SelectPassengersRequest(travelPartiesResult);

                // calling post select passengers end point
                HttpResponseMessage selectPassengersResponse = PCCOciWeb.SelectPassengers(deltaAWSTRMAPIToken, selectPassengersRequest, travelPartyId, clientId).EnsureIsSuccessStatusCode();
            }

            var retrieveRequirementsRequest = DeltaMicroserviceRequestBuilder.RetrieveRequirements(recordLocator, givenNames, surname);

            HttpResponseMessage retrieveRequirementsResponse = PCCOciWeb.RetrieveRequirements(deltaAWSTRMAPIToken, clientId, trmVSDigitalTestId, retrieveRequirementsRequest).EnsureIsSuccessStatusCode();

            JToken retrieveRequirementsResult = JsonConvert.DeserializeObject<JToken>(retrieveRequirementsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string requirementsResourceId = retrieveRequirementsResult.SelectToken("$.requirementsResourceId")?.ToString();

            Assert.True(!string.IsNullOrEmpty(requirementsResourceId), $"Invalid response for retrieve requirements : {retrieveRequirementsResult}");

            HttpResponseMessage getRequirementsResponseByResourceId = PCCOciWeb.GetRequirementsByResourceId(deltaAWSTRMAPIToken, clientId, trmVSDigitalTestId, requirementsResourceId).EnsureIsSuccessStatusCode();

            dynamic getRequirementsResult = JsonConvert.DeserializeObject<dynamic>(getRequirementsResponseByResourceId.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(DynamicHelper.HasProperty(getRequirementsResult, "requirementsResourceId"), "RequirementsResourceId property not found, invalid response");

            var passengers = retrieveRequirementsResult["travelRequirements"]?["passengers"];

            foreach (var passenger in passengers)
            {
                var passengerResourceId = passenger["passengerResourceId"]?.ToString();
                var firstName = passenger["givenNames"]?.ToString();
                var lastName = passenger["surname"]?.ToString();
                var birthDate = OCIHelper.FormatBirthDate(passenger["birthDate"]?.ToString());

                var savePassportRequest = DeltaMicroserviceRequestBuilder.SavePassportRequest(firstName, lastName, birthDate, passengerResourceId);

                HttpResponseMessage savePassportResponse = PCCOciWeb.TrmApiPatchRequest(deltaAWSTRMAPIToken, clientId, trmVSDigitalTestId, savePassportRequest, requirementsResourceId).EnsureIsSuccessStatusCode();

                var saveEmergencyContactRequest = DeltaMicroserviceRequestBuilder.SaveEmergencyContactsRequest(passengerResourceId, firstName, lastName);

                HttpResponseMessage saveEmergencyContactResponse = PCCOciWeb.TrmApiPatchRequest(deltaAWSTRMAPIToken, clientId, trmVSDigitalTestId, saveEmergencyContactRequest, requirementsResourceId).EnsureIsSuccessStatusCode();

                string tripId = passenger.SelectToken("$..passengerTripHealthRequirements[*].tripId")?.ToString();

                var saveContactTracingRequest = DeltaMicroserviceRequestBuilder.SaveContactTracingRequest(passengerResourceId, tripId);

                HttpResponseMessage saveContactTracingResponse = PCCOciWeb.TrmApiPatchRequest(deltaAWSTRMAPIToken, clientId, trmVSDigitalTestId, saveContactTracingRequest, requirementsResourceId).EnsureIsSuccessStatusCode();
            }

            //call generate boarding documents end point
            HttpResponseMessage boardingDocumentsResponse = PCCOciWeb.BoardingDocuments(deltaAWSTRMAPIToken, travelPartyId, clientId).EnsureIsSuccessStatusCode();

            JToken boardingDocuments = JsonConvert.DeserializeObject<JToken>(boardingDocumentsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            bool checkinFailed = boardingDocuments.SelectToken("$.checkinFailed").Value<bool>();

            Assert.False(checkinFailed, $"Boarding documents generation failed \n {boardingDocuments.ToString()}");
        }
    }
}