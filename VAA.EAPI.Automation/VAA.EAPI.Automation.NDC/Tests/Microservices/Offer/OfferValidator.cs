using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.NDC.Tests.Microservices.Offer
{
    public class OfferValidator
    {
        public static bool ValidateCabinNames(dynamic response, List<string> expectedCabinTypes, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            List<string> cabinTypes = new List<string>();

            for (int i = 0; i < response.dataList.priceClass.Count; i++)
            {
                JToken priceClassArray = (JToken)response.dataList.priceClass[i];

                cabinTypes.Add(priceClassArray["cabinType"]["cabinName"].ToString());
            }

            foreach (string cabinType in cabinTypes)
            {
                if (!expectedCabinTypes.Contains(cabinType))
                {
                    result.Message.Add($"Cabin names returned were not as expected. Returned {cabinType} and expected {string.Join(",", expectedCabinTypes)}");
                }
            }

            return result.IsValid;
        }

        public static bool ValidateBaggageWeightAndDimensions(dynamic response, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            foreach (dynamic baggage in response.dataList.baggageAllowances)
            {
                for (int i = 0; i < baggage.freeBaggageAllowance.bagDescriptors.Count; i++)
                {
                    dynamic freeBaggageAllowance = baggage.freeBaggageAllowance;
                    string bagDescriptor = freeBaggageAllowance.bagDescriptors[i].bagDescriptorName.ToString();
                    dynamic bagDescriptorWeight = freeBaggageAllowance.bagDescriptors[0].bagDescriptorDesc[0];

                    if (freeBaggageAllowance.weightAllowance is not null && freeBaggageAllowance.weightAllowance.Count > 0
                        && freeBaggageAllowance.dimensionAllowance is not null
                        && freeBaggageAllowance.dimensionAllowance.Count > 0)
                    {
                        dynamic weightKg = freeBaggageAllowance.weightAllowance[i].maximumWeightMeasure;
                        dynamic weightPound = freeBaggageAllowance.weightAllowance[i + baggage.freeBaggageAllowance.bagDescriptors.Count].maximumWeightMeasure;
                        dynamic heightCm = freeBaggageAllowance.dimensionAllowance[0].heightMeasure;
                        dynamic widthCm = freeBaggageAllowance.dimensionAllowance[0].widthMeasure;
                        dynamic lengthCm = freeBaggageAllowance.dimensionAllowance[0].lengthMeasure;
                        dynamic linearCm = freeBaggageAllowance.dimensionAllowance[0].linearMeasure;

                        string bagDescriptorName = string.Empty;

                        //Might need to re-add Hand Baggage as it seems dodgy
                        if (!bagDescriptor.Has("SPORTING EQUIPMENT") && !bagDescriptor.Has("MUSICAL INSTRUMENT"))
                        {
                            if (bagDescriptor.Has("UPTO"))
                            {
                                weightKg = freeBaggageAllowance.weightAllowance[i].maximumWeightMeasure;
                                bagDescriptorName = $"UPTO70LB {weightKg}KG AND81LI {linearCm}LCM";
                            }
                            else
                            {
                                bagDescriptorName = $"{weightKg}KG {lengthCm}X{heightCm}X{widthCm}CM TOTAL {linearCm}LICM";
                            }

                            if (bagDescriptorWeight != $"Up to {weightPound} lb/{weightKg} kg")
                            {
                                result.Message.Add($"Baggage weight was not parsed correctly.");
                            }
                            if (!bagDescriptor.Has("HAND BAGGAGE"))
                            {
                                if (!bagDescriptor.Is(bagDescriptorName))
                                {
                                    result.Message.Add($"Baggage dimensions was not parsed correctly expected {bagDescriptor} but was {bagDescriptorName} for.");
                                }
                            }
                        }
                    }
                }
            }

            return result.IsValid;
        }

        public static bool ValidateFareComponent(HttpResponseMessage response, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string[] baseFare = offerSearchResult.SelectTokens("$.offers[*].offerItems[*].offeritemPricing[*].salePrice[?(@.priceComponentType == 'BaseFare')].amount.monetaryAmount").Select(x => x.ToString()).ToArray();

            string[] basePrice = offerSearchResult.SelectTokens("$.offers[*].offerItems[*].offeritemPricing[*].salePrice[?(@.priceComponentType == 'BasePrice')].amount.monetaryAmount").Select(x => x.ToString()).ToArray();

            string[] totalTax = offerSearchResult.SelectTokens("$.offers[*].offerItems[*].offeritemPricing[*].salePrice[?(@.priceComponentType == 'TotalTax')].amount.monetaryAmount").Select(x => x.ToString()).ToArray();

            if (!baseFare.Any())
            {
                result.Message.Add("Base price from offer search not Found");
            }

            if (!basePrice.Any())
            {
                result.Message.Add("Base price from offer search not Found");
            }

            if (!totalTax.Any())
            {
                result.Message.Add("Total tax from offer search not Found");
            }

            return result.IsValid;
        }

        public static bool ValidateOriginAndDestination(string request, HttpResponseMessage response, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            JToken flightSearchRequest = JsonConvert.DeserializeObject<JToken>(request);

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string[] requestedOrigin = flightSearchRequest.SelectTokens("$.flightsearchRequest.searchOriginDestinations[*].origin").Select(x => x.ToString()).ToArray();

            string[] responseOrigin = offerSearchResult.SelectTokens("$.dataList.originDestinationAssociations[*].originDestination.originAirport").Select(x => x.ToString()).ToArray();

            string[] requestedDestination = flightSearchRequest.SelectTokens("$.flightsearchRequest.searchOriginDestinations[*].destination").Select(x => x.ToString()).ToArray();

            string[] responseDestination = offerSearchResult.SelectTokens("$.dataList.originDestinationAssociations[*].originDestination.destinationAirport").Select(x => x.ToString()).ToArray();

            AirportCodeMapper mapper = new AirportCodeMapper();

            if (requestedOrigin.Length == responseOrigin.Length)
            {
                foreach (string origin in requestedOrigin)
                {
                    if (!responseOrigin.Contains(mapper.MapCode(origin)))
                    {
                        result.Message.Add($"Requested flight origin {requestedOrigin} and response origin was {origin} do not match");
                    }
                }
            }
            else
            {
                result.Message.Add($"Requested flight origin {requestedOrigin.Length} and flight origin from response {responseOrigin.Length} is empty");
            }

            if (requestedDestination.Length == responseDestination.Length)
            {
                foreach (string destination in requestedDestination)
                {
                    if (!responseDestination.Contains(mapper.MapCode(destination)))
                    {
                        result.Message.Add("Requested flight destination and flight destination from response not matched");
                    }
                }
            }
            else
            {
                result.Message.Add($"Requested flight destination {requestedDestination.Length} and flight destination from response {responseDestination.Length} is empty");
            }

            return result.IsValid;
        }

        public static bool ValidatePaxList(string request, HttpResponseMessage response, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            JToken flightSearchRequest = JsonConvert.DeserializeObject<JToken>(request);

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string[] paxListFromRequest = flightSearchRequest.SelectTokens("$.customerDetails[*]").Select(x => x.ToString()).ToArray();

            string[] paxListFromResponse = offerSearchResult.SelectTokens("$.dataList.customerDetails[*]").Select(x => x.ToString()).ToArray();

            string[] ptc = flightSearchRequest.SelectTokens("$.customerDetails[*].ptc").Select(x => x.ToString()).ToArray();

            string[] itaPtc = offerSearchResult.SelectTokens("$.dataList.customerDetails[*].itaPtc").Select(x => x.ToString()).ToArray();

            if (!paxListFromResponse.ToString().Has(paxListFromRequest.ToString()) || !ptc.SequenceEqual(itaPtc))
            {
                result.Message.Add($"Pax list from request and pax list from flight response not matched. Expected -> {paxListFromRequest} but actual is -> {paxListFromResponse} ");
            }

            return result.IsValid;
        }

        public static bool ValidateJourneyPaxList(string request, HttpResponseMessage response, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            JToken flightSearchRequest = JsonConvert.DeserializeObject<JToken>(request);

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string[] paxListFromRequest = flightSearchRequest.SelectTokens("$.customerDetails[*].custId").Select(x => x.ToString()).ToArray();

            string[] paxListFromResponse = offerSearchResult.SelectTokens("$.offers[*].offerItems[*].services[*].serviceBeneficiaries[*]").Select(x => x.ToString()).ToArray();

            if (!paxListFromRequest.Any(x => paxListFromResponse.Contains(x)))
            {
                result.Message.Add("Pax list from request and journey pax list from flight response not matched");
            }

            return result.IsValid;
        }

        public static bool ValidateOfferExpirationTTL(HttpResponseMessage response, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string[] offerExpirationTTL = ((JToken)offerSearchResult).SelectTokens("$.offers[*].offerExpirationTimeLimit").Select(x => x.ToString()).ToArray();

            if (offerExpirationTTL.Count() != offerSearchResult.offers.Count)
            {
                result.Message.Add("Offer count and offer Expiration TTL Count not matched");
            }

            return result.IsValid;
        }

        public static bool ValidatePointOfSale(string request, HttpResponseMessage response, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            dynamic flightSearchRequest = JsonConvert.DeserializeObject<dynamic>(request);

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            if (flightSearchRequest.Pos.cityCode.ToString() != offerSearchResult.pos.cityCode.ToString())
            {
                result.Message.Add($"cityCode in the response {offerSearchResult.pos.cityCode.ToString()} does not match the city Code in the request {flightSearchRequest.Pos.cityCode.ToString()}");
            }

            if (flightSearchRequest.Pos.countryCode.ToString() != offerSearchResult.pos.countryCode.ToString())
            {
                result.Message.Add($"countryCode in the response {offerSearchResult.pos.countryCode.ToString()} does not match the city Code in the request {flightSearchRequest.Pos.countryCode.ToString()}");
            }

            if (flightSearchRequest.Pos.currencyCode.ToString() != offerSearchResult.pos.currencyCode.ToString())
            {
                result.Message.Add($"currencyCode in the response {offerSearchResult.pos.currencyCode.ToString()} does not match the city Code in the request {flightSearchRequest.Pos.currencyCode.ToString()}");
            }
            return result.IsValid;
        }

        public static bool ValidateTotalTaxInLocalCurrency(string request, HttpResponseMessage response, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            dynamic flightSearchRequest = JsonConvert.DeserializeObject<dynamic>(request);

            string baseCurrency = flightSearchRequest.Pos.currencyCode.ToString();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            IEnumerable<JToken> salePrices = offerSearchResult.SelectTokens("$..salePrice[?(@.priceComponentType == 'TotalTax')]");

            foreach (dynamic totalTax in salePrices)
            {
                if (totalTax.priceComponentType != "TotalTax" || totalTax.amount.baseCurrency != baseCurrency)
                {
                    result.Message.Add($"Total tax is not in local currency {totalTax}");
                }
            }
            return result.IsValid;
        }

        public static bool ValidatePriceClassList(HttpResponseMessage response, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            List<string> priceClassList = offerSearchResult.SelectTokens("$.dataList.priceClass.[*].id")
                                                  .Select(x => x.ToString())
                                                  .ToList();


            IEnumerable<JToken> offerItems = offerSearchResult.SelectTokens("$.offers[?(@.offerId)].offerItems[*].services[*]");

            List<string> priceClassRefIds = offerItems.SelectMany(x => x.SelectTokens("priceClassRefIds").Select(y => y.ToString())).ToList();

            bool priceClassListExists = priceClassRefIds.All(p => priceClassList.Any(x => p.Has(x)));

            if (!priceClassListExists)
                result.Message.Add($"Price classes do not exist in response {offerSearchResult}");

            return result.IsValid;
        }

        public static bool ValidateJourneyRefIds(HttpResponseMessage response, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            List<string> journeyRefList = offerSearchResult.SelectTokens("$.dataList.originDestinationAssociations.[*].journeyRefIds[*]")
                                                  .Select(x => x.ToString())
                                                  .ToList();

            IEnumerable<JToken> offerItems = offerSearchResult.SelectTokens("$.offers[?(@.offerId)].offerItems[*].services[*]");

            List<string> journeyRefIds = offerItems.SelectMany(x => x.SelectTokens("journeyRefIds").Select(y => y.ToString())).ToList();

            bool journeyRefIdsListExists = journeyRefIds.All(p => journeyRefList.Any(x => p.Has(x)));

            if (!journeyRefIdsListExists)
                result.Message.Add($"Journey Ref Ids do not exist in response {offerSearchResult}");

            return result.IsValid;
        }

        public static bool ValidatePartiesParticipants(string request, HttpResponseMessage response, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            dynamic flightSearchRequest = JsonConvert.DeserializeObject<dynamic>(request);

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            if (flightSearchRequest.Parties.participants[0].partyType.ToString() != offerSearchResult.parties.participants[0].partyType.ToString())
                result.Message.Add($"Party Type did not match the request {request} with the response {response.ReadResponseAsString()}");

            if (flightSearchRequest.Parties.participants[0].name.ToString() != offerSearchResult.parties.participants[0].name.ToString())
                result.Message.Add($"Name did not match the request {request} with the response {response.ReadResponseAsString()}");

            if (flightSearchRequest.Parties.participants[0].agencyId.ToString() != offerSearchResult.parties.participants[0].agencyID.ToString())
                result.Message.Add($"Agency Id did not match the request {request} with the response {response.ReadResponseAsString()}");

            if (flightSearchRequest.Parties.participants[0].ordinal.ToString() != offerSearchResult.parties.participants[0].ordinal.ToString())
                result.Message.Add($"Ordinal did not match the request {request} with the response {response.ReadResponseAsString()}");

            return result.IsValid;
        }
    }
}
