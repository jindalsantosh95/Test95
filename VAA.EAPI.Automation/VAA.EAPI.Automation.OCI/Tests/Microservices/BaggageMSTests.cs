using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.OCI.Constants;
using VAA.EAPI.Automation.OCI.Requests.Endpoints;
using SearchMicroserviceEndpoint = VAA.EAPI.Automation.Search.Requests.Endpoints.MicroserviceEndpoint;

namespace VAA.EAPI.Automation.OCI.Tests.Microservices
{
    public class BaggageMSTests
    {
        public static string ocpSubKey = Vault.Default.GetSecretValue("AVATEUNAFNCHECKINMICROSERV-Subscription-Automation-Key");
        public static string baggageOcpSubKey = Vault.Default.GetSecretValue("AVATEUNAFNBAGGAGEMICROSERV-Subscription-Automation-Key");

        [Theory]
        [Trait("Category", Traits.BaggageMicroserviceCiCd)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public void BaggageServiceTests(JourneyType journeyType, ReservationType reservationType)
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

            foreach (var passenger in passengers)
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

            //calling get baggage end point
            HttpResponseMessage getBaggageResponse = MicroserviceEndpoint.GetBaggageDetails(travelPartyId, baggageOcpSubKey, hidePaidBaggage: false).EnsureIsSuccessStatusCode();

            JToken getBaggageResult = JsonConvert.DeserializeObject<JToken>(getBaggageResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            bool hasAnyOffers = getBaggageResult?.SelectTokens("$.passengers[*].baggageOffers[*]").Any() ?? false;

            Assert.True(hasAnyOffers, $"Expected baggage offers to be returned, but none were found. Result: {getBaggageResult}");

            string saveBaggageRequest = MicroserviceRequestBuilder.SaveBaggageMSRequest(getBaggageResponse, travelPartyId);

            //calling save baggage end point
            HttpResponseMessage saveBaggageResponse = MicroserviceEndpoint.SaveBaggageDetails(saveBaggageRequest, baggageOcpSubKey).EnsureIsSuccessStatusCode();

            dynamic saveBaggageResult = JsonConvert.DeserializeObject<dynamic>(saveBaggageResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            //Create baggage basket request based on save baggage details.
            string createBaggageBasketRequest = MicroserviceRequestBuilder.CreateBaggageBasketRequest(getBaggageResponse);

            //Post basket micro service call.
            HttpResponseMessage baggageBasketResponse = SearchMicroserviceEndpoint.CreateBasket(createBaggageBasketRequest).EnsureIsSuccessStatusCode();

            string basket_getURL = baggageBasketResponse.RequestMessage.RequestUri.AbsoluteUri;

            // get basket create response
            HttpResponseMessage getBasketCreateResponse = SearchMicroserviceEndpoint.GetBasket(basket_getURL).EnsureIsSuccessStatusCode();

            string checkPaymentRequest = MicroserviceRequestBuilder.CheckPaymentMSRequest(travelPartyId);

            HttpResponseMessage checkPaymentResponse = MicroserviceEndpoint.CheckPayment(checkPaymentRequest, baggageOcpSubKey);
        }
    }
}
