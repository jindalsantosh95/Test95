using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Search.Helpers;
using VAA.EAPI.Automation.Search.Tests.NewBookFlow.GraphQL.OfferSearch;
using LocationType = VAA.EAPI.Automation.Core.Helpers.Enums.LocationType;

namespace VAA.EAPI.Automation.Book.Helpers
{
    public class GraphQLHelper
    {
        public static string env = Config.Environment;

        public static int SeatColumnsCount(HttpResponseMessage jsonResponse, string columnName)
        {
            JObject jsonObject = JObject.Parse(jsonResponse.ReadResponseAsString());

            string[] seatMap = jsonObject.SelectTokens("$.data.retrieveSeatMap.seatMap.rows[*].columns[*]").Select(x => x.ToString()).ToArray();

            int count = seatMap.Where(x => x.Has(columnName)).Count();

            Debug.WriteLine($"Column name -> {columnName} and count is -> {count}");

            return count;
        }

        public static void RetrieveBasket(string basketId, out HttpResponseMessage retrieveBasketGQLResponse)
        {
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();

            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(retrieveBasketGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(retrieveBasketResult.data.retrieveBasket.basketId.Value == basketId, "Basket ID dont match");
        }

        public static void UpdateBasketWithPassengerContactAndTnCs(string basketId, out string sectorID, out HttpResponseMessage retrieveBasketAfterGQLResponse, bool fcFlag = false, string tier = "", string b2cToken = "", string firstName = "", string lastName = "", string partnerFqtProgramme = "", string partnerFqtNumber = "")
        {
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(retrieveBasketGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(retrieveBasketResult.data.retrieveBasket.basketId.Value == basketId, "Basket ID dont match");

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(retrieveBasketResult, basketId, fcFlag, tier, b2cToken: b2cToken, passengerFirstName: firstName, passengerLastName: lastName, partnerFqtProgramme: partnerFqtProgramme, partnerFqtNumber: partnerFqtNumber);

            ContactBasketUpdateGraqphQL(out string emailId, out string phoneNumber, out string altPhoneNumber, out bool acceptPost, out bool acceptTelephone, out bool acceptEmail, out bool acceptTextMsg, out string countryCode);

            //Contact Basket Graph QL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(basketId, emailId, phoneNumber, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);

            HttpResponseMessage contactBasketGQLResponse = GraphQLEndpoint.Book(contactBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic contactBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(contactBasketGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(contactBasketGQLResult.data.updateContactDetails.basketId.Value == basketId, "Basket ID dont match");

            //Term and Condition Graph QL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(basketId);

            HttpResponseMessage tncBasketGQLResponse = GraphQLEndpoint.Book(tncBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic rtncBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(tncBasketGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(rtncBasketGQLResult.data.updateTnC.basketId.Value == basketId, "Basket ID dont match");

            //Retrieve Basket Graph ql request
            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            retrieveBasketAfterGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(retrieveBasketAfterGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(retrieveBasketResult.data.retrieveBasket.basketId.Value == basketId, "Basket ID dont match");

            sectorID = retrieveBasketResult.data.retrieveBasket.summary.slices[0].sectors[0].sectorId.Value;
        }

        public static string[] ReturnAssignedSeats(HttpResponseMessage retrieveBasketAfterGQLResponse)
        {
            JObject retrieveBasket = JObject.Parse(retrieveBasketAfterGQLResponse.ReadResponseAsString());

            return retrieveBasket.SelectTokens("$.data.retrieveBasket.flightPassengerSeatMap[*].seatNumber").Select(x => x.ToString()).ToArray();
        }

        [Obsolete("Marked as obsolete in favour of config driven approach. Please use 'OfferSearchGraphQL(TestConfig config, out string basketId, out dynamic offerPriceResult)'.")]
        public static void OfferSearchGraphQL(JourneyType journeyType, FareFamilyType fareFamily, FlightType flightType, LocationType origin, LocationType destination, int adults, int teens, int child, int infants, bool rewardFlag, out string basketId, out dynamic offerPriceResult, string departureDate = "", string returnDate = "", bool randomOffer = true, string b2cToken = "")
        {
            if (departureDate.IsNullOrWhiteSpace())
                departureDate = OfferSearchHelper.DepartureDate();
            if (returnDate.IsNullOrWhiteSpace())
                returnDate = OfferSearchHelper.ReturnDate(departureDate);

            if (journeyType == JourneyType.OneWay)
            {
                //generating offer search graph ql request.
                string offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(journeyType, departureDate, locationOne: origin, locationTwo: destination, numberOfAdults: adults, numberOfChildren: child, numberOfYouths: teens, numberOfInfants: infants, rewardSearch: rewardFlag);

                //post search GRAPHQL endpoint.
                HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

                dynamic offerSearchResults = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                Assert.True(GraphQlAssertions.ValidateSearchResult(offerSearchResults, offerSearchGQLRequest, out string result), result);

                //get basket id from search graphql response body
                basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

                string[] summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(offerSearchResponse.ReadResponseAsString(), fareFamily, flightType);

                Assert.True(summaryLinks.Any(), $"Summary link not returned for {fareFamily} and flight type {flightType} with request being {offerSearchGQLRequest}");

                //getting offer id from summary link
                string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(summaryLinks, randomOffer);

                //generating offer price graph ql request.
                string offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(journeyType, basketId, offerId);

                //post price GRAPHQL endpoint.
                HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

                offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                Assert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);
            }
            else
            {
                //generating offer search graph ql request.
                string offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(journeyType, departureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfAdults: adults, numberOfChildren: child, numberOfYouths: teens, numberOfInfants: infants, rewardSearch: rewardFlag);

                //post search GRAPHQL endpoint.
                HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, b2cToken: b2cToken).EnsureGQLResponseSuccess();

                dynamic offerSearchResults = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                Assert.True(GraphQlAssertions.ValidateSearchResult(offerSearchResults, offerSearchGQLRequest, out string result), result);

                string[] fareLinks = OfferSearchHelper.FindAllFareLinksForSpecificCabinAndAirline(offerSearchResponse.ReadResponseAsString(), fareFamily, flightType);

                Assert.True(fareLinks.Any(), "Fare link not returned for the cabin and flight type passed");

                //getting basket id from graphql response body
                basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

                //getting offer id from fare link
                string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinks, randomOffer);

                //generating offer search return graph ql request.
                string offerSearchReturnGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(journeyType, departureDate, returnDate, locationOne: origin, locationTwo: destination, locationThree: destination, locationFour: origin, numberOfAdults: adults, numberOfChildren: child, numberOfYouths: teens, numberOfInfants: infants, flexiDateSearch: false, calenderSearch: false, offerId, rewardSearch: rewardFlag, basketId: basketId);

                //post search GRAPHQL endpoint.
                HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, basketId: basketId).EnsureGQLResponseSuccess();

                string offerSearchReturnResult = offerReturnSearchResponse.ReadResponseAsString();

                string[] summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(offerSearchReturnResult, fareFamily, flightType);

                Assert.True(summaryLinks.Any(), $"Summary link not returned for {fareFamily} and flight type {flightType} with request being {offerSearchReturnGQLRequest}");

                //getting offer id from summary link
                string offerReturnId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(summaryLinks, randomOffer);

                //generating offer price graph ql request.
                string offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(journeyType, basketId, offerReturnId);

                //post price GRAPHQL endpoint.
                HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

                offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                Assert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);
            }
        }

        public static dynamic OfferSearchGraphQL(
            TestConfig config,
            bool bffTest = false,
            bool includeVoucherDetails = false,
            bool includeVoucherBasketDetails = false)
        {
            bool randomOffer = config.OfferFilterCriteria?.VSFlightRandomOffer ?? false;

            if (config.Search.JourneyType == JourneyType.OneWay)
            {
                //generating offer search graph ql request.
                string offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(config);

                //post search GRAPHQL endpoint.
                HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "", config.Internal.Bearer).EnsureGQLResponseSuccess();

                dynamic offerSearchResults = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                Assert.True(GraphQlAssertions.ValidateSearchResult(offerSearchResults, offerSearchGQLRequest, out string result), result);

                //get basket id from search graphql response body
                config.Internal.BasketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

                string responseContent = offerSearchResponse.ReadResponseAsString();

                string[] summaryLinks = GetSummaryLinks(config, responseContent);

                Assert.True(summaryLinks.Any(), $"Summary link not returned for {config.Search.FareFamilyType} and flight type {config.Search.FlightType} with request being {offerSearchGQLRequest}");

                string selectedSummaryLink = summaryLinks.FirstOrDefault();

                if (bffTest)
                    return selectedSummaryLink;

                //getting offer id from summary link
                string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(summaryLinks, randomOffer);

                //generating offer price graph ql request.
                string offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(
                    config,
                    offerId,
                    includeVoucherDetails: includeVoucherDetails,
                    includeVoucherBasketDetails: includeVoucherBasketDetails);

                //post price GRAPHQL endpoint.
                HttpResponseMessage offerPriceResponse = GraphQLEndpoint
                    .OfferPrice(offerPriceGQLRequest, config.Internal.Bearer)
                    .EnsureGQLResponseSuccess();

                dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                if (includeVoucherDetails)
                    Assert.NotNull(((JToken)offerPriceResult).SelectToken("$.data.searchSummary"));
                else
                    Assert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

                return offerPriceResult;
            }
            else
            {
                //generating offer search graph ql request.
                string offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(config);

                //post search GRAPHQL endpoint.
                HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "", config.Internal.Bearer).EnsureGQLResponseSuccess();

                dynamic offerSearchResults = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                Assert.True(GraphQlAssertions.ValidateSearchResult(offerSearchResults, offerSearchGQLRequest, out string result), result);

                string searchResponseContent = offerSearchResponse.ReadResponseAsString();
                string[] fareLinks = FindOutboundFareOrSummaryLinksForFareFamily(config, searchResponseContent, "fare");

                if (!fareLinks.Any())
                {
                    // Some responses omit outbound "fare" links for the requested cabin/carrier while still returning valid "summary" links.
                    // Fallback keeps the flow stable without changing the requested fare-family/airline constraints.
                    string[] fallbackSummaryLinks = FindOutboundFareOrSummaryLinksForFareFamily(config, searchResponseContent, "summary");
                    if (fallbackSummaryLinks.Any())
                    {
                        fareLinks = fallbackSummaryLinks;
                    }
                }

                Assert.True(
                    fareLinks.Any(),
                    $"Fare/summary link not returned for outbound cabin and flight type. FareFamily={config.Search.FareFamilyType}, FlightType={config.Search.FlightType}, Request={offerSearchGQLRequest}, SelectionDiagnostics={BuildFareSelectionDiagnostics(searchResponseContent)}");

                //getting basket id from graphql response body
                config.Internal.BasketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

                //getting offer id from fare link
                config.Internal.OfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinks, randomOffer);

                //generating offer search return graph ql request.
                string offerSearchReturnGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(config);

                //post search GRAPHQL endpoint.
                HttpResponseMessage offerReturnSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchReturnGQLRequest, config.Internal.BasketId, config.Internal.Bearer).EnsureGQLResponseSuccess();

