using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
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
    public class UpsellTests
    {
        public string env = Config.Environment;


        [SkippableTheory]
        [Trait("Category", Traits.UpsellMobileRegression)]
        [Trait("Category", GlobalTraits.MobileAppsSanityTests)]
        [InlineData(JourneyType.OneWay, LocationType.LHR, LocationType.DEL, FlightType.VS, CabinType.EconomyClassic)]
        [InlineData(JourneyType.OneWay, LocationType.LHR, LocationType.ATL, FlightType.VS, CabinType.EconomyClassic)]
        public async Task RetrieveUpsellOffersForVsPassenger_ReturnsOffers(JourneyType journeyType, LocationType originAirport, LocationType destinationAirport, FlightType flighttype, CabinType cabinType)
        {
            #region - pre-requisite for fetching the PNR from the storage table

            string journeyTypeString = journeyType.ToString();
            string originString = originAirport.ToString();
            string destinationString = destinationAirport.ToString();
            string flightTypeString = flighttype.ToString();
            string cabinTypeString = cabinType.ToString();

            List<Dictionary<string, string>> allPnrDetails = RetrieveFromStorageTable.RetrieveAllPNRDetailsFromStorageTableForSpecificRoute(journeyTypeString, originString, destinationString, flightTypeString, cabinTypeString);
            if (!allPnrDetails.Any())
            {
                TestDataPrerequisiteHelper.Skip($"Test skipped: No PNRs found in storage for {journeyTypeString}, {originString}-{destinationString}, {flightTypeString}, {cabinTypeString}");
                return;
            }

            #endregion

            #region - Iterate candidates until one is upsell eligible

            string usingString = "PNR";
            dynamic upsellQuery = null;
            string debugContext = string.Empty;

            foreach (Dictionary<string, string> pnrDetails in allPnrDetails)
            {
                string pnr = pnrDetails["pnr"];
                string firstName = pnrDetails["firstName"];
                string lastName = pnrDetails["lastName"];
                debugContext = RequestHelper.FormatTripsBookingContext(pnr, firstName, lastName);

                HttpResponseMessage bookingResponse = GraphQlEndpoint.Booking(env, usingString, pnr, firstName, lastName).EnsureIsSuccessStatusCode();
                dynamic bookingResult = JsonConvert.DeserializeObject<dynamic>(await bookingResponse.Content.ReadAsStringAsync());

                if (DynamicHelper.HasProperty(bookingResult, "errors") || !DynamicHelper.HasProperty(bookingResult, "data") || !DynamicHelper.HasProperty(bookingResult.data, "Booking"))
                    continue;

                string bookingId = bookingResult.data.Booking.id;
                if (string.IsNullOrEmpty(bookingId))
                    continue;

                HttpResponseMessage upsellResponse = GraphQlEndpoint.GetUpsellOffers(env, bookingId).EnsureIsSuccessStatusCode();
                dynamic upsellResult = JsonConvert.DeserializeObject<dynamic>(await upsellResponse.Content.ReadAsStringAsync());

                if (DynamicHelper.HasProperty(upsellResult, "errors") || !DynamicHelper.HasProperty(upsellResult, "data") || !DynamicHelper.HasProperty(upsellResult.data, "UpsellOffersQuery"))
                    continue;

                dynamic candidate = upsellResult.data.UpsellOffersQuery;
                if ((bool)candidate.upsellEligible)
                {
                    upsellQuery = candidate;
                    break;
                }
            }

            if (upsellQuery == null)
            {
                TestDataPrerequisiteHelper.Skip($"Test skipped: No upsell-eligible PNR found among {allPnrDetails.Count} candidate(s) for {journeyTypeString}, {originString}-{destinationString}. Last context: {debugContext}");
                return;
            }

            #endregion

            #region - Validations

            Assert.True(!string.IsNullOrEmpty((string)upsellQuery.bookingId), $"BookingId should not be null or empty. {debugContext}");
            Assert.True(upsellQuery.slices != null && upsellQuery.slices.HasValues, $"Slices should not be null or empty. {debugContext}");

            dynamic firstSlice = upsellQuery.slices.First;
            Assert.True(!string.IsNullOrEmpty((string)firstSlice.id), "Slice ID should not be null or empty");
            Assert.True(!string.IsNullOrEmpty((string)firstSlice.origin.code), "Origin code should not be null or empty");
            Assert.True(!string.IsNullOrEmpty((string)firstSlice.destination.code), "Destination code should not be null or empty");

            Assert.True(upsellQuery.segments != null && upsellQuery.segments.HasValues, "Segments should not be null or empty");

            dynamic firstSegment = upsellQuery.segments.First;
            Assert.True(!string.IsNullOrEmpty((string)firstSegment.id), "Segment ID should not be null or empty");
            Assert.True(!string.IsNullOrEmpty((string)firstSegment.operatingCarrier.carrierCode), "Operating carrier code should not be null or empty");
            Assert.True(firstSegment.upsellOffers != null && firstSegment.upsellOffers.HasValues, "Upsell offers should not be null or empty");

            foreach (dynamic offer in firstSegment.upsellOffers)
            {
                Assert.True(!string.IsNullOrEmpty((string)offer.offerItemNumber), "Offer item number should not be null or empty");
                Assert.True(!string.IsNullOrEmpty((string)offer.cabinClass.code), "Cabin class code should not be null or empty");
                Assert.True(!string.IsNullOrEmpty((string)offer.cabinClass.name), "Cabin class name should not be null or empty");
                Assert.True(offer.upsellOfferDetails != null && offer.upsellOfferDetails.HasValues, "Upsell offer details should not be null or empty");

                dynamic offerDetail = offer.upsellOfferDetails.First;
                Assert.True(!string.IsNullOrEmpty((string)offerDetail.offerType), "Offer type should not be null or empty");
                Assert.True(!string.IsNullOrEmpty((string)offerDetail.currencyCode), "Currency code should not be null or empty");
                Assert.True(decimal.Parse((string)offerDetail.baseAmt) >= 0, "Base amount should be non-negative");
                Assert.True(decimal.Parse((string)offerDetail.totalAmount) >= 0, "Total amount should be non-negative");
                Assert.True(offer.cabinClass.amenities != null && offer.cabinClass.amenities.HasValues, "Amenities should not be null or empty");
            }

            #endregion
        }

        [SkippableTheory]
        [Trait("Category", Traits.UpsellMobileRegression)]
        [InlineData(FlightType.VS, CabinType.UpperClass)]
        [InlineData(FlightType.DL, CabinType.DeltaOne)]
        public async Task RetrieveUpsellOffers_WhenUnavailable_ReturnsNoOffers(FlightType flighttype, CabinType cabinType)
        {
            #region - pre-requisite for fetching the PNR from the storage table
            
            string journeyTypeString = string.Empty;
            string originString = string.Empty;
            string destinationString = string.Empty;
            string flightTypeString = flighttype.ToString();
            string cabinTypeString = cabinType.ToString();

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForSpecificRoute(journeyTypeString, originString, destinationString, flightTypeString, cabinTypeString);
            
            ValidatePnrDetails(pnrDetails);
            
            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            int paxCount = int.Parse(pnrDetails["passengersCount"]);
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
            
            #endregion
            
            #region - GraphQL call for Upsell Offers
            
            HttpResponseMessage getUpsellOffersResponse = GraphQlEndpoint.GetUpsellOffers(env, bookingId).EnsureIsSuccessStatusCode();
            dynamic getUpsellOffersResult = JsonConvert.DeserializeObject<dynamic>(await getUpsellOffersResponse.Content.ReadAsStringAsync());
            
            Assert.True(!DynamicHelper.HasProperty(getUpsellOffersResult, "errors"), $"{pnr} - Expected a valid response from Graphql but found \n {getUpsellOffersResult}. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(getUpsellOffersResult, "data"), $"Response does not contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(getUpsellOffersResult.data, "UpsellOffersQuery"), $"{pnr} - Response should contain UpsellOffersQuery property. {debugContext}");

            JObject upsellQuery = (JObject)getUpsellOffersResult.data.UpsellOffersQuery;
            // Validate booking ID
            string bookingIdFromResponse = upsellQuery["bookingId"]?.Value<string>();
            
            Assert.True(!string.IsNullOrEmpty(bookingIdFromResponse), $"{pnr} -BookingId should not be null or empty. {debugContext}");
            // Validate upsell eligibility is set to false
            
            bool upsellEligible = upsellQuery["upsellEligible"]?.Value<bool>() ?? false;
            
            Assert.False(upsellEligible, $"{pnr} - Booking should not be upsell eligible. {debugContext}");
            // Validate slices is empty array
            Assert.True(upsellQuery["slices"] != null && !upsellQuery["slices"].HasValues, $" {pnr} - Slices should be empty array. {debugContext}");
            // Validate segments is null
            Assert.True(upsellQuery["segments"] == null || upsellQuery["segments"].Type == JTokenType.Null, $" {pnr} -Segments should be null. {debugContext}");

            #endregion
        }

        private void ValidatePnrDetails(Dictionary<string, string> pnrDetails)
            => TestDataPrerequisiteHelper.ValidatePnrDetailsOrSkip(pnrDetails);

    }
}
