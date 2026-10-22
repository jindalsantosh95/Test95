using AventStack.ExtentReports;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using static VAA.EAPI.Automation.Core.Config;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.WebBFF
{
    public class WebBFFAssertions
    {
        public static bool ValidatePtcsCount(string jsonRequest, dynamic basketResponse, out string message)
        {
            message = string.Empty;

            dynamic myRequest = JsonConvert.DeserializeObject<dynamic>(jsonRequest);

            var ptcCountRequest = myRequest.variables.request.customerDetails.Count;

            var ptcCountFromBasket = basketResponse.people.Count;

            if (ptcCountRequest != ptcCountFromBasket)
            {
                message = $"PTC count was not the same for Offer Search Request being {ptcCountRequest} and Basket Response being {ptcCountFromBasket}";
                return false;
            }

            return true;
        }

        public static bool ValidatePtcsType(string jsonRequest, dynamic basketResponse, out string message)
        {
            message = string.Empty;

            var ptcCodeMapping = new Dictionary<string, string>
            {
                { "ADT", "ADULT" },
                { "GBE", "TEEN" },
                { "CNN", "CHILD" },
                { "INF", "INFANT" }
            };

            dynamic myRequest = JsonConvert.DeserializeObject<dynamic>(jsonRequest);

            var ptcCountRequest = ((JArray)myRequest.variables.request.customerDetails).Select(x => ptcCodeMapping[x["ptc"].ToString()]).OrderBy(x => x);
            var ptcTypeInBasket = ((JObject)basketResponse).SelectTokens("$.people[*].type").Select(x => x.ToString()).OrderBy(x => x);

            var ptcs = ptcCountRequest.SequenceEqual(ptcTypeInBasket);

            if (!ptcs)
            {
                message = $"PTCs returned in the Basket were not the same as the request. Request [{string.Join(',', ptcCountRequest)}] and Basket response [{string.Join(',', ptcTypeInBasket)}]";
                return false;
            }

            return true;
        }

        public static bool ValidateBrandNames(string jsonResponse)
        {
            var message = string.Empty;

            dynamic myResponse = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

            var cabinCountWithValue = ((JObject)myResponse).SelectTokens("$..fareSegments[?(@.cabinName && @.cabinName != '')]").Count();
            var cabinCount = ((JObject)myResponse).SelectTokens("$..fareSegments[*]").Count();

            var counts = cabinCountWithValue.Equals(cabinCount);

            if (!counts)
            {
                message = $"Cabin count returned which has values being {cabinCountWithValue} is not the same as the cabin count count for attributes returned being {cabinCount}";
                return false;
            }

            return true;
        }

        public static bool ValidateSummaryLinks(string jsonResponse, out string[] messages)
        {
            JObject jsonObject = JObject.Parse(jsonResponse);

            var flightDetails = jsonObject.SelectTokens($"$..flightsAndFares[ ?( @..links[?( @.rel == 'summary')])]").ToArray();

            var errorMessages = new List<string>();

            var checks = flightDetails.Select(f =>
            {
                var summaryLinks = f.SelectTokens("$..links[ ?( @.rel == 'summary') ].href").Select(x => x.ToString()).ToArray();

                return summaryLinks.AsParallel().All(summaryLink =>
                {
                    var response = WebBFFEndpoint.PostLink(summaryLink, "");

                    bool validateResponse = response.IsSuccessStatusCode && response.Content.Headers.ContentLength > 0;

                    var basketId = summaryLink.Split('/')[5];
                    var slice = summaryLink.Split('/')[7];
                    var offerId = summaryLink.Split('/').LastOrDefault();

                    //Need to build url for Summary link from returned url
                    summaryLink = WebBFFHelper.BuildSummaryUrl(basketId, slice, offerId);

                    var getSummaryResponse = WebBFFEndpoint.GetLink(summaryLink);

                    bool validateGetSummaryResponse = getSummaryResponse.IsSuccessStatusCode && getSummaryResponse.Content.Headers.ContentLength > 0;

                    var isValid = validateResponse && validateGetSummaryResponse;

                    if (!isValid)
                        errorMessages.Add($"Failed link {summaryLink}\nFlight Details:\n{f.SelectToken("$.flight")}");

                    return isValid;
                });
            }).ToArray();

            var isValid = checks.All(x => x);

            messages = errorMessages.ToArray();

            return isValid;
        }

        public static bool ValidateDynamicCabinLinks(string jsonResponse, out string[] messages)
        {
            JObject jsonObject = JObject.Parse(jsonResponse);

            var dynamicCabinLinks = jsonObject.SelectTokens($"$..links[ ?( @.rel == 'dynamicCabin') ].href").Select(x => x.ToString()).ToArray();

            var errorMessages = new List<string>();

            var checks = dynamicCabinLinks.AsParallel().Select(x =>
            {
                var response = WebBFFEndpoint.GetLink(x);

                dynamic dynamicResponse = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                bool validateResponse = response.IsSuccessStatusCode && dynamicResponse.cabinDetails.legs.Count > 0;

                var isValid = validateResponse;

                if (!isValid)
                    errorMessages.Add($"Dynamic cabin link failed, link was: {x}");

                return isValid;
            }).ToArray();

            var isValid = checks.All(x => x);

            messages = errorMessages.ToArray();

            return isValid;
        }

        public static bool ValidateSelfLinks(string jsonResponse, ExtentTest logger)
        {
            JObject jsonObject = JObject.Parse(jsonResponse);

            var selfLinks = jsonObject.SelectTokens($"$..links[ ?( @.rel == 'self') ].href").Select(x => x.ToString()).ToArray();

            var checks = selfLinks.Select(x =>
            {
                var response = WebBFFEndpoint.PostLink(x, "");

                bool validateResponse = response.IsSuccessStatusCode && response.Content.Headers.ContentLength > 0;

                var isValid = validateResponse;

                if (!isValid)
                    logger.Log(Status.Fail, $"Self link failed link was: {x}");

                return isValid;
            }).ToArray();

            return checks.All(x => x);
        }

        public static bool ValidateFareFamily(string jsonResponse, FareFamilyType fareFamilyType, out string message)
        {
            message = string.Empty;

            JObject jsonObject = JObject.Parse(jsonResponse);

            string selectedFareFamilyType = fareFamilyType.GetEnumDescription();

            var responseFareFamilyType = jsonObject.SelectTokens("$.flights[*].fareFamilyType").Select(x => x.ToString());

            if (responseFareFamilyType.Count() != responseFareFamilyType.Count(x => x == selectedFareFamilyType))
            {
                message = $"Fare Family Types were not the same expected {selectedFareFamilyType} but returned {string.Join(',', responseFareFamilyType)}";
                return false;
            }

            return true;
        }

        public static bool ValidateBasketCurrency(dynamic flightSummaryResponse, dynamic basketResponse, out string message)
        {
            message = string.Empty;

            dynamic FlightSummaryResp = JsonConvert.DeserializeObject<dynamic>(flightSummaryResponse);

            var basketSummaryCurrency = basketResponse.summary.priceCurrency;
            var flightSummaryCurrency = FlightSummaryResp.price.currency;

            if (basketSummaryCurrency != flightSummaryCurrency)
            {
                message = $"Basket summary currecny data validation failed in response \n ----------------- \n Basket Summary Currency: {basketSummaryCurrency} \n  flightSummary Summary Currency: {flightSummaryCurrency}\n ";
                return false;
            }

            return true;
        }

        public static bool ValidateOrderCurrency(dynamic basketResponse, dynamic OrderResponse, out string message)
        {
            message = string.Empty;

            var basketSummaryCurrency = basketResponse.summary.priceCurrency;
            var OrderResponseCurrency = OrderResponse.summary.priceCurrency;

            if (basketSummaryCurrency != OrderResponseCurrency)
            {
                message = $"Order summary currecny data validation failed in response \n ----------------- \n Basket Summary Currency: {basketSummaryCurrency} \n  Order Confirmation Currency: {OrderResponseCurrency}\n ";
                return false;
            }

            return true;
        }

        public static bool ValidatePriceSortResult(dynamic offerSearchResponse, out string message)
        {
            message = string.Empty;

            var sortedPriceResult = ((JToken)offerSearchResponse).SelectTokens("$.result.slice.flightsAndFares[*].fares[0].price.amountIncludingTax").Select(x => x.ToString()).ToArray();

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

        public static bool ValidatePriceCurrency(dynamic offerSearchResponse, string currency, out string message)
        {
            message = string.Empty;

            var priceCurrency = ((JToken)offerSearchResponse).SelectTokens("$.result.slice.flightsAndFares[*].fares[*].price.currency").Select(x => x.ToString()).ToArray();

            for (int i = 0; i < priceCurrency.Length; i++)
            {
                if (priceCurrency[i] != currency)
                {
                    message = $"Price currency{priceCurrency[i]} is not same as expected currency {currency}";

                    return false;
                }
            }

            return true;
        }

        public static bool ValidateDepartureTimeSortResult(dynamic offerSearchResponse, out string message)
        {
            message = string.Empty;

            var sortedDepTimeResult = ((JToken)offerSearchResponse).SelectTokens("$.result.slice.flightsAndFares[*].flight.departure").Select(x => x.ToString()).ToArray();

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

        public static bool ValidateDurationTimeSortResult(dynamic offerSearchResponse, out string message)
        {
            message = string.Empty;

            var sortedDurationTimeResult = ((JToken)offerSearchResponse).SelectTokens("$.result.slice.flightsAndFares[*].flight.duration").Select(x => x.ToString()).ToArray();

            for (int i = 1; i < sortedDurationTimeResult.Length; i++)
            {
                var duration = Regex.Match(sortedDurationTimeResult[i - 1], @"\d+").Value;
                var duration1 = Regex.Match(sortedDurationTimeResult[i], @"\d+").Value;

                if (int.Parse(duration1) < int.Parse(duration))
                {
                    message = $"Sorted duration time results are not in lowest{sortedDurationTimeResult[i]} to highest {sortedDurationTimeResult[i - 1]}order";

                    return false;
                }
            }

            return true;
        }

        public static bool FQTProgrammeBFFAssertion(string programCode, string airlineName, string programName, dynamic fqtProgrammeGQLResponse, out string message)
        {
            message = string.Empty;

            var FQTProgrammes = (JArray)fqtProgrammeGQLResponse;

            foreach (var item in FQTProgrammes)
            {
                if (item["programCode"].ToString() == programCode && item["airlineName"].ToString() == airlineName && item["programName"].ToString() == programName)
                    return true;
            }
            message = $" Expected --------- \n\n \t ProgramCode: {programCode} \n \t airlineName:{airlineName} \n \t programName:{programName} \n\n Actual Response --------- \n \t {fqtProgrammeGQLResponse}";

            return false;
        }

        public static bool ValidateAirlineFilterResult(dynamic offerSearchResponse, string flightTypeCode, out string message)
        {
            message = string.Empty;

            var offerSearchAirlineCodes = ((JToken)offerSearchResponse).SelectTokens("$.result.slice.flightsAndFares[*].flight.segments[*].operatingAirline.code").Select(x => x.ToString()).ToArray();

            foreach (var airlineCode in offerSearchAirlineCodes)
            {
                if (airlineCode != flightTypeCode)
                {
                    message = $"Airline filter result actual airline{airlineCode} and expected airline not matched{flightTypeCode}";

                    return false;
                }
            }

            return true;
        }

        public static bool ValidateConnectionsFilterResult(dynamic offerSearchResponse, int stopCounts, out string message)
        {
            message = string.Empty;

            var offerSearchSegmentsResult = ((JToken)offerSearchResponse).SelectTokens("$.data.searchOffersBFF.result.slice.flightsAndFares[*].flight.segments").Select(x => x.ToString()).ToArray();

            foreach (var segment in offerSearchSegmentsResult)
            {
                var count = ((JToken)segment).SelectTokens("$.[*].stopCount").Count();

                if (count != stopCounts)
                {
                    message = $"Connections filter segment count {count} and actual count {stopCounts}";

                    return false;
                }
            }

            return true;
        }

        public static bool ValidateFCMemberDetails(string fcMembershipNumber, string tierCode, dynamic offerSearchResponse, out string message)
        {
            message = string.Empty;

            var details = offerSearchResponse.flyingClubMemberDetails;

            if (details.fcMembershipNumber.Value != fcMembershipNumber)
            {
                message = $"FC number not matched -> {details.fcMembershipNumber.Value}";
                return false;
            }

            if (details.tierCode.Value != tierCode)
            {
                message = $"Tier code not exists -> {details.tierCode.Value}";
                return false;
            }

            if (string.IsNullOrEmpty(details.firstName.Value))
            {
                message = $"First name not exists -> {details.firstName.Value}";
                return false;
            }

            if (string.IsNullOrEmpty(details.lastName.Value))
            {
                message = $"Last name not exists -> {details.lastName.Value}";
                return false;
            }

            if (string.IsNullOrEmpty(details.totalVirginPoints.Value))
            {
                message = $"Total points not exists -> {details.totalVirginPoints.Value}";
                return false;
            }

            return true;
        }

        public static bool ValidateSummaryPoints(dynamic summaryResult, string requiredPoints, out string message)
        {
            message = string.Empty;

            string totalAwardPoints = summaryResult.price.awardPoints.Value;
            bool hasSufficientPoints = summaryResult.hasSufficientPoints.Value;

            if (hasSufficientPoints)
            {
                if (int.Parse(totalAwardPoints) > int.Parse(requiredPoints))
                {
                    message = $"Summary total points {totalAwardPoints} are less than required points {requiredPoints} from the loyalty account.";
                    return false;
                }
            }

            return true;
        }
    }
}