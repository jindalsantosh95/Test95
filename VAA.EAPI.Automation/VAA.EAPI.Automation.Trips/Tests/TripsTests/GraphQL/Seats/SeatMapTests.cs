using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.GraphQL.Seats
{
    [Collection("SeatSharedServicesSignOff")]
    public class SeatMapTests
    {
        public string env = Config.Environment;

        [SkippableFact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "SeatMS")]
        [Trait("SharedServiceConsumer", "Trips")]
        public async Task SharedServicesSignOff_SeatMap() =>
            await VirginAtlanticSeatMap_ReturnsSeatMap(JourneyType.OneWay, FlightType.VS, CabinName.VSCL, "LHR", "JFK");

        [SkippableFact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "SeatMS")]
        [Trait("SharedServiceConsumer", "Trips")]
        public async Task SharedServicesSignOff_DeltaSeatMap() =>
            await DeltaSeatMap_ReturnsSeatMap(JourneyType.OneWay, FlightType.DL, CabinName.MAIN, "LHR", "JFK");

        [SkippableTheory]
        [Trait("Category", Traits.SeatMapMobileRegression)]
        [InlineData(JourneyType.OneWay, FlightType.VS, CabinName.VSCL)]
        [InlineData(JourneyType.OneWay, FlightType.VS, CabinName.VSUP)]
        [InlineData(JourneyType.OneWay, FlightType.VS, CabinName.VSPE)]
        public async Task VirginAtlanticSeatMap_ReturnsSeatMap(JourneyType journeyType, FlightType flighttype, CabinName cabinName, string originFilter = "", string destinationFilter = "")
        {
            #region - pre-requisite for fetching the PNR from the storage table

            string journeyTypeString = journeyType.ToString();
            string originString = originFilter;
            string destinationString = destinationFilter;
            string flightTypeString = flighttype.ToString();
            string cabinNameString = cabinName.ToString();

            string mappedCabinName = cabinName switch
            {
                CabinName.VSCL => "EconomyClassic",
                CabinName.VSPE => "Premium",
                CabinName.VSUP => "UpperClass",
                _ => cabinName.ToString()
            };

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForSpecificRoute(journeyTypeString, originString, destinationString, flightTypeString, mappedCabinName);

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
            Assert.True(totalSegmentCount > 0, $"Total segment count should be greater than 0. {debugContext}");

            #endregion

            #region Graphql call for GetSeatMap response for all segments

            // Loop through all segments and validate seat map for each
            int segmentIndex = 1;
            foreach (dynamic slice in slices)
            {
                dynamic segments = slice.segments;
                if (segments != null)
                {
                    foreach (object segment in segments)
                    {
                        string segmentId = segmentIndex.ToString();

                        HttpResponseMessage getSeatMapResponse = GraphQlEndpoint.GetSeatMap(env, bookingId, segmentId).EnsureIsSuccessStatusCode();
                        dynamic getSeatMapResult = JsonConvert.DeserializeObject<dynamic>(await getSeatMapResponse.Content.ReadAsStringAsync());

                        // Basic response validation
                        Assert.True(!DynamicHelper.HasProperty(getSeatMapResult, "errors"), $"Expected a valid response from Graphql for segment {segmentId} but found \n {getSeatMapResult}. {debugContext}");
                        Assert.True(DynamicHelper.HasProperty(getSeatMapResult, "data"), $"Response for segment {segmentId} does not contain data property. {debugContext}");

                        dynamic dataObj = getSeatMapResult.data;
                        Assert.True(dataObj != null, $"Data property should not be null for segment {segmentId}. {debugContext}");
                        Assert.True(DynamicHelper.HasProperty(dataObj, "seatMapQuery"), $"Response for segment {segmentId} does not contain seatMapQuery property. {debugContext}");

                        dynamic seatMapQuery = dataObj.seatMapQuery;
                        Assert.True(seatMapQuery != null, $"SeatMapQuery property should not be null for segment {segmentId}. {debugContext}");

                        // Validate seatMapQuery structure
                        ValidateSeatMapQueryStructure(seatMapQuery, segmentId);

                        // Validate seatMap array
                        Assert.True(DynamicHelper.HasProperty(seatMapQuery, "seatMap"), $"seatMapQuery should contain 'seatMap' property for segment {segmentId}. {debugContext}");
                        dynamic seatMapArray = seatMapQuery.seatMap;
                        Assert.True(seatMapArray != null && seatMapArray.HasValues, $"SeatMap array should not be null or empty for segment {segmentId}. {debugContext}");

                        // Validate each seat map in the array
                        foreach (object seatMap in seatMapArray)
                        {
                            ValidateSeatMapStructure(seatMap, segmentId);
                        }

                        // Validate flightPassengerSeatMap
                        Assert.True(DynamicHelper.HasProperty(seatMapQuery, "flightPassengerSeatMap"), $"seatMapQuery should contain 'flightPassengerSeatMap' property for segment {segmentId}. {debugContext}");
                        dynamic flightPassengerSeatMap = seatMapQuery.flightPassengerSeatMap;
                        Assert.True(flightPassengerSeatMap != null, $"FlightPassengerSeatMap should not be null for segment {segmentId}. {debugContext}");

                        if (flightPassengerSeatMap.HasValues)
                        {
                            foreach (object passengerSeat in flightPassengerSeatMap)
                            {
                                ValidateFlightPassengerSeatMapStructure(passengerSeat, segmentId);
                            }
                        }

                        segmentIndex++;
                    }
                }
            }

            #endregion
        }


        [SkippableTheory]
        [Trait("Category", Traits.SeatMapMobileRegression)]
        [InlineData(JourneyType.OneWay, FlightType.DL, CabinName.MAIN)]
        [InlineData(JourneyType.OneWay, FlightType.DL, CabinName.D1)]
        [InlineData(JourneyType.OneWay, FlightType.DL, CabinName.DPPS)]
        public async Task DeltaSeatMap_ReturnsSeatMap(JourneyType journeyType, FlightType flighttype, CabinName cabinName, string originFilter = "", string destinationFilter = "")
        {
            #region - pre-requisite for fetching the PNR from the storage table

            string originString = originFilter;
            string destinationString = destinationFilter;
            string journeyTypeString = journeyType.ToString();
            string flightTypeString = flighttype.ToString();

            // Map Delta cabin names for PNR search
            string mappedCabinName = cabinName switch
            {
                CabinName.MAIN => "DeltaMain",
                CabinName.D1 => "DeltaOne",
                CabinName.DPPS => "PremiumSelect",
                _ => cabinName.ToString()
            };

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForSpecificRoute(journeyTypeString, originString, destinationString, flightTypeString, mappedCabinName);

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
            Assert.True(totalSegmentCount > 0, $"Total segment count should be greater than 0. {debugContext}");

            #endregion

            #region Graphql call for GetSeatMap response for all segments

            // Loop through all segments and validate seat map for each
            int segmentIndex = 1;
            foreach (dynamic slice in slices)
            {
                dynamic segments = slice.segments;
                if (segments != null)
                {
                    foreach (dynamic segment in segments)
                    {
                        string segmentId = segmentIndex.ToString();

                        HttpResponseMessage getSeatMapResponse = GraphQlEndpoint.GetSeatMap(env, bookingId, segmentId).EnsureIsSuccessStatusCode();
                        dynamic getSeatMapResult = JsonConvert.DeserializeObject<dynamic>(await getSeatMapResponse.Content.ReadAsStringAsync());

                        // Basic response validation
                        Assert.True(!DynamicHelper.HasProperty(getSeatMapResult, "errors"), $"Expected a valid response from Graphql for segment {segmentId} but found \n {getSeatMapResult}. {debugContext}");
                        Assert.True(DynamicHelper.HasProperty(getSeatMapResult, "data"), $"Response for segment {segmentId} does not contain data property. {debugContext}");

                        dynamic dataObj = getSeatMapResult.data;
                        Assert.True(dataObj != null, $"Data property should not be null for segment {segmentId}. {debugContext}");
                        Assert.True(DynamicHelper.HasProperty(dataObj, "seatMapQuery"), $"Response for segment {segmentId} does not contain seatMapQuery property. {debugContext}");

                        dynamic seatMapQuery = dataObj.seatMapQuery;
                        Assert.True(seatMapQuery != null, $"SeatMapQuery property should not be null for segment {segmentId}. {debugContext}");

                        // Validate seatMapQuery structure
                        ValidateSeatMapQueryStructure(seatMapQuery, segmentId);

                        // Validate seatMap array
                        Assert.True(DynamicHelper.HasProperty(seatMapQuery, "seatMap"), $"seatMapQuery should contain 'seatMap' property for segment {segmentId}. {debugContext}");
                        dynamic seatMapArray = seatMapQuery.seatMap;
                        Assert.True(seatMapArray != null && seatMapArray.HasValues, $"SeatMap array should not be null or empty for segment {segmentId}. {debugContext}");

                        // Validate each seat map in the array
                        foreach (dynamic seatMap in seatMapArray)
                        {
                            ValidateSeatMapStructure(seatMap, segmentId);
                        }

                        // Validate flightPassengerSeatMap
                        Assert.True(DynamicHelper.HasProperty(seatMapQuery, "flightPassengerSeatMap"), $"seatMapQuery should contain 'flightPassengerSeatMap' property for segment {segmentId}. {debugContext}");
                        dynamic flightPassengerSeatMap = seatMapQuery.flightPassengerSeatMap;
                        Assert.True(flightPassengerSeatMap != null, $"FlightPassengerSeatMap should not be null for segment {segmentId}. {debugContext}");

                        if (flightPassengerSeatMap.HasValues)
                        {
                            foreach (dynamic passengerSeat in flightPassengerSeatMap)
                            {
                                ValidateFlightPassengerSeatMapStructure(passengerSeat, segmentId);
                            }
                        }

                        segmentIndex++;
                    }
                }
            }

            #endregion
        }

        private static void ValidatePnrDetails(Dictionary<string, string> pnrDetails)
            => TestDataPrerequisiteHelper.ValidatePnrDetailsOrSkip(pnrDetails);

        private void ValidateSeatMapQueryStructure(dynamic seatMapQuery, string segmentId)
        {
            SoftAssertHelper softAssert = new();

            // Validate basic properties
            softAssert.True(DynamicHelper.HasProperty(seatMapQuery, "tripReservationId"), $"seatMapQuery should contain 'tripReservationId' property for segment {segmentId}");
            softAssert.True(DynamicHelper.HasProperty(seatMapQuery, "pnrLocator"), $"seatMapQuery should contain 'pnrLocator' property for segment {segmentId}");
            softAssert.True(DynamicHelper.HasProperty(seatMapQuery, "flightSegment"), $"seatMapQuery should contain 'flightSegment' property for segment {segmentId}");

            // Validate flightSegment structure
            dynamic flightSegment = seatMapQuery.flightSegment;
            if (flightSegment != null)
            {
                softAssert.True(DynamicHelper.HasProperty(flightSegment, "id"), $"flightSegment should contain 'id' property for segment {segmentId}");
                softAssert.True(DynamicHelper.HasProperty(flightSegment, "operatingCarrier"), $"flightSegment should contain 'operatingCarrier' property for segment {segmentId}");
                softAssert.True(DynamicHelper.HasProperty(flightSegment, "marketingCarrier"), $"flightSegment should contain 'marketingCarrier' property for segment {segmentId}");
                softAssert.True(DynamicHelper.HasProperty(flightSegment, "origin"), $"flightSegment should contain 'origin' property for segment {segmentId}");
                softAssert.True(DynamicHelper.HasProperty(flightSegment, "destination"), $"flightSegment should contain 'destination' property for segment {segmentId}");

                // Validate carrier structures
                ValidateCarrierStructure(flightSegment.operatingCarrier, "operatingCarrier", segmentId, softAssert);
                ValidateCarrierStructure(flightSegment.marketingCarrier, "marketingCarrier", segmentId, softAssert);

                // Validate airport structures
                ValidateAirportStructure(flightSegment.origin, "origin", segmentId, softAssert);
                ValidateAirportStructure(flightSegment.destination, "destination", segmentId, softAssert);
            }

            softAssert.AssertAll();
        }

        private static void ValidateCarrierStructure(dynamic carrier, string carrierType, string segmentId, SoftAssertHelper softAssert)
        {
            if (carrier != null)
            {
                softAssert.True(DynamicHelper.HasProperty(carrier, "flightNumber"), $"{carrierType} should contain 'flightNumber' property for segment {segmentId}");
                softAssert.True(DynamicHelper.HasProperty(carrier, "carrierCode"), $"{carrierType} should contain 'carrierCode' property for segment {segmentId}");
            }
        }

        private static void ValidateAirportStructure(dynamic airport, string airportType, string segmentId, SoftAssertHelper softAssert)
        {
            if (airport != null)
            {
                softAssert.True(DynamicHelper.HasProperty(airport, "code"), $"{airportType} should contain 'code' property for segment {segmentId}");
            }
        }

        private void ValidateSeatMapStructure(dynamic seatMap, string segmentId)
        {
            SoftAssertHelper softAssert = new();

            // Validate required properties
            softAssert.True(DynamicHelper.HasProperty(seatMap, "seatOfferId"), $"seatMap should contain 'seatOfferId' property for segment {segmentId}");
            softAssert.True(DynamicHelper.HasProperty(seatMap, "classType"), $"seatMap should contain 'classType' property for segment {segmentId}");
            softAssert.True(DynamicHelper.HasProperty(seatMap, "equipment"), $"seatMap should contain 'equipment' property for segment {segmentId}");
            softAssert.True(DynamicHelper.HasProperty(seatMap, "details"), $"seatMap should contain 'details' property for segment {segmentId}");
            softAssert.True(DynamicHelper.HasProperty(seatMap, "configuration"), $"seatMap should contain 'configuration' property for segment {segmentId}");
            softAssert.True(DynamicHelper.HasProperty(seatMap, "rows"), $"seatMap should contain 'rows' property for segment {segmentId}");

            // Validate seatOfferId is not null or empty
            string seatOfferId = seatMap.seatOfferId?.ToString();
            softAssert.True(!string.IsNullOrEmpty(seatOfferId), $"seatOfferId should not be null or empty for segment {segmentId}");

            string classType = seatMap.classType?.ToString();
            softAssert.True(!string.IsNullOrEmpty(classType), $"classType should not be null or empty for VS or DL flights in segment {segmentId}");


            // Validate configuration array
            dynamic configuration = seatMap.configuration;
            softAssert.True(configuration != null && configuration.HasValues, $"configuration should not be null or empty for segment {segmentId}");

            // Validate rows array
            dynamic rows = seatMap.rows;
            softAssert.True(rows != null && rows.HasValues, $"rows should not be null or empty for segment {segmentId}");

            // Validate each row structure
            foreach (object row in rows)
            {
                ValidateRowStructure(row, segmentId, classType, softAssert);
            }

            softAssert.AssertAll();
        }

        private void ValidateRowStructure(dynamic row, string segmentId, string classType, SoftAssertHelper softAssert)
        {
            // Validate row properties
            softAssert.True(DynamicHelper.HasProperty(row, "columns"), $"row should contain 'columns' property for segment {segmentId} in {classType}");
            softAssert.True(DynamicHelper.HasProperty(row, "exitLeft"), $"row should contain 'exitLeft' property for segment {segmentId} in {classType}");
            softAssert.True(DynamicHelper.HasProperty(row, "exitRight"), $"row should contain 'exitRight' property for segment {segmentId} in {classType}");

            dynamic columns = row.columns;
            softAssert.True(columns != null, $"columns should not be null for segment {segmentId} in {classType}");

            // Validate each column structure
            if (columns.HasValues)
            {
                foreach (object column in columns)
                {
                    ValidateColumnStructure(column, segmentId, classType, softAssert);
                }
            }
        }

        private void ValidateColumnStructure(dynamic column, string segmentId, string classType, SoftAssertHelper softAssert)
        {
            // Validate column properties
            softAssert.True(DynamicHelper.HasProperty(column, "type"), $"column should contain 'type' property for segment {segmentId} in {classType}");
            softAssert.True(DynamicHelper.HasProperty(column, "availability"), $"column should contain 'availability' property for segment {segmentId} in {classType}");
            softAssert.True(DynamicHelper.HasProperty(column, "overWing"), $"column should contain 'overWing' property for segment {segmentId} in {classType}");
            softAssert.True(DynamicHelper.HasProperty(column, "missing"), $"column should contain 'missing' property for segment {segmentId} in {classType}");
            softAssert.True(DynamicHelper.HasProperty(column, "economyComfort"), $"column should contain 'economyComfort' property for segment {segmentId} in {classType}");
            softAssert.True(DynamicHelper.HasProperty(column, "exitOverride"), $"column should contain 'exitOverride' property for segment {segmentId} in {classType}");
            softAssert.True(DynamicHelper.HasProperty(column, "overrideSeatType"), $"column should contain 'overrideSeatType' property for segment {segmentId} in {classType}");

            // Validate seat-specific properties if type is SEAT
            string columnType = column.type?.ToString();
            if (columnType == "SEAT")
            {
                softAssert.True(DynamicHelper.HasProperty(column, "seatNumber"), $"SEAT column should contain 'seatNumber' property for segment {segmentId} in {classType}");
                softAssert.True(DynamicHelper.HasProperty(column, "pos"), $"SEAT column should contain 'pos' property for segment {segmentId} in {classType}");

                // Validate availability values for seats
                string availability = column.availability?.ToString();
                if (!string.IsNullOrEmpty(availability))
                {
                    string[] validAvailabilityValues = new[] { "FREE", "OCCUPIED", "BLOCKED" };
                    softAssert.True(validAvailabilityValues.Contains(availability),
                        $"Seat availability should be one of {string.Join(", ", validAvailabilityValues)} but found '{availability}' for segment {segmentId} in {classType}");
                }

                // Validate offers if present
                if (DynamicHelper.HasProperty(column, "offers") && column.offers != null && column.offers.HasValues)
                {
                    foreach (object offer in column.offers)
                    {
                        ValidateOfferStructure(offer, segmentId, classType, softAssert);
                    }
                }
            }
        }

        private static void ValidateOfferStructure(dynamic offer, string segmentId, string classType, SoftAssertHelper softAssert)
        {
            // Validate offer properties
            softAssert.True(DynamicHelper.HasProperty(offer, "offerId"), $"offer should contain 'offerId' property for segment {segmentId} in {classType}");
            softAssert.True(DynamicHelper.HasProperty(offer, "offerItemId"), $"offer should contain 'offerItemId' property for segment {segmentId} in {classType}");
            softAssert.True(DynamicHelper.HasProperty(offer, "customerId"), $"offer should contain 'customerId' property for segment {segmentId} in {classType}");
            softAssert.True(DynamicHelper.HasProperty(offer, "productId"), $"offer should contain 'productId' property for segment {segmentId} in {classType}");
            softAssert.True(DynamicHelper.HasProperty(offer, "productName"), $"offer should contain 'productName' property for segment {segmentId} in {classType}");
            softAssert.True(DynamicHelper.HasProperty(offer, "fareProduct"), $"offer should contain 'fareProduct' property for segment {segmentId} in {classType}");
            softAssert.True(DynamicHelper.HasProperty(offer, "flightIdRef"), $"offer should contain 'flightIdRef' property for segment {segmentId} in {classType}");
        }

        private static void ValidateFlightPassengerSeatMapStructure(dynamic passengerSeat, string segmentId)
        {
            SoftAssertHelper softAssert = new();

            // Validate passenger seat map properties
            softAssert.True(DynamicHelper.HasProperty(passengerSeat, "passengerId"), $"flightPassengerSeatMap should contain 'passengerId' property for segment {segmentId}");
            softAssert.True(DynamicHelper.HasProperty(passengerSeat, "segmentId"), $"flightPassengerSeatMap should contain 'segmentId' property for segment {segmentId}");
            softAssert.True(DynamicHelper.HasProperty(passengerSeat, "seatOfferId"), $"flightPassengerSeatMap should contain 'seatOfferId' property for segment {segmentId}");
            softAssert.True(DynamicHelper.HasProperty(passengerSeat, "seatNumber"), $"flightPassengerSeatMap should contain 'seatNumber' property for segment {segmentId}");
            softAssert.True(DynamicHelper.HasProperty(passengerSeat, "pos"), $"flightPassengerSeatMap should contain 'pos' property for segment {segmentId}");
            softAssert.True(DynamicHelper.HasProperty(passengerSeat, "price"), $"flightPassengerSeatMap should contain 'price' property for segment {segmentId}");

            // Validate price structure
            dynamic price = passengerSeat.price;
            if (price != null)
            {
                softAssert.True(DynamicHelper.HasProperty(price, "amount"), $"passenger seat price should contain 'amount' property for segment {segmentId}");
            }

            // Validate linkedInfant structure if present
            if (DynamicHelper.HasProperty(passengerSeat, "linkedInfant") && passengerSeat.linkedInfant != null)
            {
                dynamic linkedInfant = passengerSeat.linkedInfant;
                softAssert.True(DynamicHelper.HasProperty(linkedInfant, "passengerId"), $"linkedInfant should contain 'passengerId' property for segment {segmentId}");
                softAssert.True(DynamicHelper.HasProperty(linkedInfant, "firstName"), $"linkedInfant should contain 'firstName' property for segment {segmentId}");
                softAssert.True(DynamicHelper.HasProperty(linkedInfant, "lastName"), $"linkedInfant should contain 'lastName' property for segment {segmentId}");
            }
            softAssert.AssertAll();
        }
    }
}
