using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Delta.Endpoint;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.TestDataConfig.PaymentCards;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.OCI.Constants;
using VAA.EAPI.Automation.OCI.Helpers;
using VAA.EAPI.Automation.OCI.Tests.Microservices;

namespace VAA.EAPI.Automation.OCI.Tests.Delta
{
    public class DeltaOciAPIMSTests : IClassFixture<TokenFixture>
    {
        public static string clientId = Vault.Default.GetSecretValue("PCCOciWebClientId");
        public static string trmVSDigitalTestId = Vault.Default.GetSecretValue("X-TRRM-VSDigital-Test");
        private static readonly string BagsPaymentCardUsdCurrency = Vault.Default.GetSecretValue("Bags-PaymentCard-USD");
        private static readonly string BagsPaymentCardAllCurrency = Vault.Default.GetSecretValue("Bags-PaymentCard-AllCurrencies");

        private readonly TokenFixture tokenFixture;
        private static string deltaAWSTRMAPIToken;

        public DeltaOciAPIMSTests(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;
            this.tokenFixture.AddTokenForDeltaAwsTRMAPIFromKeyVault("awsDelta");
            deltaAWSTRMAPIToken = tokenFixture.GetToken("awsDelta");
        }

        [Theory]
        [Trait("Category", Traits.DeltaOciMicroservices)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public void CheckInSsciAPITests(JourneyType journeyType, ReservationType reservationType)
        {
            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(journeyTypeString, reservationTypeString);

            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out string? errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string recordLocator = pnrDetails["pnr"];
            string originCode = pnrDetails["origin"];
            int passengersCount = Convert.ToInt32(pnrDetails["passengersCount"]);

            Assert.True(!string.IsNullOrEmpty(recordLocator) && !string.IsNullOrEmpty(originCode), "PNR and Origin should not be null or empty");

            string travelPartiesRequest = DeltaMicroserviceRequestBuilder.TravelPartiesRequest(recordLocator, originCode);

            // calling post travel party end point
            HttpResponseMessage travelPartiesResponse = PCCOciWeb.TravelParties(deltaAWSTRMAPIToken, travelPartiesRequest, clientId).EnsureIsSuccessStatusCode();

            JToken travelPartiesResult = JsonConvert.DeserializeObject<JToken>(travelPartiesResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string travelPartyId = travelPartiesResult.SelectToken("$.id")?.ToString();

            Assert.True(!string.IsNullOrEmpty(travelPartyId), $"Travel party ID not exists in response: {travelPartiesResult.ToString()}");

            if (passengersCount > 1)
            {
                string selectPassengersRequest = DeltaMicroserviceRequestBuilder.SelectPassengersRequest(travelPartiesResult);

                bool selectedPassengersValid = MicroserviceAssertions.ValidateSelectedPassengerFieldsNotNullOrEmpty(selectPassengersRequest, travelPartiesResult, out string selectedPassengersFailureMessage);

                Assert.True(selectedPassengersValid, selectedPassengersFailureMessage);

                // calling post select passengers end point
                HttpResponseMessage selectPassengersResponse = PCCOciWeb.SelectPassengers(deltaAWSTRMAPIToken, selectPassengersRequest, travelPartyId, clientId).EnsureIsSuccessStatusCode();
            }
        }

        [Theory]
        [Trait("Category", Traits.DeltaOciMicroservices)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public void CheckInTRMAPITests(JourneyType journeyType, ReservationType reservationType)
        {
            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(journeyTypeString, reservationTypeString);

            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out string? errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string recordLocator = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];

            Assert.True(!string.IsNullOrEmpty(recordLocator) && !string.IsNullOrEmpty(givenNames) && !string.IsNullOrEmpty(surname), "PNR, givenNames and surname should not be null or empty");

            string retrieveRequirementsRequest = DeltaMicroserviceRequestBuilder.RetrieveRequirements(recordLocator, givenNames, surname);

            HttpResponseMessage retrieveRequirementsResponse = PCCOciWeb.RetrieveRequirements(deltaAWSTRMAPIToken, clientId, trmVSDigitalTestId, retrieveRequirementsRequest).EnsureIsSuccessStatusCode();

            JToken retrieveRequirementsResult = JsonConvert.DeserializeObject<JToken>(retrieveRequirementsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string requirementsResourceId = retrieveRequirementsResult.SelectToken("$.requirementsResourceId")?.ToString();

            Assert.True(!string.IsNullOrEmpty(requirementsResourceId), $"Invalid response for retrieve requirements : {retrieveRequirementsResult}");

            HttpResponseMessage getRequirementsResponseByResourceId = PCCOciWeb.GetRequirementsByResourceId(deltaAWSTRMAPIToken, clientId, trmVSDigitalTestId, requirementsResourceId).EnsureIsSuccessStatusCode();

            dynamic getRequirementsResult = JsonConvert.DeserializeObject<dynamic>(getRequirementsResponseByResourceId.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(DynamicHelper.HasProperty(getRequirementsResult, "requirementsResourceId"), "RequirementsResourceId property not found, invalid response");
        }

        [Fact(Skip = "Local run only; requires a PNR and details")]
        public async Task DeltaApi_Bags_Purchase_E2E_Flow_LocalRun()
        {
            string recordLocator = "";
            string originCode = "";
            string givenNames = "";
            string surname = "";

            Assert.True(
                !recordLocator.IsNullOrWhiteSpace()
                && !originCode.IsNullOrWhiteSpace()
                && !givenNames.IsNullOrWhiteSpace()
                && !surname.IsNullOrWhiteSpace(),
                "Set the PNR, origin, given names, and surname before manually running this live payment test.");

            string travelPartiesRequest = DeltaMicroserviceRequestBuilder.TravelPartiesRequest(recordLocator, originCode);
            HttpResponseMessage travelPartiesResponse = PCCOciWeb.TravelParties(deltaAWSTRMAPIToken, travelPartiesRequest, clientId).EnsureIsSuccessStatusCode();

            JObject travelPartiesResult = JObject.Parse(travelPartiesResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string travelPartyId = travelPartiesResult.RequireToken("id").ToString();

            JArray passengers = Assert.IsType<JArray>(travelPartiesResult.SelectTokens("$..passengers").OfType<JArray>().FirstOrDefault());
            Assert.NotEmpty(passengers);

            JObject firstPassenger = Assert.IsType<JObject>(passengers[0]);
            string passengerId = firstPassenger.RequireToken("id").ToString();
            string customerId = firstPassenger.RequireToken("customerId").ToString();
            string firstNin = firstPassenger.RequireToken("firstNIN").ToString();
            string lastNin = firstPassenger.RequireToken("lastNIN").ToString();

            if (passengers.Count > 1)
            {
                string selectPassengerRequest = DeltaMicroserviceRequestBuilder.SelectPassengerRequest(passengerId, customerId, firstNin, lastNin);

                PCCOciWeb.SelectPassengers(deltaAWSTRMAPIToken, selectPassengerRequest, travelPartyId, clientId).EnsureIsSuccessStatusCode();
            }

            JObject paymentPassenger = SavePassportsForAllPassengers(recordLocator, givenNames, surname);

            HttpResponseMessage baggageOffersResponse = PCCOciWeb.GetBaggage(deltaAWSTRMAPIToken, travelPartyId, clientId).EnsureIsSuccessStatusCode();

            JObject baggageOffersResult = JObject.Parse(baggageOffersResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            JArray passengerBagInfo = Assert.IsType<JArray>(baggageOffersResult.RequireToken("passengerBagInfo"));

            JObject firstPassengerBagInfo = Assert.IsType<JObject>(Assert.Single(passengerBagInfo));

            string passengerNin = $"{firstPassengerBagInfo.RequireToken("firstNIN")}.{firstPassengerBagInfo.RequireToken("lastNIN")}";

            JArray baggagePrices = Assert.IsType<JArray>(firstPassengerBagInfo.RequireToken("baggagePrices"));

            Assert.NotEmpty(baggagePrices);

            JObject oneBagOffer = baggagePrices
                .OfType<JObject>()
                .FirstOrDefault(baggageOffer => decimal.TryParse(baggageOffer["baggagePrice"]?.ToString(), out decimal offeredBagFee) && offeredBagFee > 0)
                ?? throw new Xunit.Sdk.XunitException($"No paid baggage offer was returned for passenger {passengerNin}: {baggageOffersResult}");

            string offerItemId = oneBagOffer.RequireToken("offerItemId").ToString();
            string bagFee = oneBagOffer.RequireToken("baggagePrice").ToString();

            Assert.True(decimal.TryParse(bagFee, out decimal parsedBagFee), $"Baggage price is invalid for passenger {passengerNin}: {baggageOffersResult}");

            string addOneBagRequest = DeltaMicroserviceRequestBuilder.AddOneBagRequest(passengerNin, offerItemId, bagFee);

            PCCOciWeb.AddBags(deltaAWSTRMAPIToken, travelPartyId, addOneBagRequest, clientId).EnsureIsSuccessStatusCode();

            string updateBagsRequest = DeltaMicroserviceRequestBuilder.UpdateBagsRequest(pendingPayment: true);

            PCCOciWeb.UpdateBags(deltaAWSTRMAPIToken, travelPartyId, updateBagsRequest, clientId).EnsureIsSuccessStatusCode();

            HttpResponseMessage reviewAndPayDetailsResponse = PCCOciWeb.ReviewAndPayDetails(deltaAWSTRMAPIToken, travelPartyId, clientId).EnsureIsSuccessStatusCode();

            JObject reviewAndPayDetailsResult = JObject.Parse(reviewAndPayDetailsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            JArray bagPurchaseDetails = Assert.IsType<JArray>(reviewAndPayDetailsResult.RequireToken("bagPurchaseDetails"));

            Assert.Single(bagPurchaseDetails);

            string totalCost = bagPurchaseDetails[0].RequireToken("totalCost").ToString();

            Assert.True(decimal.TryParse(totalCost, out decimal parsedTotalCost), $"Bag purchase total cost is invalid: {reviewAndPayDetailsResult}");

            Assert.Equal(parsedBagFee, parsedTotalCost);

            JArray allPassengerAmountDueTotal = Assert.IsType<JArray>(reviewAndPayDetailsResult.RequireToken("allPassengerAmountDueTotal"));
            JObject totalAmountDue = Assert.IsType<JObject>(Assert.Single(allPassengerAmountDueTotal));
            string totalAmount = totalAmountDue.RequireToken("price").ToString();
            string currencyCode = totalAmountDue.RequireToken("currency").ToString();

            Assert.True(decimal.TryParse(totalAmount, out decimal parsedTotalAmount), $"Total payment amount is invalid: {reviewAndPayDetailsResult}");

            string paymentFirstName = paymentPassenger.RequireToken("givenNames").ToString();
            string paymentLastName = paymentPassenger.RequireToken("surname").ToString();
            string paymentBirthDate = OCIHelper.FormatBirthDate(paymentPassenger.RequireToken("birthDate").ToString());
            string paymentReferenceIdRequest = DeltaMicroserviceRequestBuilder.GeneratePaymentReferenceIdRequest(
                recordLocator,
                paymentFirstName,
                paymentLastName,
                paymentBirthDate,
                parsedTotalAmount,
                currencyCode,
                parsedBagFee);

            HttpResponseMessage paymentReferenceIdResponse = PCCOciWeb.GeneratePaymentReferenceId(deltaAWSTRMAPIToken, paymentReferenceIdRequest).EnsureIsSuccessStatusCode();
            JObject paymentReferenceIdResult = JObject.Parse(paymentReferenceIdResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            string paymentReferenceId = paymentReferenceIdResult.RequireToken("paymentReferenceId").ToString();

            Assert.False(paymentReferenceId.IsNullOrWhiteSpace(), $"Payment reference ID is missing: {paymentReferenceIdResult}");

            TestConfig hostedPaymentConfig = new();
            hostedPaymentConfig.Book.Order.HppUrl = Config.DeltaHostedPaymentEndPoint.GetUrl(paymentReferenceId);
            string cardNumber = currencyCode.Is("USD") ? BagsPaymentCardUsdCurrency : BagsPaymentCardAllCurrency;

            Assert.False(cardNumber.IsNullOrWhiteSpace(), "The configured bags payment card is missing.");

            hostedPaymentConfig.Book.Payment = new Payment
            {
                CardDetails = new CardDetails
                {
                    CardNumber = cardNumber
                }
            };

            await HostedPaymentAutomation.SubmitCardAsync(
                hostedPaymentConfig,
                usePlaywright: true,
                expectedTotalPrice: (double)parsedTotalAmount);

            HttpResponseMessage paymentDetailsResponse = PCCOciWeb.GetPaymentDetails(deltaAWSTRMAPIToken, paymentReferenceId).EnsureIsSuccessStatusCode();
            JObject paymentDetailsResult = JObject.Parse(paymentDetailsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            JObject paymentDetails = JObject.Parse(paymentDetailsResult.RequireToken("objectContentText").ToString());
            string confirmedPaymentReferenceId = paymentDetails.RequireToken("paymentReferenceId").ToString();
            string confirmedPaymentAmount = paymentDetails.RequireToken("price.currencyEquivalentPrice.currencyAmt").ToString();
            string confirmedCurrencyCode = paymentDetails.RequireToken("price.currencyEquivalentPrice.currencyCode").ToString();

            Assert.True(confirmedPaymentReferenceId.Is(paymentReferenceId), "Payment details returned an unexpected payment reference ID.");
            Assert.True(decimal.TryParse(confirmedPaymentAmount, out decimal parsedConfirmedPaymentAmount), "Payment details returned an invalid payment amount.");
            Assert.Equal(parsedTotalAmount, parsedConfirmedPaymentAmount);
            Assert.True(confirmedCurrencyCode.Is(currencyCode), "Payment details returned an unexpected currency code.");

            string submitReviewAndPayDetailsRequest = DeltaMicroserviceRequestBuilder.ReviewAndPayDetailsRequest(paymentDetails, passengerNin);
            HttpResponseMessage submitReviewAndPayDetailsResponse = PCCOciWeb.SubmitReviewAndPayDetails(
                deltaAWSTRMAPIToken,
                travelPartyId,
                submitReviewAndPayDetailsRequest,
                clientId).EnsureIsSuccessStatusCode();
            JObject submitReviewAndPayDetailsResult = JObject.Parse(submitReviewAndPayDetailsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(submitReviewAndPayDetailsResult.RequireToken("nextAction").ToString().Is("success"), "Review and pay details did not complete successfully.");
            Assert.False(submitReviewAndPayDetailsResult.RequireToken("showPaymentWidget").Value<bool>(), "Review and pay details still requires payment input.");
        }

        private JObject SavePassportsForAllPassengers(string recordLocator, string givenNames, string surname)
        {
            string retrieveRequirementsRequest = DeltaMicroserviceRequestBuilder.RetrieveRequirements(recordLocator, givenNames, surname);
            HttpResponseMessage retrieveRequirementsResponse = PCCOciWeb.RetrieveRequirements(deltaAWSTRMAPIToken, clientId, trmVSDigitalTestId, retrieveRequirementsRequest).EnsureIsSuccessStatusCode();
            JObject retrieveRequirementsResult = JObject.Parse(retrieveRequirementsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            string requirementsResourceId = retrieveRequirementsResult.RequireToken("requirementsResourceId").ToString();
            JArray passengers = Assert.IsType<JArray>(retrieveRequirementsResult.RequireToken("travelRequirements.passengers"));

            Assert.NotEmpty(passengers);

            foreach (JToken passenger in passengers)
            {
                string passengerResourceId = passenger.RequireToken("passengerResourceId").ToString();
                string firstName = passenger.RequireToken("givenNames").ToString();
                string lastName = passenger.RequireToken("surname").ToString();
                string birthDate = OCIHelper.FormatBirthDate(passenger.RequireToken("birthDate").ToString());
                string savePassportRequest = DeltaMicroserviceRequestBuilder.SavePassportRequest(firstName, lastName, birthDate, passengerResourceId);

                PCCOciWeb.TrmApiPatchRequest(deltaAWSTRMAPIToken, clientId, trmVSDigitalTestId, savePassportRequest, requirementsResourceId).EnsureIsSuccessStatusCode();
            }

            return passengers
                .OfType<JObject>()
                .FirstOrDefault(passenger => passenger.RequireToken("givenNames").ToString().Is(givenNames)
                    && passenger.RequireToken("surname").ToString().Is(surname))
                ?? throw new Xunit.Sdk.XunitException($"No travel-requirements passenger matches the PNR passenger: {retrieveRequirementsResult}");
        }

        [Fact]
        public void CheckInDeltaAPIsE2EFlowTest()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(JourneyType.OneWay.ToString(), ReservationType.REVENUE.ToString());

            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out string? errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string recordLocator = pnrDetails["pnr"];
            string originCode = pnrDetails["origin"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];
            int passengersCount = Convert.ToInt32(pnrDetails["passengersCount"]);

            Assert.True(!string.IsNullOrEmpty(recordLocator) && !string.IsNullOrEmpty(originCode), "PNR and Origin should not be null or empty");

            string travelPartiesRequest = DeltaMicroserviceRequestBuilder.TravelPartiesRequest(recordLocator, originCode);

            // calling post travel party end point
            HttpResponseMessage travelPartiesResponse = PCCOciWeb.TravelParties(deltaAWSTRMAPIToken, travelPartiesRequest, clientId).EnsureIsSuccessStatusCode();

            JToken travelPartiesResult = JsonConvert.DeserializeObject<JToken>(travelPartiesResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string travelPartyId = travelPartiesResult.SelectToken("$.id")?.ToString();

            Assert.True(!string.IsNullOrEmpty(travelPartyId), $"Travel party ID not exists in response: {travelPartiesResult.ToString()}");

            if (passengersCount > 1)
            {
                string selectPassengersRequest = DeltaMicroserviceRequestBuilder.SelectPassengersRequest(travelPartiesResult);

                // calling post select passengers end point
                HttpResponseMessage selectPassengersResponse = PCCOciWeb.SelectPassengers(deltaAWSTRMAPIToken, selectPassengersRequest, travelPartyId, clientId).EnsureIsSuccessStatusCode();
            }

            string retrieveRequirementsRequest = DeltaMicroserviceRequestBuilder.RetrieveRequirements(recordLocator, givenNames, surname);

            HttpResponseMessage retrieveRequirementsResponse = PCCOciWeb.RetrieveRequirements(deltaAWSTRMAPIToken, clientId, trmVSDigitalTestId, retrieveRequirementsRequest).EnsureIsSuccessStatusCode();

            JToken retrieveRequirementsResult = JsonConvert.DeserializeObject<JToken>(retrieveRequirementsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string requirementsResourceId = retrieveRequirementsResult.SelectToken("$.requirementsResourceId")?.ToString();

            Assert.True(!string.IsNullOrEmpty(requirementsResourceId), $"Invalid response for retrieve requirements : {retrieveRequirementsResult}");

            HttpResponseMessage getRequirementsResponseByResourceId = PCCOciWeb.GetRequirementsByResourceId(deltaAWSTRMAPIToken, clientId, trmVSDigitalTestId, requirementsResourceId).EnsureIsSuccessStatusCode();

            dynamic getRequirementsResult = JsonConvert.DeserializeObject<dynamic>(getRequirementsResponseByResourceId.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(DynamicHelper.HasProperty(getRequirementsResult, "requirementsResourceId"), "RequirementsResourceId property not found, invalid response");

            JToken? passengers = retrieveRequirementsResult["travelRequirements"]?["passengers"];

            Assert.False(passengers == null, $"travelRequirements.passengers is missing from the response.{retrieveRequirementsResult}");

            foreach (JToken passenger in passengers)
            {
                string? passengerResourceId = passenger["passengerResourceId"]?.ToString();
                string? firstName = passenger["givenNames"]?.ToString();
                string? lastName = passenger["surname"]?.ToString();
                string birthDate = OCIHelper.FormatBirthDate(passenger["birthDate"]?.ToString());

                string savePassportRequest = DeltaMicroserviceRequestBuilder.SavePassportRequest(firstName, lastName, birthDate, passengerResourceId);

                HttpResponseMessage savePassportResponse = PCCOciWeb.TrmApiPatchRequest(deltaAWSTRMAPIToken, clientId, trmVSDigitalTestId, savePassportRequest, requirementsResourceId).EnsureIsSuccessStatusCode();

                string saveEmergencyContactRequest = DeltaMicroserviceRequestBuilder.SaveEmergencyContactsRequest(passengerResourceId, firstName, lastName);

                HttpResponseMessage saveEmergencyContactResponse = PCCOciWeb.TrmApiPatchRequest(deltaAWSTRMAPIToken, clientId, trmVSDigitalTestId, saveEmergencyContactRequest, requirementsResourceId).EnsureIsSuccessStatusCode();

                string tripId = passenger.SelectToken("$..passengerTripHealthRequirements[*].tripId")?.ToString();

                string saveContactTracingRequest = DeltaMicroserviceRequestBuilder.SaveContactTracingRequest(passengerResourceId, tripId);

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
