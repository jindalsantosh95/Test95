using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using OfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.OfferSearchHelper;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.GraphQL.OfferSearch
{
    public class GraphQlAssertions
    {
        private const int JsonPreviewMaxLength = 2_000;
        private const int IncompleteLegPreviewLimit = 3;

        public static bool ValidatePOSGraphQlOfferSearchResponse(dynamic offerSearchResponse, out string message)
        {
            message = string.Empty;

            if (string.IsNullOrWhiteSpace(offerSearchResponse.data.searchOffers.flightOfferResponse.pos.cityCode.Value))
            {
                message = "POS city code not returned in offer search graph ql response";
                return false;
            }

            if (string.IsNullOrWhiteSpace(offerSearchResponse.data.searchOffers.flightOfferResponse.pos.countryCode.Value))
            {
                message = "POS country code not returned in offer search graph ql response";
                return false;
            }

            return true;
        }

        public static bool ValidatePOSGraphQlOfferPriceResponse(dynamic offerPriceResponse, out string message)
        {
            message = string.Empty;

            if (string.IsNullOrWhiteSpace(offerPriceResponse.data.offerPrice.pos.cityCode.Value))
            {
                message = "POS city code not returned in offer price graph ql response";
                return false;
            }

            if (string.IsNullOrWhiteSpace(offerPriceResponse.data.offerPrice.pos.countryCode.Value))
            {
                message = "POS country code not returned in offer price graph ql response";
                return false;
            }

            return true;
        }

        public static bool ValidateCalenderGQLSearchResponse(dynamic offerSearchResponse, out string message)
        {
            message = string.Empty;

            string[] calendar = ((JToken)offerSearchResponse).SelectTokens("$.data.searchOffers.calendar.fromPrices[*]").Select(x => x.ToString()).ToArray();

            if (calendar.Count() < 0)
            {
                message = "Calendar can not be empty in offer search graph ql response";
                return false;
            }
            return true;
        }

        public static bool ValidateGQLOriginAndDestination(string searchRequest, dynamic offerSearchResponse, out string message)
        {
            message = string.Empty;

            JToken flightSearchRequest = JsonConvert.DeserializeObject<JToken>(searchRequest);

            string[] requestedOrigin = flightSearchRequest.SelectTokens("$.variables.request.flightSearchRequest.searchOriginDestinations[*].origin").Select(x => x.ToString()).ToArray();

            string[] requestedDestination = flightSearchRequest.SelectTokens("$.variables.request.flightSearchRequest.searchOriginDestinations[*].destination").Select(x => x.ToString()).ToArray();

            string[] responseOrigin = ((JToken)offerSearchResponse).SelectTokens("$..result.criteria.origin.code").Select(x => x.ToString()).ToArray();

            string[] responseDestination = ((JToken)offerSearchResponse).SelectTokens("$..result.criteria.destination.code").Select(x => x.ToString()).ToArray();

            if (!requestedOrigin.SequenceEqual(responseOrigin))
            {
                message = "Requested flight origin and flight origin from response not matched";

                return false;
            }

            if (!requestedDestination.SequenceEqual(responseDestination))
            {
                message = "Requested flight destination and flight destination from response not matched";

                return false;
            }
            return true;
        }

        public static bool ValidateAirCraftInfo(dynamic offerSearchResponse, out string message)
        {
            message = string.Empty;

            string[] metalName = ((JToken)offerSearchResponse).SelectTokens("$..result.slice.flightsAndFares[*].flight.segments[*].metal[*].name").Select(x => x.ToString()).ToArray();

            string[] airlineDetails = ((JToken)offerSearchResponse).SelectTokens("$..result.slice.flightsAndFares[*].flight.segments[*].airline").Select(x => x.ToString()).ToArray();

            string[] flightNumber = ((JToken)offerSearchResponse).SelectTokens("$..result.slice.flightsAndFares[*].flight.segments[*].flightNumber").Select(x => x.ToString()).ToArray();

            string[] operatingFlightNumber = ((JToken)offerSearchResponse).SelectTokens("$..result.slice.flightsAndFares[*].flight.segments[*].operatingFlightNumber").Select(x => x.ToString()).ToArray();

            string[] operatingAirline = ((JToken)offerSearchResponse).SelectTokens("$..result.slice.flightsAndFares[*].flight.segments[*].operatingAirline").Select(x => x.ToString()).ToArray();

            if (!metalName.Any())
            {
                message = "Metal name can not be empty in resposne";

                return false;
            }
            if (!airlineDetails.Any())
            {
                message = "Airline details can not be empty in resposne";

                return false;
            }
            if (!flightNumber.Any())
            {
                message = "Flight number can not be empty in resposne";

                return false;
            }
            if (!operatingFlightNumber.Any())
            {
                message = "Operating flight number can not be empty in resposne";

                return false;
            }
            if (!operatingAirline.Any())
            {
                message = "Operating airline details can not be empty in resposne";

                return false;
            }
            return true;
        }

        public static bool ValidateLegOperatingAirlineDetails(dynamic offerSearchResponse, string expectedOperatingAirlineCode, string expectedOperatingAirlineName, out string message)
        {
            message = string.Empty;

            JToken[] legs = ((JToken)offerSearchResponse)
                .SelectTokens("$..result.slice.flightsAndFares[*].flight.segments[*].legs[*]")
                .ToArray();

            if (!legs.Any())
            {
                message = "No leg details were returned in offer search response.";
                return false;
            }

            JToken[] incompleteOperatingAirlineLegs = legs
                .Where(leg => leg.SelectToken("operatingAirline.code")?.ToString().IsNullOrWhiteSpace() != false
                    || leg.SelectToken("operatingAirline.name")?.ToString().IsNullOrWhiteSpace() != false)
                .ToArray();

            if (incompleteOperatingAirlineLegs.Any())
            {
                string incompleteLegPreview = string.Join(" | ", incompleteOperatingAirlineLegs
                    .Take(IncompleteLegPreviewLimit)
                    .Select(leg => BuildJsonPreview(leg)));

                message = $"One or more legs did not include operatingAirline code and name. Showing {Math.Min(incompleteOperatingAirlineLegs.Length, IncompleteLegPreviewLimit)} of {incompleteOperatingAirlineLegs.Length} incomplete legs: {incompleteLegPreview}";
                return false;
            }

            bool hasExpectedOperatingAirline = legs.Any(leg =>
                leg.SelectToken("operatingAirline.code")?.ToString().Is(expectedOperatingAirlineCode) == true
                && leg.SelectToken("operatingAirline.name")?.ToString().Contains(expectedOperatingAirlineName, StringComparison.OrdinalIgnoreCase) == true);

            if (!hasExpectedOperatingAirline)
            {
                string returnedOperators = BuildLegOperatingAirlineSummary(legs);
                message = $"Expected leg operatingAirline {expectedOperatingAirlineCode}/{expectedOperatingAirlineName}, but returned leg operators were: {returnedOperators}";
                return false;
            }

            return true;
        }

        private static string BuildLegOperatingAirlineSummary(IEnumerable<JToken> legs)
        {
            return string.Join(", ", legs
                .Select(leg => $"{leg.SelectToken("origin.code")?.ToString()}-{leg.SelectToken("destination.code")?.ToString()}:{leg.SelectToken("operatingAirline.code")?.ToString()}:{leg.SelectToken("operatingAirline.name")?.ToString()}")
                .Distinct(StringComparer.OrdinalIgnoreCase));
        }

        public static string BuildJsonPreview(string json)
        {
            if (json.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            try
            {
                return BuildJsonPreview(JToken.Parse(json));
            }
            catch (JsonReaderException)
            {
                return Truncate(json);
            }
        }

        public static string BuildJsonPreview(JToken token)
        {
            return Truncate(token.ToString(Formatting.None));
        }

        private static string Truncate(string value)
        {
            if (value.Length <= JsonPreviewMaxLength)
            {
                return value;
            }

            return value.Substring(0, JsonPreviewMaxLength) + "...[truncated]";
        }

        public static bool ValidatePriceGrid(dynamic offerSearchResponse, out string message)
        {
            message = string.Empty;

            string[] returning = ((JToken)offerSearchResponse).SelectTokens("$.data.searchOffers.priceGrid.returning[*]").Select(x => x.ToString()).ToArray();

            string[] departures = ((JToken)offerSearchResponse).SelectTokens("$.data.searchOffers.priceGrid.departures[*]").Select(x => x.ToString()).ToArray();

            if (!returning.Any() && !departures.Any())
            {
                message = $"Price grid data not exist in response for return {returning} and departure {departures}";

                return false;
            }

            return true;
        }

        public static bool SummaryUpgradeOffersValidation(dynamic offersPriceResponse, out string message)
        {
            message = string.Empty;

            string[] upgradeOffers = ((JObject)offersPriceResponse).SelectTokens("$.data.searchSummary.upgrades[*]").Select(x => x.ToString()).ToArray();

            string[] fareFamilyTypes = ((JObject)offersPriceResponse).SelectTokens("$.data.searchSummary.flights[*].fareFamilyType").Select(x => x.ToString()).ToArray();

            // Validate if fare family type is "BUS-FIRST-FLEXIBLE"
            bool isBusFirstFlexibleFare = fareFamilyTypes.Contains(FareFamilyType.BUSFIRSTFLEXIBLE.GetEnumDescription());

            // If fare family is valid and there are no upgrade offers, return true early
            if (isBusFirstFlexibleFare && !upgradeOffers.Any())
            {
                return true;
            }

            // If no upgrade offers were returned, set an error message and return false
            if (!upgradeOffers.Any())
            {
                message = $"No upgrade offers returned for summary:{offersPriceResponse}";

                return false;
            }

            // If everything is valid, return true
            return true;
        }

        public static bool ValidateOfferSearchFilterResult(dynamic offerSearchResponse, string flightTypeCode, out string message)
        {
            message = string.Empty;

            string[] offerSearchAirlineCodes = ((JToken)offerSearchResponse).SelectTokens($"$..flightsAndFares[*].flight.segments[?(@.operatingAirline.code == '{flightTypeCode}')].operatingAirline.code").Select(x => x.ToString()).ToArray();

            foreach (string airlineCode in offerSearchAirlineCodes)
            {
                if (airlineCode != flightTypeCode)
                {
                    message = $"Airline filter result actual airline{airlineCode} and expected airline not matched{flightTypeCode}";

                    return false;
                }
            }

            return true;
        }

        public static bool ValidateOfferSearchConnectionsFilterResult(dynamic offerSearchResponse, int stopCounts, out string message)
        {
            message = string.Empty;

            JToken[] offerSearchSegmentsResult = ((JToken)offerSearchResponse).SelectTokens("$.data.searchOffers.result.slice.flightsAndFares[*].flight.segments").ToArray();

            foreach (JToken segment in offerSearchSegmentsResult)
            {
                int count = segment.SelectTokens("$.[*].stopCount").Count();

                if (count != stopCounts)
                {
                    message = $"Connections filter segment count {count} and actual count {stopCounts}";

                    return false;
                }
            }

            return true;
        }

        public static bool ValidateOfferSearchPriceSortResult(dynamic offerSearchResponse, FareFamilyType fareFamilyType, out string message)
        {
            message = string.Empty;

            string fareFamilyTypes = fareFamilyType.GetEnumDescription();

            string[] sortedPriceResult = ((JToken)offerSearchResponse).SelectTokens($"$..result.slice.flightsAndFares[*].fares[?(@.fareFamilyType == '{fareFamilyTypes}')].price.amountIncludingTax").Select(x => x.ToString()).ToArray();

            for (int i = 1; i < sortedPriceResult.Length; i++)
            {
                if (Convert.ToDecimal(sortedPriceResult[i]) < Convert.ToDecimal(sortedPriceResult[i - 1]))
                {
                    message = $"Sorted price results are not in lowest{sortedPriceResult[i]} to highest {sortedPriceResult[i - 1]}order";

                    return false;
                }
            }

            return true;
        }

        public static bool ValidateOfferSearchDepartureTimeSortResult(dynamic offerSearchResponse, out string message)
        {
            message = string.Empty;

            string[] sortedDepTimeResult = ((JToken)offerSearchResponse).SelectTokens("$.data.searchOffers.result.slice.flightsAndFares[*].flight.departure").Select(x => x.ToString()).ToArray();

            for (int i = 1; i < sortedDepTimeResult.Length; i++)
            {
                if (Convert.ToDateTime(sortedDepTimeResult[i]) < Convert.ToDateTime(sortedDepTimeResult[i - 1]))
                {
                    message = $"Sorted departure time results are not in lowest{sortedDepTimeResult[i]} to highest {sortedDepTimeResult[i - 1]}order";

                    return false;
                }
            }

            return true;
        }

        public static bool ValidateOfferSearchDurationTimeSortResult(dynamic offerSearchResponse, out string message)
        {
            message = string.Empty;

            string[] sortedDurationTimeResult = ((JToken)offerSearchResponse).SelectTokens("$.data.searchOffers.result.slice.flightsAndFares[*].flight.duration").Select(x => x.ToString()).ToArray();

            for (int i = 1; i < sortedDurationTimeResult.Length; i++)
            {
                string duration = Regex.Match(sortedDurationTimeResult[i - 1], @"\d+").Value;
                string duration1 = Regex.Match(sortedDurationTimeResult[i], @"\d+").Value;

                if (int.Parse(duration1) < int.Parse(duration))
                {
                    message = $"Sorted duration time results are not in lowest{sortedDurationTimeResult[i]} to highest {sortedDurationTimeResult[i - 1]}order";

                    return false;
                }
            }

            return true;
        }

        public static bool ValidateOfferSearchGraphqlResponse(HttpResponseMessage offerSearchResponse, bool writeAcceptedNoResultsDiagnostic = true)
        {
            // Deserialize the response content to a dynamic object
            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            // "No results" is a valid search outcome and should not fail regression assertions.
            string errorCode = offerSearchResult?.errors?[0]?.extensions?.code?.Value ?? string.Empty;
            string normalizedErrorPayload = NormalizeNoResultsText(offerSearchResult?.errors?.ToString() ?? string.Empty);
            string[] acceptedNoResultsFragments =
            [
                "find any results for your search",
                "unable to find a flight that meets your current search criteria"
            ];
            bool isAcceptedNoResultsError = errorCode.Is("422") &&
                                            acceptedNoResultsFragments.Any(fragment =>
                                                normalizedErrorPayload.Contains(fragment, StringComparison.OrdinalIgnoreCase));
            if (isAcceptedNoResultsError)
            {
                if (writeAcceptedNoResultsDiagnostic)
                {
                    Console.WriteLine(
                        $"No flight results for this search scenario.{Environment.NewLine}" +
                        $"Request: {offerSearchResponse.ReadRequestAsString()}{Environment.NewLine}" +
                        $"Response: {offerSearchResponse.ReadResponseAsString()}");
                }

                return true;
            }

            // Extract the relevant data
            dynamic searchOffers = offerSearchResult?.data?.searchOffers;

            // Validate that 'result' is null and either 'calendar' or 'priceGrid' is not null
            bool hasInvalidResult = searchOffers?.result == null &&
                                    (searchOffers?.calendar != null || searchOffers?.priceGrid != null);

            return hasInvalidResult;
        }

        public static bool HasGraphQlErrors(HttpResponseMessage response)
        {
            string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (string.IsNullOrWhiteSpace(responseBody) || responseBody.Is("null"))
            {
                return false;
            }

            JToken gqlResponse = JsonConvert.DeserializeObject<JToken>(responseBody);
            if (gqlResponse == null)
            {
                return false;
            }

            return gqlResponse.SelectTokens("$.errors[*]").Any();
        }

        private static string NormalizeNoResultsText(string source)
        {
            return source
                .Replace("\u2019", "'")
                .Replace("’", "'")
                .ToLowerInvariant();
        }

        public static bool ValidateLoyaltyPointsEligibility(dynamic summaryResult, dynamic vRedResult, out string message)
        {
            message = string.Empty;

            string totalAwardPoints = summaryResult.data.searchSummary.price.awardPoints.Value;
            bool hasSufficientPoints = summaryResult.data.searchSummary.hasSufficientPoints.Value;

            long loyaltyMemberPoints = vRedResult.balance.current.Value;
            long requiredPoints = long.Parse(totalAwardPoints);

            if (loyaltyMemberPoints > requiredPoints && !hasSufficientPoints)
            {
                message = $"Summary total points {totalAwardPoints} are less than required points {loyaltyMemberPoints} from the loyalty account.";
                return false;
            }

            if (loyaltyMemberPoints < requiredPoints && hasSufficientPoints)
            {
                message = $"Summary total points {totalAwardPoints} are more than required points {loyaltyMemberPoints} from the loyalty account.";
                return false;
            }

            return true;
        }

        public static bool ValidateRewardVoucherDetails(JObject searchSummaryResponse, string expectedVoucherPayload, string expectedRedemptionType, int expectedPassengerCount, out string message, bool validateVoucherAmountCalculations = false)
        {
            message = string.Empty;

            if (expectedVoucherPayload.IsNullOrWhiteSpace())
            {
                message = "Expected voucher payload from storage table was empty.";
                return false;
            }

            JArray expectedVouchers;
            try
            {
                expectedVouchers = JArray.Parse(expectedVoucherPayload);
            }
            catch (JsonException ex)
            {
                message = $"Expected voucher payload from storage table was not valid JSON. Error: {ex.Message}. Payload: {expectedVoucherPayload}";
                return false;
            }

            List<string> expectedVoucherIds = ExtractExpectedVoucherValues(expectedVouchers, "voucherNumber", "voucherId", "number", "id");
            List<string> expectedVoucherTypes = ExtractExpectedVoucherValues(expectedVouchers, "type", "category");
            List<string> expectedIdentifiers = expectedVoucherIds.Any() ? expectedVoucherIds : expectedVoucherTypes;

            if (!expectedIdentifiers.Any())
            {
                message = $"Expected voucher payload did not contain a voucherNumber/voucherId/number/id or type/category. Payload: {expectedVoucherPayload}";
                return false;
            }

            JToken searchSummary = searchSummaryResponse.SelectToken("$.data.searchSummary");
            if (searchSummary == null)
            {
                message = $"searchSummary was not returned in response. Response: {searchSummaryResponse}";
                return false;
            }

            JToken voucherDetails = searchSummary.SelectToken("voucherDetails");
            if (voucherDetails == null)
            {
                message = $"voucherDetails was not returned in searchSummary. Expected one of: {string.Join(", ", expectedIdentifiers)}. Response: {searchSummaryResponse}";
                return false;
            }

            JArray availableVouchers = voucherDetails.SelectToken("availableVouchers") as JArray;
            if (availableVouchers == null || !availableVouchers.Any())
            {
                message = $"voucherDetails.availableVouchers was not returned or was empty. Expected one of: {string.Join(", ", expectedIdentifiers)}. voucherDetails: {voucherDetails}";
                return false;
            }

            List<string> actualVoucherIds = ExtractActualVoucherValues(availableVouchers, "voucherId", "voucherNumber", "number", "id");
            List<string> actualVoucherTypes = ExtractActualVoucherValues(availableVouchers, "type", "category");

            if (expectedVoucherIds.Any())
            {
                if (expectedVoucherIds.Count > 2)
                {
                    List<string> unexpectedVoucherIds = actualVoucherIds
                        .Where(x => !expectedVoucherIds.Any(y => y.Is(x)))
                        .ToList();

                    if (actualVoucherIds.Count != 2 || unexpectedVoucherIds.Any())
                    {
                        message = $"availableVouchers should contain two vouchers belonging to the member when storage contains more than two. UnexpectedIds: {string.Join(", ", unexpectedVoucherIds)}. ExpectedIds: {string.Join(", ", expectedVoucherIds)}. ActualIds: {string.Join(", ", actualVoucherIds)}. voucherDetails: {voucherDetails}";
                        return false;
                    }
                }
                else
                {
                    List<string> missingVoucherIds = expectedVoucherIds
                        .Where(x => !actualVoucherIds.Any(y => y.Is(x)))
                        .ToList();

                    if (missingVoucherIds.Any())
                    {
                        message = $"availableVouchers did not include all storage-table voucher IDs. MissingIds: {string.Join(", ", missingVoucherIds)}. ExpectedIds: {string.Join(", ", expectedVoucherIds)}. ActualIds: {string.Join(", ", actualVoucherIds)}. voucherDetails: {voucherDetails}";
                        return false;
                    }
                }
            }
            else if (expectedVoucherTypes.Any())
            {
                List<string> missingVoucherTypes = expectedVoucherTypes
                    .Where(x => !actualVoucherTypes.Any(y => y.Is(x)))
                    .ToList();

                if (missingVoucherTypes.Any())
                {
                    message = $"availableVouchers did not include all storage-table voucher types. MissingTypes: {string.Join(", ", missingVoucherTypes)}. ExpectedTypes: {string.Join(", ", expectedVoucherTypes)}. ActualTypes: {string.Join(", ", actualVoucherTypes)}. voucherDetails: {voucherDetails}";
                    return false;
                }
            }
            else
            {
                message = $"Expected voucher payload did not include IDs or types to compare against availableVouchers. Payload: {expectedVoucherPayload}";
                return false;
            }

            JArray redemptionOptions = voucherDetails.SelectToken("redemptionOptions") as JArray;
            if (redemptionOptions == null || !redemptionOptions.Any())
            {
                message = $"voucherDetails.redemptionOptions was not returned or was empty for a reward voucher search. voucherDetails: {voucherDetails}";
                return false;
            }

            if (!expectedRedemptionType.IsNullOrWhiteSpace())
            {
                List<string> redemptionTypes = ExtractActualVoucherValues(redemptionOptions, "redemptionType");
                if (!redemptionTypes.Any(x => x.Is(expectedRedemptionType)))
                {
                    message = $"Expected redemption type '{expectedRedemptionType}' was not returned. RedemptionTypes: {string.Join(", ", redemptionTypes)}. voucherDetails: {voucherDetails}";
                    return false;
                }

                List<JObject> matchingRedemptionOptions = redemptionOptions
                    .Children<JObject>()
                    .Where(x => x.SelectToken("redemptionType")?.ToString().Is(expectedRedemptionType) == true)
                    .ToList();

                if (!ValidateCoveredPassengers(matchingRedemptionOptions, expectedPassengerCount, out message))
                {
                    message = $"{message} voucherDetails: {voucherDetails}";
                    return false;
                }

                if (expectedRedemptionType.Is("UPGRADE") && !ValidateUpgradeFlightsUseSingleCabin(searchSummary, out message))
                {
                    message = $"{message} Response: {searchSummaryResponse}";
                    return false;
                }
            }

            JToken bestOption = voucherDetails.SelectToken("bestOption");
            if (bestOption == null)
            {
                message = $"voucherDetails.bestOption was not returned for a reward voucher search. voucherDetails: {voucherDetails}";
                return false;
            }

            string bestOptionId = bestOption.SelectToken("optionId")?.ToString() ?? string.Empty;
            if (bestOptionId.IsNullOrWhiteSpace())
            {
                message = $"voucherDetails.bestOption.optionId was not returned for a reward voucher search. voucherDetails: {voucherDetails}";
                return false;
            }

            List<JObject> redemptionOptionObjects = redemptionOptions.Children<JObject>().ToList();
            JObject bestRedemptionOption = redemptionOptionObjects.FirstOrDefault(x => x.SelectToken("optionId")?.ToString().Is(bestOptionId) == true);
            if (bestRedemptionOption == null)
            {
                List<string> redemptionOptionIds = ExtractActualVoucherValues(redemptionOptions, "optionId");
                message = $"bestOption.optionId '{bestOptionId}' did not match any redemptionOptions.optionId. RedemptionOptionIds: {string.Join(", ", redemptionOptionIds)}. voucherDetails: {voucherDetails}";
                return false;
            }

            if (validateVoucherAmountCalculations && !ValidateRewardVoucherAmountCalculations(searchSummary, bestRedemptionOption, out message))
            {
                message = $"{message} voucherDetails: {voucherDetails}";
                return false;
            }

            decimal maximumAppliedVoucherPoints = redemptionOptionObjects.Max(GetAppliedVoucherPoints);
            decimal bestAppliedVoucherPoints = GetAppliedVoucherPoints(bestRedemptionOption);
            if (maximumAppliedVoucherPoints <= 0M)
            {
                message = $"No redemption option used voucher points, so best value could not be validated. voucherDetails: {voucherDetails}";
                return false;
            }

            if (bestAppliedVoucherPoints != maximumAppliedVoucherPoints)
            {
                message = $"bestOption.optionId '{bestOptionId}' did not select an option using the maximum voucher points. BestOptionPoints: {bestAppliedVoucherPoints}. MaximumOptionPoints: {maximumAppliedVoucherPoints}. voucherDetails: {voucherDetails}";
                return false;
            }

            JObject expectedBestRedemptionOption = redemptionOptionObjects
                .OrderByDescending(GetAppliedVoucherPoints)
                .ThenBy(GetRedemptionOptionPointsToPay)
                .ThenByDescending(option => option.SelectToken("redemptionType")?.ToString().Is("UPGRADE") == true)
                .ThenByDescending(GetAppliedVoucherCount)
                .ThenBy(GetRedemptionOptionId)
                .First();
            string expectedBestOptionId = expectedBestRedemptionOption.SelectToken("optionId")?.ToString() ?? string.Empty;
            if (!bestOptionId.Is(expectedBestOptionId))
            {
                message = $"bestOption.optionId '{bestOptionId}' did not follow voucher selection precedence. ExpectedOptionId: {expectedBestOptionId}. Precedence: highest voucher points, lowest points to pay, Upgrade on a Companion/Upgrade tie, most vouchers, then lowest option id. voucherDetails: {voucherDetails}";
                return false;
            }

            string bestOptionRedemptionType = bestOption.SelectToken("redemptionType")?.ToString()
                ?? bestOption.SelectToken("redemptionStrategy")?.ToString()
                ?? string.Empty;
            if (!bestOptionRedemptionType.IsNullOrWhiteSpace())
            {
                List<string> redemptionTypes = ExtractActualVoucherValues(redemptionOptions, "redemptionType");
                if (!redemptionTypes.Any(x => x.Is(bestOptionRedemptionType)))
                {
                    message = $"bestOption.redemptionType '{bestOptionRedemptionType}' did not match any redemptionOptions.redemptionType. RedemptionTypes: {string.Join(", ", redemptionTypes)}. voucherDetails: {voucherDetails}";
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateRewardVoucherOutboundExpiry(JObject searchSummaryResponse, string outboundDepartureDate, out string message)
        {
            message = string.Empty;

            if (!DateTime.TryParse(outboundDepartureDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime outboundDateTime))
            {
                message = $"Outbound departure date could not be parsed for voucher expiry validation. Value: '{outboundDepartureDate}'.";
                return false;
            }

            JArray availableVouchers = searchSummaryResponse.SelectToken("$.data.searchSummary.voucherDetails.availableVouchers") as JArray;
            if (availableVouchers == null || !availableVouchers.Any())
            {
                message = $"No available vouchers were returned for outbound expiry validation. Response: {searchSummaryResponse}";
                return false;
            }

            DateTime outboundDate = outboundDateTime.Date;
            foreach (JObject voucher in availableVouchers.Children<JObject>())
            {
                string voucherId = voucher.SelectToken("voucherId")?.ToString() ?? string.Empty;
                string expiryDateText = voucher.SelectToken("expiryDate")?.ToString() ?? string.Empty;
                if (!DateTime.TryParse(expiryDateText, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime expiryDate))
                {
                    message = $"Available voucher '{voucherId}' returned an invalid expiry date '{expiryDateText}'. Voucher: {voucher}";
                    return false;
                }

                if (expiryDate.Date < outboundDate)
                {
                    message = $"Available voucher '{voucherId}' expired before outbound travel. ExpiryDate: {expiryDate:yyyy-MM-dd}. OutboundDate: {outboundDate:yyyy-MM-dd}.";
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateExpiredRewardVouchersExcluded(
            JObject searchSummaryResponse,
            string expectedVoucherPayload,
            string outboundDepartureDate,
            out string message)
        {
            return ValidateExpiredRewardVouchersExcludedAtPath(
                searchSummaryResponse,
                expectedVoucherPayload,
                outboundDepartureDate,
                "$.data.searchSummary.voucherDetails",
                out message);
        }

        public static bool ValidateExpiredRewardVouchersExcludedFromBasket(
            JObject retrieveBasketResponse,
            string expectedVoucherPayload,
            string outboundDepartureDate,
            out string message)
        {
            return ValidateExpiredRewardVouchersExcludedAtPath(
                retrieveBasketResponse,
                expectedVoucherPayload,
                outboundDepartureDate,
                "$.data.retrieveBasket.summary.voucherDetails",
                out message);
        }

        public static bool ValidateEligibleRewardVouchersByOutboundDate(
            JObject searchSummaryResponse,
            string expectedVoucherPayload,
            string outboundDepartureDate,
            out string message)
        {
            message = string.Empty;
            if (!DateTime.TryParse(outboundDepartureDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime outboundDateTime))
            {
                message = $"Outbound departure date could not be parsed for eligible voucher validation. Value: '{outboundDepartureDate}'.";
                return false;
            }

            if (!TryParseExpectedVoucherPayload(expectedVoucherPayload, out JArray expectedVouchers, out message))
                return false;

            List<string> expectedEligibleVoucherIds = expectedVouchers
                .Children<JObject>()
                .Where(voucher => DateTime.TryParse(
                    GetVoucherValue(voucher, "expiryDate"),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime expiryDate) && expiryDate.Date >= outboundDateTime.Date)
                .Select(voucher => GetVoucherValue(voucher, "voucherNumber", "voucherId", "number", "id"))
                .Where(x => !x.IsNullOrWhiteSpace())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (!expectedEligibleVoucherIds.Any())
            {
                message = $"Storage-table test data did not contain a voucher valid on outbound date {outboundDateTime:yyyy-MM-dd}. Payload: {expectedVoucherPayload}";
                return false;
            }

            JToken voucherDetails = searchSummaryResponse.SelectToken("$.data.searchSummary.voucherDetails");
            JArray availableVouchers = voucherDetails?.SelectToken("availableVouchers") as JArray;
            if (availableVouchers == null || !availableVouchers.Any())
            {
                message = $"No available vouchers were returned for outbound date {outboundDateTime:yyyy-MM-dd}. voucherDetails: {voucherDetails}";
                return false;
            }

            List<string> returnedVoucherIds = ExtractActualVoucherValues(
                availableVouchers,
                "voucherId", "voucherNumber", "number", "id");
            List<string> missingEligibleVoucherIds = expectedEligibleVoucherIds
                .Where(expectedId => !returnedVoucherIds.Any(returnedId => returnedId.Is(expectedId)))
                .ToList();
            if (missingEligibleVoucherIds.Any())
            {
                message = $"Vouchers valid for outbound travel were not returned. MissingIds: {string.Join(", ", missingEligibleVoucherIds)}. OutboundDate: {outboundDateTime:yyyy-MM-dd}. voucherDetails: {voucherDetails}";
                return false;
            }

            bool hasExpiredVoucher = expectedVouchers
                .Children<JObject>()
                .Any(voucher => DateTime.TryParse(
                    GetVoucherValue(voucher, "expiryDate"),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime expiryDate) && expiryDate.Date < outboundDateTime.Date);
            if (!hasExpiredVoucher)
                return true;

            return ValidateExpiredRewardVouchersExcluded(
                searchSummaryResponse,
                expectedVoucherPayload,
                outboundDepartureDate,
                out message);
        }

        public static bool ValidateVoucherEligibilityIndicator(
            JObject searchSummaryResponse,
            bool expectedEligible,
            out string message)
        {
            const string expiryReasonCode = "VOUCHER_EXPIRES_BEFORE_OUTBOUND";
            JToken voucherEligibility = searchSummaryResponse.SelectToken("$.data.searchSummary.voucherEligibility");
            if (voucherEligibility == null || voucherEligibility.Type == JTokenType.Null)
            {
                message = $"searchSummary.voucherEligibility was not returned. Response: {BuildJsonPreview(searchSummaryResponse)}";
                return false;
            }

            JToken eligibleToken = voucherEligibility.SelectToken("eligible");
            if (eligibleToken == null || eligibleToken.Type != JTokenType.Boolean)
            {
                message = $"voucherEligibility.eligible was not returned as a Boolean. Value: {eligibleToken ?? "null"}.";
                return false;
            }

            bool actualEligible = eligibleToken.Value<bool>();
            string actualReasonCode = voucherEligibility.SelectToken("reasonCode")?.ToString() ?? string.Empty;
            if (actualEligible != expectedEligible)
            {
                message = $"Expected voucherEligibility.eligible to be {expectedEligible}, but it was {actualEligible}. Indicator: {voucherEligibility}.";
                return false;
            }

            if (!expectedEligible && !actualReasonCode.Is(expiryReasonCode))
            {
                message = $"Expected expired-voucher reasonCode '{expiryReasonCode}', but received '{actualReasonCode}'. Indicator: {voucherEligibility}.";
                return false;
            }

            if (expectedEligible && !actualReasonCode.IsNullOrWhiteSpace())
            {
                message = $"Eligible vouchers returned an unexpected reasonCode '{actualReasonCode}'. Indicator: {voucherEligibility}.";
                return false;
            }

            message = $"Voucher eligibility matched. Eligible: {actualEligible}. ReasonCode: {(actualReasonCode.IsNullOrWhiteSpace() ? "none" : actualReasonCode)}.";
            return true;
        }

        public static bool ValidateTwoEarliestEligibleRewardVouchers(
            JObject searchSummaryResponse,
            string expectedVoucherPayload,
            string outboundDepartureDate,
            out string message)
        {
            message = string.Empty;
            if (!DateTime.TryParse(outboundDepartureDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime outboundDateTime))
            {
                message = $"Outbound departure date could not be parsed. Value: '{outboundDepartureDate}'.";
                return false;
            }

            if (!TryParseExpectedVoucherPayload(expectedVoucherPayload, out JArray expectedVouchers, out message))
                return false;

            List<string> expectedVoucherIds = expectedVouchers
                .Children<JObject>()
                .Select(voucher => new
                {
                    Id = GetVoucherValue(voucher, "voucherNumber", "voucherId", "number", "id"),
                    Expiry = DateTime.TryParse(
                        GetVoucherValue(voucher, "expiryDate"),
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime expiryDate)
                        ? expiryDate.Date
                        : DateTime.MinValue
                })
                .Where(voucher => !voucher.Id.IsNullOrWhiteSpace() && voucher.Expiry >= outboundDateTime.Date)
                .OrderBy(voucher => voucher.Expiry)
                .ThenBy(voucher => voucher.Id, StringComparer.OrdinalIgnoreCase)
                .Take(2)
                .Select(voucher => voucher.Id)
                .ToList();
            if (expectedVoucherIds.Count != 2)
            {
                message = $"Storage test data did not contain at least two identifiable vouchers valid on {outboundDateTime:yyyy-MM-dd}. Payload: {expectedVoucherPayload}";
                return false;
            }

            JArray availableVouchers = searchSummaryResponse.SelectToken("$.data.searchSummary.voucherDetails.availableVouchers") as JArray;
            if (availableVouchers == null)
            {
                message = $"availableVouchers was not returned. Response: {BuildJsonPreview(searchSummaryResponse)}";
                return false;
            }

            List<string> actualVoucherIds = ExtractActualVoucherValues(availableVouchers, "voucherId", "voucherNumber", "number", "id");
            bool matches = actualVoucherIds.Count == 2 &&
                expectedVoucherIds.All(expectedId => actualVoucherIds.Any(actualId => actualId.Is(expectedId)));
            message = $"Expected earliest eligible IDs: {string.Join(", ", expectedVoucherIds)}. Actual IDs: {string.Join(", ", actualVoucherIds)}.";
            return matches;
        }

        private static bool ValidateExpiredRewardVouchersExcludedAtPath(
            JObject response,
            string expectedVoucherPayload,
            string outboundDepartureDate,
            string voucherDetailsPath,
            out string message)
        {
            message = string.Empty;

            if (!DateTime.TryParse(outboundDepartureDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime outboundDateTime))
            {
                message = $"Outbound departure date could not be parsed for expired voucher validation. Value: '{outboundDepartureDate}'.";
                return false;
            }

            if (!TryParseExpectedVoucherPayload(expectedVoucherPayload, out JArray expectedVouchers, out message))
                return false;

            List<string> expiredVoucherIds = expectedVouchers
                .Children<JObject>()
                .Where(voucher => DateTime.TryParse(
                    GetVoucherValue(voucher, "expiryDate"),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime expiryDate) && expiryDate.Date < outboundDateTime.Date)
                .Select(voucher => GetVoucherValue(voucher, "voucherNumber", "voucherId", "number", "id"))
                .Where(x => !x.IsNullOrWhiteSpace())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!expiredVoucherIds.Any())
            {
                message = $"Storage-table test data did not contain an identifiable voucher expiring before {outboundDateTime:yyyy-MM-dd}. Payload: {expectedVoucherPayload}";
                return false;
            }

            JToken voucherDetails = response.SelectToken(voucherDetailsPath);
            if (voucherDetails == null || voucherDetails.Type == JTokenType.Null)
                return true;

            List<string> returnedVoucherIds = ExtractActualVoucherValues(
                voucherDetails.SelectToken("availableVouchers") as JArray ?? new JArray(),
                "voucherId", "voucherNumber", "number", "id");
            returnedVoucherIds.AddRange(
                voucherDetails.SelectTokens("redemptionOptions[*].appliedVouchers[*].voucherId")
                    .Select(x => x?.ToString())
                    .Where(x => !x.IsNullOrWhiteSpace()));

            List<string> incorrectlyReturnedVoucherIds = expiredVoucherIds
                .Where(expiredId => returnedVoucherIds.Any(returnedId => returnedId.Is(expiredId)))
                .ToList();
            if (incorrectlyReturnedVoucherIds.Any())
            {
                message = $"Expired vouchers were returned in availableVouchers or appliedVouchers. VoucherIds: {string.Join(", ", incorrectlyReturnedVoucherIds)}. OutboundDate: {outboundDateTime:yyyy-MM-dd}. voucherDetails: {voucherDetails}";
                return false;
            }

            return true;
        }

        public static bool ValidateUpgradePreferredOnCompanionTieAndPersisted(
            JObject searchSummaryResponse,
            JObject retrieveBasketResponse,
            out string message)
        {
            if (!TryGetUpgradeCompanionTie(
                searchSummaryResponse,
                out JObject tiedUpgradeOption,
                out _,
                out _,
                out message))
            {
                return false;
            }

            JToken voucherDetails = searchSummaryResponse.SelectToken("$.data.searchSummary.voucherDetails");
            string expectedUpgradeOptionId = tiedUpgradeOption.SelectToken("optionId")?.ToString() ?? string.Empty;
            JToken summaryBestOption = voucherDetails.SelectToken("bestOption");
            string summaryBestOptionId = summaryBestOption?.SelectToken("optionId")?.ToString() ?? string.Empty;
            if (!summaryBestOptionId.Is(expectedUpgradeOptionId))
            {
                message = $"Summary bestOption did not prefer Upgrade for an exact Companion tie. ExpectedUpgradeOptionId: {expectedUpgradeOptionId}. ActualBestOptionId: {summaryBestOptionId}. voucherDetails: {voucherDetails}";
                return false;
            }

            JToken basketBestOption = retrieveBasketResponse.SelectToken("$.data.retrieveBasket.summary.voucherDetails.bestOption");
            string basketBestOptionId = basketBestOption?.SelectToken("optionId")?.ToString() ?? string.Empty;
            string basketRedemptionType = basketBestOption?.SelectToken("redemptionType")?.ToString() ?? string.Empty;
            if (!basketBestOptionId.Is(expectedUpgradeOptionId) || !basketRedemptionType.Is("UPGRADE"))
            {
                message = $"Payment basket did not persist the tied Upgrade option as bestOption. ExpectedOptionId: {expectedUpgradeOptionId}. ActualOptionId: {basketBestOptionId}. ActualRedemptionType: {basketRedemptionType}. BasketBestOption: {basketBestOption}";
                return false;
            }

            return true;
        }

        public static bool TryGetUpgradeCompanionTie(
            JObject searchSummaryResponse,
            out JObject tiedUpgradeOption,
            out JObject tiedCompanionOption,
            out List<string> selectedVoucherIds,
            out string message)
        {
            tiedUpgradeOption = null;
            tiedCompanionOption = null;
            selectedVoucherIds = new List<string>();
            message = string.Empty;
            JToken voucherDetails = searchSummaryResponse.SelectToken("$.data.searchSummary.voucherDetails");
            JArray redemptionOptions = voucherDetails?.SelectToken("redemptionOptions") as JArray;
            if (redemptionOptions == null || redemptionOptions.Count == 0)
            {
                message = $"No redemption options were returned to validate an Upgrade/Companion tie. voucherDetails: {voucherDetails}";
                return false;
            }

            List<JObject> upgradeOptions = redemptionOptions.Children<JObject>()
                .Where(option => option.SelectToken("redemptionType")?.ToString().Is("UPGRADE") == true)
                .ToList();
            List<JObject> companionOptions = redemptionOptions.Children<JObject>()
                .Where(option => option.SelectToken("redemptionType")?.ToString().Is("COMPANION") == true)
                .ToList();
            (JObject Upgrade, JObject Companion) tie = upgradeOptions
                .SelectMany(upgrade => companionOptions.Select(companion => (Upgrade: upgrade, Companion: companion)))
                .FirstOrDefault(pair =>
                    GetAppliedVoucherPoints(pair.Upgrade) == GetAppliedVoucherPoints(pair.Companion) &&
                    GetRedemptionOptionPointsToPay(pair.Upgrade) == GetRedemptionOptionPointsToPay(pair.Companion) &&
                    GetRedemptionOptionCashToPay(pair.Upgrade) == GetRedemptionOptionCashToPay(pair.Companion) &&
                    GetRedemptionOptionPointsToPay(pair.Upgrade) > 0M);

            if (tie.Upgrade == null || tie.Companion == null)
            {
                message = $"The response did not contain Upgrade and Companion options above the voucher limit with matching voucher points, points to pay and cash to pay. voucherDetails: {voucherDetails}";
                return false;
            }

            tiedUpgradeOption = tie.Upgrade;
            tiedCompanionOption = tie.Companion;
            selectedVoucherIds = ExtractActualVoucherValues(
                tie.Upgrade.SelectToken("appliedVouchers") as JArray ?? new JArray(),
                "voucherId");

            return true;
        }

        public static bool ValidateUpgradeSectorDeltasAndVoucherCap(
            JObject searchSummaryResponse,
            decimal outboundEconomyPoints,
            decimal outboundSelectedCabinPoints,
            decimal inboundEconomyPoints,
            decimal inboundSelectedCabinPoints,
            out string message)
        {
            message = string.Empty;
            decimal outboundDelta = outboundSelectedCabinPoints - outboundEconomyPoints;
            decimal inboundDelta = inboundSelectedCabinPoints - inboundEconomyPoints;
            if (outboundDelta < 0M || inboundDelta < 0M || outboundDelta + inboundDelta <= 0M)
            {
                message = $"Selected sectors must not fall below Economy and at least one must have a positive upgrade delta. Outbound: {outboundSelectedCabinPoints} - {outboundEconomyPoints} = {outboundDelta}. Inbound: {inboundSelectedCabinPoints} - {inboundEconomyPoints} = {inboundDelta}.";
                return false;
            }

            decimal totalSectorDelta = outboundDelta + inboundDelta;
            JToken voucherDetails = searchSummaryResponse.SelectToken("$.data.searchSummary.voucherDetails");
            JArray redemptionOptions = voucherDetails?.SelectToken("redemptionOptions") as JArray;
            JObject upgradeOption = redemptionOptions?
                .Children<JObject>()
                .Where(option => option.SelectToken("redemptionType")?.ToString().Is("UPGRADE") == true)
                .OrderByDescending(GetAppliedVoucherPoints)
                .FirstOrDefault();
            if (upgradeOption == null)
            {
                message = $"No Upgrade redemption option was returned for sector delta validation. voucherDetails: {voucherDetails}";
                return false;
            }

            if (!TryReadDecimal(upgradeOption, "totalPointsUsed", out decimal totalPointsUsed, out message))
                return false;

            decimal appliedVoucherPoints = GetAppliedVoucherPoints(upgradeOption);
            if (totalPointsUsed != appliedVoucherPoints)
            {
                message = $"Upgrade totalPointsUsed did not match the sum of applied voucher points. TotalPointsUsed: {totalPointsUsed}. AppliedVoucherPoints: {appliedVoucherPoints}. UpgradeOption: {upgradeOption}";
                return false;
            }

            List<string> appliedVoucherIds = ExtractActualVoucherValues(
                upgradeOption.SelectToken("appliedVouchers") as JArray ?? new JArray(),
                "voucherId");
            JArray availableVouchers = voucherDetails?.SelectToken("availableVouchers") as JArray ?? new JArray();
            decimal availableAppliedVoucherBalance = availableVouchers
                .Children<JObject>()
                .Where(voucher => appliedVoucherIds.Any(id => id.Is(voucher.SelectToken("voucherId")?.ToString())))
                .Sum(voucher => decimal.TryParse(
                    voucher.SelectToken("balancePoints")?.ToString(),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out decimal balance) ? balance : 0M);
            if (availableAppliedVoucherBalance <= 0M)
            {
                message = $"The balance for applied Upgrade vouchers could not be established. AppliedVoucherIds: {string.Join(", ", appliedVoucherIds)}. AvailableVouchers: {availableVouchers}";
                return false;
            }

            decimal expectedVoucherPoints = Math.Min(totalSectorDelta, availableAppliedVoucherBalance);
            if (totalPointsUsed != expectedVoucherPoints)
            {
                message = $"Upgrade voucher usage was not the sum of the selected-flight sector deltas capped by available voucher balance. OutboundDelta: {outboundDelta}. InboundDelta: {inboundDelta}. TotalSectorDelta: {totalSectorDelta}. AvailableVoucherBalance: {availableAppliedVoucherBalance}. ExpectedPointsUsed: {expectedVoucherPoints}. ActualPointsUsed: {totalPointsUsed}. UpgradeOption: {upgradeOption}";
                return false;
            }

            message = $"OutboundEconomyPoints: {outboundEconomyPoints}. OutboundSelectedCabinPoints: {outboundSelectedCabinPoints}. OutboundDelta: {outboundDelta}. InboundCumulativeEconomyPoints: {inboundEconomyPoints}. InboundCumulativeSelectedCabinPoints: {inboundSelectedCabinPoints}. InboundDelta: {inboundDelta}. TotalSectorDelta: {totalSectorDelta}. AvailableVoucherBalance: {availableAppliedVoucherBalance}. ExpectedPointsUsed: {expectedVoucherPoints}. ActualPointsUsed: {totalPointsUsed}.";
            return true;
        }

        public static bool ValidateUpgradeBaselinePointsAndVoucherCap(
            JObject searchSummaryResponse,
            out string message)
        {
            message = string.Empty;
            JToken searchSummary = searchSummaryResponse.SelectToken("$.data.searchSummary");
            JToken voucherDetails = searchSummary?.SelectToken("voucherDetails");
            JArray redemptionOptions = voucherDetails?.SelectToken("redemptionOptions") as JArray;
            JObject upgradeOption = redemptionOptions?
                .Children<JObject>()
                .Where(option => option.SelectToken("redemptionType")?.ToString().Is("UPGRADE") == true)
                .OrderByDescending(GetAppliedVoucherPoints)
                .FirstOrDefault();
            if (upgradeOption == null)
            {
                message = $"No Upgrade redemption option was returned for baseline validation. voucherDetails: {voucherDetails}";
                return false;
            }

            if (!TryReadDecimal(searchSummary, "price.awardPoints", out decimal selectedJourneyPoints, out message) ||
                !TryReadDecimal(upgradeOption, "baseLinePoints", out decimal baselinePoints, out message) ||
                !TryReadDecimal(upgradeOption, "totalPointsUsed", out decimal totalPointsUsed, out message))
            {
                return false;
            }

            decimal upgradePoints = selectedJourneyPoints - baselinePoints;
            if (upgradePoints <= 0M)
            {
                message = $"The selected journey did not produce a positive upgrade value from the GQL baseline. SelectedJourneyPoints: {selectedJourneyPoints}. BaselinePoints: {baselinePoints}. UpgradePoints: {upgradePoints}. UpgradeOption: {upgradeOption}";
                return false;
            }

            decimal appliedVoucherPoints = GetAppliedVoucherPoints(upgradeOption);
            if (totalPointsUsed != appliedVoucherPoints)
            {
                message = $"Upgrade totalPointsUsed did not match the sum of applied voucher points. TotalPointsUsed: {totalPointsUsed}. AppliedVoucherPoints: {appliedVoucherPoints}. UpgradeOption: {upgradeOption}";
                return false;
            }

            List<string> appliedVoucherIds = ExtractActualVoucherValues(
                upgradeOption.SelectToken("appliedVouchers") as JArray ?? new JArray(),
                "voucherId");
            JArray availableVouchers = voucherDetails?.SelectToken("availableVouchers") as JArray ?? new JArray();
            decimal availableAppliedVoucherBalance = availableVouchers
                .Children<JObject>()
                .Where(voucher => appliedVoucherIds.Any(id => id.Is(voucher.SelectToken("voucherId")?.ToString())))
                .Sum(voucher => decimal.TryParse(
                    voucher.SelectToken("balancePoints")?.ToString(),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out decimal balance) ? balance : 0M);
            if (availableAppliedVoucherBalance <= 0M)
            {
                message = $"The balance for applied Upgrade vouchers could not be established. AppliedVoucherIds: {string.Join(", ", appliedVoucherIds)}. AvailableVouchers: {availableVouchers}";
                return false;
            }

            decimal expectedVoucherPoints = Math.Min(upgradePoints, availableAppliedVoucherBalance);
            message = $"SelectedJourneyPoints: {selectedJourneyPoints}. BaselinePoints: {baselinePoints}. UpgradePoints: {upgradePoints}. AvailableVoucherBalance: {availableAppliedVoucherBalance}. ExpectedPointsUsed: {expectedVoucherPoints}. ActualPointsUsed: {totalPointsUsed}.";
            return totalPointsUsed == expectedVoucherPoints;
        }

        public static bool ValidateRewardVoucherDetailsOrAcceptedNoUpgradeOption(
            JObject searchSummaryResponse,
            string expectedVoucherPayload,
            int expectedPassengerCount,
            out string message,
            out bool acceptedNoUpgradeOption,
            bool validateVoucherAmountCalculations = false)
        {
            acceptedNoUpgradeOption = false;

            if (ValidateRewardVoucherDetails(searchSummaryResponse, expectedVoucherPayload, "UPGRADE", expectedPassengerCount, out message, validateVoucherAmountCalculations))
                return true;

            if (IsAcceptedNoUpgradeVoucherOption(searchSummaryResponse, expectedVoucherPayload, out string acceptedMessage))
            {
                message = acceptedMessage;
                acceptedNoUpgradeOption = true;
                return true;
            }

            return false;
        }

        public static bool ValidateRewardVoucherDetailsPersistedInBasket(JObject searchSummaryResponse, JObject retrieveBasketResponse, out string message)
        {
            message = string.Empty;

            JToken searchSummaryVoucherDetails = searchSummaryResponse.SelectToken("$.data.searchSummary.voucherDetails");
            if (searchSummaryVoucherDetails == null || searchSummaryVoucherDetails.Type == JTokenType.Null)
            {
                message = $"searchSummary.voucherDetails was not returned, so basket voucher persistence could not be validated. Response: {searchSummaryResponse}";
                return false;
            }

            JToken basketVoucherDetails = SelectFirstToken(
                retrieveBasketResponse,
                "$.data.retrieveBasket.voucherDetails",
                "$.data.retrieveBasket.summary.voucherDetails",
                "$.data.retrieveBasket.flightSummary.voucherDetails");

            if (basketVoucherDetails == null || basketVoucherDetails.Type == JTokenType.Null)
            {
                message = $"Voucher details were not returned in retrieveBasket after searchSummary. RetrieveBasketResponse: {retrieveBasketResponse}";
                return false;
            }

            if (!ValidateVoucherBestOptionPersisted(searchSummaryVoucherDetails, basketVoucherDetails, out message))
            {
                return false;
            }

            if (!ValidateAvailableVouchersPersisted(searchSummaryVoucherDetails, basketVoucherDetails, out message))
            {
                return false;
            }

            if (!ValidateRedemptionOptionsPersisted(searchSummaryVoucherDetails, basketVoucherDetails, out message))
            {
                return false;
            }

            return true;
        }

        private static bool IsAcceptedNoUpgradeVoucherOption(JObject searchSummaryResponse, string expectedVoucherPayload, out string message)
        {
            message = string.Empty;

            if (!TryParseExpectedVoucherPayload(expectedVoucherPayload, out JArray expectedVouchers, out message))
                return false;

            List<string> expectedVoucherIds = ExtractExpectedVoucherValues(expectedVouchers, "voucherNumber", "voucherId", "number", "id");
            List<string> expectedVoucherTypes = ExtractExpectedVoucherValues(expectedVouchers, "type", "category");
            JToken voucherDetails = searchSummaryResponse.SelectToken("$.data.searchSummary.voucherDetails");
            if (voucherDetails == null || voucherDetails.Type == JTokenType.Null)
            {
                message = $"voucherDetails was not returned, so no-upgrade voucher outcome could not be accepted. Response: {searchSummaryResponse}";
                return false;
            }

            JArray availableVouchers = voucherDetails.SelectToken("availableVouchers") as JArray;
            if (availableVouchers == null || !availableVouchers.Any())
            {
                message = $"voucherDetails.availableVouchers was not returned or was empty, so no-upgrade voucher outcome could not be accepted. voucherDetails: {voucherDetails}";
                return false;
            }

            if (!ExpectedVouchersAreAvailable(availableVouchers, expectedVoucherIds, expectedVoucherTypes, out message))
            {
                message = $"{message} voucherDetails: {voucherDetails}";
                return false;
            }

            JArray redemptionOptions = voucherDetails.SelectToken("redemptionOptions") as JArray;
            JToken bestOption = voucherDetails.SelectToken("bestOption");
            bool hasNoRedemptionOptions = redemptionOptions == null || !redemptionOptions.Any();
            bool hasNoBestOption = bestOption == null || bestOption.Type == JTokenType.Null;

            if (!hasNoRedemptionOptions || !hasNoBestOption)
            {
                message = $"No-upgrade voucher outcome was not accepted because redemptionOptions or bestOption were returned. voucherDetails: {voucherDetails}";
                return false;
            }

            message = $"Accepted no upgrade redemption option because the member voucher was available but the selected premium fare did not produce an eligible upgrade saving against the economy baseline. voucherDetails: {voucherDetails}";
            return true;
        }

        private static bool TryParseExpectedVoucherPayload(string expectedVoucherPayload, out JArray expectedVouchers, out string message)
        {
            expectedVouchers = new JArray();
            message = string.Empty;

            if (expectedVoucherPayload.IsNullOrWhiteSpace())
            {
                message = "Expected voucher payload from storage table was empty.";
                return false;
            }

            try
            {
                expectedVouchers = JArray.Parse(expectedVoucherPayload);
                return true;
            }
            catch (JsonException ex)
            {
                message = $"Expected voucher payload from storage table was not valid JSON. Error: {ex.Message}. Payload: {expectedVoucherPayload}";
                return false;
            }
        }

        private static bool ExpectedVouchersAreAvailable(JArray availableVouchers, List<string> expectedVoucherIds, List<string> expectedVoucherTypes, out string message)
        {
            message = string.Empty;
            List<string> actualVoucherIds = ExtractActualVoucherValues(availableVouchers, "voucherId", "voucherNumber", "number", "id");
            List<string> actualVoucherTypes = ExtractActualVoucherValues(availableVouchers, "type", "category");

            if (expectedVoucherIds.Any())
            {
                List<string> missingVoucherIds = expectedVoucherIds
                    .Where(x => !actualVoucherIds.Any(y => y.Is(x)))
                    .ToList();

                if (missingVoucherIds.Any())
                {
                    message = $"availableVouchers did not include all storage-table voucher IDs. MissingIds: {string.Join(", ", missingVoucherIds)}. ExpectedIds: {string.Join(", ", expectedVoucherIds)}. ActualIds: {string.Join(", ", actualVoucherIds)}.";
                    return false;
                }

                return true;
            }

            if (expectedVoucherTypes.Any())
            {
                List<string> missingVoucherTypes = expectedVoucherTypes
                    .Where(x => !actualVoucherTypes.Any(y => y.Is(x)))
                    .ToList();

                if (missingVoucherTypes.Any())
                {
                    message = $"availableVouchers did not include all storage-table voucher types. MissingTypes: {string.Join(", ", missingVoucherTypes)}. ExpectedTypes: {string.Join(", ", expectedVoucherTypes)}. ActualTypes: {string.Join(", ", actualVoucherTypes)}.";
                    return false;
                }

                return true;
            }

            message = "Expected voucher payload did not include IDs or types to compare against availableVouchers.";
            return false;
        }

        public static bool ValidateRewardVoucherDetailsNotPersistedInBasket(JObject retrieveBasketResponse, out string message)
        {
            message = string.Empty;

            JToken basketVoucherDetails = SelectFirstToken(
                retrieveBasketResponse,
                "$.data.retrieveBasket.voucherDetails",
                "$.data.retrieveBasket.summary.voucherDetails",
                "$.data.retrieveBasket.flightSummary.voucherDetails");

            if (basketVoucherDetails == null || basketVoucherDetails.Type == JTokenType.Null)
            {
                return true;
            }

            JArray availableVouchers = basketVoucherDetails.SelectToken("availableVouchers") as JArray;
            JArray redemptionOptions = basketVoucherDetails.SelectToken("redemptionOptions") as JArray;
            JToken bestOption = basketVoucherDetails.SelectToken("bestOption");

            bool hasVoucherDetails = (availableVouchers != null && availableVouchers.Any())
                || (redemptionOptions != null && redemptionOptions.Any())
                || (bestOption != null && bestOption.Type != JTokenType.Null);

            if (hasVoucherDetails)
            {
                message = $"Voucher details were persisted in retrieveBasket for an account without voucher rows. BasketVoucherDetails: {basketVoucherDetails}";
                return false;
            }

            return true;
        }

        public static bool ValidateRewardVoucherRedemptionTypeNotReturned(JObject searchSummaryResponse, string redemptionType, out string message)
        {
            message = string.Empty;

            JToken searchSummary = searchSummaryResponse.SelectToken("$.data.searchSummary");
            if (searchSummary == null)
            {
                message = $"searchSummary was not returned in response. Response: {searchSummaryResponse}";
                return false;
            }

            JArray redemptionOptions = searchSummary.SelectToken("voucherDetails.redemptionOptions") as JArray;
            if (redemptionOptions == null || !redemptionOptions.Any())
            {
                return true;
            }

            List<string> matchingOptionIds = redemptionOptions
                .Children<JObject>()
                .Where(x => x.SelectToken("redemptionType")?.ToString().Is(redemptionType) == true)
                .Select(x => x.SelectToken("optionId")?.ToString())
                .Where(x => !x.IsNullOrWhiteSpace())
                .ToList();

            if (matchingOptionIds.Any())
            {
                message = $"Redemption type '{redemptionType}' was returned but was not expected. MatchingOptionIds: {string.Join(", ", matchingOptionIds)}. Response: {searchSummaryResponse}";
                return false;
            }

            return true;
        }

        public static bool TrySelectRewardVoucherOption(JObject searchSummaryResponse, out JObject selectedOption, out List<string> selectedVoucherIds, out string message)
        {
            selectedOption = new JObject();
            selectedVoucherIds = new List<string>();
            message = string.Empty;

            JToken voucherDetails = searchSummaryResponse.SelectToken("$.data.searchSummary.voucherDetails");
            if (voucherDetails == null || voucherDetails.Type == JTokenType.Null)
            {
                message = $"voucherDetails was not returned in searchSummary. Response: {searchSummaryResponse}";
                return false;
            }

            string bestOptionId = voucherDetails.SelectToken("bestOption.optionId")?.ToString() ?? string.Empty;
            JArray redemptionOptions = voucherDetails.SelectToken("redemptionOptions") as JArray;
            if (bestOptionId.IsNullOrWhiteSpace() || redemptionOptions == null || redemptionOptions.Count == 0)
            {
                message = $"At least one redemption option and a bestOption are required to validate explicit voucher selection. voucherDetails: {voucherDetails}";
                return false;
            }

            List<JObject> optionObjects = redemptionOptions.Children<JObject>().ToList();
            selectedOption = optionObjects
                .Where(x => x.SelectToken("optionId")?.ToString().Is(bestOptionId) != true)
                .OrderByDescending(GetRedemptionOptionPointsToPay)
                .FirstOrDefault()
                ?? optionObjects.FirstOrDefault(x => x.SelectToken("optionId")?.ToString().Is(bestOptionId) == true);

            if (selectedOption == null)
            {
                message = $"No selectable redemption option matched the returned bestOption. BestOptionId: {bestOptionId}. voucherDetails: {voucherDetails}";
                return false;
            }

            selectedVoucherIds = ExtractActualVoucherValues(selectedOption.SelectToken("appliedVouchers") as JArray ?? new JArray(), "voucherId");
            if (!selectedVoucherIds.Any())
            {
                message = $"Selected redemption option did not include applied voucher IDs. SelectedOption: {selectedOption}";
                return false;
            }

            return true;
        }

        public static bool ValidateCompanionVoucherPassengerComposition(
            JObject searchSummaryResponse,
            int expectedAppliedVoucherCount,
            int expectedCoveredPassengerCount,
            bool requireRemainingPoints,
            out string message)
        {
            message = string.Empty;
            JArray redemptionOptions = searchSummaryResponse
                .SelectToken("$.data.searchSummary.voucherDetails.redemptionOptions") as JArray;
            List<JObject> companionOptions = redemptionOptions?
                .Children<JObject>()
                .Where(x => x.SelectToken("redemptionType")?.ToString().Is("COMPANION") == true)
                .ToList() ?? new List<JObject>();

            if (!companionOptions.Any())
            {
                message = $"No COMPANION redemption option was returned. Response: {searchSummaryResponse}";
                return false;
            }

            JObject matchingOption = companionOptions.FirstOrDefault(option =>
                (option.SelectToken("appliedVouchers") as JArray)?.Count == expectedAppliedVoucherCount);
            if (matchingOption == null)
            {
                string actualCounts = string.Join(", ", companionOptions.Select(option =>
                    ((option.SelectToken("appliedVouchers") as JArray)?.Count ?? 0).ToString(CultureInfo.InvariantCulture)));
                message = $"No COMPANION option applied exactly {expectedAppliedVoucherCount} voucher(s). Actual applied-voucher counts: {actualCounts}. Options: {redemptionOptions}";
                return false;
            }

            int actualCoveredPassengerCount = GetCoveredPassengerCount(matchingOption.SelectToken("coveredPassengers"));
            if (actualCoveredPassengerCount != expectedCoveredPassengerCount)
            {
                message = $"The COMPANION option covered {actualCoveredPassengerCount} passenger(s); expected {expectedCoveredPassengerCount}. Option: {matchingOption}";
                return false;
            }

            if (requireRemainingPoints)
            {
                JToken searchSummary = searchSummaryResponse.SelectToken("$.data.searchSummary");
                if (!TryReadDecimal(searchSummary, "price.awardPoints", out decimal originalJourneyPoints, out message) ||
                    !TryReadDecimal(matchingOption, "totalPointsUsed", out decimal totalPointsUsed, out message) ||
                    !TryReadDecimal(matchingOption, "pointsToPay", out decimal pointsToPay, out message))
                    return false;

                decimal expectedPointsToPay = originalJourneyPoints - totalPointsUsed;
                if (pointsToPay <= 0M || pointsToPay != expectedPointsToPay)
                {
                    message = $"The non-companion passenger points were not retained. Expected pointsToPay {expectedPointsToPay} from original journey awardPoints {originalJourneyPoints} minus totalPointsUsed {totalPointsUsed}, but found {pointsToPay}. Option: {matchingOption}";
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateCompanionVoucherCountIsCapped(
            JObject searchSummaryResponse,
            int maximumAppliedVoucherCount,
            out string message)
        {
            message = string.Empty;
            List<JObject> companionOptions = (searchSummaryResponse
                    .SelectToken("$.data.searchSummary.voucherDetails.redemptionOptions") as JArray)?
                .Children<JObject>()
                .Where(x => x.SelectToken("redemptionType")?.ToString().Is("COMPANION") == true)
                .ToList() ?? new List<JObject>();

            if (!companionOptions.Any())
            {
                message = $"No COMPANION redemption option was returned. Response: {searchSummaryResponse}";
                return false;
            }

            int actualMaximum = companionOptions.Max(option =>
                (option.SelectToken("appliedVouchers") as JArray)?.Count ?? 0);
            if (actualMaximum != maximumAppliedVoucherCount)
            {
                message = $"The maximum COMPANION voucher count was {actualMaximum}; expected exactly {maximumAppliedVoucherCount}. Options: {string.Join(", ", companionOptions)}";
                return false;
            }

            return true;
        }

        public static bool ValidateRewardVoucherSelectionMutationResponse(JObject selectionResponse, JObject selectedOption, List<string> selectedVoucherIds, out string message)
        {
            message = string.Empty;

            JToken selection = selectionResponse.SelectToken("$.data.setVoucherRedemptionSelection.voucherRedemptionSelection");
            if (selection == null || selection.Type == JTokenType.Null)
            {
                message = $"setVoucherRedemptionSelection did not return voucherRedemptionSelection. Response: {selectionResponse}";
                return false;
            }

            string expectedOptionId = selectedOption.SelectToken("optionId")?.ToString() ?? string.Empty;
            string actualOptionId = selection.SelectToken("selectedOptionId")?.ToString() ?? string.Empty;
            if (expectedOptionId.IsNullOrWhiteSpace() || !expectedOptionId.Is(actualOptionId))
            {
                message = $"Selected option id was not persisted by setVoucherRedemptionSelection. ExpectedOptionId: {expectedOptionId}. ActualOptionId: {actualOptionId}. Selection: {selection}";
                return false;
            }

            List<string> actualVoucherIds = ExtractStringArrayValues(selection.SelectToken("selectedVoucherIds") as JArray ?? new JArray());
            List<string> missingVoucherIds = selectedVoucherIds
                .Where(x => !actualVoucherIds.Any(y => y.Is(x)))
                .ToList();

            if (missingVoucherIds.Any())
            {
                message = $"Selected voucher ids were not persisted by setVoucherRedemptionSelection. MissingIds: {string.Join(", ", missingVoucherIds)}. ExpectedIds: {string.Join(", ", selectedVoucherIds)}. ActualIds: {string.Join(", ", actualVoucherIds)}. Selection: {selection}";
                return false;
            }

            return true;
        }

        public static bool ValidateRewardVoucherDeselectionMutationResponse(JObject deselectionResponse, out string message)
        {
            message = string.Empty;

            JToken selection = deselectionResponse.SelectToken("$.data.setVoucherRedemptionSelection.voucherRedemptionSelection");
            if (selection == null || selection.Type == JTokenType.Null)
            {
                message = $"setVoucherRedemptionSelection did not return voucherRedemptionSelection after deselecting vouchers. Response: {deselectionResponse}";
                return false;
            }

            if (!ValidateVoucherSelectionCleared(selection, "setVoucherRedemptionSelection", out message))
                return false;

            JToken voucherApplied = deselectionResponse.SelectToken("$.data.setVoucherRedemptionSelection.voucherApplied");
            if (TokenIsTrue(voucherApplied))
            {
                message = $"voucherApplied was true after deselecting vouchers. Response: {deselectionResponse}";
                return false;
            }

            return true;
        }

        public static bool ValidateMixedCabinSelection(
            JObject searchSummaryResponse,
            FareFamilyType outboundFareFamily,
            FareFamilyType inboundFareFamily,
            out string message)
        {
            List<string> actualFareFamilies = searchSummaryResponse
                .SelectTokens("$.data.searchSummary.flights[*].fareFamilyType")
                .Select(token => token.ToString())
                .ToList();
            string expectedOutbound = outboundFareFamily.GetEnumDescription();
            string expectedInbound = inboundFareFamily.GetEnumDescription();
            message = $"Expected outbound/inbound fare families: {expectedOutbound}, {expectedInbound}. Actual: {string.Join(", ", actualFareFamilies)}.";
            return actualFareFamilies.Count >= 2 &&
                actualFareFamilies[0].Is(expectedOutbound) &&
                actualFareFamilies[^1].Is(expectedInbound);
        }

        public static bool ValidateRewardVoucherSelectionPersistedInPaymentBasket(
            JObject selectionResponse,
            JObject retrieveBasketResponse,
            out string message)
        {
            message = string.Empty;
            JToken selectionResult = selectionResponse.SelectToken("$.data.setVoucherRedemptionSelection");
            JToken basketSummary = retrieveBasketResponse.SelectToken("$.data.retrieveBasket.summary");
            if (selectionResult == null || basketSummary == null)
            {
                message = $"Selection result or payment basket summary was missing. Selection: {selectionResponse}. Basket: {retrieveBasketResponse}";
                return false;
            }

            if (!TryReadDecimal(selectionResult, "voucherPoints", out decimal selectedVoucherPoints, out message) ||
                !TryReadDecimal(selectionResult, "pointsToPay", out decimal selectedPointsToPay, out message) ||
                !TryReadDecimal(basketSummary, "membershipPoints", out decimal basketMembershipPoints, out message))
            {
                return false;
            }

            decimal expectedMembershipPoints = selectedVoucherPoints + selectedPointsToPay;
            if (basketMembershipPoints != expectedMembershipPoints)
            {
                message = $"Payment basket membershipPoints did not preserve the original journey points. Expected {expectedMembershipPoints} from voucherPoints {selectedVoucherPoints} plus pointsToPay {selectedPointsToPay}, but found {basketMembershipPoints}. Selection: {selectionResult}. BasketSummary: {basketSummary}";
                return false;
            }

            List<(string SelectionPath, string BasketPath)> comparisons = new List<(string SelectionPath, string BasketPath)>
            {
                ("voucherPoints", "voucherPoints"),
                ("pointsToPay", "pointsToPay"),
                ("hasSufficientPoints", "hasSufficientPoints"),
                ("voucherApplied", "voucherApplied")
            };
            foreach ((string selectionPath, string basketPath) in comparisons)
            {
                string expected = selectionResult.SelectToken(selectionPath)?.ToString() ?? string.Empty;
                string actual = basketSummary.SelectToken(basketPath)?.ToString() ?? string.Empty;
                if (expected.IsNullOrWhiteSpace() || !expected.Is(actual))
                {
                    message = $"Payment basket did not retain '{basketPath}' from voucher selection. Expected: '{expected}'. Actual: '{actual}'. Selection: {selectionResult}. BasketSummary: {basketSummary}";
                    return false;
                }
            }

            JToken selected = selectionResult.SelectToken("voucherRedemptionSelection");
            JToken persisted = basketSummary.SelectToken("voucherRedemptionSelection");
            if (!JToken.DeepEquals(selected, persisted))
            {
                message = $"Payment basket did not retain the voucher selection. Expected: {selected}. Actual: {persisted}.";
                return false;
            }

            return true;
        }

        public static bool ValidateRewardVoucherDeselectionPersistedInBasket(JObject retrieveBasketResponse, out string message)
        {
            message = string.Empty;

            JToken selection = SelectFirstToken(
                retrieveBasketResponse,
                "$.data.retrieveBasket.voucherRedemptionSelection",
                "$.data.retrieveBasket.summary.voucherRedemptionSelection",
                "$.data.retrieveBasket.flightSummary.voucherRedemptionSelection");

            if (selection != null && !ValidateVoucherSelectionCleared(selection, "retrieveBasket", out message))
                return false;

            JToken voucherApplied = SelectFirstToken(
                retrieveBasketResponse,
                "$.data.retrieveBasket.voucherApplied",
                "$.data.retrieveBasket.summary.voucherApplied",
                "$.data.retrieveBasket.flightSummary.voucherApplied");

            if (TokenIsTrue(voucherApplied))
            {
                message = $"voucherApplied was true in retrieveBasket after deselecting vouchers. RetrieveBasketResponse: {retrieveBasketResponse}";
                return false;
            }

            return true;
        }

        public static bool ValidateRewardVoucherPaymentAvailabilitySignals(
            JObject summaryOrSelectionResponse,
            JObject retrieveBasketResponse,
            bool expectedHasSufficientPoints,
            out string message)
        {
            message = string.Empty;
            JToken responseSummary = SelectFirstToken(
                summaryOrSelectionResponse,
                "$.data.searchSummary",
                "$.data.setVoucherRedemptionSelection");
            JToken basketSummary = retrieveBasketResponse.SelectToken("$.data.retrieveBasket.summary");
            if (responseSummary == null || basketSummary == null)
            {
                message = $"Summary/selection result or payment basket summary was missing. Response: {summaryOrSelectionResponse}. Basket: {retrieveBasketResponse}";
                return false;
            }

            JToken responseSufficientPoints = responseSummary.SelectToken("hasSufficientPoints");
            JToken basketSufficientPoints = basketSummary.SelectToken("hasSufficientPoints");
            if (responseSufficientPoints?.Type != JTokenType.Boolean || basketSufficientPoints?.Type != JTokenType.Boolean)
            {
                message = $"hasSufficientPoints was not returned as a Boolean by both the response and payment basket. ResponseValue: {responseSufficientPoints}. BasketValue: {basketSufficientPoints}.";
                return false;
            }

            bool responseIsSufficient = responseSufficientPoints.Value<bool>();
            bool basketIsSufficient = basketSufficientPoints.Value<bool>();
            if (responseIsSufficient != expectedHasSufficientPoints || basketIsSufficient != expectedHasSufficientPoints)
            {
                message = $"Payment availability state was inconsistent. Expected hasSufficientPoints: {expectedHasSufficientPoints}. Response: {responseIsSufficient}. Basket: {basketIsSufficient}.";
                return false;
            }

            string responsePointsPath = summaryOrSelectionResponse.SelectToken("$.data.setVoucherRedemptionSelection") != null
                ? "pointsToPay"
                : "price.pointsToPay";
            if (!TryReadDecimal(responseSummary, responsePointsPath, out decimal responsePointsToPay, out message))
            {
                return false;
            }

            JToken basketPointsToPayToken = basketSummary.SelectToken("pointsToPay");
            if ((basketPointsToPayToken == null || basketPointsToPayToken.Type == JTokenType.Null) &&
                !expectedHasSufficientPoints &&
                basketSummary.SelectToken("voucherRedemptionSelection")?.Type is null or JTokenType.Null)
            {
                return responsePointsToPay > 0M;
            }

            if (!TryReadDecimal(basketSummary, "pointsToPay", out decimal basketPointsToPay, out message))
                return false;

            if (responsePointsToPay < 0 || basketPointsToPay < 0 || responsePointsToPay != basketPointsToPay)
            {
                message = $"Required points were not consistently returned for the payment decision. Response pointsToPay: {responsePointsToPay}. Basket pointsToPay: {basketPointsToPay}.";
                return false;
            }

            return true;
        }

        public static bool ValidateSelectedRewardVoucherOptionAmounts(JObject searchSummaryResponse, JObject selectedOption, string availableAwardPoints, out string message)
        {
            message = string.Empty;

            JToken searchSummary = searchSummaryResponse.SelectToken("$.data.searchSummary");
            if (searchSummary == null || searchSummary.Type == JTokenType.Null)
            {
                message = $"searchSummary was not returned after voucher selection. Response: {searchSummaryResponse}";
                return false;
            }

            if (!ValidateRewardVoucherAmountCalculations(searchSummary, selectedOption, out message))
            {
                return false;
            }

            if (!availableAwardPoints.IsNullOrWhiteSpace() &&
                decimal.TryParse(availableAwardPoints, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal availablePoints) &&
                TryReadDecimal(selectedOption, "pointsToPay", out decimal selectedPointsToPay, out message))
            {
                bool expectedHasSufficientPoints = selectedPointsToPay <= availablePoints;
                bool actualHasSufficientPoints = searchSummary.SelectToken("hasSufficientPoints")?.Value<bool>() ?? false;
                if (expectedHasSufficientPoints != actualHasSufficientPoints)
                {
                    message = $"searchSummary.hasSufficientPoints did not reflect the selected option pointsToPay. Expected: {expectedHasSufficientPoints}. Actual: {actualHasSufficientPoints}. AvailablePoints: {availablePoints}. SelectedOption: {selectedOption}";
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateRewardVoucherInsufficientPointsBuyPointsState(
            JObject searchSummaryResponse,
            string expectedVoucherPayload,
            string expectedRedemptionType,
            int expectedPassengerCount,
            string availableAwardPoints,
            out string message)
        {
            message = string.Empty;

            if (!ValidateRewardVoucherDetails(searchSummaryResponse, expectedVoucherPayload, expectedRedemptionType, expectedPassengerCount, out message))
            {
                if (message.Contains("voucherDetails.redemptionOptions was not returned or was empty", StringComparison.OrdinalIgnoreCase))
                {
                    message = $"No '{expectedRedemptionType}' redemption option was returned, so this run did not exercise the insufficient-points Buy Virgin Points rule. This can be a valid backend outcome for the selected fare/pricing, but it is invalid test data for this scenario. {message}";
                }

                return false;
            }

            if (!decimal.TryParse(availableAwardPoints, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal availablePoints))
            {
                message = $"Available member points '{availableAwardPoints}' was not numeric.";
                return false;
            }

            JToken searchSummary = searchSummaryResponse.SelectToken("$.data.searchSummary");
            if (searchSummary == null || searchSummary.Type == JTokenType.Null)
            {
                message = $"searchSummary was not returned in response. Response: {searchSummaryResponse}";
                return false;
            }

            JToken voucherDetails = searchSummary.SelectToken("voucherDetails");
            JArray redemptionOptions = voucherDetails?.SelectToken("redemptionOptions") as JArray;
            JObject expectedRedemptionOption = redemptionOptions?
                .Children<JObject>()
                .Where(x => x.SelectToken("redemptionType")?.ToString().Is(expectedRedemptionType) == true)
                .OrderByDescending(GetRedemptionOptionPointsToPay)
                .FirstOrDefault();

            if (expectedRedemptionOption == null)
            {
                message = $"Expected redemption type '{expectedRedemptionType}' did not match a redemption option. voucherDetails: {voucherDetails}";
                return false;
            }

            JToken bestOption = voucherDetails?.SelectToken("bestOption");
            string bestOptionId = bestOption?.SelectToken("optionId")?.ToString() ?? string.Empty;
            JObject bestRedemptionOption = redemptionOptions?
                .Children<JObject>()
                .FirstOrDefault(x => x.SelectToken("optionId")?.ToString().Is(bestOptionId) == true);

            if (bestRedemptionOption == null)
            {
                message = $"bestOption '{bestOptionId}' did not match a redemption option, so the Buy Virgin Points state could not be associated with a selected option. voucherDetails: {voucherDetails}";
                return false;
            }

            if (!TryReadDecimal(expectedRedemptionOption, "pointsToPay", out decimal expectedOptionPointsToPay, out message))
                return false;

            if (expectedOptionPointsToPay <= availablePoints)
            {
                message = $"Test data did not exercise insufficient points for '{expectedRedemptionType}'. pointsToPay: {expectedOptionPointsToPay}. AvailablePoints: {availablePoints}. RedemptionOption: {expectedRedemptionOption}";
                return false;
            }

            if (!TryReadDecimal(bestRedemptionOption, "pointsToPay", out decimal bestOptionPointsToPay, out message))
                return false;

            if (bestOptionPointsToPay <= availablePoints)
            {
                message = $"The requested '{expectedRedemptionType}' option required additional points, but bestOption did not, so the global Buy Virgin Points state should not be expected. BestOptionPointsToPay: {bestOptionPointsToPay}. AvailablePoints: {availablePoints}. ExpectedOption: {expectedRedemptionOption}. BestOption: {bestRedemptionOption}";
                return false;
            }

            bool hasSufficientPoints = searchSummary.SelectToken("hasSufficientPoints")?.Value<bool>() ?? true;
            if (hasSufficientPoints)
            {
                message = $"searchSummary.hasSufficientPoints was true even though the selected best option required more points than the member had, so Buy Virgin Points would not be enabled. BestOptionPointsToPay: {bestOptionPointsToPay}. AvailablePoints: {availablePoints}. ExpectedOption: {expectedRedemptionOption}. BestOption: {bestRedemptionOption}";
                return false;
            }

            return true;
        }

        private static bool ValidateCoveredPassengers(List<JObject> redemptionOptions, int expectedPassengerCount, out string message)
        {
            message = string.Empty;

            foreach (JObject redemptionOption in redemptionOptions)
            {
                int coveredPassengerCount = GetCoveredPassengerCount(redemptionOption.SelectToken("coveredPassengers"));
                if (coveredPassengerCount <= 0)
                {
                    message = $"Redemption option '{redemptionOption.SelectToken("optionId")}' did not cover any passengers.";
                    return false;
                }

                if (expectedPassengerCount > 0 && coveredPassengerCount > expectedPassengerCount)
                {
                    message = $"Redemption option '{redemptionOption.SelectToken("optionId")}' covered {coveredPassengerCount} passenger(s), which is more than the expected passenger count {expectedPassengerCount}.";
                    return false;
                }
            }

            return true;
        }

        private static int GetCoveredPassengerCount(JToken coveredPassengers)
        {
            if (coveredPassengers == null || coveredPassengers.Type == JTokenType.Null)
            {
                return 0;
            }

            if (coveredPassengers is JArray coveredPassengerArray)
            {
                return coveredPassengerArray.Count;
            }

            if (coveredPassengers.Type == JTokenType.Integer && int.TryParse(coveredPassengers.ToString(), out int integerCount))
            {
                return integerCount;
            }

            if (coveredPassengers.Type == JTokenType.String)
            {
                string coveredPassengerText = coveredPassengers.ToString();
                if (int.TryParse(coveredPassengerText, out int stringCount))
                {
                    return stringCount;
                }

                return coveredPassengerText
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Length;
            }

            return 0;
        }

        private static bool ValidateVoucherBestOptionPersisted(JToken searchSummaryVoucherDetails, JToken basketVoucherDetails, out string message)
        {
            message = string.Empty;

            JToken expectedBestOption = searchSummaryVoucherDetails.SelectToken("bestOption");
            JToken actualBestOption = basketVoucherDetails.SelectToken("bestOption");
            if (expectedBestOption == null || expectedBestOption.Type == JTokenType.Null)
            {
                message = $"searchSummary voucher bestOption was not returned. SearchSummaryVoucherDetails: {searchSummaryVoucherDetails}";
                return false;
            }

            if (actualBestOption == null || actualBestOption.Type == JTokenType.Null)
            {
                message = $"Basket voucher bestOption was not returned. BasketVoucherDetails: {basketVoucherDetails}";
                return false;
            }

            return ValidateMatchingScalarField(expectedBestOption, actualBestOption, "optionId", "basket voucher bestOption", out message) &&
                   ValidateMatchingScalarField(expectedBestOption, actualBestOption, "optionName", "basket voucher bestOption", out message, required: false) &&
                   ValidateMatchingScalarField(expectedBestOption, actualBestOption, "redemptionType", "basket voucher bestOption", out message, required: false) &&
                   ValidateMatchingScalarField(expectedBestOption, actualBestOption, "selectionReason", "basket voucher bestOption", out message, required: false);
        }

        private static bool ValidateAvailableVouchersPersisted(JToken searchSummaryVoucherDetails, JToken basketVoucherDetails, out string message)
        {
            message = string.Empty;

            JArray expectedAvailableVouchers = searchSummaryVoucherDetails.SelectToken("availableVouchers") as JArray;
            JArray actualAvailableVouchers = basketVoucherDetails.SelectToken("availableVouchers") as JArray;
            if (expectedAvailableVouchers == null || !expectedAvailableVouchers.Any())
            {
                message = $"searchSummary voucher availableVouchers were not returned. SearchSummaryVoucherDetails: {searchSummaryVoucherDetails}";
                return false;
            }

            if (actualAvailableVouchers == null || !actualAvailableVouchers.Any())
            {
                message = $"Basket voucher availableVouchers were not returned. BasketVoucherDetails: {basketVoucherDetails}";
                return false;
            }

            foreach (JObject expectedVoucher in expectedAvailableVouchers.Children<JObject>())
            {
                string voucherId = expectedVoucher.SelectToken("voucherId")?.ToString() ?? string.Empty;
                JObject actualVoucher = FindObjectByField(actualAvailableVouchers, "voucherId", voucherId);
                if (actualVoucher == null)
                {
                    List<string> actualVoucherIds = ExtractActualVoucherValues(actualAvailableVouchers, "voucherId");
                    message = $"Basket voucher availableVouchers did not include voucherId '{voucherId}'. ActualIds: {string.Join(", ", actualVoucherIds)}. BasketVoucherDetails: {basketVoucherDetails}";
                    return false;
                }

                if (!ValidateMatchingScalarField(expectedVoucher, actualVoucher, "balancePoints", $"availableVoucher '{voucherId}'", out message) ||
                    !ValidateMatchingScalarField(expectedVoucher, actualVoucher, "expiryDate", $"availableVoucher '{voucherId}'", out message))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ValidateRedemptionOptionsPersisted(JToken searchSummaryVoucherDetails, JToken basketVoucherDetails, out string message)
        {
            message = string.Empty;

            JArray expectedRedemptionOptions = searchSummaryVoucherDetails.SelectToken("redemptionOptions") as JArray;
            JArray actualRedemptionOptions = basketVoucherDetails.SelectToken("redemptionOptions") as JArray;
            if (expectedRedemptionOptions == null || !expectedRedemptionOptions.Any())
            {
                message = $"searchSummary voucher redemptionOptions were not returned. SearchSummaryVoucherDetails: {searchSummaryVoucherDetails}";
                return false;
            }

            if (actualRedemptionOptions == null || !actualRedemptionOptions.Any())
            {
                message = $"Basket voucher redemptionOptions were not returned. BasketVoucherDetails: {basketVoucherDetails}";
                return false;
            }

            foreach (JObject expectedOption in expectedRedemptionOptions.Children<JObject>())
            {
                string optionId = expectedOption.SelectToken("optionId")?.ToString() ?? string.Empty;
                JObject actualOption = FindObjectByField(actualRedemptionOptions, "optionId", optionId);
                if (actualOption == null)
                {
                    List<string> actualOptionIds = ExtractActualVoucherValues(actualRedemptionOptions, "optionId");
                    message = $"Basket voucher redemptionOptions did not include optionId '{optionId}'. ActualOptionIds: {string.Join(", ", actualOptionIds)}. BasketVoucherDetails: {basketVoucherDetails}";
                    return false;
                }

                if (!ValidateMatchingScalarField(expectedOption, actualOption, "optionName", $"redemptionOption '{optionId}'", out message, required: false) ||
                    !ValidateMatchingScalarField(expectedOption, actualOption, "redemptionType", $"redemptionOption '{optionId}'", out message) ||
                    !ValidateMatchingScalarField(expectedOption, actualOption, "coveredPassengers", $"redemptionOption '{optionId}'", out message) ||
                    !ValidateMatchingScalarField(expectedOption, actualOption, "pointsToPay", $"redemptionOption '{optionId}'", out message) ||
                    !ValidateMatchingScalarField(expectedOption, actualOption, "totalPointsUsed", $"redemptionOption '{optionId}'", out message) ||
                    !ValidateMatchingScalarField(expectedOption, actualOption, "cashToPay", $"redemptionOption '{optionId}'", out message) ||
                    !ValidateAppliedVouchersPersisted(expectedOption, actualOption, optionId, out message))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ValidateAppliedVouchersPersisted(JToken expectedOption, JToken actualOption, string optionId, out string message)
        {
            message = string.Empty;

            JArray expectedAppliedVouchers = expectedOption.SelectToken("appliedVouchers") as JArray;
            JArray actualAppliedVouchers = actualOption.SelectToken("appliedVouchers") as JArray;
            if (expectedAppliedVouchers == null || !expectedAppliedVouchers.Any())
            {
                message = $"searchSummary redemptionOption '{optionId}' did not return appliedVouchers. ExpectedOption: {expectedOption}";
                return false;
            }

            if (actualAppliedVouchers == null || !actualAppliedVouchers.Any())
            {
                message = $"Basket redemptionOption '{optionId}' did not return appliedVouchers. ActualOption: {actualOption}";
                return false;
            }

            foreach (JObject expectedAppliedVoucher in expectedAppliedVouchers.Children<JObject>())
            {
                string voucherId = expectedAppliedVoucher.SelectToken("voucherId")?.ToString() ?? string.Empty;
                JObject actualAppliedVoucher = FindObjectByField(actualAppliedVouchers, "voucherId", voucherId);
                if (actualAppliedVoucher == null)
                {
                    List<string> actualVoucherIds = ExtractActualVoucherValues(actualAppliedVouchers, "voucherId");
                    message = $"Basket redemptionOption '{optionId}' appliedVouchers did not include voucherId '{voucherId}'. ActualVoucherIds: {string.Join(", ", actualVoucherIds)}. ActualOption: {actualOption}";
                    return false;
                }

                if (!ValidateMatchingScalarField(expectedAppliedVoucher, actualAppliedVoucher, "pointsUsed", $"redemptionOption '{optionId}' appliedVoucher '{voucherId}'", out message))
                {
                    return false;
                }
            }

            return true;
        }

        private static JObject FindObjectByField(JArray source, string fieldName, string expectedValue)
        {
            if (expectedValue.IsNullOrWhiteSpace())
            {
                return null;
            }

            return source
                .Children<JObject>()
                .FirstOrDefault(x => x.SelectToken(fieldName)?.ToString().Is(expectedValue) == true);
        }

        private static bool ValidateMatchingScalarField(JToken expectedSource, JToken actualSource, string fieldName, string context, out string message, bool required = true)
        {
            message = string.Empty;

            string expectedValue = expectedSource.SelectToken(fieldName)?.ToString() ?? string.Empty;
            string actualValue = actualSource.SelectToken(fieldName)?.ToString() ?? string.Empty;
            if (expectedValue.IsNullOrWhiteSpace() && !required)
            {
                return true;
            }

            if (expectedValue.IsNullOrWhiteSpace())
            {
                message = $"Expected field '{fieldName}' was not returned for {context}. ExpectedSource: {expectedSource}";
                return false;
            }

            if (decimal.TryParse(expectedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal expectedNumber) &&
                decimal.TryParse(actualValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal actualNumber))
            {
                if (expectedNumber == actualNumber)
                {
                    return true;
                }

                message = $"Basket numeric field '{fieldName}' did not match searchSummary for {context}. Expected: {expectedNumber}. Actual: {actualNumber}. ExpectedSource: {expectedSource}. ActualSource: {actualSource}";
                return false;
            }

            if (!expectedValue.Is(actualValue))
            {
                message = $"Basket field '{fieldName}' did not match searchSummary for {context}. Expected: {expectedValue}. Actual: {actualValue}. ExpectedSource: {expectedSource}. ActualSource: {actualSource}";
                return false;
            }

            return true;
        }

        private static bool ValidateUpgradeFlightsUseSingleCabin(JToken searchSummary, out string message)
        {
            message = string.Empty;

            JArray flights = searchSummary.SelectToken("flights") as JArray;
            if (flights == null || !flights.Any())
            {
                message = "Upgrade voucher search summary did not return flights, so same-cabin validation could not be performed.";
                return false;
            }

            List<JObject> flightObjects = flights.Children<JObject>().ToList();
            List<JObject> flightsMissingCabinDetails = flightObjects
                .Where(x => x.SelectToken("cabinName")?.ToString().IsNullOrWhiteSpace() == true ||
                            x.SelectToken("fareFamilyType")?.ToString().IsNullOrWhiteSpace() == true)
                .ToList();

            if (flightsMissingCabinDetails.Any())
            {
                message = $"Upgrade voucher search summary returned {flightsMissingCabinDetails.Count} flight(s) without cabinName or fareFamilyType.";
                return false;
            }

            List<string> cabinNames = flightObjects
                .Select(x => x.SelectToken("cabinName")?.ToString())
                .Where(x => !x.IsNullOrWhiteSpace())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            List<string> fareFamilyTypes = flightObjects
                .Select(x => x.SelectToken("fareFamilyType")?.ToString())
                .Where(x => !x.IsNullOrWhiteSpace())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (cabinNames.Count > 1 || fareFamilyTypes.Count > 1)
            {
                message = $"Upgrade voucher search summary returned mixed cabins or fare families. Cabins: {string.Join(", ", cabinNames)}. FareFamilyTypes: {string.Join(", ", fareFamilyTypes)}.";
                return false;
            }

            return true;
        }

        private static decimal GetAppliedVoucherPoints(JObject redemptionOption)
        {
            JArray appliedVouchers = redemptionOption.SelectToken("appliedVouchers") as JArray;
            if (appliedVouchers == null)
            {
                return 0M;
            }

            decimal totalPoints = 0M;
            foreach (JObject appliedVoucher in appliedVouchers.Children<JObject>())
            {
                JToken pointsUsed = appliedVoucher.SelectToken("pointsUsed");
                if (pointsUsed != null && decimal.TryParse(pointsUsed.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsedPoints))
                {
                    totalPoints += parsedPoints;
                }
            }

            return totalPoints;
        }

        private static int GetAppliedVoucherCount(JObject redemptionOption)
        {
            return (redemptionOption.SelectToken("appliedVouchers") as JArray)?.Count ?? 0;
        }

        private static decimal GetRedemptionOptionId(JObject redemptionOption)
        {
            JToken optionId = redemptionOption.SelectToken("optionId");
            return optionId != null && decimal.TryParse(optionId.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsedOptionId)
                ? parsedOptionId
                : decimal.MaxValue;
        }

        private static decimal GetRedemptionOptionPointsToPay(JObject redemptionOption)
        {
            return TryReadDecimal(redemptionOption, "pointsToPay", out decimal pointsToPay, out _)
                ? pointsToPay
                : 0M;
        }

        private static decimal GetRedemptionOptionCashToPay(JObject redemptionOption)
        {
            return TryReadDecimal(redemptionOption, "cashToPay", out decimal cashToPay, out _)
                ? cashToPay
                : 0M;
        }

        private static bool ValidateRewardVoucherAmountCalculations(JToken searchSummary, JObject bestRedemptionOption, out string message)
        {
            message = string.Empty;

            JToken voucherApplied = searchSummary.SelectToken("voucherApplied");
            if (voucherApplied == null || voucherApplied.Type == JTokenType.Null || !bool.TryParse(voucherApplied.ToString(), out bool isVoucherApplied) || !isVoucherApplied)
            {
                message = $"searchSummary.voucherApplied was not true for the selected voucher option. voucherApplied: {voucherApplied ?? "null"}";
                return false;
            }

            JToken price = searchSummary.SelectToken("price");
            if (price == null)
            {
                message = "searchSummary.price was not returned for voucher amount validation.";
                return false;
            }

            if (!TryReadDecimal(price, "awardPoints", out decimal awardPoints, out message) ||
                !TryReadDecimal(price, "voucherPoints", out decimal voucherPoints, out message) ||
                !TryReadDecimal(price, "pointsToPay", out decimal pricePointsToPay, out message) ||
                !TryReadDecimal(bestRedemptionOption, "totalPointsUsed", out decimal totalPointsUsed, out message) ||
                !TryReadDecimal(bestRedemptionOption, "pointsToPay", out decimal optionPointsToPay, out message))
            {
                return false;
            }

            decimal appliedVoucherPoints = GetAppliedVoucherPoints(bestRedemptionOption);
            if (totalPointsUsed != appliedVoucherPoints)
            {
                message = $"redemptionOptions.totalPointsUsed did not match the sum of appliedVouchers.pointsUsed. TotalPointsUsed: {totalPointsUsed}. AppliedVoucherPoints: {appliedVoucherPoints}. BestOption: {bestRedemptionOption}";
                return false;
            }

            decimal expectedVoucherPoints = Math.Min(totalPointsUsed, awardPoints);
            if (voucherPoints != expectedVoucherPoints)
            {
                message = $"searchSummary.price.voucherPoints did not match the selected voucher amount. ExpectedVoucherPoints: {expectedVoucherPoints}. ActualVoucherPoints: {voucherPoints}. AwardPoints: {awardPoints}. BestOption: {bestRedemptionOption}";
                return false;
            }

            if (pricePointsToPay != optionPointsToPay)
            {
                message = $"searchSummary.price.pointsToPay did not match selected redemptionOptions.pointsToPay. PricePointsToPay: {pricePointsToPay}. OptionPointsToPay: {optionPointsToPay}. BestOption: {bestRedemptionOption}";
                return false;
            }

            return true;
        }

        private static bool TryReadDecimal(JToken source, string path, out decimal value, out string message)
        {
            value = 0M;
            message = string.Empty;

            JToken token = source.SelectToken(path);
            if (token == null || token.Type == JTokenType.Null)
            {
                message = $"Expected numeric field '{path}' was not returned. Source: {source}";
                return false;
            }

            if (!decimal.TryParse(token.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            {
                message = $"Expected numeric field '{path}' could not be parsed. Value: {token}. Source: {source}";
                return false;
            }

            return true;
        }

        public static bool ValidateRewardVoucherDetailsNotReturned(JObject searchSummaryResponse, out string message)
        {
            JToken searchSummary = searchSummaryResponse.SelectToken("$.data.searchSummary");
            if (searchSummary == null)
            {
                message = $"searchSummary was not returned in response. Response: {searchSummaryResponse}";
                return false;
            }

            return ValidateRewardVoucherDetailsNotReturned(searchSummary.SelectToken("voucherDetails"), out message);
        }

        public static bool ValidateRewardVoucherDetailsNotReturnedFromBasket(JObject retrieveBasketResponse, out string message)
        {
            JToken basketSummary = retrieveBasketResponse.SelectToken("$.data.retrieveBasket.summary");
            if (basketSummary == null)
            {
                message = $"retrieveBasket.summary was not returned, so non-voucher continuation could not be validated. Response: {retrieveBasketResponse}";
                return false;
            }

            return ValidateRewardVoucherDetailsNotReturned(basketSummary.SelectToken("voucherDetails"), out message);
        }

        public static bool ValidateNonVoucherBasketContinuation(JObject retrieveBasketResponse, out string message)
        {
            return ValidateNonVoucherBasketContinuation(retrieveBasketResponse, string.Empty, out message);
        }

        public static bool ValidateNonVoucherBasketContinuation(JObject retrieveBasketResponse, string expectedBasketId, out string message)
        {
            message = string.Empty;
            JToken basket = retrieveBasketResponse.SelectToken("$.data.retrieveBasket");
            if (basket == null || basket.Type == JTokenType.Null)
            {
                message = $"retrieveBasket did not return a basket after the ineligible voucher itinerary continued. Response: {retrieveBasketResponse}";
                return false;
            }

            if (!expectedBasketId.IsNullOrWhiteSpace())
            {
                string actualBasketId = basket.SelectToken("basketId")?.ToString()
                    ?? basket.SelectToken("id")?.ToString()
                    ?? string.Empty;
                if (!actualBasketId.Is(expectedBasketId))
                {
                    message = $"The continued non-voucher basket id did not match the search basket. ExpectedBasketId: {expectedBasketId}. ActualBasketId: {actualBasketId}.";
                    return false;
                }
            }

            return true;
        }

        private static bool ValidateRewardVoucherDetailsNotReturned(JToken voucherDetails, out string message)
        {
            message = string.Empty;

            if (voucherDetails == null || voucherDetails.Type == JTokenType.Null)
            {
                return true;
            }

            JArray availableVouchers = voucherDetails.SelectToken("availableVouchers") as JArray;
            JArray redemptionOptions = voucherDetails.SelectToken("redemptionOptions") as JArray;
            JToken bestOption = voucherDetails.SelectToken("bestOption");

            bool hasVoucherDetails = (availableVouchers != null && availableVouchers.Any())
                || (redemptionOptions != null && redemptionOptions.Any())
                || (bestOption != null && bestOption.Type != JTokenType.Null);

            if (hasVoucherDetails)
            {
                message = $"Voucher redemption details were returned for an ineligible itinerary. voucherDetails: {voucherDetails}";
                return false;
            }

            return true;
        }

        private static List<string> ExtractExpectedVoucherValues(JArray expectedVouchers, params string[] fieldNames)
        {
            return expectedVouchers
                .Children<JObject>()
                .SelectMany(x => fieldNames.Select(fieldName => x.SelectToken(fieldName)?.ToString()))
                .Where(x => !x.IsNullOrWhiteSpace())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string GetVoucherValue(JObject voucher, params string[] fieldNames)
        {
            return fieldNames
                .Select(fieldName => voucher.GetValue(fieldName, StringComparison.OrdinalIgnoreCase)?.ToString())
                .FirstOrDefault(value => !value.IsNullOrWhiteSpace()) ?? string.Empty;
        }

        private static List<string> ExtractActualVoucherValues(JArray actualValues, params string[] fieldNames)
        {
            return actualValues
                .Children<JObject>()
                .SelectMany(x => fieldNames.Select(fieldName => x.SelectToken(fieldName)?.ToString()))
                .Where(x => !x.IsNullOrWhiteSpace())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<string> ExtractStringArrayValues(JArray actualValues)
        {
            return actualValues
                .Select(x => x?.ToString())
                .Where(x => !x.IsNullOrWhiteSpace())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static bool ValidateVoucherSelectionCleared(JToken selection, string sourceName, out string message)
        {
            message = string.Empty;

            List<string> selectedVoucherIds = ExtractStringArrayValues(selection.SelectToken("selectedVoucherIds") as JArray ?? new JArray());
            if (selectedVoucherIds.Any())
            {
                message = $"Voucher ids were still selected after deselecting vouchers. Source: {sourceName}. SelectedVoucherIds: {string.Join(", ", selectedVoucherIds)}. Selection: {selection}";
                return false;
            }

            JToken selectedOptionIdToken = selection.SelectToken("selectedOptionId");
            string selectedOptionId = selectedOptionIdToken?.ToString() ?? string.Empty;
            if (!selectedOptionId.IsNullOrWhiteSpace() && !selectedOptionId.Is("0"))
            {
                message = $"Voucher option id was still selected after deselecting vouchers. Source: {sourceName}. SelectedOptionId: {selectedOptionId}. Selection: {selection}";
                return false;
            }

            return true;
        }

        private static bool TokenIsTrue(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return false;

            if (token.Type == JTokenType.Boolean)
                return token.Value<bool>();

            return bool.TryParse(token.ToString(), out bool value) && value;
        }

        public static bool ValidateSummaryLinkInBasket(dynamic retrieveBasketResult, string summaryLink, out string message)
        {
            message = string.Empty;

            JToken basketToken = retrieveBasketResult as JToken ?? JToken.FromObject(retrieveBasketResult);
            string updatedSummaryLinkInBasket = ReadStringFromFirstPath(
                basketToken,
                "$.data.retrieveBasket.summary.summaryLink",
                "$.data.retrieveBasket.summary.link",
                "$.data.retrieveBasket.summary.url");

            if (updatedSummaryLinkInBasket.IsNullOrEmpty())
            {
                message = $"Summary link was not found in basket response. Response: {basketToken}";
                return false;
            }

            if (summaryLink != updatedSummaryLinkInBasket)
            {
                message = $"Updated summary link in basket: {updatedSummaryLinkInBasket} and the summary sent in request not matched: {summaryLink}";
                return false;
            }

            return true;
        }

        public static bool ValidateSummaryFCNumberInBasket(dynamic retrieveBasketResult, string fcNubmer, out string message)
        {
            message = string.Empty;

            JToken basketToken = retrieveBasketResult as JToken ?? JToken.FromObject(retrieveBasketResult);
            string updatedFCNumberInBasket = ReadStringFromFirstPath(
                basketToken,
                "$.data.retrieveBasket.customerProfile.membershipDetails.fqtNumber",
                "$.data.retrieveBasket.customerProfile.membershipDetails[0].fqtNumber",
                "$.data.retrieveBasket.customerProfile.membershipDetails.membershipNumber",
                "$.data.retrieveBasket.customerProfile.membershipDetails[0].membershipNumber",
                "$.data.retrieveBasket.customerProfile.membershipDetails");

            if (updatedFCNumberInBasket.IsNullOrEmpty())
            {
                message = $"FC membership number was not found in basket response. Response: {basketToken}";
                return false;
            }

            if (fcNubmer != updatedFCNumberInBasket)
            {
                message = $"The FCNumber in basket: {updatedFCNumberInBasket} and the FCNumber used for login not matched: {fcNubmer}";
                return false;
            }

            return true;
        }

        private static string ReadStringFromFirstPath(JToken source, params string[] jsonPaths)
        {
            foreach (string jsonPath in jsonPaths)
            {
                JToken token = source.SelectToken(jsonPath);
                if (token == null)
                {
                    continue;
                }

                if (token.Type != JTokenType.String &&
                    token.Type != JTokenType.Integer &&
                    token.Type != JTokenType.Float &&
                    token.Type != JTokenType.Boolean &&
                    token.Type != JTokenType.Date &&
                    token.Type != JTokenType.Guid &&
                    token.Type != JTokenType.Uri &&
                    token.Type != JTokenType.TimeSpan)
                {
                    continue;
                }

                string value = token.Value<string>() ?? token.ToString();
                if (!value.IsNullOrEmpty())
                {
                    return value;
                }
            }

            return string.Empty;
        }

        private static JToken SelectFirstToken(JToken source, params string[] jsonPaths)
        {
            foreach (string jsonPath in jsonPaths)
            {
                JToken token = source.SelectToken(jsonPath);
                if (token != null && token.Type != JTokenType.Null)
                {
                    return token;
                }
            }

            return null;
        }

        public static bool ValidateOriginAndDestinationInSummary(string offerSearchRequest, HttpResponseMessage offerSearchResponse, out string message)
        {
            message = string.Empty;

            // Deserialize the JSON data
            JToken flightSearchRequest = JsonConvert.DeserializeObject<JToken>(offerSearchRequest);

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Dictionary<string, List<string>> cityAirportCodes = OfferSearchHelper.GetCityAirportCodes();

            // Extract origins and destinations from the request and response
            string[] requestedOrigins = flightSearchRequest.SelectTokens("$..flightSearchRequest.searchOriginDestinations[*].origin")
                                                       .Select(x => x.ToString()).ToArray();
            string[] responseOrigins = offerSearchResult.SelectTokens("$..flights[*].origin.code")
                                                    .Select(x => x.ToString()).ToArray();

            string[] requestedDestinations = flightSearchRequest.SelectTokens("$..flightSearchRequest.searchOriginDestinations[*].destination")
                                                           .Select(x => x.ToString()).ToArray();
            string[] responseDestinations = offerSearchResult.SelectTokens("$..flights[*].destination.code")
                                                         .Select(x => x.ToString()).ToArray();

            // Validate origins
            if (!ValidateOrigins(requestedOrigins, responseOrigins, cityAirportCodes, out message))
            {
                return false;
            }

            // Validate destinations
            if (!ValidateDestinations(requestedDestinations, responseDestinations, cityAirportCodes, out message))
            {
                return false;
            }

            return true;
        }

        private static bool ValidateOrigins(string[] requestedOrigins, string[] responseOrigins, Dictionary<string, List<string>> cityAirportCodes, out string message)
        {
            if (requestedOrigins.Length != responseOrigins.Length)
            {
                message = $"Mismatch in origin count: requested {requestedOrigins.Length}, response {responseOrigins.Length}";
                return false;
            }

            foreach (string origin in requestedOrigins)
            {
                // Check if the origin is a city code that needs further validation
                if (cityAirportCodes.ContainsKey(origin))
                {
                    // If origin is a city code, check if any of its airport codes match the response origins
                    bool matchFound = false;

                    foreach (string code in cityAirportCodes[origin])
                    {
                        if (responseOrigins.Contains(code))
                        {
                            matchFound = true;
                            break;
                        }
                    }

                    if (!matchFound)
                    {
                        message = "Requested flight origin and flight origin from response do not match";
                        return false;
                    }
                }
                else
                {
                    // If origin is not a city code, check directly in the response origins
                    if (!responseOrigins.Contains(origin))
                    {
                        message = "Requested flight origin and flight origin from response do not match";
                        return false;
                    }
                }
            }

            message = string.Empty;
            return true;
        }

        private static bool ValidateDestinations(string[] requestedDestinations, string[] responseDestinations, Dictionary<string, List<string>> cityAirportCodes, out string message)
        {
            if (requestedDestinations.Length != responseDestinations.Length)
            {
                message = $"Mismatch in destination count: requested {requestedDestinations.Length}, response {responseDestinations.Length}";
                return false;
            }

            foreach (string destination in requestedDestinations)
            {
                // Check if the destination is a city code that needs further validation
                if (cityAirportCodes.ContainsKey(destination))
                {
                    // If destination is a city code, check if any of its airport codes match the response destination
                    bool matchFound = false;

                    foreach (string code in cityAirportCodes[destination])
                    {
                        if (responseDestinations.Contains(code))
                        {
                            matchFound = true;
                            break;
                        }
                    }

                    if (!matchFound)
                    {
                        message = "Requested flight destination and flight destination from response do not match";
                        return false;
                    }
                }
                else
                {
                    // If destination is not a city code, check directly in the response destinations
                    if (!responseDestinations.Contains(destination))
                    {
                        message = "Requested flight destination and flight destination from response do not match";
                        return false;
                    }
                }
            }

            message = string.Empty;
            return true;
        }

        public static bool ValidateTotalTaxAndPointsInSummary(JourneyType journeyType, HttpResponseMessage outboundResponse, HttpResponseMessage summaryResponse, string outboundOfferId, out string message, HttpResponseMessage inboundResponse = null, string inboundOfferId = "")
        {
            message = string.Empty;

            // Deserialize the JSON data
            JToken outboundResult = JsonConvert.DeserializeObject<JToken>(outboundResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            JToken summaryResult = JsonConvert.DeserializeObject<JToken>(summaryResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            double amountIncludingTax = outboundResult.SelectToken($"$..flightsAndFares[*].fares[?(@.id == '{outboundOfferId}')].price.amountIncludingTax").Value<double>();

            long awardPoints = outboundResult.SelectToken($"$..flightsAndFares[*].fares[?(@.id == '{outboundOfferId}')].price.awardPoints").Value<long>();

            double amountIncludingTaxSummary = summaryResult.SelectToken("$.data.searchSummary.price.amountIncludingTax").Value<double>();

            long totalAwardPointsSummary = summaryResult.SelectToken("$.data.searchSummary.price.awardPoints").Value<long>();

            if (journeyType == JourneyType.OneWay)
            {
                if (amountIncludingTax != amountIncludingTaxSummary && awardPoints != totalAwardPointsSummary)
                {
                    message = $"Total tax and total points for redemption not matched actual tax&points: {amountIncludingTax},{awardPoints} and expected tax&points: {amountIncludingTaxSummary},{totalAwardPointsSummary}";
                    return false;
                }
            }
            else
            {
                JToken inboundResult = JsonConvert.DeserializeObject<JToken>(inboundResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                double priceDifference = inboundResult.SelectToken($"$..flightsAndFares[*].fares[?(@.id == '{inboundOfferId}')].price.priceDifference").Value<double>();

                string priceDifferenceSign = inboundResult.SelectToken($"$..flightsAndFares[*].fares[?(@.id == '{inboundOfferId}')].price.priceDifferenceSign").ToString();

                long awardPointsDifference = inboundResult.SelectToken($"$..flightsAndFares[*].fares[?(@.id == '{inboundOfferId}')].price.awardPointsDifference").Value<long>();

                string awardPointsDifferenceSign = inboundResult.SelectToken($"$..flightsAndFares[*].fares[?(@.id == '{inboundOfferId}')].price.awardPointsDifferenceSign").ToString();

                double totalTax = priceDifferenceSign == "+" ? amountIncludingTax + priceDifference : amountIncludingTax - priceDifference;

                long totalAwardPoints = awardPointsDifferenceSign == "+" ? awardPoints + awardPointsDifference : awardPoints - awardPointsDifference;

                if (Math.Round(totalTax, 2) != amountIncludingTaxSummary && totalAwardPoints != totalAwardPointsSummary)
                {
                    message = $"Total tax and total points for redemption not matched actual tax&points: {totalTax},{totalAwardPoints} and expected tax&points: {amountIncludingTaxSummary},{totalAwardPointsSummary}";
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateTotalTaxAndAmountInSummary(JourneyType journeyType, HttpResponseMessage outboundResponse, HttpResponseMessage summaryResponse, string outboundOfferId, out string message, HttpResponseMessage inboundResponse = null, string inboundOfferId = "")
        {
            message = string.Empty;

            // Deserialize the JSON data
            JToken outboundResult = JsonConvert.DeserializeObject<JToken>(outboundResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            JToken summaryResult = JsonConvert.DeserializeObject<JToken>(summaryResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            double amountIncludingTax = outboundResult.SelectToken($"$..flightsAndFares[*].fares[?(@.id == '{outboundOfferId}')].price.amountIncludingTax").Value<double>();

            double totalTaxOutbound = outboundResult.SelectToken($"$..flightsAndFares[*].fares[?(@.id == '{outboundOfferId}')].price.tax").Value<double>();

            double amountIncludingTaxSummary = summaryResult.SelectToken("$.data.searchSummary.price.amountIncludingTax").Value<double>();

            long totalTaxInSummary = summaryResult.SelectToken("$.data.searchSummary.price.tax").Value<long>();

            if (journeyType == JourneyType.OneWay)
            {
                if (amountIncludingTax != amountIncludingTaxSummary && totalTaxOutbound != totalTaxInSummary)
                {
                    message = $"Total tax and total amount including tax not matched actual tax&amount: {totalTaxOutbound},{amountIncludingTax} and expected tax&amount: {amountIncludingTaxSummary},{totalTaxInSummary}";
                    return false;
                }
            }
            else
            {
                JToken inboundResult = JsonConvert.DeserializeObject<JToken>(inboundResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                double priceDifference = inboundResult.SelectToken($"$..flightsAndFares[*].fares[?(@.id == '{inboundOfferId}')].price.priceDifference").Value<double>();

                double totalTax = inboundResult.SelectToken($"$..flightsAndFares[*].fares[?(@.id == '{inboundOfferId}')].price.tax").Value<double>();

                string priceDifferenceSign = inboundResult.SelectToken($"$..flightsAndFares[*].fares[?(@.id == '{inboundOfferId}')].price.priceDifferenceSign").ToString();

                double totalAmountIncludingTax = priceDifferenceSign == "+" ? amountIncludingTax + priceDifference : amountIncludingTax - priceDifference;

                if (Math.Round(totalAmountIncludingTax, 2) != amountIncludingTaxSummary && totalTax != totalTaxInSummary)
                {
                    message = $"Total tax and total amount including tax not matched actual tax&amount: {totalTax},{amountIncludingTax} and expected tax&amount: {amountIncludingTaxSummary},{totalTaxInSummary}";
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateOfferIdExistsInSummaryResponse(dynamic offerPriceResult, out string message)
        {
            message = string.Empty;

            string offerId = offerPriceResult?.data?.searchSummary?.id?.Value;

            if (offerId.IsNullOrEmpty())
            {
                message = $"Offer not returned for summary {offerPriceResult}";
                return false;
            }
            return true;
        }

        public static bool ValidateInboundSearchResult(string offerSearchReturnRequest, dynamic offerSearchReturnResult, out string message)
        {
            message = string.Empty;

            JToken response = offerSearchReturnResult as JToken;
            JContainer searchResult = response?.SelectToken("data.searchOffersNextSlice.result") as JContainer;

            if (searchResult == null || searchResult.Count <= 0)
            {
                message = $"No result returned for the inbound search: {offerSearchReturnRequest} \n{offerSearchReturnResult}";
                return false;
            }

            return true;
        }

        public static bool ValidateSaverFaresExistForSummary(dynamic offerPriceResult, out string message)
        {
            message = string.Empty;

            bool isSaverFare = offerPriceResult?.data?.searchSummary?.isSaverFare;

            if (!isSaverFare)
            {
                message = $"Saver fare not exists for summary {offerPriceResult}";
                return false;
            }

            return true;
        }

        public static bool ValidatePromoCodeAppliedExistInSummaryResponse(dynamic offerPriceResult, string promoCode, out string message)
        {
            message = string.Empty;

            string promoCodeApplied = offerPriceResult?.data?.searchSummary?.promoCodeApplied;

            if (!promoCodeApplied.Equals(promoCode))
            {
                message = $"Promo code '{promoCode}' does not match the applied promo code '{promoCodeApplied}' in the summary result {offerPriceResult}.";

                return false;
            }

            return true;
        }

        public static bool ValidateSearchResult(dynamic offerSearchResult, string offerSearchGQLRequest, out string message)
        {
            message = string.Empty;

            // Extract the relevant data
            dynamic searchOffers = offerSearchResult?.data?.searchOffers;

            // Check if 'result' is null
            if (searchOffers?.result == null)
            {
                message = $"GraphQL search request {offerSearchGQLRequest} \nSearch result is invalid: {offerSearchResult}";

                return false;
            }
            return true;
        }

        public static bool ValidateEarnPoints(dynamic searchSummaryResult, string offerSearchGQLRequest, out string message, bool isAwardSearch = false)
        {
            message = string.Empty;

            // Extract earn points details
            dynamic tierPoints = searchSummaryResult?.data?.searchSummary?.earnPoints?.tierPoints?.Value;
            dynamic virginPoints = searchSummaryResult?.data?.searchSummary?.earnPoints?.virginPoints?.Value;

            // Validate points based on the search type
            if (isAwardSearch)
            {
                // In an award search, tier points should exist
                if (tierPoints == null && virginPoints != null)
                {
                    message = $"Error: Tier points are missing, but virgin points are present. Search result: {searchSummaryResult}, GraphQL request: {offerSearchGQLRequest}";
                    return false;
                }
            }
            else
            {
                // For non-award searches, both tier and virgin points must be present
                if (tierPoints == null || virginPoints == null)
                {
                    message = $"Error: Both tier points and virgin points are required. Search result: {searchSummaryResult}, GraphQL request: {offerSearchGQLRequest}";
                    return false;
                }
            }

            return true;
        }

        public static bool CheckIfErrorExistsInSummary(string graphqlSearchRequest, string graphqlSummaryRequest, HttpResponseMessage summaryResponse, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (summaryResponse == null)
            {
                errorMessage = $"Error: HTTP response is null for request: {graphqlSummaryRequest}.";
                return false;
            }

            try
            {
                // Parse the response content as JSON
                JToken jsonResponse = JsonConvert.DeserializeObject<JToken>(summaryResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                // Check if "searchSummary" exists in the response
                JToken searchSummary = jsonResponse.SelectToken("data.searchSummary");
                if (searchSummary == null)
                {
                    errorMessage = $"Error: 'searchSummary' is missing in the response for summary request: {graphqlSummaryRequest} \n search request: {graphqlSearchRequest} \n summary response: {jsonResponse}";
                    return false;
                }

                // If no error exists
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Exception occurred while processing the response for request: {graphqlSummaryRequest}. Error: {ex.Message}";
                return false;
            }
        }

        public static bool ValidatePendingGovtApprovalFlag(HttpResponseMessage httpResponse, out string message)
        {
            message = string.Empty;

            // Parse the response content as JSON
            JToken jsonResponse = JsonConvert.DeserializeObject<JToken>(httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            // Extract "pendingGovtApproval" flag value
            bool hasPendingGovtApprovalFlag = jsonResponse.SelectTokens("$..segments[*].pendingGovtApproval").Any();

            if (!hasPendingGovtApprovalFlag)
            {
                message = $"Pending govt approval flag value should be true {jsonResponse}";
                return false;
            }

            return true;
        }
    }
}
