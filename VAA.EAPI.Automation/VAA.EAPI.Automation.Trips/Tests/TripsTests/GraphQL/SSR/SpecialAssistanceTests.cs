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
    public class SpecialAssistanceTests
    {
        public string env = Config.Environment;

        [SkippableFact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "FlightServicesMS")]
        [Trait("SharedServiceConsumer", "Trips")]
        public async Task SharedServicesSignOff_RetrieveSpecialAssistance() =>
            await GetSpecialAssistance_ReturnsAssistance(JourneyType.OneWay, ReservationType.REVENUE);

        [SkippableFact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "FlightServicesMS")]
        [Trait("SharedServiceConsumer", "Trips")]
        public async Task SharedServicesSignOff_UpdateSpecialAssistance() =>
            await UpdateSpecialAssistance_UpdatesBooking(JourneyType.OneWay, LocationType.LHR, LocationType.JFK, FlightType.VS, SpecialAssistanceCategory.WCH);

        [Theory]
        [Trait("Category", Traits.SpecialAssistanceMobileRegression)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public async Task GetSpecialAssistance_ReturnsAssistance(JourneyType journeyType, ReservationType reservationType)
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

            #region Graphql call for GetSpecialAssistance response (single call for all segments)

            HttpResponseMessage getSpecialAssistanceResponse = GraphQlEndpoint.GetSpecialAssistance(env, bookingId).EnsureIsSuccessStatusCode();
            dynamic getSpecialAssistanceResult = JsonConvert.DeserializeObject<dynamic>(await getSpecialAssistanceResponse.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(getSpecialAssistanceResult, "errors"), $"Expected a valid response from Graphql but found \n {getSpecialAssistanceResult}");
            Assert.True(DynamicHelper.HasProperty(getSpecialAssistanceResult, "data"), "Response does not contain data property");
            dynamic? dataObj = getSpecialAssistanceResult.data;
            Assert.True(dataObj != null, "data property should not be null");
            Assert.True(DynamicHelper.HasProperty(dataObj, "GetSpecialAssistance"), "Response does not contain getSpecialAssistance property");
            dynamic? getSpecialAssistance = dataObj != null ? dataObj.GetSpecialAssistance : null;
            Assert.True(getSpecialAssistance != null, "getSpecialAssistance property should not be null");
            Assert.True(DynamicHelper.HasProperty(getSpecialAssistance, "passengers"), "getSpecialAssistance should contain 'passengers' property");
            dynamic? passengersArray = getSpecialAssistance != null ? getSpecialAssistance.passengers : null;
            Assert.True(passengersArray != null, "Passengers array should not be null");
            dynamic passengerList = passengersArray as IEnumerable<dynamic>;
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
                dynamic segmentList = passengerSegments as IEnumerable<dynamic>;
                softAssert.True(segmentList != null, "PassengerSegments list should not be null");
                foreach (dynamic? segment in segmentList)
                {
                    softAssert.True(DynamicHelper.HasProperty(segment, "flightSegment"), "Segment should have 'flightSegment'");
                    softAssert.True(DynamicHelper.HasProperty(segment, "serviceRequests"), "Segment should have 'serviceRequests'");
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
                    dynamic? serviceRequests = segment != null ? segment.serviceRequests : null;
                    softAssert.True(serviceRequests != null, "ServiceRequests should not be null");
                    dynamic requestList = serviceRequests as IEnumerable<dynamic>;
                    softAssert.True(requestList != null, "ServiceRequests list should not be null");
                    foreach (dynamic serviceRequest in requestList)
                    {
                        softAssert.True(DynamicHelper.HasProperty(serviceRequest, "categoryCode"), "ServiceRequest should have 'categoryCode'");
                        softAssert.True(DynamicHelper.HasProperty(serviceRequest, "categoryDetails"), "ServiceRequest should have 'categoryDetails'");
                        softAssert.True(DynamicHelper.HasProperty(serviceRequest, "isSegmentSelectEligible"), "ServiceRequest should have 'isSegmentSelectEligible'");
                        dynamic? categoryDetails = serviceRequest != null ? serviceRequest.categoryDetails : null;
                        softAssert.True(categoryDetails != null, "CategoryDetails should not be null");
                        dynamic detailList = categoryDetails as IEnumerable<dynamic>;
                        softAssert.True(detailList != null, "CategoryDetails list should not be null");
                        if (detailList != null)
                        {
                            foreach (dynamic detail in detailList)
                            {
                                softAssert.True(DynamicHelper.HasProperty(detail, "serviceCode"), "CategoryDetail should have 'serviceCode'");
                                softAssert.True(DynamicHelper.HasProperty(detail, "serviceName"), "CategoryDetail should have 'serviceName'");
                                softAssert.True(DynamicHelper.HasProperty(detail, "isSelected"), "CategoryDetail should have 'isSelected'");
                                softAssert.True(DynamicHelper.HasProperty(detail, "isEligible"), "CategoryDetail should have 'isEligible'");
                                softAssert.True(DynamicHelper.HasProperty(detail, "features"), "CategoryDetail should have 'features'");
                                softAssert.True(DynamicHelper.HasProperty(detail, "isChangeAllowed"), "CategoryDetail should have 'isChangeAllowed'");
                            }
                        }
                    }
                }
                softAssert.AssertAll();
            }

            passengersArray = getSpecialAssistance != null ? getSpecialAssistance.passengers : null;
            if (passengersArray is IEnumerable<dynamic> passengerEnumerable)
            {
                foreach (dynamic passenger in passengerEnumerable)
                {
                    SoftAssertHelper softAssert = new();
                    softAssert.True(DynamicHelper.HasProperty(passenger, "passengerResourceId"), "Passenger should have 'passengerResourceId'");
                    softAssert.True(DynamicHelper.HasProperty(passenger, "firstName"), "Passenger should have 'firstName'");
                    softAssert.True(DynamicHelper.HasProperty(passenger, "lastName"), "Passenger should have 'lastName'");
                    softAssert.True(DynamicHelper.HasProperty(passenger, "passengerSegments"), "Passenger should have 'passengerSegments'");

                    dynamic? passengerSegments = passenger != null ? passenger.passengerSegments : null;
                    softAssert.True(passengerSegments != null, "PassengerSegments should not be null");
                    dynamic segmentList = passengerSegments as IEnumerable<dynamic>;
                    softAssert.True(segmentList != null, "PassengerSegments list should not be null");
                    if (segmentList != null)
                    {
                        foreach (dynamic? segment in segmentList)
                        {
                            softAssert.True(DynamicHelper.HasProperty(segment, "flightSegment"), "Segment should have 'flightSegment'");
                            softAssert.True(DynamicHelper.HasProperty(segment, "serviceRequests"), "Segment should have 'serviceRequests'");

                            dynamic? flightSegment = segment != null ? segment.flightSegment : null;
                            softAssert.True(flightSegment != null, "FlightSegment should not be null");
                            if (flightSegment != null)
                            {
                                softAssert.True(DynamicHelper.HasProperty(flightSegment, "segmentId"), "FlightSegment should have 'segmentId'");
                                softAssert.True(DynamicHelper.HasProperty(flightSegment, "ordinal"), "FlightSegment should have 'ordinal'");
                                softAssert.True(DynamicHelper.HasProperty(flightSegment, "operatingAirlines"), "FlightSegment should have 'operatingAirlines'");
                                softAssert.True(DynamicHelper.HasProperty(flightSegment, "flightNumber"), "FlightSegment should have 'flightNumber'");
                                softAssert.True(DynamicHelper.HasProperty(flightSegment, "marketingFlight"), "FlightSegment should have 'marketingFlight'");
                                softAssert.True(DynamicHelper.HasProperty(flightSegment, "departureAirport"), "FlightSegment should have 'departureAirport'");
                                softAssert.True(DynamicHelper.HasProperty(flightSegment, "arrivalAirport"), "FlightSegment should have 'arrivalAirport'");
                                softAssert.True(DynamicHelper.HasProperty(flightSegment, "checkinStatus"), "FlightSegment should have 'checkinStatus'");
                                softAssert.True(DynamicHelper.HasProperty(flightSegment, "flightLegs"), "FlightSegment should have 'flightLegs'");
                            }

                            dynamic? serviceRequests = segment != null ? segment.serviceRequests : null;
                            softAssert.True(serviceRequests != null, "ServiceRequests should not be null");
                            if (serviceRequests != null)
                            {
                                foreach (dynamic serviceRequest in serviceRequests)
                                {
                                    softAssert.True(DynamicHelper.HasProperty(serviceRequest, "categoryCode"), "ServiceRequest should have 'categoryCode'");
                                    softAssert.True(DynamicHelper.HasProperty(serviceRequest, "categoryDetails"), "ServiceRequest should have 'categoryDetails'");
                                    softAssert.True(DynamicHelper.HasProperty(serviceRequest, "isSegmentSelectEligible"), "ServiceRequest should have 'isSegmentSelectEligible'");

                                    dynamic? categoryDetails = serviceRequest.categoryDetails;
                                    softAssert.True(categoryDetails != null, "CategoryDetails should not be null");
                                    dynamic detailList = categoryDetails as IEnumerable<dynamic>;
                                    softAssert.True(detailList != null, "CategoryDetails list should not be null");
                                    if (detailList != null)
                                    {
                                        foreach (dynamic detail in detailList)
                                        {
                                            softAssert.True(DynamicHelper.HasProperty(detail, "serviceCode"), "CategoryDetail should have 'serviceCode'");
                                            softAssert.True(DynamicHelper.HasProperty(detail, "serviceName"), "CategoryDetail should have 'serviceName'");
                                            softAssert.True(DynamicHelper.HasProperty(detail, "isSelected"), "CategoryDetail should have 'isSelected'");
                                            softAssert.True(DynamicHelper.HasProperty(detail, "isEligible"), "CategoryDetail should have 'isEligible'");
                                            softAssert.True(DynamicHelper.HasProperty(detail, "features"), "CategoryDetail should have 'features'");
                                            softAssert.True(DynamicHelper.HasProperty(detail, "isChangeAllowed"), "CategoryDetail should have 'isChangeAllowed'");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    softAssert.AssertAll();
                }
            }
            #endregion
        }

        [Theory]
        [Trait("Category", Traits.SpecialAssistanceMobileRegression)]
        [InlineData(JourneyType.OneWay, LocationType.LHR, LocationType.DEL, FlightType.VS, SpecialAssistanceCategory.WCH)]
        [InlineData(JourneyType.OneWay, LocationType.LHR, LocationType.JFK, FlightType.VS, SpecialAssistanceCategory.WCH)]
        [InlineData(JourneyType.OneWay, LocationType.LHR, LocationType.ATL, FlightType.VS, SpecialAssistanceCategory.WCH)]
        [InlineData(JourneyType.OneWay, LocationType.LHR, LocationType.DEL, FlightType.VS, SpecialAssistanceCategory.DEA)]
        [InlineData(JourneyType.OneWay, LocationType.LHR, LocationType.JFK, FlightType.VS, SpecialAssistanceCategory.DEA)]
        [InlineData(JourneyType.OneWay, LocationType.LHR, LocationType.ATL, FlightType.VS, SpecialAssistanceCategory.DEA)]
        [InlineData(JourneyType.OneWay, LocationType.LHR, LocationType.DEL, FlightType.VS, SpecialAssistanceCategory.BLN)]
        [InlineData(JourneyType.OneWay, LocationType.LHR, LocationType.JFK, FlightType.VS, SpecialAssistanceCategory.BLN)]
        [InlineData(JourneyType.OneWay, LocationType.LHR, LocationType.ATL, FlightType.VS, SpecialAssistanceCategory.BLN)]
        public async Task UpdateSpecialAssistance_UpdatesBooking(JourneyType journeyType, LocationType originAirport, LocationType destinationAirport, FlightType flighttype, SpecialAssistanceCategory ssrCategoryCode)
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

            #region Graphql call for special Assistance and checking ssr code assigned to the Pax

            HttpResponseMessage getSpecialAssistanceResponse = GraphQlEndpoint.GetSpecialAssistance(env, bookingId).EnsureIsSuccessStatusCode();
            dynamic getSpecialAssistanceResult = JsonConvert.DeserializeObject<dynamic>(await getSpecialAssistanceResponse.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(getSpecialAssistanceResult, "errors"), $"Expected a valid response from Graphql but found \n {getSpecialAssistanceResult}");
            Assert.True(DynamicHelper.HasProperty(getSpecialAssistanceResult, "data"), "Response does not contain data property");
            dynamic? dataObj = getSpecialAssistanceResult.data;
            Assert.True(dataObj != null, "data property should not be null");
            Assert.True(DynamicHelper.HasProperty(dataObj, "GetSpecialAssistance"), "Response does not contain GetSpecialAssistance property");
            dynamic? getSpecialAssistance = dataObj != null ? dataObj.GetSpecialAssistance : null;
            Assert.True(getSpecialAssistance != null, "getSpecialAssistance property should not be null");
            Assert.True(DynamicHelper.HasProperty(getSpecialAssistance, "passengers"), "getSpecialAssistance should contain 'passengers' property");
            dynamic? passengersArray = getSpecialAssistance != null ? getSpecialAssistance.passengers : null;
            Assert.True(passengersArray != null, "Passengers array should not be null");
            dynamic passengerList = passengersArray as IEnumerable<dynamic>;
            Assert.True(passengerList != null, "Passengers list should not be null");

            // Save WCHR categories selection details for each passenger/segment
            dynamic wchSelections = new List<(string passengerResourceId, List<string> segmentId, string wchSelectedChoice)>();

            // save DEA category selection details for each passenger/segment
            dynamic deaSelections = new List<(string passengerResourceId, List<string> segmentId, string deaSelectedChoice)>();

            // Save BLN categories selection details for each passenger/segment
            dynamic blnSelections = new List<(string passengerResourceId, List<string> segmentId, string blnSelectedChoice)>();


            foreach (dynamic? passenger in passengerList)
            {
                dynamic? passengerSegments = passenger != null ? passenger.passengerSegments : null;
                dynamic segmentList = passengerSegments as IEnumerable<dynamic>;
                foreach (dynamic? segment in segmentList)
                {
                    dynamic? flightSegment = segment != null ? segment.flightSegment : null;
                    dynamic? serviceRequests = segment != null ? segment.serviceRequests : null;
                    dynamic serviceRequestList = serviceRequests as IEnumerable<dynamic>;
                    foreach (dynamic serviceRequest in serviceRequestList)
                    {
                        string categoryCode = serviceRequest.categoryCode;
                        if (categoryCode == "WCH")
                        {
                            dynamic categoryDetails = serviceRequest.categoryDetails as IEnumerable<dynamic>;
                            if (categoryDetails != null)
                            {
                                foreach (dynamic detail in categoryDetails)
                                {
                                    bool isSelected = detail.isSelected;
                                    if (isSelected)
                                    {
                                        string wchSelectedChoice = detail.serviceCode;
                                        string passengerResourceId = passenger.passengerResourceId;
                                        List<string> segmentIds = new()
                                        { ((JObject)flightSegment)["segmentId"].ToString() };
                                        wchSelections.Add((passengerResourceId, segmentIds, wchSelectedChoice));
                                    }
                                }
                            }
                        }
                        else if (categoryCode == "DEA")
                        {
                            dynamic categoryDetails = serviceRequest.categoryDetails as IEnumerable<dynamic>;
                            if (categoryDetails != null)
                            {
                                foreach (dynamic detail in categoryDetails)
                                {
                                    bool isSelected = detail.isSelected;
                                    if (isSelected)
                                    {
                                        string deaSelectedChoice = detail.serviceCode;
                                        string passengerResourceId = passenger.passengerResourceId;
                                        List<string> segmentIds = new()
                                        { ((JObject)flightSegment)["segmentId"].ToString() };
                                        deaSelections.Add((passengerResourceId, segmentIds, deaSelectedChoice));
                                    }
                                }
                            }
                        }
                        else if (categoryCode == "BLN")
                        {
                            dynamic categoryDetails = serviceRequest.categoryDetails as IEnumerable<dynamic>;
                            if (categoryDetails != null)
                            {
                                foreach (dynamic detail in categoryDetails)
                                {
                                    bool isSelected = detail.isSelected;
                                    if (isSelected)
                                    {
                                        string blnSelectedChoice = detail.serviceCode;
                                        string passengerResourceId = passenger.passengerResourceId;
                                        List<string> segmentIds = new()
                                        { ((JObject)flightSegment)["segmentId"].ToString() };
                                        blnSelections.Add((passengerResourceId, segmentIds, blnSelectedChoice));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region - Update the ssrCategories and validate the response

            // List of possible SSR codes
            string ssrCategoryCodeString = ssrCategoryCode.ToString();

            if (ssrCategoryCodeString == "WCH")
            {
                List<string> possibleWchCodes = new() { "WCHR", "WCHS", "WCHC", "NASS" };
                foreach ((string passengerResourceId, List<string> segmentId, string wchSelectedChoice) selection in wchSelections)
                {
                    string newWchCode = possibleWchCodes.FirstOrDefault(code => code != selection.wchSelectedChoice);
                    if (!string.IsNullOrEmpty(newWchCode))
                    {
                        HttpResponseMessage SelectSpecialAssistanceResponse = GraphQlEndpoint.SelectSpecialAssistance(env, bookingId, selection.passengerResourceId, selection.segmentId, newWchCode, ssrCategoryCodeString).EnsureIsSuccessStatusCode();
                        dynamic selectSpecialAssistanceMutationResult = JsonConvert.DeserializeObject<dynamic>(await SelectSpecialAssistanceResponse.Content.ReadAsStringAsync());
                        Assert.True(!DynamicHelper.HasProperty(selectSpecialAssistanceMutationResult, "errors"), $"Expected a valid response from Graphql but found \n {selectSpecialAssistanceMutationResult}");
                        Assert.True(DynamicHelper.HasProperty(selectSpecialAssistanceMutationResult, "data"), "Response does not contain data property");
                        dynamic? selectSpecialAssistanceObj = selectSpecialAssistanceMutationResult.data;
                        Assert.True(selectSpecialAssistanceObj != null, "data property should not be null");
                        Assert.True(DynamicHelper.HasProperty(selectSpecialAssistanceObj, "selectSpecialAssistanceMutation"), "Response does not contain selectSpecialAssistanceMutation property");
                        dynamic? SelectSpecialAssistance = selectSpecialAssistanceObj != null ? selectSpecialAssistanceObj.selectSpecialAssistanceMutation : null;
                        dynamic passengersAfterUpdate = SelectSpecialAssistance.passengers as IEnumerable<dynamic>;
                        bool found = false;
                        foreach (dynamic passenger in passengersAfterUpdate)
                        {
                            if (passenger.passengerResourceId == selection.passengerResourceId)
                            {
                                dynamic passengerSegments = passenger.passengerSegments as IEnumerable<dynamic>;
                                Assert.NotNull(passengerSegments);
                                foreach (dynamic segment in passengerSegments)
                                {
                                    dynamic? flightSegment = segment.flightSegment;
                                    if (flightSegment != null && selection.segmentId.Contains(((JObject)flightSegment)["segmentId"].ToString()))
                                    {
                                        dynamic serviceRequests = segment.serviceRequests as IEnumerable<dynamic>;
                                        Assert.NotNull(serviceRequests);
                                        foreach (dynamic serviceRequest in serviceRequests)
                                        {
                                            if (serviceRequest.categoryCode == ssrCategoryCodeString)
                                            {
                                                dynamic categoryDetails = serviceRequest.categoryDetails as IEnumerable<dynamic>;
                                                Assert.NotNull(categoryDetails);
                                                foreach (dynamic detail in categoryDetails)
                                                {
                                                    string serviceCode = detail.serviceCode;
                                                    bool isSelected = detail.isSelected;
                                                    if (serviceCode == newWchCode)
                                                    {
                                                        Assert.True(isSelected, $"Expected SSR code '{newWchCode}' to be selected for passenger {selection.passengerResourceId} segment {selection.segmentId}.");
                                                        found = true;
                                                    }
                                                    else
                                                    {
                                                        Assert.False(isSelected, $"SSR code '{serviceCode}' should not be selected for passenger {selection.passengerResourceId} segment {selection.segmentId}.");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        Assert.True(found, $"Updated SSR code '{newWchCode}' not found in response for passenger {selection.passengerResourceId} segment {selection.segmentId}.");
                    }
                }
            }


            else if (ssrCategoryCodeString == "BLN")
            {
                List<string> possibleBlnCodes = new()
                { "BLND", "NASS" };
                foreach ((string passengerResourceId, List<string> segmentId, string blnSelectedChoice) selection in blnSelections)
                {
                    string newBlnCode = possibleBlnCodes.FirstOrDefault(code => code != selection.blnSelectedChoice);
                    if (!string.IsNullOrEmpty(newBlnCode))
                    {
                        HttpResponseMessage SelectSpecialAssistanceResponse = GraphQlEndpoint.SelectSpecialAssistance(env, bookingId, selection.passengerResourceId, selection.segmentId, newBlnCode, ssrCategoryCodeString).EnsureIsSuccessStatusCode();
                        dynamic selectSpecialAssistanceMutationResult = JsonConvert.DeserializeObject<dynamic>(await SelectSpecialAssistanceResponse.Content.ReadAsStringAsync());
                        Assert.True(!DynamicHelper.HasProperty(selectSpecialAssistanceMutationResult, "errors"), $"Expected a valid response from Graphql but found \n {selectSpecialAssistanceMutationResult}");
                        Assert.True(DynamicHelper.HasProperty(selectSpecialAssistanceMutationResult, "data"), "Response does not contain data property");
                        dynamic? selectSpecialAssistanceObj = selectSpecialAssistanceMutationResult.data;
                        Assert.True(selectSpecialAssistanceObj != null, "data property should not be null");
                        Assert.True(DynamicHelper.HasProperty(selectSpecialAssistanceObj, "selectSpecialAssistanceMutation"), "Response does not contain selectSpecialAssistanceMutation property");
                        dynamic? SelectSpecialAssistance = selectSpecialAssistanceObj != null ? selectSpecialAssistanceObj.selectSpecialAssistanceMutation : null;
                        dynamic passengersAfterUpdate = SelectSpecialAssistance.passengers as IEnumerable<dynamic>;
                        bool found = false;
                        foreach (dynamic passenger in passengersAfterUpdate)
                        {
                            if (passenger.passengerResourceId == selection.passengerResourceId)
                            {
                                dynamic passengerSegments = passenger.passengerSegments as IEnumerable<dynamic>;
                                Assert.NotNull(passengerSegments);
                                foreach (dynamic segment in passengerSegments)
                                {
                                    dynamic? flightSegment = segment.flightSegment;
                                    if (flightSegment != null && selection.segmentId.Contains(((JObject)flightSegment)["segmentId"].ToString()))
                                    {
                                        dynamic serviceRequests = segment.serviceRequests as IEnumerable<dynamic>;
                                        Assert.NotNull(serviceRequests);
                                        foreach (dynamic serviceRequest in serviceRequests)
                                        {
                                            if (serviceRequest.categoryCode == ssrCategoryCodeString)
                                            {
                                                dynamic categoryDetails = serviceRequest.categoryDetails as IEnumerable<dynamic>;
                                                Assert.NotNull(categoryDetails);
                                                foreach (dynamic detail in categoryDetails)
                                                {
                                                    string serviceCode = detail.serviceCode;
                                                    bool isSelected = detail.isSelected;
                                                    if (serviceCode == newBlnCode)
                                                    {
                                                        Assert.True(isSelected, $"Expected SSR code '{newBlnCode}' to be selected for passenger {selection.passengerResourceId} segment {selection.segmentId}.");
                                                        found = true;
                                                    }
                                                    else
                                                    {
                                                        Assert.False(isSelected, $"SSR code '{serviceCode}' should not be selected for passenger {selection.passengerResourceId} segment {selection.segmentId}.");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        Assert.True(found, $"Updated SSR code '{newBlnCode}' not found in response for passenger {selection.passengerResourceId} segment {selection.segmentId}.");
                    }
                }
            }

            if (ssrCategoryCodeString == "DEA")
            {
                List<string> possibleDeaCodes = new()
                { "DEAF", "NASS" };
                foreach ((string passengerResourceId, List<string> segmentId, string deaSelectedChoice) selection in deaSelections)
                {
                    string newDeaCode = possibleDeaCodes.FirstOrDefault(code => code != selection.deaSelectedChoice);
                    if (!string.IsNullOrEmpty(newDeaCode))
                    {
                        HttpResponseMessage SelectSpecialAssistanceResponse = GraphQlEndpoint.SelectSpecialAssistance(env, bookingId, selection.passengerResourceId, selection.segmentId, newDeaCode, ssrCategoryCodeString).EnsureIsSuccessStatusCode();
                        dynamic selectSpecialAssistanceMutationResult = JsonConvert.DeserializeObject<dynamic>(await SelectSpecialAssistanceResponse.Content.ReadAsStringAsync());
                        Assert.True(!DynamicHelper.HasProperty(selectSpecialAssistanceMutationResult, "errors"), $"Expected a valid response from Graphql but found \n {selectSpecialAssistanceMutationResult}");
                        Assert.True(DynamicHelper.HasProperty(selectSpecialAssistanceMutationResult, "data"), "Response does not contain data property");
                        dynamic? selectSpecialAssistanceObj = selectSpecialAssistanceMutationResult.data;
                        Assert.True(selectSpecialAssistanceObj != null, "data property should not be null");
                        Assert.True(DynamicHelper.HasProperty(selectSpecialAssistanceObj, "selectSpecialAssistanceMutation"), "Response does not contain selectSpecialAssistanceMutation property");
                        dynamic? SelectSpecialAssistance = selectSpecialAssistanceObj != null ? selectSpecialAssistanceObj.selectSpecialAssistanceMutation : null;
                        dynamic passengersAfterUpdate = SelectSpecialAssistance.passengers as IEnumerable<dynamic>;
                        bool found = false;
                        foreach (dynamic passenger in passengersAfterUpdate)
                        {
                            if (passenger.passengerResourceId == selection.passengerResourceId)
                            {
                                dynamic passengerSegments = passenger.passengerSegments as IEnumerable<dynamic>;
                                Assert.NotNull(passengerSegments);
                                foreach (dynamic segment in passengerSegments)
                                {
                                    dynamic? flightSegment = segment.flightSegment;
                                    if (flightSegment != null && selection.segmentId.Contains(((JObject)flightSegment)["segmentId"].ToString()))
                                    {
                                        dynamic serviceRequests = segment.serviceRequests as IEnumerable<dynamic>;
                                        Assert.NotNull(serviceRequests);
                                        foreach (dynamic serviceRequest in serviceRequests)
                                        {
                                            if (serviceRequest.categoryCode == ssrCategoryCodeString)
                                            {
                                                dynamic categoryDetails = serviceRequest.categoryDetails as IEnumerable<dynamic>;
                                                Assert.NotNull(categoryDetails);
                                                foreach (dynamic detail in categoryDetails)
                                                {
                                                    string serviceCode = detail.serviceCode;
                                                    bool isSelected = detail.isSelected;
                                                    if (serviceCode == newDeaCode)
                                                    {
                                                        Assert.True(isSelected, $"Expected SSR code '{newDeaCode}' to be selected for passenger {selection.passengerResourceId} segment {selection.segmentId}.");
                                                        found = true;
                                                    }
                                                    else
                                                    {
                                                        Assert.False(isSelected, $"SSR code '{serviceCode}' should not be selected for passenger {selection.passengerResourceId} segment {selection.segmentId}.");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        Assert.True(found, $"Updated SSR code '{newDeaCode}' not found in response for passenger {selection.passengerResourceId} segment {selection.segmentId}.");
                    }
                }
            }



            #endregion
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

        [Fact]
        [Trait("Category", Traits.SpecialAssistanceMobileRegression)]
        public async Task UpdateSpecialAssistance_InvalidRequest_ReturnsExpectedError()
        {

            #region - pre-requisite for fetching the PNR from the storage table

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek("OneWay", "REVENUE");

            ValidatePnrDetails(pnrDetails);

            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            string origin = pnrDetails["origin"];
            string destination = pnrDetails["destination"];
            string debugContext = RequestHelper.FormatTripsBookingContext(pnr, firstName, lastName);

            Assert.True(!string.IsNullOrEmpty(pnr) && !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName), $"PNR, First Name, and Last Name should not be null. {debugContext}");

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

            #endregion

            #region - Graphql call for special Assistance and checking for the error scenario validations

            string passengerResourceId = "PAX123";
            List<string> segmentIds = new()
            { "SEG123" };
            string ssrCode = "WCHR";
            string ssrCategoryCode = "WCH";

            HttpResponseMessage selectSpecialAssistanceResponse = GraphQlEndpoint.SelectSpecialAssistance(env, bookingId, passengerResourceId, segmentIds, ssrCode, ssrCategoryCode).EnsureIsSuccessStatusCode();

            dynamic selectSpecialAssistanceMutationResult = JsonConvert.DeserializeObject<dynamic>(await selectSpecialAssistanceResponse.Content.ReadAsStringAsync());

            Assert.True(DynamicHelper.HasProperty(selectSpecialAssistanceMutationResult, "errors"), $"Expected a valid response from Graphql but found \n {selectSpecialAssistanceMutationResult}. {debugContext}");

            ValidateSpecialAssistanceErrorResponse(selectSpecialAssistanceMutationResult);

            #endregion
        }

        private static void ValidateSpecialAssistanceErrorResponse(dynamic errorResult)
        {
            Assert.True(DynamicHelper.HasProperty(errorResult, "errors"), "Response should contain errors");

            dynamic? errors = errorResult.errors;

            Assert.True(errors != null && errors.Count > 0, "Response should contain at least one error and errors should not be null or empty");

            Assert.True(errors[0].ToString().Contains("We couldn’t process your special assistance request at the moment. Please try again later."), $"Unexpected error message. Actual: {errors[0]}");

            dynamic extensions = errors[0].extensions;
            List<dynamic>? responseArray = JsonConvert.DeserializeObject<List<dynamic>>((string)extensions.response);

            Assert.True(responseArray != null && ((string)responseArray[0].code).Equals("MA03") && ((string)responseArray[0].message).Equals("We couldn’t process your special assistance request at the moment. Please try again later."), $"Unexpected response values. Actual: code={(string)responseArray[0].code}, message={(string)responseArray[0].message}");
        }
    }
}