                string offerSearchReturnResult = offerReturnSearchResponse.ReadResponseAsString();

                string[] summaryLinks = FindInboundFareOrSummaryLinksForFareFamily(config, offerSearchReturnResult, "summary");

                Assert.True(
                    summaryLinks.Any(),
                    $"Summary link not returned for inbound cabin and flight type. FareFamily={config.Search.InboundFareFamilyType}, FlightType={config.Search.InboundFlightType}, Request={offerSearchReturnGQLRequest}, SelectionDiagnostics={BuildFareSelectionDiagnostics(offerSearchReturnResult)}");

                string selectedSummaryLink = summaryLinks.FirstOrDefault();

                if (bffTest)
                    return selectedSummaryLink;

                //getting offer id from summary link
                string offerReturnId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(summaryLinks, randomOffer);

                //generating offer price graph ql request.
                string offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(
                    config,
                    offerReturnId,
                    includeVoucherDetails: includeVoucherDetails,
                    includeVoucherBasketDetails: includeVoucherBasketDetails);

                //post price GRAPHQL endpoint.
                HttpResponseMessage offerPriceResponse = GraphQLEndpoint
                    .OfferPrice(offerPriceGQLRequest, config.Internal.Bearer)
                    .EnsureGQLResponseSuccess();

                dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                if (includeVoucherDetails)
                    Assert.NotNull(((JToken)offerPriceResult).SelectToken("$.data.searchSummary"));
                else
                    Assert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

