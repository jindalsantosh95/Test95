using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using ChooseResponse = VAA.EAPI.Automation.Search.Models.SustainabilityMS.ChooseResponse;
using OfferSearchMSResponse = VAA.EAPI.Automation.Search.Models.OfferSearch.MSResponse.OfferSearchMSResponse;
using SearchErrorResponse = VAA.EAPI.Automation.Search.Models.OfferSearch.MSResponse.SearchErrorResponse;
using SustainabiltyResponse = VAA.EAPI.Automation.Search.Models.SustainabilityMS.SustainabiltyResponse;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.Microservices.OfferSearch
{
    public class OfferSearchAssertions
    {
        public static bool ValidatePriceDifferenceInboundReturn(JToken outboundResponse, JToken inboundResponse, out string message)
        {
            message = string.Empty;

            var outboundSelectedOfferPrice = outboundResponse.SelectTokens("$.offers[*].salePrice[?(@.priceComponentType == 'BasePrice')].amount.monetaryAmount").Select(x => x.ToString()).ToArray().First();

            var inboundOfferPricings = inboundResponse.SelectTokens("$.offers[*].salePrice[?(@.priceComponentType == 'BasePrice')].amount").Select(x => x.ToString()).ToArray();

            foreach (var records in inboundOfferPricings)
            {
                dynamic data = JObject.Parse(records);
                var currentSalePrice = data.monetaryAmount.Value;
                var pricediff = Convert.ToDecimal(currentSalePrice) - Convert.ToDecimal(outboundSelectedOfferPrice);
                if (pricediff > 0)
                {
                    if (pricediff != Convert.ToDecimal(data.priceDifference.Value) && data.priceDifferenceSign.Value != "+")
                    {
                        message = $"price difference from inbound offer {data.priceDifference.Value} and expected price difference not matched {pricediff}";

                        return false;
                    }
                }
                if (currentSalePrice == outboundSelectedOfferPrice)
                {
                    if (data.priceDifferenceSign.Value != "+")
                    {
                        message = "price difference sign not matched";

                        return false;
                    }
                }
                if (pricediff < 0)
                {
                    if (data.priceDifferenceSign.Value != "-")
                    {
                        message = "price difference sign not matched";

                        return false;
                    }
                }
            }
            return true;
        }

        public static bool upsellOfferValidation(HttpResponseMessage offerPriceResponse, out string message)
        {
            message = string.Empty;

            JToken offerPriceResult = JsonConvert.DeserializeObject<JToken>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var upsellOffersCount = offerPriceResult.SelectTokens("$.offers[*].upsellOffer").Count();

            if (upsellOffersCount < 1)
            {
                message = $"upsell offers not returned in summary response and count is {upsellOffersCount}";

                return false;
            }
            return true;
        }

        public static bool ValidateDataList(HttpResponseMessage deltaOfferSearchResponse, HttpResponseMessage offerSearchResponse, out string message)
        {
            message = string.Empty;

            DeltaMSResponse deltaMSResponse = JsonConvert.DeserializeObject<DeltaMSResponse>(deltaOfferSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            OfferSearchMSResponse offerSearchMSResponse = JsonConvert.DeserializeObject<OfferSearchMSResponse>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            if (deltaMSResponse != null && offerSearchMSResponse != null)
            {
                if (deltaMSResponse.offerDataList.flightSegments.Count != offerSearchMSResponse.dataList.flightSegments.Count)
                {
                    message = "Flight segment count didnt matched with delta";

                    return false;
                }
                if (deltaMSResponse.offerDataList.trips.Count != offerSearchMSResponse.dataList.journey.Count)
                {
                    message = "Journey count didnt matched with delta";

                    return false;
                }
            }
            return true;
        }

        public static bool ValidateFareComponent(HttpResponseMessage deltaOfferSearchResponse, HttpResponseMessage offerSearchResponse, out string message)
        {
            message = string.Empty;

            JToken deltaSearchResult = JsonConvert.DeserializeObject<JToken>(deltaOfferSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var totalAmt = deltaSearchResult.SelectTokens("$.offersSets[*].offers[*].offerItems[*].offerItemPricing[*].totalAmt.currencyEquivalentPrice.formattedCurrencyAmt").Select(x => x.ToString()).ToArray();

            var baseAmt = deltaSearchResult.SelectTokens("$.offersSets[*].offers[*].offerItems[*].offerItemPricing[*].baseAmt.currencyEquivalentPrice.formattedCurrencyAmt").Select(x => x.ToString()).ToArray();

            var taxAmt = deltaSearchResult.SelectTokens("$.offersSets[*].offers[*].offerItems[*].offerItemPricing[*].taxSummary.totalTaxAmt.currencyEquivalentPrice.formattedCurrencyAmt").Select(x => x.ToString()).ToArray();

            var baseFare = offerSearchResult.SelectTokens("$.offers[*].offerItems[*].offeritemPricing[*].salePrice[?(@.priceComponentType == 'BaseFare')].amount.monetaryAmount").Select(x => x.ToString()).ToArray();

            var basePrice = offerSearchResult.SelectTokens("$.offers[*].offerItems[*].offeritemPricing[*].salePrice[?(@.priceComponentType == 'BasePrice')].amount.monetaryAmount").Select(x => x.ToString()).ToArray();

            var totalTax = offerSearchResult.SelectTokens("$.offers[*].offerItems[*].offeritemPricing[*].salePrice[?(@.priceComponentType == 'TotalTax')].amount.monetaryAmount").Select(x => x.ToString()).ToArray();

            if (!basePrice.Any() && totalAmt.Any())
            {
                message = "Total amount from delta and base price from offer search not Found";

                return false;
            }

            if (!baseFare.Any() && baseAmt.Any())
            {
                message = "Base amount from delta and base fare from offer search not Found";

                return false;
            }

            if (!totalTax.Any() && taxAmt.Any())
            {
                message = "Tax amount from delta and total tax from offer search not Found";

                return false;
            }

            return true;
        }

        public static bool ValidateOriginAndDestination(string offerSearchRequest, HttpResponseMessage offerSearchResponse, out string message)
        {
            message = string.Empty;

            JToken flightSearchRequest = JsonConvert.DeserializeObject<JToken>(offerSearchRequest);

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var requestedOrigin = flightSearchRequest.SelectTokens("$.flightsearchRequest.searchOriginDestinations[*].origin").Select(x => x.ToString()).ToArray();

            var responseOrigin = offerSearchResult.SelectTokens("$.dataList.originDestinationAssociations[*].originDestination.originAirport").Select(x => x.ToString()).ToArray();

            var requestedDestination = flightSearchRequest.SelectTokens("$.flightsearchRequest.searchOriginDestinations[*].destination").Select(x => x.ToString()).ToArray();

            var responseDestination = offerSearchResult.SelectTokens("$.dataList.originDestinationAssociations[*].originDestination.destinationAirport").Select(x => x.ToString()).ToArray();

            if (requestedOrigin.Length == responseOrigin.Length)
            {
                foreach (var origin in requestedOrigin)
                {
                    if (!responseOrigin.Contains(origin))
                    {
                        message = "Requested flight origin and flight origin from response not matched";

                        return false;
                    }
                }
            }
            else
            {
                message = $"Requested flight origin {requestedOrigin.Length} and flight origin from response {responseOrigin.Length} is empty";

                return false;
            }

            if (requestedDestination.Length == responseDestination.Length)
            {
                foreach (var destination in requestedDestination)
                {
                    if (!responseDestination.Contains(destination))
                    {
                        message = "Requested flight destination and flight destination from response not matched";

                        return false;
                    }
                }
            }
            else
            {
                message = $"Requested flight destination {requestedDestination.Length} and flight destination from response {responseDestination.Length} is empty";

                return false;
            }

            return true;
        }

        public static bool ValidatePaxList(string offerSearchRequest, HttpResponseMessage offerSearchResponse, out string message)
        {
            message = string.Empty;

            JToken flightSearchRequest = JsonConvert.DeserializeObject<JToken>(offerSearchRequest);

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var paxListFromRequest = flightSearchRequest.SelectTokens("$.customerDetails[*]").Select(x => x.ToString()).ToArray();

            var paxListFromResponse = offerSearchResult.SelectTokens("$.dataList.customerDetails[*]").Select(x => x.ToString()).ToArray();

            if (!paxListFromRequest.SequenceEqual(paxListFromResponse))
            {
                message = "Pax list from request and pax list from flight response not matched";

                return false;
            }

            return true;
        }

        public static bool ValidateJourneyPaxList(string offerSearchRequest, HttpResponseMessage offerSearchResponse, out string message)
        {
            message = string.Empty;

            JToken flightSearchRequest = JsonConvert.DeserializeObject<JToken>(offerSearchRequest);

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var paxListFromRequest = flightSearchRequest.SelectTokens("$.customerDetails[*].custId").Select(x => x.ToString()).ToArray();

            var paxListFromResponse = offerSearchResult.SelectTokens("$.offers[*].offerItems[*].services[*].serviceBeneficiaries[*]").Select(x => x.ToString()).ToArray();

            if (!paxListFromRequest.Any(x => paxListFromResponse.Contains(x)))
            {
                message = "Pax list from request and journey pax list from flight response not matched";

                return false;
            }

            return true;
        }

        public static bool ValidateOfferExpirationTTL(HttpResponseMessage offerSearchResponse, out string message)
        {
            message = string.Empty;

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var offerExpirationTTL = ((JToken)offerSearchResult).SelectTokens("$.offers[*].offerExpirationTimeLimit").Select(x => x.ToString()).ToArray();

            if (offerExpirationTTL.Count() != offerSearchResult.offers.Count)
            {
                message = "Offer count and offer Expiration TTL Count not matched";

                return false;
            }

            return true;
        }

        public static bool ValidateFareSegments(string offerSearchRequest, HttpResponseMessage offerSearchResponse, out string message)
        {
            message = string.Empty;

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var offerSegments = ((JToken)offerSearchResult).SelectTokens("$.offers[*].offerItems[*].services[*].fareDetails.fareSegments").Select(x => x.ToString()).ToArray();

            foreach (var fareSegment in offerSegments)
            {
                if (!fareSegment.Any())
                {
                    message = $"Fare segment array is empty for Offer Request->{offerSearchResult} \n Offer Response->{offerSearchRequest}";
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateSustainabilityResponse(ChooseResponse[] chooseResponse, SustainabiltyResponse[] sustainabilityResponse, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            if (chooseResponse[0].request.origin != sustainabilityResponse[0].originAirport)
            {
                result.Message.Add($"Origin not matched from choose response->{chooseResponse[0].request.origin} to sustainability response->{sustainabilityResponse[0].originAirport}");
            }
            if (chooseResponse[0].request.destination != sustainabilityResponse[0].destinationAirport)
            {
                result.Message.Add($"Destination not matched from choose response->{chooseResponse[0].request.destination} to sustainability response->{sustainabilityResponse[0].destinationAirport}");
            }
            if (chooseResponse[0].response.KilosCo2e != sustainabilityResponse[0].kilosCo2e)
            {
                result.Message.Add($"KilosCo2e not matched from choose response->{chooseResponse[0].response.KilosCo2e} to sustainability response->{sustainabilityResponse[0].kilosCo2e}");
            }
            if (chooseResponse[0].response.LbsCo2e != sustainabilityResponse[0].lbsCo2e)
            {
                result.Message.Add($"LbsCo2e not matched from choose response->{chooseResponse[0].response.LbsCo2e} to sustainability response->{sustainabilityResponse[0].lbsCo2e}");
            }
            if (chooseResponse[0].response.SourceMethodology != sustainabilityResponse[0].sourceMethodology)
            {
                result.Message.Add($"SourceMethodology not matched from choose response->{chooseResponse[0].response.SourceMethodology} to sustainability response->{sustainabilityResponse[0].sourceMethodology}");
            }
            return result.IsValid;
        }

        public static bool ValidateTotalTaxInLocalCurrency(HttpResponseMessage offerSearchResponse, string baseCurrency, out string message)
        {
            message = string.Empty;

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var salePrices = offerSearchResult.SelectTokens("$..salePrice[?(@.priceComponentType == 'TotalTax')]");

            foreach (dynamic totalTax in salePrices)
            {
                if (totalTax.priceComponentType != "TotalTax" || totalTax.amount.baseCurrency != baseCurrency)
                {
                    message = $"Total tax is not in local currency {totalTax}";
                    return false;
                }
            }
            return true;
        }

        public static bool ValidateTotalPointsForAward(HttpResponseMessage offerSearchResponse, out string message)
        {
            message = string.Empty;

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var totalPoints = offerSearchResult.SelectTokens("..salePrice[?(@.priceComponentType == 'TotalPoints')].pointsAmount.pointsCount").Select(x => x.ToString()).ToArray();

            if (!totalPoints.Any())
            {
                message = $"Total points is not exists in response {totalPoints}";

                return false;
            }

            return true;
        }

        public static bool ValidateFareIdExists(HttpResponseMessage offerSearchResponse, string offerSearchRequest, out string message)
        {
            message = string.Empty;

            dynamic flightSearchRequest = JsonConvert.DeserializeObject<dynamic>(offerSearchRequest);
            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var offerItems = offerSearchResult.SelectTokens("$.offers[?(@.offerId)].offerItems[*].services[*]");
            var fareBasisCodes = offerItems.SelectMany(x => x.SelectTokens("fareDetails.fareBasisCode").Select(y => y.ToString())).ToArray();
            var fareDetails = offerItems.Select(x => x.SelectToken("fareDetails").ToString()).ToArray();

            bool isCurrentTripIndexValid = int.Parse(flightSearchRequest.flightsearchRequest.currentTripIndexId.Value) > 0;

            if (isCurrentTripIndexValid && fareBasisCodes.Length != fareDetails.Length - 1 ||
                !isCurrentTripIndexValid && fareBasisCodes.Length != fareDetails.Length)
            {
                message = $"Fare ID does not exist in response {offerSearchResult} for request {offerSearchRequest}";
                return false;
            }

            return true;
        }

        public static bool ValidateFareSegmentsExists(HttpResponseMessage offerSearchResponse, string offerSearchRequest, out string message)
        {
            message = string.Empty;

            dynamic flightSearchRequest = JsonConvert.DeserializeObject<dynamic>(offerSearchRequest);
            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var fareDetails = offerSearchResult.SelectTokens("$.offers[?(@.offerId)].offerItems[*].services[*].fareDetails");
            var fareSegments = fareDetails.SelectMany(fd => fd.SelectTokens("fareSegments")).ToArray();

            bool isCurrentTripIndexValid = int.Parse(flightSearchRequest.flightsearchRequest.currentTripIndexId.Value) > 0;

            if (isCurrentTripIndexValid && fareSegments.Length != fareDetails.Count() - 1 ||
                !isCurrentTripIndexValid && fareSegments.Length != fareDetails.Count())
            {
                message = $"Fare segments do not exist in the response {offerSearchResult} for the request {offerSearchRequest}";
                return false;
            }

            return true;
        }

        public static bool ValidateBaggageExists(HttpResponseMessage brandMSResponse, string brandRequest, string slice, out string message)
        {
            message = string.Empty;

            JToken brandMSResult = JsonConvert.DeserializeObject<JToken>(brandMSResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var carryOnBaggage = brandMSResult.SelectTokens("$.itineraries[*].legs[*].fares[*].segments[*].carryOnBaggage[*]").Select(x => x.ToString()).ToArray();
            var checkedBaggage = brandMSResult.SelectTokens("$.itineraries[*].legs[*].fares[*].segments[*].checkedBaggage[*]").Select(x => x.ToString()).ToArray();

            if (!carryOnBaggage.Any() || !checkedBaggage.Any())
            {
                message = $"Baggage info {slice} is missing from brand response {brandMSResult} for the request {brandRequest}";
                return false;
            }

            return true;
        }

        public static bool ValidateTotalAmount(HttpResponseMessage offerSearchResponse, HttpResponseMessage offerSummaryResponse, string offerId, out string message)
        {
            message = string.Empty;

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            JToken offerSummaryResult = JsonConvert.DeserializeObject<JToken>(offerSummaryResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var offerTotalAmount = offerSearchResult.SelectToken($"$.offers[?(@.offerId == '{offerId}')].salePrice[?(@.priceComponentType == 'BasePrice')].amount.monetaryAmount")?.ToString();
            var summaryTotalAmount = offerSummaryResult.SelectToken("$.offers[?(@.selected == true)].salePrice[?(@.priceComponentType == 'BasePrice')].amount.monetaryAmount")?.ToString();

            if (offerTotalAmount != summaryTotalAmount)
            {
                message = $"Offer total amount {offerTotalAmount} and summary total amount not matched {summaryTotalAmount}";
                return false;
            }

            return true;
        }

        public static bool ValidatePayWithPointsFlag(HttpResponseMessage offerSearchResponse, string offerSearchRequest, out string message)
        {
            message = string.Empty;

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var payWithPointsEligible = offerSearchResult.SelectTokens("$.offers[?(@.offerId)].payWithPointsEligible")?.ToString();

            if (!payWithPointsEligible.Any())
            {
                message = $"Pay with points flag should exists for revenue search ->{offerSearchRequest} \n{offerSearchResult}";
                return false;
            }

            return true;
        }

        public static bool CheckIfStatusCodeIsUnprocessableEntity(HttpResponseMessage offerSearchResponse)
        {
            return offerSearchResponse.StatusCode == HttpStatusCode.UnprocessableEntity;
        }

        public static bool ValidateOfferSearchMSErrorResponse(SearchErrorResponse[] searchErrorResponse, string errorCode, string errorSource, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            var errorResponse = searchErrorResponse[0];

            if (errorResponse.code != errorCode)
            {
                result.Message.Add($"Expected error code {errorResponse.code} not returned");
            }
            if (errorResponse.errorSource != errorSource)
            {
                result.Message.Add($"Expected error source {errorResponse.errorSource} not returned");
            }
            if (errorResponse.developerMessage.IsNullOrEmpty())
            {
                result.Message.Add($"Developer message {errorResponse.developerMessage} can not be empty");
            }
            if (errorResponse.message.IsNullOrEmpty())
            {
                result.Message.Add($"Error message {errorResponse.message} can not be empty");
            }
            return result.IsValid;
        }

        public static bool ValidateCalendarPriceAmountInLocalCurrency(HttpResponseMessage offerSearchResponse, string baseCurrency, out string message)
        {
            message = string.Empty;

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            // Extract calendar prices using a JSON path query
            var calendarPrices = offerSearchResult.SelectTokens("$.calendar.fromPrices[*]");

            // Validate each price
            foreach (var priceToken in calendarPrices)
            {
                var amount = priceToken["price"]?["amount"]?.ToObject<decimal?>();
                var currency = priceToken["price"]?["currency"]?.ToString();

                if (amount.HasValue && currency != baseCurrency)
                {
                    message = $"Calendar amount {amount} is not in local currency {currency}.";
                    return false;
                }
            }
            return true;
        }
    }
}