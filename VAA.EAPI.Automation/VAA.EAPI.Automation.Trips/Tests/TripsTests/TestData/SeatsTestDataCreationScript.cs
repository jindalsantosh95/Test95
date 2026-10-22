using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.UI.Helpers;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.TestData
{
    public class SeatsTestDataCreationScript
    {
        public string env = Config.Environment;
        private TestConfig _testConfig;

        [Theory]
        [Trait("Category", "SeatTestDataCreationForMobileApps")]
        [InlineData("C2SNMT", "C", "WEHRWEIN", 1, "1")]
        [InlineData("C2SOZA", "H", "HEWARD", 1, "1")]
        [InlineData("C2SPWZ", "M", "ALDO", 1, "1")]
        public async Task CreateSeatTestData_SuccessScenario(string pnr, string firstName, string lastName, int paxCount, string segmentId)
        {
            #region - GraphQL call for retrieve trip to retrieve booking id and passenger ids

            string usingString = "PNR";
            HttpResponseMessage response = GraphQlEndpoint.Booking(env, usingString, pnr, firstName, lastName).EnsureIsSuccessStatusCode();
            dynamic BookingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(BookingResult, "errors"), $"Expected a valid response from Graphql but found \n {BookingResult}");

            string bookingId = BookingResult.data.Booking.id;
            dynamic passengers = BookingResult.data.Booking.passengers;

            List<string> passengerIds = new();
            foreach (dynamic pax in passengers)
            {
                passengerIds.Add(pax.passengerId.ToString());
            }

            Assert.True(passengerIds.Count >= paxCount, $"Expected at least {paxCount} passengers, found {passengerIds.Count}");

            #endregion

            #region - Get seat map for first segment

            HttpResponseMessage getSeatMapResponse = GraphQlEndpoint.GetSeatMap(env, bookingId, segmentId).EnsureIsSuccessStatusCode();
            dynamic getSeatMapResult = JsonConvert.DeserializeObject<dynamic>(await getSeatMapResponse.Content.ReadAsStringAsync());

            dynamic seatMap = getSeatMapResult.data.seatMapQuery.seatMap;
            List<dynamic> availableSeats = new();

            string[] cabinPriority = { "UPPER CLASS", "PREMIUM ECONOMY", "ECONOMY" };

            foreach (string cabinClass in cabinPriority)
            {
                foreach (dynamic cabin in seatMap)
                {
                    if (cabin.classType.ToString() == cabinClass)
                    {
                        foreach (dynamic row in cabin.rows)
                        {
                            foreach (dynamic column in row.columns)
                            {
                                if (column.type == "SEAT" && column.availability == "FREE" && column.offers != null && column.offers.HasValues)
                                {
                                    dynamic firstOffer = column.offers.First;
                                    if (firstOffer.price != null && firstOffer.price.amount > 0)
                                    {
                                        availableSeats.Add(column);
                                        if (availableSeats.Count >= paxCount)
                                            break;
                                    }
                                }
                            }
                            if (availableSeats.Count >= paxCount)
                                break;
                        }
                        if (availableSeats.Count >= paxCount)
                            break;
                    }
                }
                if (availableSeats.Count >= paxCount)
                    break;
            }

            Assert.True(availableSeats.Count >= paxCount, $"Not enough available seats found. Required: {paxCount}, Found: {availableSeats.Count}");

            #endregion

            #region - Get flight passenger seat map to retrieve seat offer IDs

            dynamic flightPassengerSeatMap = getSeatMapResult.data.seatMapQuery.flightPassengerSeatMap;
            string seatOfferId = flightPassengerSeatMap.First.seatOfferId;

            #endregion

            #region - Select seats for each passenger

            List<dynamic> selectedSeats = new();
            decimal totalAmount = 0;

            for (int i = 0; i < paxCount; i++)
            {
                dynamic seat = availableSeats[i];
                string seatNumber = seat.seatNumber;
                decimal seatPrice = seat.offers.First.price.amount;

                HttpResponseMessage selectSeatResponse = GraphQlEndpoint.SelectSeat(env, bookingId, segmentId, passengerIds[i], seatNumber, seatOfferId).EnsureIsSuccessStatusCode();
                dynamic selectSeatResult = JsonConvert.DeserializeObject<dynamic>(await selectSeatResponse.Content.ReadAsStringAsync());

                Assert.True(!DynamicHelper.HasProperty(selectSeatResult, "errors"), $"Seat selection failed for passenger {i + 1}");

                selectedSeats.Add(new { PassengerId = passengerIds[i], SeatNumber = seatNumber, Price = seatPrice });
                totalAmount += seatPrice;
            }

            #endregion

            #region - GraphQL call for seat payment mutation and Validate seat payment response

            HttpResponseMessage seatPaymentMutationResponse = GraphQlEndpoint.SeatPayment(env, bookingId).EnsureIsSuccessStatusCode();
            dynamic seatPaymentMutationResult = JsonConvert.DeserializeObject<dynamic>(await seatPaymentMutationResponse.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(seatPaymentMutationResult, "errors"), $"Expected a valid response from Graphql for seat confirmation but found {seatPaymentMutationResult}");

            dynamic seatPaymentMutation = seatPaymentMutationResult.data.seatPaymentMutation;

            Assert.True(seatPaymentMutation is not null, "seatConfirmationMutation should not be null");

            string paymentReferenceId = seatPaymentMutation.paymentReferenceId;

            Assert.True(!string.IsNullOrEmpty(paymentReferenceId), "paymentReferenceId should not be null or empty");

            string hppUrl = seatPaymentMutation.hppLink;
            Assert.True(!string.IsNullOrEmpty(hppUrl), "hppUrl should not be null or empty");

            #endregion

            #region - chrome browser call to open the hpp link and make the payment

            _testConfig = new TestConfig();
            using NavigateToHostedPaymentPage enterHppCardDetails = new(hppUrl);

            _testConfig.Book.Payment = new Payment(CardType.Visa);
            enterHppCardDetails.SubmitHppCardDetails(_testConfig);

            #endregion 

            #region - GraphQL call for seat fulfilment mutation to confirm the selected seats

            string isPaymentStatus = "SUCCESS";

            HttpResponseMessage SeatFulfilmentMutationResponse = GraphQlEndpoint.SeatFulfilmentMutation(env, bookingId, isPaymentStatus).EnsureIsSuccessStatusCode();
            dynamic SeatFulfilmentResult = JsonConvert.DeserializeObject<dynamic>(await SeatFulfilmentMutationResponse.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(SeatFulfilmentResult, "errors"), $"Expected a valid response from Graphql for seat confirmation but found {SeatFulfilmentResult}");

            dynamic SeatFulfilmentMutation = SeatFulfilmentResult.data.seatConfirmationMutation;
            Assert.True(SeatFulfilmentMutation != null, "seatConfirmationMutation should not be null");

            // Validate PNR locator
            string pnrLocatorFromResponse = SeatFulfilmentMutation.pnrLocator;
            Assert.True(pnrLocatorFromResponse.Equals(pnr), $"expected pnr value in SeatConfirmationMutation {pnr} but found {pnrLocatorFromResponse}");

            // Validate passenger seat selection
            Assert.True(SeatFulfilmentMutation.passengerSeatSelection != null && SeatFulfilmentMutation.passengerSeatSelection.HasValues, "passengerSeatSelection should not be null or empty");

            // Validate each passenger has a seat assigned
            dynamic passengerSeatSelections = SeatFulfilmentMutation.passengerSeatSelection;
            Assert.Equal(paxCount, ((IEnumerable<dynamic>)passengerSeatSelections).Count());

            foreach (dynamic passengerSeat in passengerSeatSelections)
            {
                Assert.True(passengerSeat.isConfirmed == true, "Seat should be confirmed");

                string passengerIdFromResponse = passengerSeat.passengerId;
                string seatNumberFromResponse = passengerSeat.seatNumber;

                Assert.True(passengerIds.Contains(passengerIdFromResponse), $"PassengerId {passengerIdFromResponse} should be in the original passenger list");
                Assert.True(!string.IsNullOrEmpty(seatNumberFromResponse), "Seat number should not be null or empty");
            }

            #endregion

            #region - Write to storage table AddPNRsForSeatScenariosToAzureStorageTable

            Dictionary<string, object> testData = new()
            {
                ["PNR"] = pnr,
                ["FirstName"] = firstName,
                ["LastName"] = lastName,
                ["PassengerCount"] = paxCount,
                ["SeatConfirmationJson"] = JsonConvert.SerializeObject(SeatFulfilmentMutation),
                ["TotalAmount"] = totalAmount,
                ["PaymentReferenceId"] = paymentReferenceId,
                ["CreatedDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            await WriteToAzureStorage.AddSeatConfirmationDataForPaymentTestingToStorageTable(testData);


            #endregion
        }
    }
}