                return offerPriceResult;
            }
        }

        [Obsolete("Marked as obsolete in favour of config driven approach. Please use 'TestConfig.Book.Contact'.")]
        public static void ContactBasketUpdateGraqphQL(out string emailId, out string phoneNumber, out string altPhoneNumber, out bool acceptPost, out bool acceptTelephone, out bool acceptEmail, out bool acceptTextMsg, out string countryCode)
        {
            //update contact details GraphQL request
            if (Config.Environment.Is("prod"))
            {
                emailId = "HeimdallMonitoring@Automation.com";
            }
            emailId = $"{DataGenerator.RandomString(9)}@example.com";
            phoneNumber = $"{DataGenerator.RandomNumber(10)}";
            altPhoneNumber = $"{DataGenerator.RandomNumber(10)}";
            acceptPost = false;
            acceptTelephone = false;
            acceptEmail = true;
            acceptTextMsg = false;
            countryCode = "44";
        }

        public static bool RetrieveSeatMapGraphQL(string basketId, string sectorID, FareFamilyType fareFamily, out HttpResponseMessage retrieveSeatMapGQLResponse, out string message, String b2cToken = "")
        {
            const int maxAttempts = 3;
            string retrieveSeatMapGQLRequest = GraphQLRequestBuilder.RetrieveSeatMapGQLRequest(basketId, sectorID);
            string responseString = string.Empty;

            retrieveSeatMapGQLResponse = null;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                retrieveSeatMapGQLResponse = GraphQLEndpoint.Book(retrieveSeatMapGQLRequest, b2cToken);
                responseString = retrieveSeatMapGQLResponse?.ReadResponseAsString() ?? string.Empty;

                bool hasTransientSeatMapError = IsTransientSeatMapError(responseString);
                if (!hasTransientSeatMapError)
                {
                    break;
                }

                if (attempt < maxAttempts)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(attempt));
                }
            }

            if (IsTransientSeatMapError(responseString))
            {
                message = $"Seat-map retrieval failed after {maxAttempts} attempts. "
                    + $"Failure={GetTransientSeatMapFailure(responseString)}, "
                    + $"BasketId={basketId}, SectorId={sectorID}, FareFamily={fareFamily}, "
                    + $"HttpStatus={(int?)retrieveSeatMapGQLResponse?.StatusCode}, Response={GetDiagnosticResponse(responseString)}";
                return false;
            }

            retrieveSeatMapGQLResponse.EnsureGQLResponseSuccess();

            return GraphQLAssertion.SeatMapValidation(retrieveSeatMapGQLResponse, fareFamily, out message);
        }

        private static bool IsTransientSeatMapError(string responseString)
        {
            return responseString.Has("Value cannot be null")
                || responseString.Has("Sequence contains no elements")
                || responseString.Has("G009")
                || responseString.Has("Miscellaneous technical error");
        }

        private static string GetTransientSeatMapFailure(string responseString)
        {
            if (responseString.Has("G009") || responseString.Has("Miscellaneous technical error"))
            {
                return "Source-system G009 miscellaneous technical error";
            }

            if (responseString.Has("Sequence contains no elements"))
            {
                return "Sequence contains no elements";
            }

            return "Value cannot be null";
        }

        private static string GetDiagnosticResponse(string responseString)
        {
            const int maximumLength = 2_000;
            if (responseString.Length <= maximumLength)
            {
                return responseString;
            }

            return responseString[..maximumLength] + $"... [truncated; original length={responseString.Length}]";
        }

        public static void SelectSeatGraphQL(string basketId, string sectorID, FareFamilyType fareFamily, HttpResponseMessage retrieveSeatMapGQLResponse, HttpResponseMessage retrieveBasketGQLResponse, string productName, CabinName cabinName, out double totalSeatPrice, string b2cToken = "")
        {
            totalSeatPrice = 0;

            dynamic retrieveBasket = JsonConvert.DeserializeObject<dynamic>(retrieveBasketGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            dynamic retrieveSeatMapResult = JsonConvert.DeserializeObject<dynamic>(retrieveSeatMapGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string seatOfferId = GraphQLHelper.FindSeatOfferID(retrieveSeatMapResult);

            JArray people = (JArray)retrieveBasket.data.retrieveBasket.people;

            FindAvailableSeat(retrieveSeatMapGQLResponse, out string seatNumber, out double amount, out string currency, productName, cabinName);

            foreach (JToken item in people)
            {
                if (item["type"].ToString() == "INFANT")
                    break;

                int passengerId = Convert.ToInt32(item["id"]);

                string selectSeatREQ = GraphQLRequestBuilder.SeatSelectionGQLRequest(basketId, sectorID, seatOfferId, passengerId.ToString(), seatNumber, amount, currency);

                HttpResponseMessage seatSelectionGQLResponse = GraphQLEndpoint.Book(selectSeatREQ, b2cToken).EnsureGQLResponseSuccess();

                GraphQLAssertion.SeatSelectionValidation(seatSelectionGQLResponse, passengerId, seatNumber);

                Assert.True(GraphQLAssertion.ValidateColumnSeatSelected(seatSelectionGQLResponse, seatNumber, out string message), message);

                Assert.True(GraphQLAssertion.ValidateColumnSeatAmountCurrencySelected(seatSelectionGQLResponse, seatNumber, amount, currency, out message), message);

                totalSeatPrice = totalSeatPrice + amount;

                FindAvailableSeat(seatSelectionGQLResponse, out seatNumber, out amount, out currency, productName, cabinName);
            }
        }

        public static void SelectSeatFirstPassenger(string basketId, string sectorID, FareFamilyType fareFamily, HttpResponseMessage retrieveSeatMapGQLResponse, HttpResponseMessage retrieveBasketGQLResponse, string productName, CabinName cabinName, out double totalSeatPrice)
        {
            totalSeatPrice = 0;

            dynamic retrieveBasket = JsonConvert.DeserializeObject<dynamic>(retrieveBasketGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            dynamic retrieveSeatMapResult = JsonConvert.DeserializeObject<dynamic>(retrieveSeatMapGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string seatOfferId = GraphQLHelper.FindSeatOfferID(retrieveSeatMapResult);

            dynamic people = retrieveBasket.data.retrieveBasket.people[0];

            FindAvailableSeat(retrieveSeatMapGQLResponse, out string seatNumber, out double amount, out string currency, productName, cabinName);
            Assert.NotNull(seatNumber);

            int passengerId = Convert.ToInt32(people["id"]);

            string selectSeatREQ = GraphQLRequestBuilder.SeatSelectionGQLRequest(basketId, sectorID, seatOfferId, passengerId.ToString(), seatNumber, amount, currency);

            HttpResponseMessage seatSelectionGQLResponse = GraphQLEndpoint.Book(selectSeatREQ).EnsureGQLResponseSuccess();

            GraphQLAssertion.SeatSelectionValidation(seatSelectionGQLResponse, passengerId, seatNumber);

            Assert.True(GraphQLAssertion.ValidateColumnSeatSelected(seatSelectionGQLResponse, seatNumber, out string message), message);

            Assert.True(GraphQLAssertion.ValidateColumnSeatAmountCurrencySelected(seatSelectionGQLResponse, seatNumber, amount, currency, out message), message);

            totalSeatPrice = totalSeatPrice + amount;

            FindAndSelectAvailableSeat(seatSelectionGQLResponse, out seatNumber, out amount, out currency, productName, cabinName);
        }

        public static string FindSeatOfferID(dynamic seatMap)
        {
            string seatOfferId = seatMap.data.retrieveSeatMap.seatMap.seatOfferId.Value;

            return seatOfferId;
        }

        [Obsolete("Marked as obsolete in favour of config driven approach. Please use 'FindAvailableSeat(HttpResponseMessage seatMap, TestConfig _testConfig)'.")]
        public static void FindAvailableSeat(HttpResponseMessage seatMap, out string seatNumber, out double amount, out string currency, string productName = "", CabinName cabinName = 0)
        {
            JObject retrieveSeatMapResult = JObject.Parse(seatMap.ReadResponseAsString());

            IEnumerable<JToken> allFreeSeats = retrieveSeatMapResult.SelectTokens("$..seatMap.rows[*].columns[?(@.type == 'SEAT' && @.availability == 'FREE')]");

            IEnumerable<JToken> freeSeat;
            if (productName.IsNullOrEmpty())
            {
                freeSeat = allFreeSeats;
            }
            else
            {
                string cabinFilter = cabinName.ToString();
                freeSeat = allFreeSeats.Where(seat =>
                    seat.SelectTokens("offers[*]").Any(offer =>
                        (offer["productName"]?.ToString() ?? "").Contains(productName, StringComparison.OrdinalIgnoreCase)
                        && (offer["fareProduct"]?.ToString() ?? "") == cabinFilter));
            }

            string seatMapContent = seatMap.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Assert.True(
                freeSeat.Any(),
                BuildSeatSelectionDiagnostic(
                    retrieveSeatMapResult,
                    seatMapContent,
                    productName,
                    cabinName.ToString(),
                    allFreeSeats.Count(),
                    freeSeat.Count()));

            JToken matchedSeat = freeSeat.First();
            seatNumber = matchedSeat.SelectToken("seatNumber").ToString();

            JToken matchedOffer = productName.IsNullOrEmpty()
                ? matchedSeat.SelectToken("offers[0]")
                : matchedSeat.SelectTokens("offers[*]").First(offer =>
                    (offer["productName"]?.ToString() ?? "").Contains(productName, StringComparison.OrdinalIgnoreCase)
                    && (offer["fareProduct"]?.ToString() ?? "") == cabinName.ToString());

            amount = matchedOffer.SelectToken("price.amount").Value<double>();
            currency = matchedOffer.SelectToken("price.currency").ToString();
        }

        public static bool FindAvailableSeat(HttpResponseMessage seatMap, TestConfig _testConfig)
        {
            JObject retrieveSeatMapResult = JObject.Parse(seatMap.ReadResponseAsString());

            IEnumerable<JToken> allFreeSeats = retrieveSeatMapResult.SelectTokens("$..seatMap.rows[*].columns[?(@.type == 'SEAT' && @.availability == 'FREE')]");
            string productFilter = _testConfig.Book.Seat.SeatProductName;
            string cabinFilter = _testConfig.Book.Seat.CabinName.ToString();
            IEnumerable<JToken> cabinSeats = allFreeSeats.Where(seat =>
                seat.SelectTokens("offers[*]").Any(offer =>
                    (offer["fareProduct"]?.ToString() ?? "") == cabinFilter));
            IEnumerable<JToken> requestedProductSeats = string.IsNullOrEmpty(productFilter)
                ? cabinSeats
                : cabinSeats.Where(seat =>
                    seat.SelectTokens("offers[*]").Any(offer =>
                        (offer["productName"]?.ToString() ?? "").Contains(productFilter, StringComparison.OrdinalIgnoreCase)));

            bool requestedProductAvailable = requestedProductSeats.Any();
            IEnumerable<JToken> availableSeats = requestedProductAvailable ? requestedProductSeats : cabinSeats;
            if (!availableSeats.Any())
            {
                _testConfig.Book.Seat.SeatNumber = null;
                _testConfig.Book.Seat.Amount = 0;
                _testConfig.Book.Seat.Currency = null;
                return false;
            }

            JToken matchedSeat = availableSeats.First();
            _testConfig.Book.Seat.SeatNumber = matchedSeat.SelectToken("seatNumber").ToString();

            JToken matchedOffer = matchedSeat.SelectTokens("offers[*]").First(offer =>
                (offer["fareProduct"]?.ToString() ?? "") == cabinFilter
                && (!requestedProductAvailable
                    || (offer["productName"]?.ToString() ?? "").Contains(productFilter, StringComparison.OrdinalIgnoreCase)));

            _testConfig.Book.Seat.Amount = matchedOffer.SelectToken("price.amount").Value<double>();
            _testConfig.Book.Seat.Currency = matchedOffer.SelectToken("price.currency").ToString();
            return true;
        }

        private static string BuildSeatSelectionDiagnostic(
            JObject retrieveSeatMapResult,
            string seatMapContent,
            string requestedProductName,
            string requestedCabinName,
            int freeSeatCount,
            int matchedSeatCount)
        {
            string seatMapCabinClass = retrieveSeatMapResult.SelectToken("$.data.retrieveSeatMap.cabinClass")?.ToString() ?? "<unknown>";

            string availableFareProducts = string.Join(
                ", ",
                retrieveSeatMapResult
                    .SelectTokens("$..offers[*].fareProduct")
                    .Select(token => token.ToString())
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value));

            string availableProductNames = string.Join(
                ", ",
                retrieveSeatMapResult
                    .SelectTokens("$..offers[*].productName")
                    .Select(token => token.ToString())
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value));

            if (string.IsNullOrWhiteSpace(availableFareProducts))
            {
                availableFareProducts = "<none>";
            }

            if (string.IsNullOrWhiteSpace(availableProductNames))
            {
                availableProductNames = "<none>";
            }

            return
                $"No matching free seat found for requested seat selection. " +
                $"RequestedProduct='{requestedProductName}', RequestedCabin='{requestedCabinName}', " +
                $"SeatMapCabinClass='{seatMapCabinClass}', FreeSeatCount={freeSeatCount}, MatchedSeatCount={matchedSeatCount}. " +
                $"AvailableFareProducts=[{availableFareProducts}], AvailableProductNames=[{availableProductNames}] " +
                $"\n\nActualResponse:\n{seatMapContent}";
        }

        public static int ReturnSeatCount(HttpResponseMessage seatMap, string productName, CabinName cabinName)
        {
            JObject retrieveSeatMapResult = JObject.Parse(seatMap.ReadResponseAsString());

            IEnumerable<JToken> filteredSeats = retrieveSeatMapResult.SelectTokens($"$.data.selectSeat.seatMap.rows[*].columns[?(@.type == 'SEAT' && @.availability == 'FREE' && @.offers[?(@.productName == '{productName}' && @.fareProduct == '{cabinName}')])]");

            return filteredSeats.Count();
        }

        public static void FindAndSelectAvailableSeat(HttpResponseMessage selectSeat, out string seatNumber, out double amount, out string currency, string productName, CabinName cabinName)
        {
            JObject retrieveSeatMapResult = JObject.Parse(selectSeat.ReadResponseAsString());

            IEnumerable<JToken> freeSeat = retrieveSeatMapResult.SelectTokens($"$...seatMap.rows[*].columns[?(@.type == 'SEAT' && @.availability == 'FREE' && @.offers[?(@.productName == '{productName}' && @.fareProduct == '{cabinName}')])]");

            Assert.True(freeSeat.Any(), $"No Free seat available \n\t {freeSeat} ----{freeSeat} for  ---- Product Name: {productName} and Cabin : {cabinName}  \n\n Actual Response-----------------\n\t{selectSeat.Content.ReadAsStringAsync().Result}");

            seatNumber = freeSeat.FirstOrDefault().SelectToken("seatNumber").ToString();
            JToken amt = freeSeat.FirstOrDefault().SelectToken("offers[0].price.amount");
            amount = amt.Value<double>();
            currency = freeSeat.FirstOrDefault().SelectToken("offers[0].price.currency").ToString();
        }

        public static void SelectSeatsForAllSectors(string basketId, FareFamilyType fareFamily, HttpResponseMessage retrieveBasketGQLResponse, CabinName cabinName, out double totalSeatprice, string productName = "")
        {
            totalSeatprice = 0;
            JObject retrieveBasketGQLResult = JObject.Parse(retrieveBasketGQLResponse.ReadResponseAsString());

            string[] allSectors = retrieveBasketGQLResult.SelectTokens("$.data.retrieveBasket.summary.slices[*].sectors[*].sectorId").Select(x => x.ToString()).ToArray();

            foreach (string sectorId in allSectors)
            {
                //Retrieve Seat Map GraphQL Request
                bool hasSeatMap = RetrieveSeatMapGraphQL(basketId, sectorId, fareFamily, out HttpResponseMessage retrieveSeatMapGQLResponse, out string message);

                if (!hasSeatMap)
                    continue;

                //SelectSeat
                SelectSeatGraphQL(basketId, sectorId, fareFamily, retrieveSeatMapGQLResponse, retrieveBasketGQLResponse, productName, cabinName, out double totalSectorSeatprice);

                totalSeatprice += totalSectorSeatprice;
            }
        }

        public static bool SelectSeatsForSlice(string basketId, int sliceNum, FareFamilyType fareFamily, HttpResponseMessage retrieveBasketGQLResponse, CabinName cabinName, string productName = "")
        {
            JObject retrieveBasketGQLResult = JObject.Parse(retrieveBasketGQLResponse.ReadResponseAsString());

            string[] sectorsInSlice = retrieveBasketGQLResult.SelectTokens($"$.data.retrieveBasket.summary.slices[{sliceNum}].sectors[*].sectorId").Select(x => x.ToString()).ToArray();

            foreach (string sectorId in sectorsInSlice)
            {
                //Retrieve Seat Map GraphQL Request
                bool responseBool = RetrieveSeatMapGraphQL(basketId, sectorId, fareFamily, out HttpResponseMessage retrieveSeatMapGQLResponse, out string message);

                if (!responseBool)
                {
                    return false;
                }

                //SelectSeat
                SelectSeatGraphQL(basketId, sectorId, fareFamily, retrieveSeatMapGQLResponse, retrieveBasketGQLResponse, productName, cabinName, out double totalSliceSeatprice);
            }
            return true;
        }

        public static void SelectSeatsForSliceFirstPassenger(string basketId, int sliceId, FareFamilyType fareFamily, HttpResponseMessage retrieveBasketGQLResponse, string productName, CabinName cabinName)
        {
            JObject retrieveBasketGQLResult = JObject.Parse(retrieveBasketGQLResponse.ReadResponseAsString());

            string[] sectorsInSlice = retrieveBasketGQLResult.SelectTokens($"$.data.retrieveBasket.summary.slices[{sliceId}].sectors[*].sectorId").Select(x => x.ToString()).ToArray();

            foreach (string sectorId in sectorsInSlice)
            {
                //Retrieve Seat Map GraphQL Request
                Assert.True(RetrieveSeatMapGraphQL(basketId, sectorId, fareFamily, out HttpResponseMessage retrieveSeatMapGQLResponse, out string message), message);

                //SelectSeat
                SelectSeatFirstPassenger(basketId, sectorId, fareFamily, retrieveSeatMapGQLResponse, retrieveBasketGQLResponse, productName, cabinName, out double totalOutboundSeatprice);
            }
        }

        public static void RetrieveSeatMapForAllSectors(string basketId, FareFamilyType fareFamily, HttpResponseMessage retrieveBasketGQLResponse)
        {
            JObject retrieveBasketGQLResult = JObject.Parse(retrieveBasketGQLResponse.ReadResponseAsString());

            string[] sectors = retrieveBasketGQLResult.SelectTokens("$.data.retrieveBasket.summary.slices[*].sectors[*].sectorId").Select(x => x.ToString()).ToArray();

            foreach (string sectorId in sectors)
            {
                //Retrieve Seat Map GraphQL Request
                Assert.True(RetrieveSeatMapGraphQL(basketId, sectorId, fareFamily, out HttpResponseMessage retrieveSeatMapGQLResponse, out string message), message);
            }
        }

        public static void RetrieveSeatMapForSlice(string basketId, int sliceId, FareFamilyType fareFamily, HttpResponseMessage retrieveBasketGQLResponse, out HttpResponseMessage retrieveSeatMapGQLResponse, string b2cToken = "")
        {
            retrieveSeatMapGQLResponse = null;

            JObject retrieveBasketGQLResult = JObject.Parse(retrieveBasketGQLResponse.ReadResponseAsString());

            string[] sectorsInSlice = retrieveBasketGQLResult.SelectTokens($"$.data.retrieveBasket.summary.slices[{sliceId}].sectors[*].sectorId").Select(x => x.ToString()).ToArray();

            foreach (string sectorId in sectorsInSlice)
            {
                //Retrieve Seat Map GraphQL Request
                Assert.True(RetrieveSeatMapGraphQL(basketId, sectorId, fareFamily, out retrieveSeatMapGQLResponse, out string message, b2cToken), message);
            }
        }

        [Obsolete("Marked as obsolete in favour of config driven approach. Please use 'ConfirmationInfo(HttpResponseMessage orderCreateGQLResponse, TestConfig config)'.")]
        public static void ConfirmationInfo(HttpResponseMessage orderCreateGQLResponse, out string leadPassengerFirstName, out string leadPassengerSurname, out string email, out string currency, out double totalPrice, out string outBoundcabinClass, out string inBoundcabinClass, out string pnr)
        {
            dynamic confirmationResponse = JsonConvert.DeserializeObject<dynamic>(orderCreateGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            JObject confirmationRes = JObject.Parse(orderCreateGQLResponse.ReadResponseAsString());

            outBoundcabinClass = string.Empty;
            inBoundcabinClass = string.Empty;

            string[] outBoundcabinClassArray = confirmationRes.SelectTokens("$.data.bookCreateOrder.summary.outboundFlights[*].cabinClass").Select(x => x.ToString()).ToArray();
            string[] inBoundcabinClassArray = confirmationRes.SelectTokens("$.data.bookCreateOrder.summary.inboundFlights[*].cabinClass").Select(x => x.ToString()).ToArray();

            foreach (string item in outBoundcabinClassArray)
                outBoundcabinClass = item + ",";
            foreach (string inbound in inBoundcabinClassArray)
                inBoundcabinClass = inbound + ",";

            leadPassengerFirstName = confirmationResponse.data.bookCreateOrder.leadPassenger.firstName.Value;
            leadPassengerSurname = confirmationResponse.data.bookCreateOrder.leadPassenger.lastName.Value;
            totalPrice = confirmationResponse.data.bookCreateOrder.summary.totalPrice.Value;
            email = confirmationResponse.data.bookCreateOrder.leadPassenger.contact.emailAddress.Value;
            currency = confirmationResponse.data.bookCreateOrder.summary.priceCurrency.Value;
            pnr = confirmationResponse.data.bookCreateOrder.summary.outboundFlights[0].sectors[0].pnr.Value;
        }

        public static void ConfirmationInfo(HttpResponseMessage orderCreateGQLResponse, TestConfig config)
        {
            dynamic confirmationResponse = JsonConvert.DeserializeObject<dynamic>(orderCreateGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            JObject confirmationRes = JObject.Parse(orderCreateGQLResponse.ReadResponseAsString());

            string outBoundcabinClass = string.Empty;
            string inBoundcabinClass = string.Empty;

            int outboundSectors = confirmationRes.SelectTokens("$.data.bookCreateOrder.summary.outboundFlights[0].sectors[*]").Count();
            int inboundSectors = 0;

            if (config.Search.JourneyType == JourneyType.Return)
                inboundSectors = confirmationRes.SelectTokens("$.data.bookCreateOrder.summary.inboundFlights[0].sectors[*]").Count();

            bool direct = true;

            if (outboundSectors > 1 || inboundSectors > 1)
                direct = false;

            config.Book.Order.IsDirectFlight = direct;
            config.Book.Order.IsCodeShare = false;

            IEnumerable<JToken> outBoundSectorsData = confirmationRes.SelectTokens("$.data.bookCreateOrder.summary.outboundFlights[0].sectors[*]");

            List<string> sectorsInfoOutbound = new List<string>();

            foreach (JToken flight in outBoundSectorsData)
            {
                string airline = flight["operatingAirline"]?.ToString();
                string departing = flight["departing"]?["iata"]?.ToString();
                string arriving = flight["arriving"]?["iata"]?.ToString();
                string cabinClass = flight["cabinClass"]?.ToString();

                if (!airline.Has("Virgin Atlantic"))
                    config.Book.Order.IsCodeShare = true;

                sectorsInfoOutbound.Add($"Outbound: {airline}, Dep - {departing}, Arr - {arriving}, Cabin - {cabinClass}");
            }
            config.Book.Order.OutboundSectorInformation = string.Join("; ", sectorsInfoOutbound);

            if (config.Search.JourneyType == JourneyType.Return)
            {
                IEnumerable<JToken> inBoundSectorsData = confirmationRes.SelectTokens("$.data.bookCreateOrder.summary.inboundFlights[0].sectors[*]");

                List<string> sectorsInfoInbound = new List<string>();

                foreach (JToken flight in inBoundSectorsData)
                {
                    string airline = flight["operatingAirline"]?.ToString();
                    string departing = flight["departing"]?["iata"]?.ToString();
                    string arriving = flight["arriving"]?["iata"]?.ToString();
                    string cabinClass = flight["cabinClass"]?.ToString();

                    if (!airline.Has("Virgin Atlantic"))
                        config.Book.Order.IsCodeShare = true;

                    sectorsInfoInbound.Add($"Inbound: {airline}, Dep - {departing}, Arr - {arriving}, Cabin - {cabinClass}");
                }
                config.Book.Order.InboundSectorInformation = string.Join("; ", sectorsInfoInbound);
            }

            string[] outBoundcabinClassArray = confirmationRes.SelectTokens("$.data.bookCreateOrder.summary.slices[0].cabinClass").Select(x => x.ToString()).ToArray();
            string[] inBoundcabinClassArray = confirmationRes.SelectTokens("$.data.bookCreateOrder.summary.slices[1].cabinClass").Select(x => x.ToString()).ToArray();

            foreach (string item in outBoundcabinClassArray)
                outBoundcabinClass = item + ",";
            foreach (string inbound in inBoundcabinClassArray)
                inBoundcabinClass = inbound + ",";

            config.Book.Order.LeadPassengerFirstName = confirmationResponse.data.bookCreateOrder.leadPassenger.firstName.Value;
            config.Book.Order.LeadPassengerLastName = confirmationResponse.data.bookCreateOrder.leadPassenger.lastName.Value;
            config.Book.Cost.TotalPrice = confirmationResponse.data.bookCreateOrder.summary.totalPrice.Value;
            config.Book.Order.EmailId = confirmationResponse.data.bookCreateOrder.leadPassenger.contact.emailAddress.Value;
            config.Book.Cost.Currency = confirmationResponse.data.bookCreateOrder.summary.priceCurrency.Value;
            config.Book.Order.PNR = confirmationResponse.data.bookCreateOrder.summary.outboundFlights[0].sectors[0].pnr.Value;
            config.Book.Order.Airline = confirmationResponse.data.bookCreateOrder.summary.outboundFlights[0].sectors[0].operatingAirline.Value;
            config.Book.Order.FlightNumber = confirmationResponse.data.bookCreateOrder.summary.outboundFlights[0].sectors[0].flightNumber.Value;
            config.Book.Order.OutBoundCabinClass = outBoundcabinClass;
            config.Book.Order.InBoundCabinClass = inBoundcabinClass;
            config.Book.Order.PassengerParty = confirmationRes.SelectToken("$.data.bookCreateOrder.summary.partyComposition").ToString();
            config.Book.Order.PassengersCount = confirmationRes.SelectToken("$.data.bookCreateOrder.summary.partyComposition.total").ToString();
            config.Book.Order.FlightPassengerSeatMap = confirmationRes.SelectToken("$.data.bookCreateOrder.summary.flightPassengerSeatMap").ToString();
            JToken departureDateAndTimeToken = confirmationRes.SelectToken("$..outboundFlights[*].sectors[?(@.sectorId=='1-1-1')].departingAt");
            DateTime departureDateAndTime = DateTime.Parse(departureDateAndTimeToken.ToString());
            config.Book.Order.DepartureDate = departureDateAndTime.ToString("yyyy-MM-ddTHH:mm", System.Globalization.CultureInfo.InvariantCulture);

            //Flying Club number details
            config.Book.Order.FcPointsPostBooking = confirmationRes.SelectToken("$.data.bookCreateOrder.customerProfile.membershipDetails.fqtPoints").ToString();
            config.Book.Order.FqtTierCode = confirmationRes.SelectToken("$.data.bookCreateOrder.customerProfile.membershipDetails.fqtTierCode").ToString();
            config.Book.Order.FqtProgramme = confirmationRes.SelectToken("$.data.bookCreateOrder.customerProfile.membershipDetails.fqtProgramme").ToString();
            config.Book.Order.FlyingClubNumber = confirmationRes.SelectToken("$.data.bookCreateOrder.customerProfile.membershipDetails.fqtNumber").ToString();
            config.Book.Order.FlightPoints = confirmationRes.SelectToken("$.data.bookCreateOrder.summary.membershipPoints").ToString();
        }

        /// <summary>
        /// Executes the full order create flow: builds the request, calls the endpoint, checks for
        /// accepted errors (e.g. PNR sell error / space sold out), validates the response, and
        /// extracts confirmation info into the TestConfig.
        /// Returns false if the order failed with an accepted error (space sold out) — the caller
        /// should treat this as a pass and return early.
        /// </summary>
        public static async Task<bool> ExecuteOrderCreate(TestConfig config, string b2cToken = null)
        {
            string orderCreateGQLRequest = GraphQLRequestBuilder.OrderCreateGQLRequestBuild(config.Internal.BasketId);

            HttpResponseMessage orderCreateGQLResponse = b2cToken != null
                ? GraphQLEndpoint.Book(orderCreateGQLRequest, b2cToken)
                : GraphQLEndpoint.Book(orderCreateGQLRequest);

            if (orderCreateGQLResponse.ReadResponseAsString().HasAny(AcceptedErrorsArray.Get("PnrSellError")))
            {
                Console.Error.WriteLine("Order Create: Space sold out (accepted error) — skipping remaining assertions.");
                return false;
            }

            orderCreateGQLResponse.EnsureGQLResponseSuccess();

            dynamic orderCreateGQLResult = JsonConvert.DeserializeObject<dynamic>(await orderCreateGQLResponse.Content.ReadAsStringAsync());

            Assert.True(GraphQLAssertion.ValidateConfirmationGraphqlResponse(orderCreateGQLRequest, orderCreateGQLResult, out string response), response);
            Assert.True(GraphQLAssertion.CheckPnrCreated(orderCreateGQLResponse, out string errorMessage),
                $"{errorMessage} \n\tRequest : \n\t{orderCreateGQLRequest} \n\tResponse : \n\t{await orderCreateGQLResponse.Content.ReadAsStringAsync()} ");

            ConfirmationInfo(orderCreateGQLResponse, config);
            return true;
        }

        public static double GetBasketSummaryTotalPrice(dynamic basketResponse)
        {
            //Total Price Basket
            double basketSummaryTotalPrice = basketResponse.data.retrieveBasket.summary.totalPrice;

            return basketSummaryTotalPrice;
        }

        public static void GetBasketOutBoundSectorId(HttpResponseMessage basketResponse, TestConfig _testConfig)
        {
            JObject retrieveBasketGQLResult = JObject.Parse(basketResponse.ReadResponseAsString());
            _testConfig.Internal.OutboundSectorIds = retrieveBasketGQLResult.SelectTokens("$.data.retrieveBasket.summary.slices[0].sectors[*].sectorId").Select(x => x.ToString()).ToArray();
        }

        public static void GetBasketInBoundSectorId(HttpResponseMessage basketResponse, TestConfig _testConfig)
        {
            JObject retrieveBasketGQLResult = JObject.Parse(basketResponse.ReadResponseAsString());
            _testConfig.Internal.InboundSectorIds = retrieveBasketGQLResult.SelectTokens("$.data.retrieveBasket.summary.slices[1].sectors[*].sectorId").Select(x => x.ToString()).ToArray();
        }

        //points selection for pay with points
        public static string PointsSelectionInCurrencyConverter(HttpResponseMessage pointToCurrencyConverterResponse)
        {
            Random random = new Random();

            string selectedPoints = string.Empty;

            JObject PointToCurrencyConverterResult = JObject.Parse(pointToCurrencyConverterResponse.ReadResponseAsString());

            string[] pointsArray = PointToCurrencyConverterResult.SelectTokens("$.data.PointsToCurrencyConverterQuery[*]").Select(x => x.ToString()).ToArray();

            int count = pointsArray.Length;
            int randomSelection = random.Next(1, count);

            selectedPoints = pointsArray
            .Skip(randomSelection - 1)
            .Select(pointItem =>
            {
                dynamic pointData = JsonConvert.DeserializeObject<dynamic>(pointItem);
                return pointData.loyaltyPoints;
            })
            .FirstOrDefault();

            return selectedPoints;
        }

        //points selection for pay with points
        public static string FullPointsSelectionInCurrencyConverter(HttpResponseMessage pointToCurrencyConverterResponse)
        {
            Random random = new Random();

            string selectedPoints = string.Empty;

            JObject PointToCurrencyConverterResult = JObject.Parse(pointToCurrencyConverterResponse.ReadResponseAsString());

            string[] pointsArray = PointToCurrencyConverterResult.SelectTokens("$.data.PointsToCurrencyConverterQuery[*]").Select(x => x.ToString()).ToArray();

            int count = pointsArray.Length;
            if (count > 0 && pointsArray?.Any() == true)
            {
                selectedPoints = JsonConvert.DeserializeObject<dynamic>(pointsArray.First()).loyaltyPoints?.ToString();
                return selectedPoints;
            }
            else
                return string.Empty;
        }

        // Helper method to generate DOB based on passenger type
        public static string GenerateDOBByPassengerType(string type) => GenerateDOBByPassengerType(type, DateTime.Today);

        public static string GenerateDOBByPassengerType(string type, DateTime latestTravelDate)
        {
            Random rnd = Random.Shared;
            DateTime referenceDate = latestTravelDate.Date > DateTime.Today ? latestTravelDate.Date : DateTime.Today;

            return type switch
            {
                "ADULT" => referenceDate.AddYears(-rnd.Next(18, 80)).ToString("yyyy-MM-dd"),
                "TEEN" => referenceDate.AddYears(-rnd.Next(13, 15)).ToString("yyyy-MM-dd"),
                "CHILD" => referenceDate.AddYears(-rnd.Next(3, 11)).ToString("yyyy-MM-dd"),
                "INFANT" => GenerateInfantDateOfBirth(referenceDate),
                _ => throw new ArgumentException($"Unknown passenger type: {type}")
            };
        }

        private static string GenerateInfantDateOfBirth(DateTime latestTravelDate)
        {
            DateTime oldestValidInfantDateOfBirth = latestTravelDate.AddYears(-2).AddDays(1);
            DateTime preferredInfantDateOfBirth = DateTime.Today.AddMonths(-6);
            DateTime minimumAgePreferenceDateOfBirth = DateTime.Today.AddMonths(-1);
            DateTime infantDateOfBirth = preferredInfantDateOfBirth > oldestValidInfantDateOfBirth
                ? preferredInfantDateOfBirth
                : oldestValidInfantDateOfBirth;

            if (infantDateOfBirth > minimumAgePreferenceDateOfBirth &&
                minimumAgePreferenceDateOfBirth >= oldestValidInfantDateOfBirth)
            {
                infantDateOfBirth = minimumAgePreferenceDateOfBirth;
            }

            return infantDateOfBirth.ToString("yyyy-MM-dd");
        }

        public static void UpdateBasketWithPassengerContactAndTnCsWithLoyaltyAccounts(string basketId, out string sectorId, out HttpResponseMessage retrieveBasketGQLResponse, string firstName, string lastName, string b2cToken)
        {
            throw new NotImplementedException();
        }

        private static string[] GetSummaryLinks(TestConfig config, string responseContent)
        {
            OfferFilterCriteria criteria = config.OfferFilterCriteria;

            int connectionTimeDuration = config.OfferFilterCriteria.LongLayoverDuration;

            string linkType = "summary";

            return criteria switch
            {
                { AllOffersWithoutFilter: true } =>
                    OfferSearchHelper.FindAllFareOrSummaryLinksForAnyFlight(responseContent, linkType),

                { DirectFlight: true } =>
                    OfferSearchHelper.FindAllFareOrSummaryLinksForSpecificAirlineAndDirectOnly(responseContent, FlightType.VS, linkType: linkType),

                { DirectWithStop: true } =>
                    OfferSearchHelper.FindAllFareOrSummaryLinksForVirginDirectWithStop(responseContent, linkType: linkType),

                { OneStop: true } =>
                    OfferSearchHelper.FindAllFareOrSummaryLinksWithOneStop(responseContent, FlightType.VS, linkType: linkType),

                { TwoStops: true } =>
                    OfferSearchHelper.FindAllFareOrSummaryLinksWithTwoStop(responseContent, FlightType.VS, linkType: linkType),

                { VSFlightRandomOffer: true } =>
                    FindOutboundFareOrSummaryLinksForFareFamily(config, responseContent, linkType),

                { LongLayover: true } =>
                                    OfferSearchHelper.FindAllFareOrSummaryLinksWithLongLayOver(responseContent, FlightType.VS, connectionTimeDuration, linkType: linkType),
                _ =>
                    FindOutboundFareOrSummaryLinksForFareFamily(config, responseContent, linkType)
            };
        }

        private static string[] FindOutboundFareOrSummaryLinksForFareFamily(TestConfig config, string responseContent, string linkType)
        {
            return FindFareOrSummaryLinksForFareFamily(
                responseContent,
                config.Search.FareFamilyType,
                config.Search.FlightType,
                config.Search.RequiredLegOperatingAirlineCode,
                linkType);
        }

        private static string[] FindInboundFareOrSummaryLinksForFareFamily(TestConfig config, string responseContent, string linkType)
        {
            return FindFareOrSummaryLinksForFareFamily(
                responseContent,
                config.Search.InboundFareFamilyType,
                config.Search.InboundFlightType,
                config.Search.InboundRequiredLegOperatingAirlineCode,
                linkType);
        }

        private static string[] FindFareOrSummaryLinksForFareFamily(string responseContent, FareFamilyType fareFamilyType, FlightType flightType, string requiredLegOperatingAirlineCode, string linkType)
        {
            if (!requiredLegOperatingAirlineCode.IsNullOrWhiteSpace())
            {
                return OfferSearchHelper.FindAllFareOrSummaryLinksForFareFamilyAndLegOperatingAirline(
                    responseContent,
                    fareFamilyType,
                    flightType,
                    requiredLegOperatingAirlineCode,
                    linkType);
            }

            return FindAllStrictCarrierLinksForFareFamily(
                responseContent,
                fareFamilyType,
                flightType,
                linkType);
        }

        private static string[] FindAllStrictCarrierLinksForFareFamily(
            string responseContent,
            FareFamilyType fareFamilyType,
            FlightType flightType,
            string linkType)
        {
            JObject response = JObject.Parse(responseContent);
            string fareFamily = fareFamilyType.GetEnumDescription();
            string airlineCode = flightType.ToString();

            return response
                .SelectTokens("$..slice.flightsAndFares[*]")
                .Where(flightAndFare => AllSegmentsUseOperatingCarrier(flightAndFare, airlineCode))
                .SelectMany(flightAndFare => flightAndFare.SelectTokens("fares[*]"))
                .Where(fare => fare.SelectToken("fareFamilyType")?.ToString().Is(fareFamily) == true)
                .SelectMany(fare => fare.SelectTokens("links[*]"))
                .Where(link => link.SelectToken("rel")?.ToString().Is(linkType) == true)
                .Select(link => link.SelectToken("href")?.ToString())
                .Where(link => !link.IsNullOrWhiteSpace())
                .ToArray();
        }

        private static bool AllSegmentsUseOperatingCarrier(JToken flightAndFare, string airlineCode)
        {
            JToken[] segments = flightAndFare.SelectTokens("flight.segments[*]").ToArray();
            return segments.Any()
                && segments.All(segment => segment.SelectToken("operatingAirline.code")?.ToString().Is(airlineCode) == true);
        }

        private static string BuildFareSelectionDiagnostics(string responseContent)
        {
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                return "Response body was empty.";
            }

            JObject response;
            try
            {
                response = JObject.Parse(responseContent);
            }
            catch (JsonException ex)
            {
                string responsePreview = responseContent.Length > 500 ? responseContent[..500] : responseContent;
                return $"Response body was not valid JSON. ParseError={ex.Message}, ResponsePreview={responsePreview}";
            }

            IEnumerable<JToken> flightsAndFares = response.SelectTokens("$..slice.flightsAndFares[*]");

            HashSet<string> operatingAirlineCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> legOperatingAirlineCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> fareFamilies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int flightsAndFaresCount = 0;

            foreach (JToken flightAndFare in flightsAndFares)
            {
                flightsAndFaresCount++;

                IEnumerable<JToken> segments = flightAndFare.SelectTokens("flight.segments[*]");
                foreach (JToken segment in segments)
                {
                    string operatingAirlineCode = segment.SelectToken("operatingAirline.code")?.ToString();
                    if (!string.IsNullOrWhiteSpace(operatingAirlineCode))
                    {
                        operatingAirlineCodes.Add(operatingAirlineCode);
                    }
                }

                IEnumerable<JToken> legs = flightAndFare.SelectTokens("flight.segments[*].legs[*]");
                foreach (JToken leg in legs)
                {
                    string legOperatingAirlineCode = leg.SelectToken("operatingAirline.code")?.ToString();
                    if (!string.IsNullOrWhiteSpace(legOperatingAirlineCode))
                    {
                        legOperatingAirlineCodes.Add(legOperatingAirlineCode);
                    }
                }

                IEnumerable<JToken> fares = flightAndFare.SelectTokens("fares[*]");
                foreach (JToken fare in fares)
                {
                    string fareFamily = fare.SelectToken("fareFamilyType")?.ToString();
                    if (!string.IsNullOrWhiteSpace(fareFamily))
                    {
                        fareFamilies.Add(fareFamily);
                    }
                }
            }

            string carriers = operatingAirlineCodes.Any() ? string.Join(",", operatingAirlineCodes.OrderBy(code => code)) : "none";
            string legCarriers = legOperatingAirlineCodes.Any() ? string.Join(",", legOperatingAirlineCodes.OrderBy(code => code)) : "none";
            string families = fareFamilies.Any() ? string.Join(",", fareFamilies.OrderBy(family => family)) : "none";

            return $"FlightsAndFares={flightsAndFaresCount}, SegmentCarriers={carriers}, LegCarriers={legCarriers}, FareFamilies={families}";
        }
    }
}
