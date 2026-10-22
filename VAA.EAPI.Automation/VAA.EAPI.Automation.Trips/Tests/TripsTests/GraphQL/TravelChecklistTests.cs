using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.GraphQL
{
    public class TravelChecklistTests
    {
        public string env = Config.Environment;

        [SkippableTheory]
        [Trait("Category", Traits.TravelChecklistMobileRegression)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public async Task GetFlyReadyStatus_ReturnsStatus(JourneyType journeyType, ReservationType reservationType)
        {
            #region - pre-requisite for fetching the PNR from the storage table

            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString);

            ValidatePnrDetails(pnrDetails);

            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            int paxCount = int.Parse(pnrDetails["passengersCount"]);
            string loyaltyNum = pnrDetails["flyingClubNumber"];
            string origin = pnrDetails["origin"];
            string destination = pnrDetails["destination"];
            string debugContext = RequestHelper.FormatTripsBookingContext(pnr, firstName, lastName);

            Assert.True(!string.IsNullOrEmpty(pnr) && !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName) && paxCount > 0, $"PNR, First Name, and Last Name should not be null or empty and Pax Count > 0. {debugContext}");

            #endregion

            #region - GraphQL call for retrieve trip to retrieve booking id and passenger ids and count of slices

            // Initialize variables for GraphQL request
            string ticketNum = string.Empty; // as we are performing a PNR Search
            string usingString = "PNR";

            HttpResponseMessage response = GraphQlEndpoint.Booking(env, usingString, pnr, firstName, lastName).EnsureIsSuccessStatusCode();
            dynamic BookingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            // Basic error check
            Assert.True(!DynamicHelper.HasProperty(BookingResult, "errors"), $"Expected a valid response from Graphql but found \n {BookingResult}. {debugContext}");

            // Validate data exists
            Assert.True(DynamicHelper.HasProperty(BookingResult, "data"), $"Response should contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(BookingResult.data, "Booking"), $"Response should contain Booking property. {debugContext}");

            // Retrieve booking id
            string bookingId = BookingResult.data.Booking.id;
            Assert.False(string.IsNullOrEmpty(bookingId), $"Booking id should not be null or empty. {debugContext}");

            // Retrieve passenger ids and validate count
            dynamic passengers = BookingResult.data.Booking.passengers;
            Assert.NotNull(passengers);
            int actualPaxCount = ((IEnumerable<dynamic>)passengers).Count();

            List<string> passengerIds = new();
            foreach (dynamic pax in passengers)
            {
                string passengerId = pax.passengerId;
                Assert.False(string.IsNullOrEmpty(passengerId), $"Passenger id should not be null or empty. {debugContext}");
                passengerIds.Add(passengerId);
            }

            // Retrieve slice count from first product (assuming airline booking)
            dynamic products = BookingResult.data.Booking.product;
            Assert.NotNull(products);
            dynamic? airlineProduct = ((IEnumerable<dynamic>)products).FirstOrDefault(p => p.bookingType == "AIRLINE");
            Assert.NotNull(airlineProduct);
            dynamic slices = airlineProduct.slices;
            int sliceCount = slices != null ? ((IEnumerable<dynamic>)slices).Count() : 0;
            Assert.True(sliceCount > 0, $"Slice count should be greater than 0. {debugContext}");

            #endregion

            #region call for GetFlyReadyStatus for each slice

            // Call GetFlyReadyStatus for each passenger but for slice 1 only
            HttpResponseMessage getFlyReadyResponse = GraphQlEndpoint.GetFlyReadyStatus(env, bookingId).EnsureIsSuccessStatusCode();
            dynamic getFlyReadyResult = JsonConvert.DeserializeObject<dynamic>(await getFlyReadyResponse.Content.ReadAsStringAsync());
            // Basic error check
            Assert.True(!DynamicHelper.HasProperty(getFlyReadyResult, "errors"), $"Expected a valid response from Graphql but found {getFlyReadyResult}. {debugContext}");
            // Validate data exists
            Assert.True(DynamicHelper.HasProperty(getFlyReadyResult.data, "getFlyReadyStatus"), $"Response should contain getFlyReadyStatus property. {debugContext}");
            // Validate the response structure
            dynamic flyReadyStatus = getFlyReadyResult.data.getFlyReadyStatus;
            Assert.NotNull(flyReadyStatus);

            // Validate that each passengerId from passengerIds exists in flyReadyStatus response for slice 1
            List<dynamic> flyReadyStatusList = ((IEnumerable<dynamic>)flyReadyStatus).ToList();
            Assert.True(passengerIds.Count >= flyReadyStatusList.Count, $"passengerIds count from the trip details response should be greater than when infant is accompanied or equal to flyReadyStatus count as flyready is not returned for infants. {debugContext}");

            // Check each passengerId is present in flyReadyStatus and status is one of the expected values
            HashSet<string> validStatuses = new()
            { "NOTSTARTED", "INPROGRESS", "COMPLETED" };
            foreach (string passengerId in passengerIds)
            {
                dynamic? statusEntry = flyReadyStatusList.FirstOrDefault(x => (string)x.passengerID == passengerId);
                if (statusEntry == null)
                {
                    // If statusEntry is null, likely the passenger is an infant (no flyready status returned)
                    // later we will assert here to ensure only infants are skipped
                    continue;
                }
                string status = statusEntry.status;
                Assert.True(validStatuses.Contains(status), $"Unexpected status '{status}' for passengerID '{passengerId}'. {debugContext}");
            }
            #endregion
        }

        private static void ValidatePnrDetails(Dictionary<string, string> pnrDetails)
            => TestDataPrerequisiteHelper.ValidatePnrDetailsOrSkip(pnrDetails);

    }
}
