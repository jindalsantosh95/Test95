using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Requests.Endpoint;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.Microservices
{
    public class TravelChecklistTests
    {
        public static string env = Config.Environment;
        public static string ocpSubKey = Vault.Default.GetSecretValue("TestingTripMicroserviceSubscriptionKey");

        [Theory]
        [Trait("Category", Traits.TravelChecklistMSRegression)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public async Task RetrieveTravelChecklist_ReturnsChecklist(JourneyType journeyType, ReservationType reservationType)
        {
            #region Test Data - Retrieve PNR from the Storage Table "PNRs" created by BookFlowGraphQL

            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString);

            // Check for error first
            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.ContainsKey("error") ? $"Test case failed: {pnrDetails["error"]}" : string.Empty);


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

            // Check if errors array exists and contains the specific error code
            Assert.True(!bookingResponse.ToString().Contains("NGTMYTR400"), $"Unexpected error:NGTMYTR400 - Unable to retrieve reservation {bookingResponse.ToString()}");

            Assert.True(!bookingResponse.ToString().Contains("Unable to retrieve reservation"), $"Unexpected error: {bookingResponse.ToString()}");

            string tripBookingId = bookingResponse["booking"]["id"].ToString();

            #endregion Trip MS API Call

            #region Response Validations

            // Assert PNR locator
            string actualPnrLocator = bookingResponse["booking"]["product"][0]["reference"]["pnrLocator"].ToString();
            Assert.True(pnr.ToUpper().Equals(actualPnrLocator), "PNR locator does not match expected value. Expected: " + pnr + " Actual: " + actualPnrLocator);

            // Assert number of slices based on journey type
            JArray? slicesArray = bookingResponse["booking"]?["product"]?[0]?["slices"] as JArray;
            Assert.True(slicesArray != null, "'slices' array should not be null");
            int slicesCount = slicesArray.Count;

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

            #region Travel Checklist MS API Call

            HttpResponseMessage travelChecklistResponse = Microservice.RetrieveTravelCheckList(tripBookingId, ocpSubKey).EnsureIsSuccessStatusCode();

            JObject travelChecklistResponseObject = JsonConvert.DeserializeObject<JObject>(await travelChecklistResponse.Content.ReadAsStringAsync());

            // Check if errors array exists and contains the specific error code
            Assert.True(!travelChecklistResponseObject.ToString().Contains("NGTMSGC400"), $"TechnicalError: ReservationResourceId not found in the cache {travelChecklistResponseObject.ToString()}");

            Assert.True(!travelChecklistResponseObject.ToString().Contains("ReservationResourceId not found in the cache"), $"Unexpected error: {travelChecklistResponseObject.ToString()}");

            #endregion

            #region Response Validations contains the Travel Checklist with message structure

            // Assert that the response contains the expected structure for trips and messages

            // Validate "trips" array exists and contains expected tripId(s)
            JArray tripsArray = (JArray)travelChecklistResponseObject["trips"];
            Assert.NotNull(tripsArray);
            Assert.True(tripsArray.Count > 0, "Trips array should not be empty");

            // Validate "messages" array exists and contains at least one message

            JArray messagesArray = (JArray)travelChecklistResponseObject["messages"];
            Assert.NotNull(messagesArray);
            Assert.True(messagesArray.Count > 0, "Messages array should not be empty");

            // Only trip 1 (upcoming trip) messages are expected in the response
            int expectedTripId = 1;

            // Assert that a trip with tripId 1 exists
            bool tripIdExists = tripsArray.Any(t => t["tripId"] != null && (int)t["tripId"] == expectedTripId);
            Assert.True(tripIdExists, $"Trip with tripId {expectedTripId} should exist in the response");

            // Assert that at least one message is associated with tripId 1
            string expectedTripIdInMessage = "1";
            bool messageForTripExists = messagesArray.Any(m => m["tripId"] != null && m["tripId"].ToString() == expectedTripIdInMessage);
            Assert.True(messageForTripExists, $"At least one message should be associated with tripId {expectedTripIdInMessage}");
            #endregion
        }

        [Theory]
        [Trait("Category", Traits.TravelChecklistMSRegression)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public async Task TravelChecklist_MapsMessageCentreResponse(JourneyType journeyType, ReservationType reservationType)
        {
            #region Test Data - Retrieve PNR from the Storage Table "PNRs" created by BookFlowGraphQL

            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();
            string origin = "LHR";

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString, origin);

            // Check for error first
            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.ContainsKey("error") ? $"Test case failed: {pnrDetails["error"]}" : string.Empty);


            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            int paxCount = int.Parse(pnrDetails["passengersCount"]);

            Assert.True(!string.IsNullOrEmpty(pnr), "PNR should not be null or empty");
            Assert.True(!string.IsNullOrEmpty(firstName), "First Name should not be null or empty");
            Assert.True(!string.IsNullOrEmpty(lastName), "Last Name should not be null or empty");
            Assert.True(paxCount > 0, "Pax Count should be greater than 0");

            #endregion

            #region msgCenter DL API call response

            string channelId = "DFMobile";
            string appId = "D2";

            HttpResponseMessage response = AIR4Services.RetrieveTravelReservations(firstName, lastName, pnr, channelId, appId).EnsureIsSuccessStatusCode();

            JObject air4Result = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());

            //To Retreive the Travel Reservation ID
            JArray travelReservations = (JArray)air4Result["travelReservations"];
            Assert.NotEmpty(travelReservations);
            //// Get the first travel reservation
            JObject firstReservation = (JObject)travelReservations[0];
            string reservationResourceId = firstReservation["reservationResourceId"].ToString();
            // Assert that the reservationResourceId is not null or empty
            Assert.True(!string.IsNullOrEmpty(reservationResourceId), "Reservation Resource ID should not be null or empty");

            //we need to pass the reservationResourceId to retrieve msg center response

            HttpResponseMessage msgCenterResponse = AIR4Services.RetreiveMsgCenterAPI(channelId, appId, reservationResourceId).EnsureIsSuccessStatusCode();

            JObject msgCenterResponseObject = JsonConvert.DeserializeObject<JObject>(await msgCenterResponse.Content.ReadAsStringAsync());

            // Check if errors array exists and contains the specific error code
            Assert.True(!msgCenterResponseObject.ToString().Contains("NGTMSGC400"), $"TechnicalError: ReservationResourceId not found in the cache {msgCenterResponseObject.ToString()}");

            #endregion

            #region Trip MS API Call

            HttpResponseMessage tripMSResponse = Microservice.RetrieveBookingUsingPNR("PNR", firstName, lastName, pnr, ocpSubKey).EnsureIsSuccessStatusCode();

            JObject bookingResponse = JsonConvert.DeserializeObject<JObject>(await tripMSResponse.Content.ReadAsStringAsync());

            // Check if errors array exists and contains the specific error code
            Assert.True(!bookingResponse.ToString().Contains("NGTMYTR400"), $"Unexpected error:NGTMYTR400 - Unable to retrieve reservation {bookingResponse.ToString()}");

            Assert.True(!bookingResponse.ToString().Contains("Unable to retrieve reservation"), $"Unexpected error: {bookingResponse.ToString()}");

            string tripBookingId = bookingResponse["booking"]["id"].ToString();
            // Assert on the trip booking ID
            Assert.True(!string.IsNullOrEmpty(tripBookingId), "Trip Booking ID should not be null or empty");

            #endregion Trip MS API Call Ended

            #region Travel Checklist MS API Call

            HttpResponseMessage travelChecklistResponse = Microservice.RetrieveTravelCheckList(tripBookingId, ocpSubKey).EnsureIsSuccessStatusCode();

            JObject travelChecklistResponseObject = JsonConvert.DeserializeObject<JObject>(await travelChecklistResponse.Content.ReadAsStringAsync());

            // Check if errors array exists and contains the specific error code
            Assert.True(!travelChecklistResponseObject.ToString().Contains("NGTMSGC400"), $"TechnicalError: ReservationResourceId not found in the cache {travelChecklistResponseObject.ToString()}");

            Assert.True(!travelChecklistResponseObject.ToString().Contains("ReservationResourceId not found in the cache"), $"Unexpected error: {travelChecklistResponseObject.ToString()}");

            #endregion

            #region Response Validations contains the Travel Checklist with message structure

            // Assume msgCenterResponseObject and travelChecklistResponseObject are both JObject
            // If not, deserialize them as shown earlier

            // Extract the first message from each response
            JObject msgCenterMessage = ((JArray)msgCenterResponseObject["messages"])[0] as JObject;
            JObject flyReadyMessage = ((JArray)travelChecklistResponseObject["messages"])[0] as JObject;

            // Compare key attributes using Assert.True
            Assert.True(msgCenterMessage["messageCode"].ToString().Equals(flyReadyMessage["messageCode"].ToString()), "messageCode does not match");

            Assert.True(msgCenterMessage["codeType"].ToString().Equals(flyReadyMessage["codeType"].ToString()), "codeType does not match");
            Assert.True(msgCenterMessage["messageContextTypeCode"].ToString().Equals(flyReadyMessage["messageContextTypeCode"].ToString()), "messageContextTypeCode does not match");


            // tripId may be int in one and string in another, so compare as string
            Assert.True(msgCenterMessage["tripId"].ToString().Equals(flyReadyMessage["tripId"].ToString()), "tripId does not match");

            Assert.True(msgCenterMessage["headerDesc"].ToString().Equals(flyReadyMessage["headerDesc"].ToString()), "headerDesc does not match");
            Assert.True(msgCenterMessage["bodyDesc"].ToString().Equals(flyReadyMessage["bodyDesc"].ToString()), "bodyDesc does not match");
            Assert.True(msgCenterMessage["appliedToAllPassengers"].ToString().Equals(flyReadyMessage["appliedToAllPassengers"].ToString()), "appliedToAllPassengers does not match");
            Assert.True(msgCenterMessage["messageSeverity"].ToString().Equals(flyReadyMessage["messageSeverity"].ToString()), "messageSeverity does not match");
            Assert.True(msgCenterMessage["ordinal"].ToString().Equals(flyReadyMessage["ordinal"].ToString()), "ordinal does not match");

            // Compare passengers array (order-insensitive)
            JArray msgCenterPassengers = (JArray)msgCenterMessage["passengers"];
            JArray flyReadyPassengers = (JArray)flyReadyMessage["passengers"];
            Assert.True(msgCenterPassengers.Count == flyReadyPassengers.Count, "Passenger count does not match");

            // Compare passengers array (order-insensitive)

            HashSet<string> msgCenterPaxIds = msgCenterPassengers.Select(p => p["id"].ToString()).ToHashSet();
            HashSet<string> flyReadyPaxIds = flyReadyPassengers.Select(p => p["id"].ToString()).ToHashSet();

            Assert.True(msgCenterPaxIds.Count == flyReadyPaxIds.Count, "Passenger count does not match");

            #endregion
        }
    }
}

