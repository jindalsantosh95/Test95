using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Requests.Endpoint;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.Microservices
{
    public class SpecialAssistanceTests
    {
        public static string env = Config.Environment;
        public static string ocpSubKey = Vault.Default.GetSecretValue("TestingTripMicroserviceSubscriptionKey");
        public static string ocpSubKeyFlightServiceMS = Vault.Default.GetSecretValue("TestingFlightServiceMicroserviceSubscriptionKey");

        [Theory]
        [Trait("Category", Traits.SpecialAssistanceMSRegression)]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "FlightServicesMS")]
        [Trait("SharedServiceConsumer", "Trips")]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public async Task RetrieveSpecialAssistance_ReturnsAssistanceDetails(JourneyType journeyType, ReservationType reservationType)
        {
            #region Test Data - Retrieve PNR from the Storage Table "PNRs" created by BookFlowGraphQL

            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();
            string origin = "LHR";

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString, origin);

            ValidatePnrDetails(pnrDetails);


            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            int paxCount = int.Parse(pnrDetails["passengersCount"]);

            Assert.True(!string.IsNullOrEmpty(pnr), "PNR should not be null or empty");
            Assert.True(!string.IsNullOrEmpty(firstName), "First Name should not be null or empty");
            Assert.True(!string.IsNullOrEmpty(lastName), "Last Name should not be null or empty");
            Assert.True(paxCount > 0, "Pax Count should be greater than 0");

            #endregion

            #region Trip MS API Call

            HttpResponseMessage response = Microservice.RetrieveBookingUsingPNR("PNR", firstName, lastName, pnr, ocpSubKey).EnsureIsSuccessStatusCode();

            JObject bookingResponse = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());
            Assert.True(bookingResponse != null && bookingResponse.HasValues, "Booking response should not be null or empty");
            Assert.True(!bookingResponse.ToString().Contains("NGTMYTR400"), $"Unexpected error:NGTMYTR400 - Unable to retrieve reservation {bookingResponse.ToString()}");
            Assert.True(!bookingResponse.ToString().Contains("Unable to retrieve reservation"), $"Unexpected error: {bookingResponse.ToString()}");
            JToken booking = bookingResponse["booking"];
            Assert.True(booking != null, "'booking' property should not be null");
            JToken bookingIdToken = booking["id"];
            Assert.True(bookingIdToken != null, "'id' property should not be null");
            string tripBookingId = bookingIdToken.ToString();

            #endregion Trip MS API Call

            #region Response Validations

            // Assert PNR locator
            JArray productArray = booking["product"] as JArray;
            Assert.True(productArray != null && productArray.HasValues, "'product' array should not be null or empty");
            JToken firstProduct = productArray[0];
            Assert.True(firstProduct != null, "First product should not be null");
            JToken reference = firstProduct["reference"];
            Assert.True(reference != null, "'reference' property should not be null");
            JToken pnrLocatorToken = reference["pnrLocator"];
            Assert.True(pnrLocatorToken != null, "'pnrLocator' property should not be null");
            string actualPnrLocator = pnrLocatorToken.ToString();
            Assert.True(pnr.ToUpper().Equals(actualPnrLocator), "PNR locator does not match expected value. Expected: " + pnr + " Actual: " + actualPnrLocator);

            // Assert number of slices based on journey type
            JArray slicesArray = firstProduct["slices"] as JArray;
            Assert.True(slicesArray != null, "'slices' array should not be null");
            int slicesCount = slicesArray.Count();

            journeyTypeString = journeyType.ToString();

            switch (journeyTypeString.ToUpper())
            {
                case "ONEWAY":
                    Assert.True(slicesCount == 1, $"One-way journey should have exactly 1 slice but found {slicesCount}");
                    break;

                case "RETURN":
                    Assert.True(slicesCount == 2, $"Return journey should have exactly 2 slices but found {slicesCount}");
                    break;

                default:
                    Assert.Fail($"Unexpected journey type: {journeyTypeString}");
                    return;
            }

            // Assert on the trip booking ID
            Assert.True(!string.IsNullOrEmpty(tripBookingId), "Trip Booking ID should not be null or empty");

            #endregion Response Validations

            #region Flight Service MS API Call

            HttpResponseMessage specialAssistanceResponse = Microservice.RetrieveSsrsFromFlightService(tripBookingId, ocpSubKeyFlightServiceMS).EnsureIsSuccessStatusCode();

            JObject specialAssistanceResponseObject = JsonConvert.DeserializeObject<JObject>(await specialAssistanceResponse.Content.ReadAsStringAsync());
            Assert.True(specialAssistanceResponseObject != null, "Special assistance response should not be null");

            #endregion

            #region Response Validations contains the pax SSR details with SSR structure

            // Validate SSRs for each passenger and segment
            Assert.True(specialAssistanceResponseObject != null && specialAssistanceResponseObject.HasValues, "Response should not be null or empty");
            JArray passengersArray = specialAssistanceResponseObject["passengers"] as JArray;
            Assert.True(passengersArray != null && passengersArray.HasValues, "Response does not contain 'passengers'");
            Assert.True(passengersArray.Count > 0, "Response does not contain 'passengers'");

            foreach (JToken passengerToken in passengersArray)
            {
                JObject passenger = passengerToken as JObject;
                SoftAssertHelper softAssert = new();
                softAssert.True(passenger != null && passenger.HasValues, "Passenger object should not be null or empty");
                softAssert.True(DynamicHelper.HasProperty(passenger, "passengerResourceId"), "Passenger should have 'passengerResourceId'");
                softAssert.True(DynamicHelper.HasProperty(passenger, "firstName"), "Passenger should have 'firstName'");
                softAssert.True(DynamicHelper.HasProperty(passenger, "lastName"), "Passenger should have 'lastName'");
                softAssert.True(DynamicHelper.HasProperty(passenger, "ssrSegments"), "Passenger should have 'ssrSegments'");

                JArray ssrSegments = passenger?["ssrSegments"] as JArray;
                softAssert.True(ssrSegments != null, "Passenger's 'ssrSegments' should not be null");
                if (ssrSegments != null)
                {
                    foreach (JToken segmentToken in ssrSegments)
                    {
                        JObject segment = segmentToken as JObject;
                        SoftAssertHelper segmentAssert = new();
                        segmentAssert.True(segment != null, "Segment should not be null");
                        segmentAssert.True(DynamicHelper.HasProperty(segment, "flightSegment"), "Segment should have 'flightSegment'");
                        segmentAssert.True(DynamicHelper.HasProperty(segment, "serviceRequests"), "Segment should have 'serviceRequests'");

                        JObject flightSegment = segment?["flightSegment"] as JObject;
                        segmentAssert.True(flightSegment != null, "FlightSegment should not be null");
                        if (flightSegment != null)
                        {
                            segmentAssert.True(DynamicHelper.HasProperty(flightSegment, "segmentId"), "FlightSegment should have 'segmentId'");
                            segmentAssert.True(DynamicHelper.HasProperty(flightSegment, "operatingAirlines"), "FlightSegment should have 'operatingAirlines'");
                            segmentAssert.True(DynamicHelper.HasProperty(flightSegment, "flightNumber"), "FlightSegment should have 'flightNumber'");
                            segmentAssert.True(DynamicHelper.HasProperty(flightSegment, "marketingFlight"), "FlightSegment should have 'marketingFlight'");
                            segmentAssert.True(DynamicHelper.HasProperty(flightSegment, "departureAirport"), "FlightSegment should have 'departureAirport'");
                            segmentAssert.True(DynamicHelper.HasProperty(flightSegment, "arrivalAirport"), "FlightSegment should have 'arrivalAirport'");
                            segmentAssert.True(DynamicHelper.HasProperty(flightSegment, "flightLegs"), "FlightSegment should have 'flightLegs'");
                        }

                        JArray serviceRequests = segment?["serviceRequests"] as JArray;
                        segmentAssert.True(serviceRequests != null, "ServiceRequests should not be null");
                        if (serviceRequests != null)
                        {
                            foreach (JToken serviceRequestToken in serviceRequests)
                            {
                                JObject serviceRequest = serviceRequestToken as JObject;
                                SoftAssertHelper serviceAssert = new();
                                serviceAssert.True(serviceRequest != null, "ServiceRequest should not be null");
                                serviceAssert.True(DynamicHelper.HasProperty(serviceRequest, "serviceTypeCode"), "ServiceRequest should have 'serviceTypeCode'");
                                serviceAssert.True(DynamicHelper.HasProperty(serviceRequest, "serviceTypeName"), "ServiceRequest should have 'serviceTypeName'");
                                serviceAssert.True(DynamicHelper.HasProperty(serviceRequest, "selectedChoice"), "ServiceRequest should have 'selectedChoice'");
                                serviceAssert.True(DynamicHelper.HasProperty(serviceRequest, "serviceCategory"), "ServiceRequest should have 'serviceCategory'");
                                serviceAssert.True(DynamicHelper.HasProperty(serviceRequest, "availableChoices"), "ServiceRequest should have 'availableChoices'");
                                serviceAssert.AssertAll();
                            }
                        }
                        segmentAssert.AssertAll();
                    }
                }
                softAssert.AssertAll();
            }
            #endregion
        }

        private static void ValidatePnrDetails(Dictionary<string, string> pnrDetails)
        {
            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.ContainsKey("error") ? $"Test case failed: {pnrDetails["error"]}" : string.Empty);

            Assert.True(!string.IsNullOrEmpty(pnrDetails["pnr"]) && !string.IsNullOrEmpty(pnrDetails["firstName"]) && !string.IsNullOrEmpty(pnrDetails["lastName"]) && int.Parse(pnrDetails["passengersCount"]) > 0, "PNR, First Name, and Last Name should not be null or empty and Pax Count > 0");
        }
    }
}
