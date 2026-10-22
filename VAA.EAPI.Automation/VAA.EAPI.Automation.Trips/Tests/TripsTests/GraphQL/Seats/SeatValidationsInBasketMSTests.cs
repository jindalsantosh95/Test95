using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using VAA.EAPI.Automation.Trips.Requests.Endpoint;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.GraphQL.Seats
{
    public class SeatValidationsInBasketMSTests
    {
        public string env = Config.Environment;
        public static string ocpSubKey = Vault.Default.GetSecretValue("BasketMicroserviceSubscriptionKey");

        [SkippableTheory]
        [Trait("Category", Traits.SeatValidationsInBasketMS)]
        [InlineData("VS")]
        [InlineData("DL")]
        public async Task GetSeatDetailsFromBasket_ReturnsSeatDetails(string flightType)
        {
            #region - pre-requisite for fetching the PNR from the storage table with conditon of direct flight with VS or DL with selected seats 

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForDirectFlightVSWithSelectedSeats(flightType);

            ValidatePnrDetails(pnrDetails);

            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            int paxCount = int.Parse(pnrDetails["passengersCount"]);
            string origin = pnrDetails["origin"];
            string destination = pnrDetails["destination"];
            string debugContext = RequestHelper.FormatTripsBookingContext(pnr, firstName, lastName);

            Assert.True(!string.IsNullOrEmpty(pnr) && !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName) && paxCount > 0, $"PNR, First Name, and Last Name should not be null or empty and Pax Count > 0. {debugContext}");

            #endregion

            #region - GraphQL call for retrieve trip to retrieve booking id and passenger ids and count of slices

            string ticketNum = string.Empty; // as we are performing a PNR Search
            string usingString = "PNR";

            HttpResponseMessage response = GraphQlEndpoint.Booking(env, usingString, pnr, firstName, lastName).EnsureIsSuccessStatusCode();
            dynamic BookingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(BookingResult, "errors"), $"Expected a valid response from Graphql but found \n {BookingResult}. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(BookingResult, "data"), $"Response should contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(BookingResult.data, "Booking"), $"Response should contain Booking property. {debugContext}");

            string bookingId = BookingResult.data.Booking.id;
            Assert.True(!string.IsNullOrEmpty(bookingId), $"Booking id should not be null or empty. {debugContext}");

            dynamic passengers = BookingResult.data.Booking.passengers;
            Assert.True(passengers != null && passengers.HasValues, $"Response does not contain 'passengers'. {debugContext}");
            int actualPaxCount = ((IEnumerable<dynamic>)passengers).Count();

            List<string> passengerIds = new();
            foreach (dynamic pax in passengers)
            {
                string passengerId = pax.passengerId;
                Assert.True(!string.IsNullOrEmpty(passengerId), $"Passenger id should not be null or empty. {debugContext}");
                passengerIds.Add(passengerId);
            }

            dynamic products = BookingResult.data.Booking.product;
            Assert.True(products != null && products.HasValues, $"Response does not contain 'product'. {debugContext}");
            dynamic airlineProduct = ((IEnumerable<dynamic>)products).FirstOrDefault(p => p.bookingType == "AIRLINE");
            Assert.True(airlineProduct != null, $"Response does not contain 'airlineProduct'. {debugContext}");

            dynamic slices = airlineProduct.slices;
            int sliceCount = slices != null ? ((IEnumerable<dynamic>)slices).Count() : 0;
            Assert.True(sliceCount > 0, $"Slice count should be greater than 0. {debugContext}");

            int totalSegmentCount = 0;
            foreach (dynamic slice in slices)
            {
                dynamic segments = slice.segments;
                int segmentCount = segments != null ? ((IEnumerable<dynamic>)segments).Count() : 0;
                totalSegmentCount += segmentCount;
            }

            #endregion

            // Extracting JWT token from the response header for subsequent calls and validate the basketId
            string[] claims = bookingId.ExtractJwtClaims("basketId");
            string basketId = claims.FirstOrDefault();

            Assert.True(!string.IsNullOrEmpty(basketId), $"BasketId should not be null or empty. {debugContext}");

            #region Basket MS call for GetSeatDetailsFromBasketMS

            JObject basketResponse = null;
            JToken seatDetails = null;

            await RetryHelper.RunWithRetryAsync(async () =>
            {
                HttpResponseMessage basket = Microservice.RetrieveBasketFromBasketMS(basketId, ocpSubKey).EnsureIsSuccessStatusCode();
                basketResponse = JsonConvert.DeserializeObject<JObject>(await basket.Content.ReadAsStringAsync());
                Assert.True(basketResponse != null && basketResponse.HasValues, $"basket response should not be null or empty. {debugContext}");

                // Verify booking details exists
                Assert.True(basketResponse["bookingDetails"] is not null, $"Basket response should contain 'bookingDetails'. {debugContext}");

                // Get seat details array
                seatDetails = basketResponse["bookingDetails"]["seatDetails"];

                // Verify seat details exist and have values
                Assert.True(seatDetails != null && seatDetails.HasValues, $"Seat details array should not be null or empty. {debugContext}");
            }, 2);  // 2 attempts total (initial + 1 retry)

            #endregion

            #region - Validate seat details in basket response


            // Get passengers array
            JToken passengersInBasketMS = basketResponse["bookingDetails"]["passenger"];
            Assert.True(passengersInBasketMS != null && passengersInBasketMS.HasValues, $"Passengers array should not be null or empty. {debugContext}");

            // Get segments from the first slice (as per your response structure)
            JToken segmentsInBasketMS = basketResponse["bookingDetails"]["slices"][0]["segments"];
            Assert.True(segmentsInBasketMS is not null && segmentsInBasketMS.HasValues, $"Segments should not be null. {debugContext}");

            // For each passenger
            foreach (JToken passengerInBasketMS in passengersInBasketMS)
            {
                string passengerIdInBasketMS = passengerInBasketMS["id"].ToString();

                // For each segment
                foreach (JToken segmentInBasketMS in segmentsInBasketMS)
                {
                    string segmentIdInBasketMS = segmentInBasketMS["id"].ToString();

                    // Find seat details for this passenger and segment

                    IEnumerable<JToken> passengerSeatDetails = seatDetails.Where(s => s["passengerId"]?.Value<string>() == passengerIdInBasketMS && s["segmentId"]?.Value<string>() == segmentIdInBasketMS);

                    // Validate seat details exist
                    Assert.True(passengerSeatDetails.Any(), $"Seat details should exist - pnr {pnr} for passenger {passengerIdInBasketMS} basketId {basketId} on segment {segmentIdInBasketMS}. {debugContext}");

                    foreach (JToken seatDetail in passengerSeatDetails)
                    {
                        // Validate seat number
                        Assert.False(string.IsNullOrEmpty(seatDetail["seatNumber"]?.ToString()), $"Seat number should not be empty for - pnr {pnr} for passenger {passengerIdInBasketMS} on segment {segmentIdInBasketMS}. {debugContext}");

                        // Validate seat class details
                        Assert.True(seatDetail["seatClass"] is not null, $"Seat class should not be null. {debugContext}");
                        Assert.True(seatDetail["seatClass"]["cabinCode"] is not null, $"Cabin code should not be null. {debugContext}");
                        Assert.True(seatDetail["seatClass"]["cabinProductCode"] is not null, $"Cabin product code should not be null. {debugContext}");

                        // Optional: Validate cabin class matches the segment's cabin class
                        JToken segmentCabinClass = segmentInBasketMS["legs"][0]["cabinClass"];
                        Assert.True(segmentCabinClass["code"].ToString() == seatDetail["seatClass"]["cabinCode"].ToString(), $"Cabin code should match segment cabin class. {debugContext}");
                    }
                }
            }

            #endregion
        }

        private static void ValidatePnrDetails(Dictionary<string, string> pnrDetails)
            => TestDataPrerequisiteHelper.ValidatePnrDetailsOrSkip(pnrDetails);
    }
}
