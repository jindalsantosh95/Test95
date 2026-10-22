using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.GraphQL.SSR
{
    [Collection("SpecialServicesSignOff")]
    public class SpecialMealsTests
    {
        [SkippableFact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "FlightServicesMS")]
        [Trait("SharedServiceConsumer", "Trips")]
        public async Task SharedServicesSignOff_RetrieveSpecialMeals() =>
            await GetSpecialMeals_ReturnsMeals(JourneyType.OneWay, ReservationType.REVENUE);

        [SkippableFact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "FlightServicesMS")]
        [Trait("SharedServiceConsumer", "Trips")]
        public async Task SharedServicesSignOff_UpdateSpecialMeals() =>
            await UpdateSpecialMeals_UpdatesBooking(JourneyType.OneWay, LocationType.LHR, LocationType.JFK, FlightType.VS);

        public string env = Config.Environment;

        [Theory]
        [Trait("Category", Traits.SpecialMealsMobileRegression)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public async Task GetSpecialMeals_ReturnsMeals(JourneyType journeyType, ReservationType reservationType)
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

            string ticketNum = string.Empty; // as we are performing a PNR Search
            string usingString = "PNR";

            HttpResponseMessage response = GraphQlEndpoint.Booking(env, usingString, pnr, firstName, lastName).EnsureIsSuccessStatusCode();
            dynamic BookingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(BookingResult, "errors"), $"Expected a valid response from Graphql but found \n {BookingResult}. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(BookingResult, "data"), $"Response should contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(BookingResult.data, "Booking"), $"Response should contain Booking property. {debugContext}");

            string bookingId = BookingResult.data.Booking.id;
            Assert.True(!string.IsNullOrEmpty(bookingId), $"Booking id should not be null or empty. {debugContext}");

            dynamic? passengers = BookingResult.data.Booking.passengers;
            Assert.True(passengers != null && passengers.HasValues, $"Response does not contain 'passengers'. {debugContext}");
            int actualPaxCount = ((IEnumerable<dynamic>)passengers).Count();

            List<string> passengerIds = new();
            foreach (dynamic pax in passengers)
            {
                string passengerId = pax.passengerId;
                Assert.True(!string.IsNullOrEmpty(passengerId), $"Passenger id should not be null or empty. {debugContext}");
                passengerIds.Add(passengerId);
            }

            dynamic? products = BookingResult.data.Booking.product;
            Assert.True(products != null && products.HasValues, $"Response does not contain 'product'. {debugContext}");
            dynamic? airlineProduct = ((IEnumerable<dynamic>)products).FirstOrDefault(p => p.bookingType == "AIRLINE");
            Assert.True(airlineProduct != null, $"Response does not contain 'airlineProduct'. {debugContext}");

            dynamic? slices = airlineProduct.slices;
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

            #region Graphql call for GetSpecialMeals response (single call for all segments)

            HttpResponseMessage getSpecialMealsResponse = GraphQlEndpoint.GetSpecialMeals(env, bookingId).EnsureIsSuccessStatusCode();
            dynamic getSpecialMealsResult = JsonConvert.DeserializeObject<dynamic>(await getSpecialMealsResponse.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(getSpecialMealsResult, "errors"), $"Expected a valid response from Graphql but found \n {getSpecialMealsResult}");
            Assert.True(DynamicHelper.HasProperty(getSpecialMealsResult, "data"), "Response does not contain data property");
            dynamic? dataObj = getSpecialMealsResult.data;
            Assert.True(dataObj != null, "data property should not be null");
            Assert.True(DynamicHelper.HasProperty(dataObj, "GetSpecialMeals"), "Response does not contain getSpecialMeals property");
            dynamic? getSpecialMeals = dataObj != null ? dataObj.GetSpecialMeals : null;
            Assert.True(getSpecialMeals != null, "getSpecialMeals property should not be null");
            Assert.True(DynamicHelper.HasProperty(getSpecialMeals, "passengers"), "getSpecialMeals should contain 'passengers' property");
            dynamic? passengersArray = getSpecialMeals != null ? getSpecialMeals.passengers : null;
            Assert.True(passengersArray != null, "Passengers array should not be null");
            IEnumerable<dynamic> passengerList = passengersArray as IEnumerable<dynamic>;
            Assert.True(passengerList != null, "Passengers list should not be null");
            foreach (dynamic passenger in passengerList)
            {
                SoftAssertHelper softAssert = new();
                softAssert.True(DynamicHelper.HasProperty(passenger, "passengerResourceId"), "Passenger should have 'passengerResourceId'");
                softAssert.True(DynamicHelper.HasProperty(passenger, "firstName"), "Passenger should have 'firstName'");
                softAssert.True(DynamicHelper.HasProperty(passenger, "lastName"), "Passenger should have 'lastName'");
                softAssert.True(DynamicHelper.HasProperty(passenger, "passengerSegments"), "Passenger should have 'passengerSegments'");
                dynamic? passengerSegments = passenger != null ? passenger.passengerSegments : null;
                softAssert.True(passengerSegments != null, "PassengerSegments should not be null");
                IEnumerable<dynamic> segmentList = passengerSegments as IEnumerable<dynamic>;
                softAssert.True(segmentList != null, "PassengerSegments list should not be null");
                foreach (dynamic? segment in segmentList)
                {
                    softAssert.True(DynamicHelper.HasProperty(segment, "flightSegment"), "Segment should have 'flightSegment'");
                    dynamic? flightSegment = segment != null ? segment.flightSegment : null;
                    softAssert.True(flightSegment != null, "FlightSegment should not be null");
                    softAssert.True(DynamicHelper.HasProperty(flightSegment, "segmentId"), "FlightSegment should have 'segmentId'");
                    softAssert.True(DynamicHelper.HasProperty(flightSegment, "ordinal"), "FlightSegment should have 'ordinal'");
                    softAssert.True(DynamicHelper.HasProperty(flightSegment, "operatingAirlines"), "FlightSegment should have 'operatingAirlines'");
                    softAssert.True(DynamicHelper.HasProperty(flightSegment, "flightNumber"), "FlightSegment should have 'flightNumber'");
                    softAssert.True(DynamicHelper.HasProperty(flightSegment, "marketingFlight"), "FlightSegment should have 'marketingFlight'");
                    softAssert.True(DynamicHelper.HasProperty(flightSegment, "departureAirport"), "FlightSegment should have 'departureAirport'");
                    softAssert.True(DynamicHelper.HasProperty(flightSegment, "arrivalAirport"), "FlightSegment should have 'arrivalAirport'");
                    softAssert.True(DynamicHelper.HasProperty(flightSegment, "checkinStatus"), "FlightSegment should have 'checkinStatus'");
                    softAssert.True(DynamicHelper.HasProperty(flightSegment, "flightLegs"), "FlightSegment should have 'flightLegs'");

                    //meal selection from the segment
                    dynamic? mealSelection = segment != null ? segment.mealSelection : null;
                    softAssert.True(mealSelection != null, "mealSelection should not be null");
                    JObject choiceArray = mealSelection as JObject;
                    if (choiceArray != null)
                    {
                        // Validate root level properties
                        softAssert.True(choiceArray["isSelected"] != null, "isSelected should not be null");
                        softAssert.True(choiceArray["selectedChoice"] != null, "selectedChoice should not be null");
                        softAssert.True(choiceArray["isEligible"] != null, "isEligible should not be null");
                        softAssert.True(choiceArray["isChangeAllowed"] != null, "isChangeAllowed should not be null");
                        softAssert.True(choiceArray["availableChoices"] != null, "availableChoices should not be null");

                        JArray availableChoices = choiceArray["availableChoices"] as JArray;
                        foreach (JToken choice in availableChoices)
                        {
                            JObject choiceObject = choice as JObject;
                            if (choiceObject != null)
                            {
                                // Check if properties exist and have values
                                softAssert.True(choiceObject.Property("code") != null, "Choice should have 'code' property");
                                softAssert.True(choiceObject.Property("description") != null, "Choice should have 'description' property");

                                if (choiceObject.Property("code") != null && choiceObject.Property("description") != null)
                                {
                                    string code = choiceObject["code"]?.ToString();
                                    string description = choiceObject["description"]?.ToString();

                                    softAssert.True(!string.IsNullOrEmpty(code), "code should not be empty");
                                    softAssert.True(!string.IsNullOrEmpty(description), "description should not be empty");
                                }
                            }
                        }
                        softAssert.AssertAll();
                    }
                    #endregion
                }

            }
        }

        [Theory]
        [Trait("Category", Traits.SpecialMealsMobileRegression)]
        [InlineData(JourneyType.OneWay, LocationType.LHR, LocationType.DEL, FlightType.VS)]
        [InlineData(JourneyType.OneWay, LocationType.LHR, LocationType.JFK, FlightType.VS)]
        [InlineData(JourneyType.OneWay, LocationType.LHR, LocationType.ATL, FlightType.VS)]
        public async Task UpdateSpecialMeals_UpdatesBooking(JourneyType journeyType, LocationType originAirport, LocationType destinationAirport, FlightType flighttype)
        {
            #region - pre-requisite for fetching the PNR from the storage table

            string journeyTypeString = journeyType.ToString();
            string originString = originAirport.ToString();
            string destinationString = destinationAirport.ToString();
            string flightTypeString = flighttype.ToString();

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForSpecificRoute(journeyTypeString, originString, destinationString, flightTypeString);

            ValidatePnrDetails(pnrDetails);

            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            int paxCount = int.Parse(pnrDetails["passengersCount"]);
            string loyaltyNum = pnrDetails["flyingClubNumber"];
            string origin = pnrDetails["origin"];
            string destination = pnrDetails["destination"];
            string flightType = pnrDetails["flightType"];
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

            dynamic? passengers = BookingResult.data.Booking.passengers;
            Assert.True(passengers != null && passengers.HasValues, $"Response does not contain 'passengers'. {debugContext}");
            int actualPaxCount = ((IEnumerable<dynamic>)passengers).Count();

            List<string> passengerIds = new();
            foreach (dynamic pax in passengers)
            {
                string passengerId = pax.passengerId;
                Assert.True(!string.IsNullOrEmpty(passengerId), $"Passenger id should not be null or empty. {debugContext}");
                passengerIds.Add(passengerId);
            }

            dynamic? products = BookingResult.data.Booking.product;
            Assert.True(products != null && products.HasValues, $"Response does not contain 'product'. {debugContext}");
            dynamic? airlineProduct = ((IEnumerable<dynamic>)products).FirstOrDefault(p => p.bookingType == "AIRLINE");
            Assert.True(airlineProduct != null, $"Response does not contain 'airlineProduct'. {debugContext}");

            dynamic? slices = airlineProduct.slices;
            int sliceCount = slices != null ? ((IEnumerable<dynamic>)slices).Count() : 0;
            Assert.True(sliceCount > 0, "Slice count should be greater than 0");

            int totalSegmentCount = 0;
            foreach (dynamic slice in slices)
            {
                dynamic segments = slice.segments;
                int segmentCount = segments != null ? ((IEnumerable<dynamic>)segments).Count() : 0;
                totalSegmentCount += segmentCount;
            }
            Assert.True(totalSegmentCount > 0, "Total segment count should be greater than 0");

            #endregion

            #region Graphql call for special Meals and checking meal code assigned to the Pax

            HttpResponseMessage getSpecialMealsResponse = GraphQlEndpoint.GetSpecialMeals(env, bookingId).EnsureIsSuccessStatusCode();
            dynamic getSpecialMealsResult = JsonConvert.DeserializeObject<dynamic>(await getSpecialMealsResponse.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(getSpecialMealsResult, "errors"), $"Expected a valid response from Graphql but found \n {getSpecialMealsResult}");
            Assert.True(DynamicHelper.HasProperty(getSpecialMealsResult, "data"), "Response does not contain data property");
            dynamic? dataObj = getSpecialMealsResult.data;
            Assert.True(dataObj != null, "data property should not be null");
            Assert.True(DynamicHelper.HasProperty(dataObj, "GetSpecialMeals"), "Response does not contain getSpecialMeals property");
            dynamic? getSpecialMeals = dataObj != null ? dataObj.GetSpecialMeals : null;
            Assert.True(getSpecialMeals != null, "getSpecialMeals property should not be null");
            Assert.True(DynamicHelper.HasProperty(getSpecialMeals, "passengers"), "getSpecialMeals should contain 'passengers' property");
            dynamic? passengersArray = getSpecialMeals != null ? getSpecialMeals.passengers : null;
            Assert.True(passengersArray != null, "Passengers array should not be null");
            IEnumerable<dynamic> passengerList = passengersArray as IEnumerable<dynamic>;
            Assert.True(passengerList != null, "Passengers list should not be null");

            // Save meal selection details for each passenger/segment
            List<(string passengerResourceId, string segmentId, string selectedChoice)> mealSelections = new();
            foreach (dynamic? passenger in passengerList)
            {
                dynamic? passengerSegments = passenger != null ? passenger.passengerSegments : null;
                IEnumerable<dynamic> segmentList = passengerSegments as IEnumerable<dynamic>;
                foreach (dynamic? segment in segmentList)
                {
                    dynamic? flightSegment = segment != null ? segment.flightSegment : null;
                    dynamic? mealSelection = segment != null ? segment.mealSelection : null;
                    JObject choiceArray = mealSelection as JObject;

                    string passengerResourceId = passenger.passengerResourceId;
                    string segmentId = flightSegment.segmentId;
                    string selectedChoice = choiceArray["selectedChoice"]?.ToString();

                    mealSelections.Add((passengerResourceId, segmentId, selectedChoice));
                }
            }

            #endregion

            #region - Update the meal and validate the response

            // List of possible SSR codes
            List<string> possibleSsrCodes = new()
            { "AVML", "DBML", "CHML", "BBML", "FPML" };

            foreach ((string? passengerResourceId, string? segmentId, string? selectedChoice) in mealSelections)
            {
                // Find a new SSR code that is different from the current selectedChoice and is available
                string ssrCode = possibleSsrCodes.FirstOrDefault(code => code != selectedChoice);
                bool applyToAll = false;

                HttpResponseMessage selectMealMutationResponse = GraphQlEndpoint.UpdateSpecialMeals(env, bookingId, passengerResourceId, segmentId, ssrCode, applyToAll).EnsureIsSuccessStatusCode();
                dynamic selectMealMutationResult = JsonConvert.DeserializeObject<dynamic>(await selectMealMutationResponse.Content.ReadAsStringAsync());

                Assert.True(!DynamicHelper.HasProperty(selectMealMutationResult, "errors"), $"Expected a valid response from Graphql but found \n {selectMealMutationResult}");
                Assert.True(DynamicHelper.HasProperty(selectMealMutationResult, "data"), "Response should contain data property");
                dynamic? updateDataObj = selectMealMutationResult.data;
                Assert.True(updateDataObj != null, "data property should not be null");
                Assert.True(DynamicHelper.HasProperty(updateDataObj, "selectMealMutation"), "Response does not contain UpdateSpecialMeals property");
                dynamic? updateSpecialMeals = updateDataObj.selectMealMutation;
                Assert.True(updateSpecialMeals != null, "UpdateSpecialMeals property should not be null");
                Assert.True(DynamicHelper.HasProperty(updateSpecialMeals, "passengers"), "UpdateSpecialMeals should contain 'passengers' property");
                IEnumerable<dynamic> updatedPassengersArray = updateSpecialMeals.passengers as IEnumerable<dynamic>;
                Assert.True(updatedPassengersArray != null, "Passengers array should not be null");

                // Find the updated passenger/segment and validate selectedChoice
                bool found = false;
                foreach (dynamic updatedPassenger in updatedPassengersArray)
                {
                    IEnumerable<dynamic> updatedSegments = updatedPassenger.passengerSegments as IEnumerable<dynamic>;
                    foreach (dynamic updatedSegment in updatedSegments)
                    {
                        dynamic updatedFlightSegment = updatedSegment.flightSegment;

                        JObject updatedMealSelection = updatedSegment.mealSelection as JObject;
                        string updatedSelectedChoice = updatedMealSelection?["selectedChoice"]?.ToString();
                        Assert.True(updatedSelectedChoice == ssrCode, $"selectedChoice should be updated to {ssrCode} but was {updatedSelectedChoice}");
                        found = true;
                        break;
                    }
                    if (found)
                        break;
                }
                Assert.True(found, $"Updated passenger/segment not found for passengerResourceId={passengerResourceId}, segmentId={segmentId}");
            }

            #endregion
        }

        [Fact]
        [Trait("Category", Traits.SpecialMealsMobileRegression)]
        public async Task UpdateSpecialMeals_IncorrectVariables_ReturnsGenericError()
        {
            #region - GraphQL call with incorrect variables expecting generic error

            string bookingId = "INVALID_TRIP";
            string segmentId = "INVALID_SEG";
            string passengerResourceId = "INVALID_PAX";
            string ssrCode = "VGML";
            bool applyToAll = true;

            HttpResponseMessage selectMealMutationResponse = GraphQlEndpoint.UpdateSpecialMeals(env, bookingId, passengerResourceId, segmentId, ssrCode, applyToAll).EnsureIsSuccessStatusCode();

            dynamic selectMealMutationResult = JsonConvert.DeserializeObject<dynamic>(await selectMealMutationResponse.Content.ReadAsStringAsync());

            Assert.True(DynamicHelper.HasProperty(selectMealMutationResult, "errors"), $"Expected errors in response but found \n {selectMealMutationResult}");

            ValidateGenericErrorResponse(selectMealMutationResult);

            #endregion
        }
        private static void ValidateGenericErrorResponse(dynamic errorResult)
        {
            Assert.True(DynamicHelper.HasProperty(errorResult, "errors"), "Response should contain errors");

            dynamic? errors = errorResult.errors;
            Assert.True(errors != null && errors.Count > 0, "Response should contain at least one error");

            string actualMessage = errors[0].message;
            string expectedMessage = "We’re unable to update your meal preference right now. Please try again later.";

            Assert.True(actualMessage.Contains(expectedMessage), $"Expected generic error message but actual mesage is {actualMessage}");
        }

        private static void ValidatePnrDetails(Dictionary<string, string> pnrDetails)
        {
            string confirmationNum = pnrDetails != null && pnrDetails.TryGetValue("pnr", out string foundPnr) ? foundPnr ?? string.Empty : string.Empty;
            string givenNames = pnrDetails != null && pnrDetails.TryGetValue("firstName", out string foundFirstName) ? foundFirstName ?? string.Empty : string.Empty;
            string surname = pnrDetails != null && pnrDetails.TryGetValue("lastName", out string foundLastName) ? foundLastName ?? string.Empty : string.Empty;
            string debugContext = RequestHelper.FormatTripsBookingContext(confirmationNum, givenNames, surname);

            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.ContainsKey("error") ? $"Test case failed: {pnrDetails["error"]}. {debugContext}" : debugContext);

            Assert.True(!string.IsNullOrEmpty(pnrDetails["pnr"]) && !string.IsNullOrEmpty(pnrDetails["firstName"]) && !string.IsNullOrEmpty(pnrDetails["lastName"]) && int.Parse(pnrDetails["passengersCount"]) > 0, $"PNR, First Name, and Last Name should not be null or empty and Pax Count > 0. {debugContext}");
        }
    }
}
