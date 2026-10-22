using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.GraphQL
{
    public class RetrieveBookingTests : TripsGraphQlTestsBase
    {

        [SkippableFact]
        [Trait("Category", Traits.RetrieveBookingMobileRegression)]
    [Trait("Category", Traits.SharedServicesSignOff)]
    [Trait("SharedService", "Retail.OrderSearchMS")]
    [Trait("SharedServiceConsumer", "Trips")]
    [Trait("TestType", "Contract")]
    public async Task ValidateLoyaltyMembershipInBookingResponse()
        {
            #region - pre-requisite for fetching the PNR from the storage table

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeekWithLoyaltyNumberPresent();

            ValidatePnrDetails(pnrDetails);

            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            int paxCount = int.Parse(pnrDetails["passengersCount"]);
            string loyaltyNum = pnrDetails["flyingClubNumber"];
            // origin/destination/journeyType are not required for this validation

            string debugContext = RequestHelper.FormatTripsBookingContext(pnr, firstName, lastName);

            Assert.True(!string.IsNullOrEmpty(pnr) && !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName) && paxCount > 0, $"PNR, First Name, and Last Name should not be null or empty and Pax Count > 0. {debugContext}");

            Assert.True(!string.IsNullOrEmpty(pnrDetails["flyingClubNumber"]), $"FlyingClubNumber should not be null or empty when present in PNR details. {debugContext}");

            #endregion

            #region - GraphQL call for retrieve trip 
            string usingString = "PNR";

            HttpResponseMessage response = GraphQlEndpoint.Booking(env, usingString, pnr, firstName, lastName).EnsureIsSuccessStatusCode();
            dynamic BookingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            #endregion

            #region - Validations

            // Basic error check
            Assert.True(!DynamicHelper.HasProperty(BookingResult, "errors"), $"Expected a valid response from Graphql but found {BookingResult}. {debugContext}");

            // Validate data exists
            Assert.True(DynamicHelper.HasProperty(BookingResult, "data"), $"Response should contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(BookingResult.data, "Booking"), $"Response should contain Booking property. {debugContext}");

            // Validate booking details
            dynamic bookingdata = BookingResult.data.Booking;
            Assert.NotNull(bookingdata);

            // Validate Products
            Assert.NotNull(bookingdata.product);
            dynamic airlineProduct = bookingdata.product[0];

            // Airline Product Validations
            Assert.True(airlineProduct["bookingType"].ToString() == "AIRLINE", $"Expected booking type to be AIRLINE but got {(string)airlineProduct.bookingType}. {debugContext}");
            Assert.True(airlineProduct["reference"]["pnrLocator"].ToString() == pnr, $"Expected PNR locator to be {pnr} but got {(string)airlineProduct.reference.pnrLocator}. {debugContext}");

            // Validate Loyalty number in the response passenger object
            Assert.True(DynamicHelper.HasProperty(bookingdata, "passengers"), $"Booking should contain passengers property. {debugContext}");
            dynamic passengers = bookingdata.passengers;
            Assert.NotNull(passengers);
            Assert.True(passengers.Count > 0, $"There should be at least one passenger in the booking. {debugContext}");

            dynamic leadPassenger = passengers[0];

            if (DynamicHelper.HasProperty(leadPassenger, "loyaltyMembership") && leadPassenger.loyaltyMembership != null)

            {

                JObject loyaltyMembership = leadPassenger.loyaltyMembership;


                Assert.True(DynamicHelper.HasProperty(loyaltyMembership, "programmeCode"), "loyaltyMembership should contain programmeCode");

                Assert.True(DynamicHelper.HasProperty(loyaltyMembership, "number"), "loyaltyMembership should contain number");


                Assert.True(loyaltyNum.Equals(loyaltyMembership["number"]?.ToString()), $"loyalty number expected to be {loyaltyNum} but got {loyaltyMembership["number"]}");

            }

            else
            {
                Assert.True(false, $"Lead passenger should have loyaltyMembership. PNR from storage has loyalty number {loyaltyNum} but API response does not contain loyaltyMembership.");
            }

            #endregion
        }


        [SkippableTheory]
        [Trait("Category", Traits.RetrieveBookingMobileRegression)]
        [Trait("Category", GlobalTraits.MobileAppsSanityTests)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public async Task ValidateCheckInOpensInBookingResponse(JourneyType journeyType, ReservationType reservationType)
        {
            #region - pre-requisite for fetching the PNR from the storage table

            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString);

            ValidatePnrDetails(pnrDetails);

            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];

            #endregion

            #region - GraphQL call for retrieve trip 
            string usingString = "PNR";

            HttpResponseMessage response = GraphQlEndpoint.Booking(env, usingString, pnr, firstName, lastName).EnsureIsSuccessStatusCode();
            dynamic BookingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            #endregion

            #region - Validations for CheckInOpens field

            Assert.True(!DynamicHelper.HasProperty(BookingResult, "errors"), $"Expected a valid response from Graphql but found {BookingResult}");
            Assert.True(DynamicHelper.HasProperty(BookingResult, "data"), "Response should contain data property");
            Assert.True(DynamicHelper.HasProperty(BookingResult.data, "Booking"), "Response should contain Booking property");

            dynamic bookingdata = BookingResult.data.Booking;
            Assert.NotNull(bookingdata.product);
            dynamic airlineProduct = bookingdata.product[0];

            Assert.True(DynamicHelper.HasProperty(airlineProduct, "slices"), "Airline product should contain slices");
            dynamic slices = airlineProduct.slices;
            Assert.True(slices.Count > 0, "There should be at least one slice in the booking");

            foreach (dynamic slice in slices)
            {
                Assert.True(DynamicHelper.HasProperty(slice, "scheduledDepartureUtc"), "Slice should contain scheduledDepartureUtc");
                string scheduledDepartureStr = slice.scheduledDepartureUtc.ToString();
                DateTime scheduledDeparture = DateTime.Parse(scheduledDepartureStr);
                DateTime currentTime = DateTime.UtcNow;
                TimeSpan timeDifference = scheduledDeparture - currentTime;

                if (timeDifference.TotalDays > 7)
                {
                    Assert.True(slice.checkInOpensLocal == null, $"checkInOpensLocal should be null when flight is more than 7 days away. Scheduled departure: {scheduledDeparture}, Current time: {currentTime}");
                }
                else if (timeDifference.TotalDays is <= 7 and > 0)
                {
                    Assert.True(slice.checkInOpensLocal != null, $"CheckInOpens should not be null when flight is within 7 days. Scheduled departure: {scheduledDeparture}");

                    DateTime checkInOpensLocal = DateTime.Parse(slice.checkInOpensLocal.ToString());
                    DateTime expectedCheckInOpensLocal = scheduledDeparture.AddHours(-24);

                    TimeSpan checkInDifference = checkInOpensLocal - expectedCheckInOpensLocal;
                    Assert.True(Math.Abs(checkInDifference.TotalMinutes) <= 1, $"CheckInOpens should be 24 hours before scheduled departure. Expected: {expectedCheckInOpensLocal}, Actual: {checkInOpensLocal}");
                }
            }

            #endregion
        }

        [SkippableTheory]
        [Trait("Category", Traits.RetrieveBookingMobileRegression)]
        [InlineData("PNR", "C9FOKD-INVALID", "", "")]
        [InlineData("PNR", "", "INVALIDFIRSTNAME", "")]
        [InlineData("PNR", "", "", "INVALIDLASTNAME")]
        public async Task ValidateBookingErrorScenarios(string usingString, string testPnr, string testFirstName, string testLastName)
        {
            #region - pre-requisite for fetching the PNR from the storage table

            string journeyTypeString = "OneWay";
            string reservationTypeString = "REVENUE";

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString);

            ValidatePnrDetails(pnrDetails);
            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];

            // Override with scenario-specific test values
            if (!string.IsNullOrEmpty(testPnr))
                pnr = testPnr;

            if (!string.IsNullOrEmpty(testFirstName))
                firstName = testFirstName;

            if (!string.IsNullOrEmpty(testLastName))
                lastName = testLastName;

            #endregion

            #region - GraphQL call

            HttpResponseMessage response = GraphQlEndpoint.Booking(env, usingString, pnr, firstName, lastName).EnsureIsSuccessStatusCode();
            dynamic errorBookingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            ValidateGenericErrorResponse(errorBookingResult);

            #endregion
        }


        [SkippableTheory]
        [Trait("Category", Traits.RetrieveBookingMobileRegression)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        public async Task ValidateAircraftDetailsInBookingResponse(JourneyType journeyType, ReservationType reservationType)
        {
            #region - pre-requisite for fetching the PNR from the storage table 

            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromOCIPNRsStorageTable();

            ValidatePnrDetails(pnrDetails);

            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            int paxCount = int.Parse(pnrDetails["passengersCount"]);
            string debugContext = RequestHelper.FormatTripsBookingContext(pnr, firstName, lastName);

            Assert.True(!string.IsNullOrEmpty(pnr) && !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName) && paxCount > 0, $"PNR, First Name, and Last Name should not be null or empty and Pax Count > 0. {debugContext}");

            #endregion
            #region - GraphQL call for retrieve trip 
            string ticketNum = string.Empty; // as we are performing a PNR Search
            string usingString = "PNR";

            HttpResponseMessage response = GraphQlEndpoint.Booking(env, usingString, pnr, firstName, lastName).EnsureIsSuccessStatusCode();
            dynamic BookingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            #endregion

            #region - Validations

            // Basic error check
            Assert.True(!DynamicHelper.HasProperty(BookingResult, "errors"), $"Expected a valid response from Graphql but found {BookingResult}. {debugContext}");

            // Validate data exists
            Assert.True(DynamicHelper.HasProperty(BookingResult, "data"), $"Response should contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(BookingResult.data, "Booking"), $"Response should contain Booking property. {debugContext}");

            // Validate booking details
            dynamic bookingdata = BookingResult.data.Booking;
            Assert.NotNull(bookingdata);

            // Validate Products
            Assert.NotNull(bookingdata.product);
            dynamic airlineProduct = bookingdata.product[0];

            // Airline Product Validations
            Assert.True(airlineProduct["bookingType"].ToString() == "AIRLINE", $"Expected booking type to be AIRLINE but got {(string)airlineProduct.bookingType}. {debugContext}");
            Assert.True(airlineProduct["reference"]["pnrLocator"].ToString() == pnr, $"Expected PNR locator to be {pnr} but got {(string)airlineProduct.reference.pnrLocator}. {debugContext}");

            // Validate Aircraft details in the response product object
            dynamic leg = airlineProduct.slices[0].segments[0].legs[0];
            Assert.True(DynamicHelper.HasProperty(leg, "aircraft"), "Airline product should contain aircraft property");
            dynamic aircraftDetails = leg.aircraft;
            Assert.True(DynamicHelper.HasProperty(aircraftDetails, "code"), "Aircraft details should contain code property");
            Assert.True(DynamicHelper.HasProperty(aircraftDetails, "name"), "Aircraft details should contain name property");
            Assert.True(DynamicHelper.HasProperty(aircraftDetails, "planeName"), "Aircraft details should contain planeName property");
            Assert.True(DynamicHelper.HasProperty(aircraftDetails, "tailNumber"), "Aircraft details should contain tailNumber property");
            Assert.True(!string.IsNullOrEmpty(aircraftDetails.code?.ToString()), "Aircraft code should be populated");
            Assert.True(!string.IsNullOrEmpty(aircraftDetails.name?.ToString()), "Aircraft name should be populated");
            Assert.True(!string.IsNullOrEmpty(aircraftDetails.planeName?.ToString()), "Aircraft planeName should be populated");
            Assert.True(!string.IsNullOrEmpty(aircraftDetails.tailNumber?.ToString()), "Aircraft tailNumber should be populated");

            #endregion
        }
        [SkippableTheory]
        [Trait("Category", Traits.RetrieveBookingMobileRegression)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public async Task ValidateCabinClassDetailsInBookingResponse(JourneyType journeyType, ReservationType reservationType)
        {
            #region - pre-requisite for fetching the PNR from the storage table

            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString);

            ValidatePnrDetails(pnrDetails);

            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            string debugContext = RequestHelper.FormatTripsBookingContext(pnr, firstName, lastName);

            #endregion

            #region - GraphQL call for retrieve trip

            HttpResponseMessage response = GraphQlEndpoint.Booking(env, "PNR", pnr, firstName, lastName).EnsureIsSuccessStatusCode();
            dynamic bookingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            #endregion

            #region - Validations

            Assert.True(!DynamicHelper.HasProperty(bookingResult, "errors"), $"Expected a valid response from Graphql but found {bookingResult}. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(bookingResult, "data"), $"Response should contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(bookingResult.data, "Booking"), $"Response should contain Booking property. {debugContext}");

            dynamic bookingData = bookingResult.data.Booking;
            Assert.NotNull(bookingData);

            Assert.True(bookingData.product != null && bookingData.product.HasValues, $"Booking should contain at least one product. {debugContext}");

            dynamic airlineProduct = bookingData.product[0];


            Assert.True(DynamicHelper.HasProperty(airlineProduct, "slices") && airlineProduct.slices != null && airlineProduct.slices.HasValues, $"Product should contain at least one slice. {debugContext}");

            dynamic slice = airlineProduct.slices[0];

            Assert.True(DynamicHelper.HasProperty(slice, "segments") && slice.segments != null && slice.segments.HasValues, $"Slice should contain at least one segment. {debugContext}");

            dynamic segment = slice.segments[0];
            Assert.True(DynamicHelper.HasProperty(segment, "legs") && segment.legs != null && segment.legs.HasValues, $"Segment should contain at least one leg. {debugContext}");

            dynamic leg = segment.legs[0];


            const string paymentIncidentHint = "Cabin class is required; missing cabin class has previously contributed to payment failures. If this fails, please investigate the payments flow alongside the Trips GraphQL response.";

            Assert.True(DynamicHelper.HasProperty(leg, "cabinClass"), $"{paymentIncidentHint} {debugContext}");
            dynamic cabinClass = leg.cabinClass;
            Assert.NotNull(cabinClass);

            Assert.True(DynamicHelper.HasProperty(cabinClass, "code"), $"Cabin class should contain code. {paymentIncidentHint} {debugContext}");
            Assert.True(DynamicHelper.HasProperty(cabinClass, "productCode"), $"Cabin class should contain productCode. {paymentIncidentHint} {debugContext}");
            Assert.True(DynamicHelper.HasProperty(cabinClass, "name"), $"Cabin class should contain name. {paymentIncidentHint} {debugContext}");

            string code = cabinClass.code?.ToString() ?? string.Empty;

            string productCode = cabinClass.productCode?.ToString() ?? string.Empty;

            string name = cabinClass.name?.ToString() ?? string.Empty;


            Assert.True(!code.IsNullOrWhiteSpace(), $"Cabin class code should be populated. {paymentIncidentHint} {debugContext}");
            Assert.True(!productCode.IsNullOrWhiteSpace(), $"Cabin class productCode should be populated. {paymentIncidentHint} {debugContext}");
            Assert.True(!name.IsNullOrWhiteSpace(), $"Cabin class name should be populated. {paymentIncidentHint} {debugContext}");

            // Light validation: ensure fields look like real values, without hard-coding the full cabin mapping.
            Assert.True(code.Trim().Length <= 2, $"Cabin class code '{code}' should be 1-2 characters. {paymentIncidentHint} {debugContext}");
            Assert.True(productCode.Trim().Length >= 2, $"Cabin class productCode '{productCode}' should be a valid value. {paymentIncidentHint} {debugContext}");

            #endregion
        }

        [SkippableTheory]
        [Trait("Category", Traits.RetrieveBookingMobileRegression)]
        [InlineData("VS", null, null)]
        [InlineData("DL", "This flight is with Delta Air Lines", "For full support to view and update your information, please contact the operating airline directly.")]
        public async Task ValidateOtherAirlineOperatedHelpAttributesInBookingResponse(
            string partnerCode,
            string? expectedHelpTitle,
            string? expectedHelpText)
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForPartnerAirline(partnerCode);
            ValidatePnrDetails(pnrDetails);

            string pnr = pnrDetails["pnr"];
            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string debugContext = RequestHelper.FormatTripsBookingContext(pnr, firstName, lastName);

            HttpResponseMessage response = GraphQlEndpoint.Booking(env, "PNR", pnr, firstName, lastName).EnsureIsSuccessStatusCode();
            dynamic bookingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(bookingResult, "errors"), $"Expected a valid response from Graphql but found {bookingResult}. {debugContext}");
            dynamic bookingData = bookingResult.data.Booking;
            dynamic airlineProduct = bookingData.product[0];
            bool foundExpectedOperatingCarrier = false;

            foreach (dynamic slice in airlineProduct.slices)
            {
                foreach (dynamic segment in slice.segments)
                {
                    string operatingCarrierCode = segment.operatingCarrier?.carrierCode?.ToString() ?? string.Empty;
                    if (!operatingCarrierCode.Equals(partnerCode, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    foundExpectedOperatingCarrier = true;
                    Assert.Equal(expectedHelpTitle, (string?)segment.otherAirlineOperatedHelpTitle);
                    Assert.Equal(expectedHelpText, (string?)segment.otherAirlineOperatedHelpText);
                }
            }

            Assert.True(foundExpectedOperatingCarrier, $"No {partnerCode}-operated segment was found in the retrieved PNR. {debugContext}");
        }

        [SkippableFact]
        [Trait("Category", Traits.RetrieveBookingMobileRegression)]
        public async Task ValidatePopsOpensSevenDaysBeforeDepartureInBookingResponse()
        {
            dynamic bookingData = await RetrieveOciTomorrowVsBookingDataAsync();
            bool foundApplicableSegment = false;

            foreach (dynamic slice in bookingData.product[0].slices)
            {
                foreach (dynamic segment in slice.segments)
                {
                    if (!IsVsTomorrowSegment(segment))
                        continue;

                    foundApplicableSegment = true;
                    DateTimeOffset departure = DateTimeOffset.Parse(segment.scheduledDepartureUtc.ToString());
                    Assert.NotNull(segment.popsOpenDateTimeUtc);
                    DateTimeOffset popsOpen = DateTimeOffset.Parse(segment.popsOpenDateTimeUtc.ToString());
                    Assert.Equal(departure.AddDays(-7), popsOpen);
                }
            }

            Assert.True(foundApplicableSegment, "No VS-operated flight segment departing tomorrow was found in the OCI PNR.");
        }

        [SkippableFact]
        [Trait("Category", Traits.RetrieveBookingMobileRegression)]
        public async Task ValidatePopsCloses24HoursBeforeDepartureInBookingResponse()
        {
            dynamic bookingData = await RetrieveOciTomorrowVsBookingDataAsync();
            bool foundApplicableSegment = false;

            foreach (dynamic slice in bookingData.product[0].slices)
            {
                foreach (dynamic segment in slice.segments)
                {
                    if (!IsVsTomorrowSegment(segment))
                        continue;

                    foundApplicableSegment = true;
                    DateTimeOffset departure = DateTimeOffset.Parse(segment.scheduledDepartureUtc.ToString());
                    Assert.NotNull(segment.popsCloseDateTimeUtc);
                    DateTimeOffset popsClose = DateTimeOffset.Parse(segment.popsCloseDateTimeUtc.ToString());
                    Assert.Equal(departure.AddHours(-24), popsClose);
                }
            }

            Assert.True(foundApplicableSegment, "No VS-operated flight segment departing tomorrow was found in the OCI PNR.");
        }

        private static bool IsVsTomorrowSegment(dynamic segment)
        {
            string operatingCarrier = segment.operatingCarrier?.carrierCode?.ToString() ?? string.Empty;
            DateTimeOffset departure = DateTimeOffset.Parse(segment.scheduledDepartureUtc.ToString());
            return operatingCarrier.Equals("VS", StringComparison.OrdinalIgnoreCase)
                && departure.UtcDateTime.Date == DateTime.UtcNow.Date.AddDays(1);
        }

        private async Task<dynamic> RetrieveOciTomorrowVsBookingDataAsync()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromOCIPNRsStorageTable();
            ValidatePnrDetails(pnrDetails);

            HttpResponseMessage response = GraphQlEndpoint.Booking(env, "PNR", pnrDetails["pnr"], pnrDetails["firstName"], pnrDetails["lastName"]).EnsureIsSuccessStatusCode();
            dynamic bookingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(bookingResult, "errors"), $"Expected a valid response from Graphql but found {bookingResult}.");
            Assert.True(DynamicHelper.HasProperty(bookingResult, "data"), "Response should contain data property.");
            Assert.True(DynamicHelper.HasProperty(bookingResult.data, "Booking"), "Response should contain Booking property.");
            return bookingResult.data.Booking;
        }

        private async Task<dynamic> RetrieveBookingDataAsync(JourneyType journeyType, ReservationType reservationType)
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyType.ToString(), reservationType.ToString());
            ValidatePnrDetails(pnrDetails);

            HttpResponseMessage response = GraphQlEndpoint.Booking(env, "PNR", pnrDetails["pnr"], pnrDetails["firstName"], pnrDetails["lastName"]).EnsureIsSuccessStatusCode();
            dynamic bookingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(bookingResult, "errors"), $"Expected a valid response from Graphql but found {bookingResult}.");
            Assert.True(DynamicHelper.HasProperty(bookingResult, "data"), "Response should contain data property.");
            Assert.True(DynamicHelper.HasProperty(bookingResult.data, "Booking"), "Response should contain Booking property.");
            return bookingResult.data.Booking;
        }


        private static void ValidateGenericErrorResponse(dynamic errorResult)
        {
            Assert.True(DynamicHelper.HasProperty(errorResult, "errors"), "Response should contain errors");
            dynamic? errors = errorResult.errors;

            Assert.True(errors != null && errors.Count > 0, "Response should contain at least one error and errors should not be null or empty");

            Assert.True(errors[0].ToString().Contains("We couldn’t add your trip at the moment. Please try again later."), $"Unexpected error message. Actual: {errors[0].ToString()}");

            dynamic extensions = errors[0].extensions;
            List<dynamic>? reasonArray = JsonConvert.DeserializeObject<List<dynamic>>((string)extensions.Reason);
            Assert.True(reasonArray != null && ((string)reasonArray[0].code).Equals("MA02") && ((string)reasonArray[0].message).Equals("We couldn’t add your trip at the moment. Please try again later."), $"Unexpected Reason values. Actual: code={(string)reasonArray[0].code}, message={(string)reasonArray[0].message}");
        }

    }
}
