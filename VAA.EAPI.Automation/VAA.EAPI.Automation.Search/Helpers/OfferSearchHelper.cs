using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;
using BrandGQLRequest = VAA.EAPI.Automation.Search.Models.OfferSearch.BrandGQLRequest;
using BrandRequest = VAA.EAPI.Automation.Search.Models.OfferSearch.BrandRequest;
using BrandVariables = VAA.EAPI.Automation.Search.Models.OfferSearch.BrandVariables;
using CabinType = VAA.EAPI.Automation.Search.Models.OfferSearch.CabinType;
using CustomerDetails = VAA.EAPI.Automation.Search.Models.OfferSearch.CustomerDetails;
using FareConditionsGQLRequest = VAA.EAPI.Automation.Search.Models.OfferSearch.FareConditionsGQLRequest;
using FareConditionsRequest = VAA.EAPI.Automation.Search.Models.OfferSearch.FareConditionsRequest;
using FareConditionsVariables = VAA.EAPI.Automation.Search.Models.OfferSearch.FareConditionsVariables;
using Filters = VAA.EAPI.Automation.Search.Models.OfferSearch.Filters;
using FlightSearchRequest = VAA.EAPI.Automation.Search.Models.OfferSearch.FlightSearchRequest;
using GQLVariables = VAA.EAPI.Automation.Search.Models.OfferSearch.GQLVariables;
using Ids = VAA.EAPI.Automation.Search.Models.OfferSearch.Ids;
using OfferSearchBFFRequest = VAA.EAPI.Automation.Search.Models.OfferSearch.OfferSearchBFFRequest;
using OfferSearchGQLNextRequest = VAA.EAPI.Automation.Search.Models.OfferSearch.OfferSearchGQLNextRequest;
using OfferSearchGQLRequest = VAA.EAPI.Automation.Search.Models.OfferSearch.OfferSearchGQLRequest;
using OfferSearchGQLSortAndFiltersRequest = VAA.EAPI.Automation.Search.Models.OfferSearch.OfferSearchGQLSortAndFiltersRequest;
using OfferSearchMSResponse = VAA.EAPI.Automation.Search.Models.OfferSearch.MSResponse.OfferSearchMSResponse;
using SalesBannerGQLRequest = VAA.EAPI.Automation.Search.Models.SalesBanner.SalesBannerGQLRequest;
using SalesBannerGQLVariables = VAA.EAPI.Automation.Search.Models.SalesBanner.SalesBannerGQLVariables;
using SalesBannerRequest = VAA.EAPI.Automation.Search.Models.SalesBanner.SalesBannerRequest;
using SearchGQLNextRequest = VAA.EAPI.Automation.Search.Models.OfferSearch.SearchGQLNextRequest;
using SearchGQLNextRequestVariables = VAA.EAPI.Automation.Search.Models.OfferSearch.SearchGQLNextRequestVariables;
using SearchGQLRequest = VAA.EAPI.Automation.Search.Models.OfferSearch.SearchGQLRequest;
using SearchOriginDestination = VAA.EAPI.Automation.Search.Models.OfferSearch.SearchOriginDestination;
using SearchSummaryGQLRequest = VAA.EAPI.Automation.Search.Models.OfferSearch.SearchSummaryGQLRequest;
using SearchSummaryGQLVariables = VAA.EAPI.Automation.Search.Models.OfferSearch.SearchSummaryGQLVariables;
using SearchSummaryRequest = VAA.EAPI.Automation.Search.Models.OfferSearch.SearchSummaryRequest;
using SetVoucherRedemptionSelectionGQLRequest = VAA.EAPI.Automation.Search.Models.OfferSearch.SetVoucherRedemptionSelectionGQLRequest;
using SetVoucherRedemptionSelectionRequest = VAA.EAPI.Automation.Search.Models.OfferSearch.SetVoucherRedemptionSelectionRequest;
using SetVoucherRedemptionSelectionVariables = VAA.EAPI.Automation.Search.Models.OfferSearch.SetVoucherRedemptionSelectionVariables;
using Slice = VAA.EAPI.Automation.Search.Models.OfferSearch.Slice;
using SortAndFiltersRequest = VAA.EAPI.Automation.Search.Models.OfferSearch.SortAndFiltersRequest;
using SustainabilityGQLRequest = VAA.EAPI.Automation.Search.Models.SustainabilityMS.SustainabilityGQLRequest;
using SustainabilityGQLVariables = VAA.EAPI.Automation.Search.Models.SustainabilityMS.SustainabilityGQLVariables;
using SustainabilityRequest = VAA.EAPI.Automation.Search.Models.SustainabilityMS.SustainabilityRequest;
using Variables = VAA.EAPI.Automation.Search.Models.OfferSearch.Variables;

namespace VAA.EAPI.Automation.Search.Helpers
{
    public static class OfferSearchHelper
    {

        public static string RandomDate(string startdate, int maxNumberOfDays)
        {
            DateTime date = DateTime.ParseExact(startdate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            Random random = new Random();

            DateTime newDate = date.AddDays(random.Next(1, maxNumberOfDays));

            string journeyDate = newDate.ToString("yyyy-MM-dd");

            return journeyDate;
        }

        public static string DepartureDate()
        {
            DateTime randomMonth = DateTime.Now.AddMonths(Random.Shared.Next(5, 10));

            // Get the total number of days in the selected month
            int daysInMonth = DateTime.DaysInMonth(randomMonth.Year, randomMonth.Month);

            // Generate a random day within the selected month
            int randomDay = Random.Shared.Next(1, daysInMonth + 1);

            // Create the final random date
            DateTime departureDate = new DateTime(randomMonth.Year, randomMonth.Month, randomDay);

            // Format the date as "yyyy-MM-ddTHH:mm:ss"
            string formattedDate = departureDate.ToString("yyyy-MM-ddT00:00:00");

            return formattedDate;
        }

        public static string ReturnDate(string departureDate)
        {
            string returnDate = departureDate.Parse<DateTime>().AddDays(7).ToString("yyyy-MM-ddT00:00:00");

            return returnDate;
        }

        //This method returns the summary link for the next call required for BFF post search
        public static string FindSummaryLinkForSpecifiedAirlineCodeAndCabinNameForOneWay(string json, FlightScenarioType flightScenarioType)
        {
            JObject jsonObject = JObject.Parse(json);

            string link = string.Empty;

            if (flightScenarioType == FlightScenarioType.Virgin)
            {
                var vsLinks = jsonObject.SelectTokens("$.result.slice.flightsAndFares[?( @.flight.segments[ ?( @.operatingAirline.code == 'VS' ) ])].fares[*].links[ ?( @.rel == 'summary') ].href").Select(x => x.ToString()).ToArray();
                link = vsLinks[0];
            }
            else if (flightScenarioType == FlightScenarioType.NonVirgin)
            {
                var nonVsLinks = jsonObject.SelectTokens("$.result.slice.flightsAndFares[?( @.flight.segments[ ?( @.operatingAirline.code != 'VS' ) ])].fares[*].links[ ?( @.rel == 'summary') ].href").Select(x => x.ToString()).ToArray();
                link = nonVsLinks[0];
            }

            return link;
        }

        [Obsolete("Marked as obsolete in favour of config driven approach. Please use 'FindAllSummaryLinksForSpecificCabinAndAirline(TestConfig config, string json)'.")]
        public static string[] FindAllSummaryLinksForSpecificCabinAndAirline(string json, FareFamilyType fareFamilyType, FlightType flightType)
        {
            JObject jsonObject = JObject.Parse(json);

            string hyphenatedValue = fareFamilyType.GetEnumDescription();

            string airlineCode = flightType.ToString();

            return jsonObject.SelectTokens($"$..slice.flightsAndFares[?( @.flight.segments[ ?( @.operatingAirline.code == '{airlineCode}' ) ])].fares[?(@.fareFamilyType=='{hyphenatedValue}')].links[ ?( @.rel == 'summary') ].href").Select(x => x.ToString()).ToArray();
        }

        public static string[] InboundFindSummaryLinkForAnyFlexibleCabin(string json, FareFamilyType fareFamilyType, FlightType flightType)
        {
            JObject jsonObject = JObject.Parse(json);

            string link = string.Empty;

            var fareFamilyTypes = Enum.GetValues<FareFamilyType>()
                                      .Where(x => fareFamilyType.HasFlag(x))
                                      .Select(x => $"@.fareFamilyType == '{x.GetEnumDescription()}'");

            var fareFamilyTypeFilter = string.Join(" || ", fareFamilyTypes);

            string airlineCode = flightType.ToString();

            return jsonObject.SelectTokens($"$..slice.flightsAndFares[?( @.flight.segments[ ?( @.operatingAirline.code == '{airlineCode}' ) ])].fares[?({fareFamilyTypeFilter})].links[ ?( @.rel == 'summary') ].href").Select(x => x.ToString()).ToArray();
        }

        public static string[] OutboundFindFareLinksForFareFamilyAndFlightType(string json, FareFamilyType fareFamilyType, FlightType flightType)
        {
            JObject jsonObject = JObject.Parse(json);

            string link = string.Empty;

            var fareFamilyTypes = Enum.GetValues<FareFamilyType>()
                                      .Where(x => fareFamilyType.HasFlag(x))
                                      .Select(x => $"@.fareFamilyType == '{x.GetEnumDescription()}'");

            var fareFamilyTypeFilter = string.Join(" || ", fareFamilyTypes);

            string airlineCode = flightType.ToString();

            return jsonObject.SelectTokens($"$..result.slice.flightsAndFares[?( @.flight.segments[ ?( @.operatingAirline.code == '{airlineCode}' ) ])].fares[?({fareFamilyTypeFilter})].links[ ?( @.rel == 'fare') ].href").Select(x => x.ToString()).ToArray();
        }

        [Obsolete("Marked as obsolete in favour of config driven approach. Please use 'FindAllFareLinksForSpecificCabinAndAirline(TestConfig config, string json)'.")]
        public static string[] FindAllFareLinksForSpecificCabinAndAirline(string json, FareFamilyType fareFamilyType, FlightType flightType)
        {
            JObject jsonObject = JObject.Parse(json);

            string hyphenatedValue = fareFamilyType.GetEnumDescription();

            string airlineCode = flightType.ToString();

            return jsonObject.SelectTokens($"$..result.slice.flightsAndFares[?( @.flight.segments[ ?( @.operatingAirline.code == '{airlineCode}' ) ])].fares[?(@.fareFamilyType=='{hyphenatedValue}')].links[ ?( @.rel == 'fare') ].href").Select(x => x.ToString()).ToArray();
        }

        public static string[] FindAllFareLinksForSpecificCabinAndAirline(TestConfig config, string json)
        {
            JObject jsonObject = JObject.Parse(json);

            string hyphenatedValue = config.Search.FareFamilyType.GetEnumDescription();

            string airlineCode = config.Search.FlightType.ToString();

            return jsonObject.SelectTokens($"$..result.slice.flightsAndFares[?( @.flight.segments[ ?( @.operatingAirline.code == '{airlineCode}' ) ])].fares[?(@.fareFamilyType=='{hyphenatedValue}')].links[ ?( @.rel == 'fare') ].href").Select(x => x.ToString()).ToArray();
        }

        public static string[] FindAllSummaryLinksForAirlineAndFareFamily(TestConfig config, string json)
        {
            JObject jsonObject = JObject.Parse(json);

            bool isReturnJourney = config.Search.JourneyType == JourneyType.Return;

            string hyphenatedValue = isReturnJourney
                ? config.Search.InboundFareFamilyType.GetEnumDescription()
                : config.Search.FareFamilyType.GetEnumDescription();

            string airlineCode = isReturnJourney
                ? config.Search.InboundFlightType.ToString()
                : config.Search.FlightType.ToString();

            return jsonObject.SelectTokens($"$..slice.flightsAndFares[?( @.flight.segments[ ?( @.operatingAirline.code == '{airlineCode}' ) ])].fares[?(@.fareFamilyType=='{hyphenatedValue}')].links[ ?( @.rel == 'summary') ].href").Select(x => x.ToString()).ToArray();
        }

        public static string[] FindAllSummaryLinksForSpecificAirline(string json, FlightType flightType)
        {
            JObject jsonObject = JObject.Parse(json);
            string airlineCode = flightType.ToString();

            return jsonObject.SelectTokens($"$..slice.flightsAndFares[?( @.flight.segments[ ?( @.operatingAirline.code == '{airlineCode}' ) ])].fares[*].links[ ?( @.rel == 'summary') ].href")
                .Select(x => x.ToString())
                .ToArray();
        }

        public static string FindSummaryLinkForSpecifiedAirlineCodeAndCabinNameForReturn(string json, FlightScenarioType flightScenarioType)
        {
            JObject jsonObject = JObject.Parse(json);

            string link = string.Empty;

            if (flightScenarioType == FlightScenarioType.Virgin)
            {
                var vsLinks = jsonObject.SelectTokens("$.slice.flightsAndFares[?( @.flight.segments[ ?( @.operatingAirline.code == 'VS' ) ])].fares[*].links[ ?( @.rel == 'summary') ].href").Select(x => x.ToString()).ToArray();
                if (vsLinks.Length == 0)
                {
                    Debug.WriteLine("No non Virgin flights returned in Search response");
                    throw new Exception("No Virgin flights returned in Search response");
                }
                else
                {
                    link = vsLinks[0];
                }
            }
            else if (flightScenarioType == FlightScenarioType.NonVirgin)
            {
                var nonVsLinks = jsonObject.SelectTokens("$.slice.flightsAndFares[?( @.flight.segments[ ?( @.operatingAirline.code != 'VS' ) ])].fares[*].links[ ?( @.rel == 'summary') ].href").Select(x => x.ToString()).ToArray();
                if (nonVsLinks.Length == 0)
                {
                    Debug.WriteLine("No non Virgin flights returned in Search response");
                    throw new Exception("No non Virgin flights returned in Search response");
                }
                else
                {
                    link = nonVsLinks[0];
                }
            }

            return link;
        }

        public static string FindSummaryLinkForFirstFlightReturnedInResults(string json)
        {
            JObject jsonObject = JObject.Parse(json);

            string link = string.Empty;

            var vsLinks = jsonObject.SelectTokens("$.result.slice.flightsAndFares[*].fares[*].links[ ?( @.rel == 'summary') ].href").Select(x => x.ToString()).ToArray();
            link = vsLinks[0];

            return link;
        }

        public static string FindSummaryLinkForSpecificCabin(string json, FareFamilyType fareFamilyType)
        {
            JObject jsonObject = JObject.Parse(json);

            string link = string.Empty;

            string hyphenatedValue = fareFamilyType.GetEnumDescription();

            var summaryLinks = jsonObject.SelectTokens($"$..slice.flightsAndFares.[*].fares[?(@.fareFamilyType=='{hyphenatedValue}')].links[ ?( @.rel == 'summary') ].href").Select(x => x.ToString()).ToArray();

            if (summaryLinks.Length > 0)
                link = summaryLinks[0];
            else
            {
                throw new Exception($"No summary links were found for Fare Family Type {hyphenatedValue}");
            }
            return link;
        }

        public static string FindOfferForReturn(string json, FlightScenarioType flightScenarioType)
        {
            JObject jsonObject = JObject.Parse(json);

            string link = string.Empty;

            if (flightScenarioType == FlightScenarioType.Virgin)
            {
                var vsLinks = jsonObject.SelectTokens("$.result.slice.flightsAndFares[?( @.flight.segments[ ?( @.operatingAirline.code == 'VS' ) ])].fares[*].links[ ?( @.rel == 'fare') ].href").Select(x => x.ToString()).ToArray();
                if (vsLinks.Length == 0)
                {
                    Debug.WriteLine("No non Virgin flights returned in Search response");
                    throw new Exception("No Virgin flights returned in Search response");
                }
                else
                {
                    link = vsLinks[0];
                }
            }
            else if (flightScenarioType == FlightScenarioType.NonVirgin)
            {
                var nonVsLinks = jsonObject.SelectTokens("$.result.slice.flightsAndFares[?( @.flight.segments[ ?( @.operatingAirline.code != 'VS' ) ])].fares[*].links[ ?( @.rel == 'fare') ].href").Select(x => x.ToString()).ToArray();
                if (nonVsLinks.Length == 0)
                {
                    Debug.WriteLine("No non Virgin flights returned in Search response");
                    throw new Exception("No non Virgin flights returned in Search response");
                }
                else
                {
                    link = nonVsLinks[0];
                }
            }

            return link;
        }

        [Obsolete("Marked as obsolete in favour of config driven approach. Please use 'OfferSearchGQLRequest(TestConfig config)'.")]
        public static string OfferSearchGQLRequest(JourneyType journeyType, string departureDate = "", string returnDate = "", LocationType locationOne = LocationType.None, LocationType locationTwo = LocationType.None,
            LocationType locationThree = LocationType.None, LocationType locationFour = LocationType.None,
           int numberOfAdults = 0, int numberOfChildren = 0, int numberOfYouths = 0, int numberOfInfants = 0, bool flexiDateSearch = false, bool calenderSearch = false, string offerId = "", bool rewardSearch = false, string promoCode = "", LocationType connectionAirportOne = LocationType.None, LocationType connectionAirportTwo = LocationType.None, string basketId = "")
        {
            var originDestination = CreateOriginDestination(journeyType, locationOne, locationTwo, locationThree, locationFour, departureDate, returnDate, connectionAirportOne, connectionAirportTwo);

            var customers = CreateCustomerDetails(numberOfAdults, numberOfChildren, numberOfYouths, numberOfInfants);

            var currentTripIndexId = journeyType switch
            {
                JourneyType.OneWay => "0",
                JourneyType.Return => !offerId.IsNullOrEmpty() ? "1" : "0",
                _ => "",
            };

            var flightSearchRequest = new FlightSearchRequest
            {
                searchOriginDestinations = originDestination.ToList(),
                cabinTypes = new List<CabinType>(),
                fareFamilies = new List<string>(),
                flexiDateSearch = flexiDateSearch,
                calendarSearch = calenderSearch,
                awardSearch = rewardSearch,
                bundleOffer = false,
                nonStopOnly = false,
                refundableOnly = false,
                checkInBaggageAllowance = true,
                carryOnBaggageAllowance = true,
                offerID = offerId,
                currentTripIndexId = currentTripIndexId,
                promoCode = promoCode,
            };

            // Create the appropriate request based on currentTripIndexId
            var offerSearchGQLRequest = currentTripIndexId == "0"
                ? (object)new OfferSearchGQLRequest
                {
                    operationName = "SearchOffers",
                    query = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQl", "SearchOffersGqlQuery.txt")),
                    variables = new Variables
                    {
                        request = new SearchGQLRequest
                        {
                            flightSearchRequest = flightSearchRequest,
                            customerDetails = customers,
                        }
                    }
                }
                : new OfferSearchGQLNextRequest
                {
                    operationName = "SearchOffersNextSlice",
                    query = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQl", "SearchOffersNextSliceGqlQuery.txt")),
                    variables = new SearchGQLNextRequestVariables
                    {
                        request = new SearchGQLNextRequest
                        {
                            offerId = offerId,
                            currentTripIndexId = int.Parse(currentTripIndexId),
                            basketId = basketId
                        }
                    }
                };

            return JsonConvert.SerializeObject(offerSearchGQLRequest);
        }

        public static string OfferSearchGQLRequest(TestConfig config)
        {
            var originDestination = CreateOriginDestination(config);

            var customers = CreateCustomerDetails(config);

            var currentTripIndexId = config.Search.JourneyType switch
            {
                JourneyType.OneWay => "0",
                JourneyType.Return => !config.Internal.OfferId.IsNullOrEmpty() ? "1" : "0",
                _ => "",
            };

            var flightSearchRequest = new FlightSearchRequest
            {
                searchOriginDestinations = originDestination.ToList(),
                cabinTypes = [],
                fareFamilies = [],
                flexiDateSearch = config.Search.IsFlexiDateSearch,
                calendarSearch = config.Search.IsCalendarSearch,
                awardSearch = config.Search.IsRewardSearch,
                bundleOffer = false,
                nonStopOnly = false,
                refundableOnly = false,
                checkInBaggageAllowance = true,
                carryOnBaggageAllowance = true,
                offerID = config.Internal.OfferId,
                currentTripIndexId = currentTripIndexId,
                promoCode = config.Search.PromoCode,
            };

            // Create the appropriate request based on currentTripIndexId
            var offerSearchGQLRequest = currentTripIndexId == "0"
                ? (object)new OfferSearchGQLRequest
                {
                    operationName = "SearchOffers",
                    query = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQl", "SearchOffersGqlQuery.txt")),
                    variables = new Variables
                    {
                        request = new SearchGQLRequest
                        {
                            flightSearchRequest = flightSearchRequest,
                            customerDetails = customers,
                        }
                    }
                }
                : new OfferSearchGQLNextRequest
                {
                    operationName = "SearchOffersNextSlice",
                    query = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQl", "SearchOffersNextSliceGqlQuery.txt")),
                    variables = new SearchGQLNextRequestVariables
                    {
                        request = new SearchGQLNextRequest
                        {
                            offerId = config.Internal.OfferId,
                            currentTripIndexId = int.Parse(currentTripIndexId),
                            basketId = config.Internal.BasketId
                        }
                    }
                };

            return JsonConvert.SerializeObject(offerSearchGQLRequest);
        }

        public static string MultiCityGraphqlRequest(LocationType[] locations = null, int numberOfAdults = 0, int numberOfChildren = 0, int numberOfYouths = 0, int numberOfInfants = 0, bool flexiDateSearch = false, bool calendarSearch = false, string offerId = "", bool rewardSearch = false, string basketId = "", string currentTripIndexId = "0", string departureDate = "")
        {
            var journey = locations != null ? CreateJourney(locations, departureDate) : Array.Empty<SearchOriginDestination>();

            var customers = CreateCustomerDetails(numberOfAdults, numberOfChildren, numberOfYouths, numberOfInfants);

            var flightSearchRequest = new FlightSearchRequest
            {
                searchOriginDestinations = journey.ToList(),
                cabinTypes = new List<CabinType>(),
                fareFamilies = new List<string>(),
                flexiDateSearch = flexiDateSearch,
                calendarSearch = calendarSearch,
                awardSearch = rewardSearch,
                bundleOffer = false,
                nonStopOnly = false,
                refundableOnly = false,
                checkInBaggageAllowance = true,
                carryOnBaggageAllowance = true,
                offerID = offerId,
                currentTripIndexId = currentTripIndexId
            };

            // Create the appropriate request based on currentTripIndexId
            var offerSearchGQLRequest = currentTripIndexId == "0"
                ? (object)new OfferSearchGQLRequest
                {
                    operationName = "SearchOffers",
                    query = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQl", "SearchOffersGqlQuery.txt")),
                    variables = new Variables
                    {
                        request = new SearchGQLRequest
                        {
                            flightSearchRequest = flightSearchRequest,
                            customerDetails = customers,
                        }
                    }
                }
                : new OfferSearchGQLNextRequest
                {
                    operationName = "SearchOffersNextSlice",
                    query = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQl", "SearchOffersNextSliceGqlQuery.txt")),
                    variables = new SearchGQLNextRequestVariables
                    {
                        request = new SearchGQLNextRequest
                        {
                            offerId = offerId,
                            currentTripIndexId = int.Parse(currentTripIndexId),
                            basketId = basketId
                        }
                    }
                };

            // Serialize and return the request
            return JsonConvert.SerializeObject(offerSearchGQLRequest);
        }

        public static string OfferSearchGQLSortAndFiltersRequest(JourneyType journeyType, string departureDate = "", string returnDate = "", LocationType locationOne = LocationType.None, LocationType locationTwo = LocationType.None,
           LocationType locationThree = LocationType.None, LocationType locationFour = LocationType.None, LocationType locationFive = LocationType.None, LocationType locationSix = LocationType.None,
           string cabinTypeCode = "", int numberOfAdults = 0, int numberOfChildren = 0, int numberOfYouths = 0, int numberOfInfants = 0, bool flexiDateSearch = false, bool calenderSearch = false, string offerId = "", string pqId = "",
           string airlineFilter = "-", string departureTimesFliter = "-", string connectionFilter = "-", string cabinFilter = "-", string sortRequestType = "RECOMMENDED", bool rewardSearch = false, string basketId = "")
        {
            var originDestination = CreateOriginDestination(journeyType, locationOne, locationTwo, locationThree, locationFour, departureDate, returnDate);

            var customers = CreateCustomerDetails(numberOfAdults, numberOfChildren, numberOfYouths, numberOfInfants);

            SortAndFiltersRequest request = new()
            {
                flightSearchRequest = new FlightSearchRequest()
                {
                    searchOriginDestinations = originDestination.ToList(),
                    cabinTypes = new List<CabinType>(),
                    fareFamilies = new List<string>(),
                    flexiDateSearch = flexiDateSearch,
                    calendarSearch = calenderSearch,
                    awardSearch = rewardSearch,
                    bundleOffer = false,
                    nonStopOnly = false,
                    refundableOnly = false,
                    checkInBaggageAllowance = true,
                    carryOnBaggageAllowance = true,
                    offerID = offerId,
                    currentTripIndexId = journeyType switch
                    {
                        JourneyType.OneWay => "0",
                        JourneyType.Return => !string.IsNullOrEmpty(offerId) ? "1" : "0",
                        _ => "",
                    }
                },
                customerDetails = customers,
                sortRequest = sortRequestType,
                basketId = basketId,
                filters = new Filters()
                {
                    airlines = airlineFilter,
                    departureTimes = departureTimesFliter,
                    connections = connectionFilter,
                    cabins = cabinFilter,
                }
            };

            OfferSearchGQLSortAndFiltersRequest offerSearchGQLSortAndFiltersRequest = new()
            {
                pqId = pqId,
                variables = new GQLVariables()
                {
                    request = request
                }
            };

            return JsonConvert.SerializeObject(offerSearchGQLSortAndFiltersRequest);
        }

        public static string SalesBannerGQLRequest(LocationType origin = LocationType.None, LocationType destination = LocationType.None, string departureDate = "", string returnDate = "", int numberOfadults = 0, int numberOfchildren = 0, int numberOfyouths = 0, int numberOfinfants = 0, string destinationUrl = "", string flightDirection = "")
        {
            var salesBannerQuery = File.ReadAllText($"Requests/Body/GraphQl/SalesBannerGqlQuery.txt");

            var displayLocations = new List<string>
            {
                "search-top",
                "seasonal-messaging"
            };

            SalesBannerGQLRequest salesBannerGQLRequest = new()
            {
                query = salesBannerQuery,
                variables = new SalesBannerGQLVariables()
                {
                    displayLocations = displayLocations,
                    request = new SalesBannerRequest()
                    {
                        originAirport = origin.ToString(),
                        destinationAirport = destination.ToString(),
                        destinationURL = destinationUrl,
                        flightDirection = flightDirection,
                        departureDate = departureDate,
                        returnDate = returnDate,
                        numAdults = numberOfadults,
                        numTeens = numberOfyouths,
                        numChildren = numberOfchildren,
                        numInfants = numberOfinfants
                    }
                }
            };

            return JsonConvert.SerializeObject(salesBannerGQLRequest);
        }

        public static string SustainabilityGQLRequest(string passengerCounts, string offerId)
        {
            SustainabilityGQLRequest sustainabilityGQLRequest = new()
            {
                pqId = "carbonIndexBFF",
                variables = new SustainabilityGQLVariables()
                {
                    request = new SustainabilityRequest()
                    {
                        offerId = offerId,
                        passengerCounts = passengerCounts
                    }
                }
            };

            return JsonConvert.SerializeObject(sustainabilityGQLRequest);
        }

        public static List<string> DepartureDateSpecificDayOfWeek(DayOfWeek dayOfWeek)
        {
            DateTime departureDate = DateTime.Now.AddMonths(1);

            DateTime endDate = DateTime.Now.AddMonths(Random.Shared.Next(5, 10));

            int days = 1;

            List<string> dates = new List<string>();

            while (departureDate <= endDate)
            {
                if (departureDate.DayOfWeek == dayOfWeek)
                {
                    dates.Add(departureDate.ToString("yyyy-MM-dd"));
                    days = 7;
                }
                departureDate = departureDate.AddDays(days);
            }
            return dates;
        }

        public static string OfferSearchBFFRequest(JourneyType journeyType, LocationType origin = LocationType.None, LocationType destination = LocationType.None, LocationType returnOrigin = LocationType.None, LocationType returnDestination = LocationType.None, int adults = 0, int teens = 0, int child = 0, int infants = 0, bool randomDatesWithin7Days = false, string departureDate = "", string returnDate = "", bool rewardFlag = false)
        {
            Debug.WriteLine($"Origin - {origin} > Destination - {destination} > Return Origin - {returnOrigin} > Return Destination - {returnDestination}");
            Debug.WriteLine($"Passengers - Adult count = {adults}, Teen count = {teens}, Child count = {child}, Infant count = {infants}");

            if (departureDate == "" && returnDate == "")
                if (randomDatesWithin7Days)
                {
                    departureDate = DateTime.Now.AddDays(Random.Shared.Next(1, 6)).ToString("yyyy-MM-dd");
                    returnDate = departureDate.Parse<DateTime>().AddDays(7).ToString("yyyy-MM-dd");
                }
                else
                {
                    departureDate = DateTime.Now.AddMonths(Random.Shared.Next(5, 10)).ToString("yyyy-MM-dd");
                    returnDate = departureDate.Parse<DateTime>().AddDays(7).ToString("yyyy-MM-dd");
                }

            Debug.WriteLine($"Departure Date - {departureDate} and Return Date - {returnDate}");

            var originDest = journeyType switch
            {
                JourneyType.OneWay => new List<Slice>
                {
                    new Slice
                    {
                        origin = origin.ToString(),
                        destination = destination.ToString(),
                        departure = departureDate
                    }
                },
                JourneyType.Return => new List<Slice>
                {
                    new()
                    {
                        origin = origin.ToString(),
                        destination = destination.ToString(),
                        departure = departureDate
                    },
                    new()
                    {
                        origin = (returnOrigin == LocationType.None ? destination : returnOrigin).ToString(),
                        destination = (returnDestination == LocationType.None ? origin : returnDestination).ToString(),
                        departure = returnDate
                    }
                },
                _ => new List<Slice>()
            };

            var passengers = new List<string>();

            passengers.AddRange(Enumerable.Range(1, adults).Select(x => "ADULT"));

            passengers.AddRange(Enumerable.Range(1, teens).Select(x => "TEEN"));

            passengers.AddRange(Enumerable.Range(1, child).Select(x => "CHILD"));

            passengers.AddRange(Enumerable.Range(1, infants).Select(x => "INFANT"));

            OfferSearchBFFRequest offerSearchBFFRequest = new()
            {
                slices = originDest,
                awardSearch = rewardFlag,
                passengers = passengers,
                cabinClasses = new List<object>()
            };

            return JsonConvert.SerializeObject(offerSearchBFFRequest);
        }

        public static string OfferSearchBFFRequestReturn(JourneyType journeyType, LocationType origin = LocationType.None, LocationType destination = LocationType.None, LocationType returnOrigin = LocationType.None, LocationType returnDestination = LocationType.None, int adults = 0, int teens = 0, int child = 0, int infants = 0, string id = "", string fareId = "", bool rewardFlag = false)
        {
            var deptDateTime = DateTime.UtcNow.AddDays(166).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 130;

            string departureDate = RandomDate(deptDateTime, maxNumberOfDays);

            string returnDate = departureDate.Parse<DateTime>().AddDays(14).ToString("yyyy-MM-dd");

            var originDest = journeyType switch
            {
                JourneyType.OneWay => new List<Slice>
                {
                    new Slice
                    {
                        origin = origin.ToString(),
                        destination = destination.ToString(),
                        departure = departureDate
                    }
                },
                JourneyType.Return => new List<Slice>
                {
                    new()
                    {
                        origin = origin.ToString(),
                        destination = destination.ToString(),
                        departure = departureDate
                    },
                    new()
                    {
                        origin = returnOrigin.ToString(),
                        destination = returnDestination.ToString(),
                        departure = returnDate
                    }
                },
                _ => new List<Slice>()
            };

            var idobject = new Ids
            {
                id = id,
                fareId = fareId
            };

            var passengers = new List<string>();

            passengers.AddRange(Enumerable.Range(1, adults).Select(x => "ADULT"));

            passengers.AddRange(Enumerable.Range(1, child).Select(x => "TEEN"));

            passengers.AddRange(Enumerable.Range(1, teens).Select(x => "CHILD"));

            passengers.AddRange(Enumerable.Range(1, infants).Select(x => "INFANT"));

            OfferSearchBFFRequest offerSearchBFFRequest = new()
            {
                slices = originDest,
                awardSearch = rewardFlag,
                ids = idobject,
                passengers = passengers,
                cabinClasses = new List<object>()
            };

            return JsonConvert.SerializeObject(offerSearchBFFRequest);
        }

        [Obsolete("Marked as obsolete in favour of config driven approach. Please use 'OfferPriceNewGQLRequest(TestConfig config, string offerId, string summaryLink = \"\", int currentTripIndexId = 3)'.")]
        public static string OfferPriceNewGQLRequest(JourneyType journeyType, string basketId, string offerId, string summaryLink = "", int currentTripIndexId = 3)
        {
            SearchSummaryGQLRequest summaryGQLRequest = new()
            {
                pqId = "searchSummary",
                variables = new SearchSummaryGQLVariables()
                {
                    request = new SearchSummaryRequest()
                    {
                        basketId = basketId,
                        offerId = offerId,
                        currentTripIndexId = journeyType switch
                        {
                            JourneyType.OneWay => 1,
                            JourneyType.Return => 2,
                            JourneyType.MultiCity => currentTripIndexId,
                            _ => 0,
                        },
                        summaryLink = summaryLink
                    }
                }
            };

            return JsonConvert.SerializeObject(summaryGQLRequest);
        }

        public static string OfferPriceNewGQLRequest(TestConfig config, string offerId, string summaryLink = "", int currentTripIndexId = 3, bool includeVoucherDetails = false, bool includeVoucherBasketDetails = false, bool includeVoucherEligibility = false)
        {
            SearchSummaryGQLRequest summaryGQLRequest = new()
            {
                operationName = includeVoucherDetails ? "SearchSummary" : null,
                pqId = includeVoucherDetails ? null : "searchSummary",
                query = includeVoucherDetails ? GetSearchSummaryVoucherQuery(includeVoucherBasketDetails, includeVoucherEligibility) : null,
                variables = new SearchSummaryGQLVariables()
                {
                    request = new SearchSummaryRequest()
                    {
                        basketId = config.Internal.BasketId,
                        offerId = offerId,
                        currentTripIndexId = config.Search.JourneyType switch
                        {
                            JourneyType.OneWay => 1,
                            JourneyType.Return => 2,
                            JourneyType.MultiCity => currentTripIndexId,
                            _ => 0,
                        },
                        summaryLink = summaryLink
                    }
                }
            };

            return JsonConvert.SerializeObject(summaryGQLRequest);
        }

        public static string SetVoucherRedemptionSelectionGQLRequest(string basketId, IEnumerable<string> selectedVoucherIds)
        {
            string[] selectedVoucherIdsArray = selectedVoucherIds?
                .Where(x => !x.IsNullOrWhiteSpace())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? Array.Empty<string>();

            SetVoucherRedemptionSelectionGQLRequest request = new SetVoucherRedemptionSelectionGQLRequest()
            {
                operationName = "SetVoucherRedemptionSelection",
                query = SetVoucherRedemptionSelectionQuery,
                variables = new SetVoucherRedemptionSelectionVariables()
                {
                    request = new SetVoucherRedemptionSelectionRequest()
                    {
                        basketId = basketId,
                        selectedVoucherIds = selectedVoucherIdsArray
                    }
                }
            };

            return JsonConvert.SerializeObject(request);
        }

        private static string GetSearchSummaryVoucherQuery(bool includeVoucherBasketDetails, bool includeVoucherEligibility)
        {
            string query = includeVoucherBasketDetails
                ? SearchSummaryWithVoucherBasketDetailsQuery
                : SearchSummaryWithVoucherDetailsQuery;

            if (!includeVoucherEligibility)
            {
                query = query.Replace(@"    voucherEligibility {
      eligible
      reasonCode
    }
", string.Empty, StringComparison.Ordinal);
            }

            return query;
        }

        private const string SearchSummaryWithVoucherDetailsQuery = @"
query SearchSummary($request: SearchSummaryRequestInput!) {
  searchSummary(request: $request) {
    passengerCounts
    price {
      amount
      currency
      tax
      amountIncludingTax
      awardPoints
    }
    flights {
      fareFamilyType
      cabinName
    }
    hasSufficientPoints
    voucherEligibility {
      eligible
      reasonCode
    }
    voucherDetails {
      redemptionOptions {
        optionId
        optionName
        redemptionType
        appliedVouchers {
          voucherId
          pointsUsed
        }
        coveredPassengers
        baseLinePoints
        pointsToPay
        totalPointsUsed
        cashToPay
      }
      bestOption {
        optionId
        optionName
        selectionReason
      }
      availableVouchers {
        voucherId
        balancePoints
        expiryDate
      }
    }
  }
}";

        private const string SearchSummaryWithVoucherBasketDetailsQuery = @"
query SearchSummary($request: SearchSummaryRequestInput!) {
  searchSummary(request: $request) {
    basketId
    passengerCounts
    price {
      amount
      currency
      tax
      amountIncludingTax
      awardPoints
      voucherPoints
      pointsToPay
    }
    flights {
      fareFamilyType
      cabinName
    }
    hasSufficientPoints
    voucherApplied
    voucherEligibility {
      eligible
      reasonCode
    }
    voucherDetails {
      redemptionOptions {
        optionId
        optionName
        redemptionType
        appliedVouchers {
          voucherId
          pointsUsed
        }
        coveredPassengers
        baseLinePoints
        pointsToPay
        totalPointsUsed
        cashToPay
      }
      bestOption {
        optionId
        optionName
        redemptionType
        selectionReason
      }
      availableVouchers {
        voucherId
        balancePoints
        expiryDate
      }
    }
  }
}";

        private const string SetVoucherRedemptionSelectionQuery = @"
mutation SetVoucherRedemptionSelection($request: SetVoucherRedemptionSelectionRequestInput!) {
  setVoucherRedemptionSelection(request: $request) {
    summary {
      membershipPoints
      totalPrice
      priceCurrency
    }
    hasSufficientPoints
    voucherPoints
    pointsToPay
    voucherApplied
    voucherRedemptionSelection {
      selectedVoucherIds
      selectedOptionId
    }
    voucherDetails {
      redemptionOptions {
        optionId
        optionName
        redemptionType
        appliedVouchers {
          voucherId
          pointsUsed
        }
        coveredPassengers
        pointsToPay
        totalPointsUsed
        cashToPay
      }
      bestOption {
        optionId
        optionName
        redemptionType
        selectionReason
      }
      availableVouchers {
        voucherId
        balancePoints
        expiryDate
      }
    }
  }
}";

        [Obsolete("Marked as obsolete in favour of config driven approach. Please use 'OfferSearchGQLReturnRequest(TestConfig config)'.")]
        public static SearchOriginDestination[] CreateOriginDestination(JourneyType journeyType, LocationType locationOne = LocationType.None, LocationType locationTwo = LocationType.None,
            LocationType locationThree = LocationType.None, LocationType locationFour = LocationType.None, string departureDate = "", string returnDate = "", LocationType connectionAirportOne = LocationType.None, LocationType connectionAirportTwo = LocationType.None)
        {
            var SearchOriginDestination = journeyType switch
            {
                JourneyType.OneWay => new SearchOriginDestination[]
                {
                                new()
                                {
                                    origin = locationOne.ToString(),
                                    destination = locationTwo.ToString(),
                                    departureDate = departureDate,
                                    connectionAirports = GetConnectionAirport(connectionAirportOne)
                                }
                },
                JourneyType.Return => new SearchOriginDestination[]
                {
                                new()
                                {
                                    origin = locationOne.ToString(),
                                    destination = locationTwo.ToString(),
                                    departureDate = departureDate,
                                    connectionAirports = GetConnectionAirport(connectionAirportOne)
                                },
                                new()
                                {
                                    origin = locationThree.ToString(),
                                    destination = locationFour.ToString(),
                                    departureDate = returnDate,
                                    connectionAirports = GetConnectionAirport(connectionAirportTwo)
                                }
                },
                _ => Array.Empty<SearchOriginDestination>()
            };
            return SearchOriginDestination;
        }

        public static SearchOriginDestination[] CreateOriginDestination(TestConfig config)
        {
            var SearchOriginDestination = config.Search.JourneyType switch
            {
                JourneyType.OneWay =>
                [
                    new()
                    {
                        origin = config.Search.Origin.ToString(),
                        destination = config.Search.Destination.ToString(),
                        departureDate = config.Search.DepartureDate,
                        connectionAirports = GetConnectionAirport(config.Search.Connections[0])
                    }
                ],
                JourneyType.Return =>
                [
                    new()
                    {
                        origin = config.Search.Origin.ToString(),
                        destination = config.Search.Destination.ToString(),
                        departureDate = config.Search.DepartureDate,
                        connectionAirports = GetConnectionAirport(config.Search.Connections[0])
                    },
                    new()
                    {
                        origin = config.Search.Destination.ToString(),
                        destination = config.Search.Origin.ToString(),
                        departureDate = config.Search.ReturnDate,
                        connectionAirports = GetConnectionAirport(config.Search.Connections[1])
                    }
                ],
                JourneyType.MultiCity =>
                [
                    new()
                    {
                        origin = config.Search.Origin.ToString(),
                        destination = config.Search.Destination.ToString(),
                        departureDate = config.Search.DepartureDate,
                        connectionAirports = GetConnectionAirport(config.Search.Connections[0])
                    },
                    new()
                    {
                        origin = config.Search.Destination.ToString(),
                        destination = config.Search.MultiCity.Location.ToString(),
                        departureDate = config.Search.ReturnDate,
                        connectionAirports = GetConnectionAirport(config.Search.Connections[1])
                    },
                    new()
                    {
                        origin = config.Search.MultiCity.Location.ToString(),
                        destination = config.Search.Origin.ToString(),
                        departureDate = config.Search.MultiCity.ReturnDate,
                        connectionAirports = GetConnectionAirport(config.Search.Connections[2])
                    }
                ],
                _ => Array.Empty<SearchOriginDestination>()
            };
            return SearchOriginDestination;
        }

        private static List<string> GetConnectionAirport(LocationType connectionAirport)
        {
            return connectionAirport == LocationType.None ? null : new List<string> { connectionAirport.ToString() };
        }

        public static string BrandGQLRequest(string offerId, string passengerCounts)
        {
            BrandRequest request = new()
            {
                offerId = offerId,
                passengerCounts = passengerCounts
            };
            BrandGQLRequest brandGQLRequest = new()
            {
                PqId = "brandBFF",
                Variables = new BrandVariables()
                {
                    request = request
                }
            };

            return JsonConvert.SerializeObject(brandGQLRequest);
        }

        public static string FareConditionsGQLRequest(string offerId)
        {
            FareConditionsRequest request = new()
            {
                offerId = offerId
            };
            FareConditionsGQLRequest fareConditionsGQLRequest = new()
            {
                PqId = "fareConditions",
                Variables = new FareConditionsVariables()
                {
                    request = request
                }
            };

            return JsonConvert.SerializeObject(fareConditionsGQLRequest);
        }

        public static string FindOfferByOperatingCarrierAndFamilyType(HttpResponseMessage offerSearchResponse, FlightType flightType, CabinName cabinName)
        {
            try
            {
                // Deserialize the response
                var offerSearchMSResponse = JsonConvert.DeserializeObject<OfferSearchMSResponse>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                // Find the segment ID based on the operating carrier
                var segmentId = offerSearchMSResponse.dataList.flightSegments
                    .FirstOrDefault(flightSegment => flightSegment.operatingCarrier.Equals(flightType.ToString()))
                    ?.segmentId;

                if (segmentId == null)
                    return string.Empty;

                // Find the offer ID based on the segment ID and cabin name
                var matchingOffer = offerSearchMSResponse.offers.FirstOrDefault(offer =>
                    offer.offerItems.Any(offerItem =>
                        offerItem.services.Any(service =>
                            service.flightSegmentsRefIds.Contains(segmentId) &&
                            service.fareDetails.fareSegments.Any(segment =>
                                segment.cabinName.Equals(cabinName.ToString())
                            )
                        )
                    )
                );

                return matchingOffer?.offerId ?? string.Empty;
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        public static string[] ReturnDeltaNotNullOffersIds(HttpResponseMessage jsonResponse)
        {
            var offerSearchResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var offersIds = offerSearchResult.SelectTokens("$..offersSets[*].offers[?(@.offerId != '')].offerId").Select(x => x.ToString()).ToArray();

            return offersIds;
        }

        public static string ReturnAvailableDepartureDate(HttpResponseMessage jsonResponse)
        {
            var departureDate = "";

            var offerSearchResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var availableFlightDates = offerSearchResult.SelectTokens("$.flexibleDateCalendar.departureDetails[*].flexibleDatePriceCalendarCell[*].originDestinations[*].departureDate").Select(x => x.ToString()).ToArray();

            if (availableFlightDates.Length > 0)
            {
                departureDate = availableFlightDates[0].Parse<DateTime>().ToString("yyyy-MM-ddT00:00:00");
            }
            else
            {
                departureDate = DepartureDate();
            }
            return departureDate;
        }

        public static string ReturnOneWayDepartureDateGQL(HttpResponseMessage jsonResponse)
        {
            var departureDate = "";

            var offerSearchResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var availableFlightDates = offerSearchResult.SelectTokens("$.data.searchOffers.calendar.fromPrices[*].fromDate").Select(x => x.ToString()).ToArray();

            if (availableFlightDates.Length > 0)
            {
                departureDate = availableFlightDates[0].Parse<DateTime>().ToString("yyyy-MM-ddT00:00:00");
            }
            else
            {
                departureDate = DepartureDate();
            }

            return departureDate;
        }

        public static void GetOfferIdAndPassengersCountsFromCarbonIndexLink(HttpResponseMessage jsonResponse, out string offerId, out string passengersCounts)
        {
            var offerSearchResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var carbonIndexLinks = offerSearchResult.SelectTokens($"$..links[ ?( @.rel == 'carbonIndex') ].href").Select(x => x.ToString()).ToArray();

            // Select a random index from the links array
            int randomIndex = Random.Shared.Next(carbonIndexLinks.Length);

            // Get carbon index link using random index
            var carbonIndexLink = carbonIndexLinks[randomIndex];

            offerId = carbonIndexLink.Split('/')[6];
            passengersCounts = carbonIndexLink.Split('/')[7];
        }

        public static string ReturnOfferIdForMarketingCarrierAndCabinNameDirectOnly(HttpResponseMessage jsonResponse, FlightType operatingCarrier, CabinName cabinName)
        {
            var query = $"$.offers[?(@.offerItems[?(@.services[?(@.fareDetails.fareSegments[?(@.cabinName == '{cabinName}')] && @.flightSegmentsRefIds[?($.dataList.flightSegments[?(@.operatingCarrier== '{operatingCarrier}')].segmentId ==@)])])])]";

            var offerSearchResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var offers = offerSearchResult.SelectTokens(query);

            var filter = offers.FirstOrDefault(offer => offer["offerItems"]
            .Any(offerItem => offerItem["services"]
            .Any(service => service["flightSegmentsRefIds"].Count() == 1)));

            var offerId = filter["offerId"].ToString();

            return offerId;
        }

        public static void GetOfferIdAndPassengersCountsFromDynamicCabinLink(HttpResponseMessage jsonResponse, out string offerId, out string passengersCounts, out string dynamicCabinLink, bool isEnvProd = false)
        {
            var offerSearchResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var dynamicCabinLinks = offerSearchResult.SelectTokens($"$..links[ ?( @.rel == 'dynamicCabin') ].href").Select(x => x.ToString()).ToArray();

            // Select a random index from the links array
            int randomIndex = Random.Shared.Next(dynamicCabinLinks.Length);

            // Get dynamic cabin link using random index
            dynamicCabinLink = dynamicCabinLinks[randomIndex];

            if (isEnvProd)
            {
                passengersCounts = dynamicCabinLink.Split('/')[4];
                offerId = dynamicCabinLink.Split('/')[5];
            }
            else
            {
                passengersCounts = dynamicCabinLink.Split('/')[5];
                offerId = dynamicCabinLink.Split('/')[6];
            }
        }

        public static string ReturnFareBasisCodeBasedOnFlightSegmentsRefId(HttpResponseMessage jsonResponse, string flightSegmentsRefId)
        {
            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string fareBasisCode = offerSearchResult.SelectTokens($"$..fareSegments[?(@.flightSegmentRefID == '{flightSegmentsRefId}')].fareBasisCode").FirstOrDefault()?.ToString();

            // If fareBasisCode is not found in fareSegments, use the general fareBasisCode
            if (fareBasisCode == null)
            {
                fareBasisCode = offerSearchResult.SelectTokens("$.offers[?(@.offerId)].offerItems[*].services[*].fareDetails.fareBasisCode").FirstOrDefault()?.ToString();
            }

            return fareBasisCode;
        }

        public static string ReturnOfferIdForMarketingCarrierAndCabinNameDirectWithStop(HttpResponseMessage jsonResponse, FlightType operatingCarrier, CabinName cabinName)
        {
            var query = $"$.offers[?(@.offerItems[?(@.services[?(@.fareDetails.fareSegments[?(@.cabinName == 'VSCL')] && @.flightSegmentsRefIds[?($.dataList.flightSegments[?(@.operatingCarrier== 'VS' && @.connectionOrStopOver == '1')].segmentId ==@)])])])]";

            var offerSearchResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var offers = offerSearchResult.SelectTokens(query);

            var filter = offers.FirstOrDefault(offer => offer["offerItems"]
            .Any(offerItem => offerItem["services"]
            .Any(service => service["flightSegmentsRefIds"].Count() == 1)));

            // Check if filter is null to avoid null reference exception
            if (filter == null)
            {
                return string.Empty;
            }

            var offerId = filter["offerId"].ToString();

            return offerId;
        }

        public static string ReferenceSalesBannersRequest(string displayLocations, string originAirport = "", string destinationAirport = "", string bookingType = "", string flightDirection = "", string departureDate = "", string returnDate = "", int numAdults = 0, int numTeens = 0, int numChildren = 0, int numInfants = 0,   string destinationUrl = "")
        {
            var payload = new
            {
                displayLocations = new List<string> { displayLocations },
                request = new JObject
                {
                    ["originAirport"] = originAirport,
                    ["destinationAirport"] = destinationAirport,
                    ["bookingType"] = bookingType,
                    ["flightDirection"] = flightDirection,
                    ["departureDate"] = departureDate,
                    ["returnDate"] = returnDate,
                    ["numAdults"] = numAdults,
                    ["numTeens"] = numTeens,
                    ["numChildren"] = numChildren,
                    ["numInfants"] = numInfants,
                    ["destinationURL"] = destinationUrl
                }
            };

            return JsonConvert.SerializeObject(payload);
        }

        public static string[] ReturnSummaryUpgradeLinks(HttpResponseMessage jsonResponse)
        {
            var summaryResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var summaryUpgradeLinks = summaryResult.SelectTokens($"$..links[ ?( @.rel == 'upgrade') ].href").Select(x => x.ToString()).ToArray();

            return summaryUpgradeLinks;
        }

        public static string ReturnRandomOfferIdFromGQLResponse(HttpResponseMessage jsonResponse)
        {
            var offerSearchResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var offersIds = offerSearchResult.SelectTokens("$..result.slice.flightsAndFares[*].fares[?(@.id != null)].id").Select(x => x.ToString()).ToArray();

            // Check if the array is empty
            if (offersIds.Length == 0)
            {
                // Handle the case where no offer IDs are found, maybe return a default value or an error message
                throw new Exception($"No offers available. {Environment.NewLine} Response was: {Environment.NewLine} {jsonResponse.ReadResponseAsString()}");
            }

            // Generate a random index
            int randomIndex = Random.Shared.Next(offersIds.Length);

            // Return the offerId at the random index
            return offersIds[randomIndex];
        }

        public static string GetFirstOrRandomOfferIdFromMSResponse(HttpResponseMessage jsonResponse, bool randomOffer = true)
        {
            // Parse the JSON response
            var offerSearchResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            // Extract all offer IDs
            var offerIds = offerSearchResult.SelectTokens("$..offerId")
                                            .Select(x => x.ToString())
                                            .ToArray();

            // Return an empty string if no offers are found
            if (offerIds.Length == 0)
            {
                return string.Empty;
            }

            // Return a random offer ID or the first one based on the input parameter
            return randomOffer
                ? offerIds[Random.Shared.Next(offerIds.Length)]
                : offerIds[0];
        }

        public static Dictionary<string, List<string>> GetCityAirportCodes()
        {
            return new Dictionary<string, List<string>>
            {
                { "LON", new List<string> { "LHR", "LCY", "LON", "LGW" } },
                { "NYC", new List<string> { "JFK", "NYC", "LGA", "EWR" } }
            };
        }

        public static string GetFirstOrRandomOfferIdFromFareOrSummaryLinks(string[] links, bool randomOffer)
        {
            string offerId = string.Empty;

            if (randomOffer)
            {
                // Select a random index from the links array
                int randomIndex = Random.Shared.Next(links.Length);

                // Get offer id from the randomly selected link
                offerId = links[randomIndex].Split('/').LastOrDefault();
            }
            else
            {
                offerId = links[0].Split('/').LastOrDefault();
            }
            return offerId;
        }

        public static string GetSaverFaresOfferIdFromGQLResponse(HttpResponseMessage jsonResponse)
        {
            var offerSearchResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var offersIds = offerSearchResult.SelectTokens("$..flightsAndFares[*].fares[?(@.isSaverFare == true)].id").Select(x => x.ToString()).ToArray();

            // Generate a random index
            int randomIndex = Random.Shared.Next(offersIds.Length);

            // Return the offerId at the random index
            return offersIds[randomIndex];
        }

        public static string GetBasketIdFromGQLResponseBody(HttpResponseMessage jsonResponse)
        {
            var offerSearchResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            // Safely check if basketId exists
            string basketId = offerSearchResult.SelectToken("$.data.searchOffers.result.basketId")?.ToString();

            // If basketId is not found or is null, error out
            if (string.IsNullOrEmpty(basketId))
            {
                throw new Exception($"Basket ID not found. Offer Search request was: {Environment.NewLine} {jsonResponse.ReadRequestAsString()} {Environment.NewLine} and the response was: {Environment.NewLine} {jsonResponse.ReadResponseAsString()}");
            }

            return basketId;
        }

        public static string GetOfferIdByPromoCodeApplied(HttpResponseMessage jsonResponse, string promoCode)
        {
            if (TryGetOfferIdByPromoCodeApplied(jsonResponse, promoCode, out string offerId))
            {
                return offerId;
            }

            string requestBody = jsonResponse.ReadRequestAsString();
            string responseBody = jsonResponse.ReadResponseAsString();

            throw new Exception(
                $"No fare id found with promoCodeApplied '{promoCode}'. " +
                $"Offer Search request: {Environment.NewLine}{requestBody}{Environment.NewLine}" +
                $"Offer Search response: {Environment.NewLine}{responseBody}");
        }

        public static bool TryGetOfferIdByPromoCodeApplied(HttpResponseMessage jsonResponse, string promoCode, out string offerId)
        {
            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            string[] offerIds = offerSearchResult.SelectTokens($"$..fares[?(@.promoCodeApplied == '{promoCode}')].id").Select(x => x.ToString()).ToArray();

            if (offerIds.Length == 0)
            {
                offerId = string.Empty;
                return false;
            }

            int randomIndex = Random.Shared.Next(offerIds.Length);
            offerId = offerIds[randomIndex];
            return true;
        }

        public static void GetCarbonIndexLinkFromResponse(HttpResponseMessage jsonResponse, out string carbonIndexLink)
        {
            var offerSearchResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var carbonIndexLinks = offerSearchResult.SelectTokens($"$..links[ ?( @.rel == 'carbonIndex') ].href").Select(x => x.ToString()).ToArray();

            // Select a random index from the links array
            int randomIndex = Random.Shared.Next(carbonIndexLinks.Length);

            // Get carbon index link using random index
            carbonIndexLink = carbonIndexLinks[randomIndex];
        }

        private static SearchOriginDestination[] CreateJourney(LocationType[] locations, string departureDate)
        {
            var journey = new SearchOriginDestination[locations.Length / 2];

            if (departureDate.IsNullOrEmpty())
            {
                departureDate = DepartureDate();
            }

            for (int i = 0; i < locations.Length; i += 2)
            {
                journey[i / 2] = new SearchOriginDestination
                {
                    origin = locations[i].ToString(),
                    destination = locations[i + 1].ToString(),
                    departureDate = departureDate
                };
                departureDate = ReturnDate(departureDate);
            }

            return journey;
        }

        [Obsolete("Marked as obsolete in favour of config driven approach. Please use 'CreateCustomerDetails(TestConfig config)'.")]
        public static List<CustomerDetails> CreateCustomerDetails(int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants)
        {
            var customers = new List<CustomerDetails>();

            customers.AddRange(GenerateCustomerList(numberOfAdults, "ADULT", "ADT"));
            customers.AddRange(GenerateCustomerList(numberOfYouths, "YOUNG", "GBE"));
            customers.AddRange(GenerateCustomerList(numberOfChildren, "CHILD", "CNN"));
            customers.AddRange(GenerateCustomerList(numberOfInfants, "INFANT", "INF"));

            return customers;
        }

        public static List<CustomerDetails> CreateCustomerDetails(TestConfig config)
        {
            var customers = new List<CustomerDetails>();

            customers.AddRange(GenerateCustomerList(config.Search.Passengers.Adults.Count, "ADULT", "ADT"));
            customers.AddRange(GenerateCustomerList(config.Search.Passengers.YoungAdults.Count, "YOUNG", "GBE"));
            customers.AddRange(GenerateCustomerList(config.Search.Passengers.Children.Count, "CHILD", "CNN"));
            customers.AddRange(GenerateCustomerList(config.Search.Passengers.Infants.Count, "INFANT", "INF"));

            return customers;
        }

        private static IEnumerable<CustomerDetails> GenerateCustomerList(int count, string prefix, string ptc)
        {
            return Enumerable.Range(1, count).Select(i => new CustomerDetails
            {
                custId = $"{prefix}_{i}",
                ptc = ptc
            });
        }

        public static string GetUpsellOfferId(HttpResponseMessage jsonResponse)
        {
            var offerSearchResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var offersIds = offerSearchResult.SelectTokens("$.offers[?(@.upsellOffer == true)].offerId").Select(x => x.ToString()).ToArray();

            // Generate a random index
            int randomIndex = Random.Shared.Next(offersIds.Length);

            // Return the offerId at the random index
            return offersIds[randomIndex];
        }

        public static string[] FindAllFareOrSummaryLinksForSpecificAirlineAndDirectOnly(string jsonResponse, FlightType flightType, string linkType = "fare")
        {
            JObject jsonObject = JObject.Parse(jsonResponse);

            string airlineCode = flightType.ToString();

            // Extract flights that meet the conditions
            var filteredFlights = jsonObject.SelectTokens("$..slice.flightsAndFares[*]")
                .Where(flight =>
                {
                    var segments = flight.SelectToken("flight.segments");
                    if (segments == null || segments.Count() != 1) return false;

                    var segment = segments[0];
                    var operatingAirlineCode = segment.SelectToken("operatingAirline.code")?.ToString();

                    return operatingAirlineCode == airlineCode;
                })
                .ToList();

            // Now, extract the fare links where 'rel' == 'fare'
            var fareLinks = filteredFlights
                .SelectMany(flight =>
                {
                    return flight.SelectTokens($"fares[*].links[?(@.rel == '{linkType}')].href")
                                .Where(link => !string.IsNullOrEmpty(link.ToString()))
                                .Select(link => link.ToString());
                })
                .ToArray();

            return fareLinks ?? Array.Empty<string>();
        }

        public static string[] FindAllFareOrSummaryLinksForVirginCodeShareWithOneStopByFlightShareCode(string jsonResponse, string codeShareFlightCode, string linkType = "fare")
        {
            JObject jsonObject = JObject.Parse(jsonResponse);

            // Filter flights based on the segment and airline code conditions
            var filteredFlights = jsonObject.SelectTokens("$..slice.flightsAndFares[*]")
                .Where(flight =>
                {
                    var segments = flight.SelectToken("flight.segments") as JArray;
                    if (segments == null || segments.Count != 2)
                        return false;

                    var firstSegmentOperatingAirline = segments[0].SelectToken("operatingAirline.code")?.ToString();
                    var secondSegmentOperatingAirline = segments[1].SelectToken("operatingAirline.code")?.ToString();

                    return firstSegmentOperatingAirline == "VS" && secondSegmentOperatingAirline == codeShareFlightCode;
                })
                .ToList();

            // Extract fare links for the filtered flights
            var fareLinks = filteredFlights
                .SelectMany(flight =>
                    flight.SelectTokens($"fares[*].links[?(@.rel == '{linkType}')].href")
                          .Select(link => link.ToString())
                          .Where(link => !string.IsNullOrEmpty(link)))
                .ToArray();

            return fareLinks ?? Array.Empty<string>();
        }

        public static string[] FindAllFareOrSummaryLinksForVirginDirectWithStop(string jsonResponse, string linkType = "fare")
        {
            JObject jsonObject = JObject.Parse(jsonResponse);

            // Extract flights that have segments, operating airline VS, and stopCount == 1
            var filteredFlights = jsonObject.SelectTokens("$..slice.flightsAndFares[*]")
                .Where(flight =>
                {
                    var segments = flight.SelectToken("flight.segments");
                    if (segments == null || segments.Count() != 1) return false;

                    var segment = segments[0];
                    var operatingAirlineCode = segment.SelectToken("operatingAirline.code")?.ToString();
                    var stopCount = segment.SelectToken("stopCount")?.ToObject<int>();

                    return operatingAirlineCode == "VS" && stopCount == 1;
                })
                .ToList();

            // Now, extract the fare links where 'rel' == 'fare'
            var fareLinks = filteredFlights
                .SelectMany(flight =>
                {
                    return flight.SelectTokens($"fares[*].links[?(@.rel == '{linkType}')].href")
                                .Where(link => !string.IsNullOrEmpty(link.ToString()))
                                .Select(link => link.ToString());
                })
                .ToArray();

            return fareLinks ?? Array.Empty<string>();
        }

        public static string[] FindAllFareOrSummaryLinksForMultiSegmentsFlight(string jsonResponse, string linkType = "fare")
        {
            JObject jsonObject = JObject.Parse(jsonResponse);

            // Extract flights that meet the conditions
            var filteredFlights = jsonObject.SelectTokens("$..slice.flightsAndFares[*]")
                .Where(flight =>
                {
                    var segments = flight.SelectToken("flight.segments");
                    return segments != null && segments.Count() == 3;
                })
                .ToList();

            // Now, extract the fare links where 'rel' == 'fare'
            var fareLinks = filteredFlights
                .SelectMany(flight =>
                {
                    return flight.SelectTokens($"fares[*].links[?(@.rel == '{linkType}')].href")
                                .Where(link => !string.IsNullOrEmpty(link.ToString()))
                                .Select(link => link.ToString());
                })
                .ToArray();

            return fareLinks ?? Array.Empty<string>();
        }

        public static string[] FindAllFareOrSummaryLinksForAnyFlight(string jsonResponse, string linkType = "fare")
        {
            JObject jsonObject = JObject.Parse(jsonResponse);

            IEnumerable<JToken> links = jsonObject.SelectTokens("$..slice.flightsAndFares[*].fares[*].links[?(@.rel == '" + linkType + "')].href");

            string[] fareOrSummaryLinks = links
                .Select(link => link.ToString())
                .Where(link => !string.IsNullOrEmpty(link))
                .ToArray();

            return fareOrSummaryLinks ?? Array.Empty<string>();
        }

        public static string[] FindAllSummaryLinksForFareFamilyAndOptionalAirline(string jsonResponse, FareFamilyType fareFamilyType, FlightType? flightType = null)
        {
            JObject jsonObject = JObject.Parse(jsonResponse);
            string fareFamily = fareFamilyType.GetEnumDescription();

            IEnumerable<JToken> flightsAndFares = jsonObject.SelectTokens("$..slice.flightsAndFares[*]");

            if (flightType.HasValue)
            {
                string airlineCode = flightType.Value.ToString();
                flightsAndFares = flightsAndFares.Where(flightAndFare =>
                    flightAndFare.SelectTokens("flight.segments[*].operatingAirline.code")
                        .Any(code => code.ToString() == airlineCode));
            }

            string[] summaryLinks = flightsAndFares
                .SelectMany(flightAndFare =>
                    flightAndFare.SelectTokens($"fares[?(@.fareFamilyType=='{fareFamily}')].links[?(@.rel == 'summary')].href")
                        .Select(link => link.ToString())
                        .Where(link => !string.IsNullOrEmpty(link)))
                .ToArray();

            return summaryLinks ?? Array.Empty<string>();
        }

        public static string[] FindAllFareLinksForFareFamilyAndOptionalAirline(string jsonResponse, FareFamilyType fareFamilyType, FlightType? flightType = null)
        {
            JObject jsonObject = JObject.Parse(jsonResponse);
            string fareFamily = fareFamilyType.GetEnumDescription();

            IEnumerable<JToken> flightsAndFares = jsonObject.SelectTokens("$..slice.flightsAndFares[*]");

            if (flightType.HasValue)
            {
                string airlineCode = flightType.Value.ToString();
                flightsAndFares = flightsAndFares.Where(flightAndFare =>
                    flightAndFare.SelectTokens("flight.segments[*].operatingAirline.code")
                        .Any(code => code.ToString() == airlineCode));
            }

            string[] fareLinks = flightsAndFares
                .SelectMany(flightAndFare =>
                    flightAndFare.SelectTokens($"fares[?(@.fareFamilyType=='{fareFamily}')].links[?(@.rel == 'fare')].href")
                        .Select(link => link.ToString())
                        .Where(link => !string.IsNullOrEmpty(link)))
                .ToArray();

            return fareLinks ?? Array.Empty<string>();
        }

        public static string[] FindAllFareOrSummaryLinksForFareFamilyAndLegOperatingAirline(string jsonResponse, FareFamilyType fareFamilyType, FlightType segmentOperatingCarrier, string legOperatingAirlineCode, string linkType = "summary")
        {
            JObject jsonObject = JObject.Parse(jsonResponse);
            string fareFamily = fareFamilyType.GetEnumDescription();
            string segmentAirlineCode = segmentOperatingCarrier.ToString();

            return jsonObject
                .SelectTokens("$..slice.flightsAndFares[*]")
                .Where(flightAndFare => AllSegmentsUseOperatingCarrier(flightAndFare, segmentAirlineCode))
                .Where(flightAndFare => HasLegOperatingAirline(flightAndFare, legOperatingAirlineCode))
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

        private static bool HasLegOperatingAirline(JToken flightAndFare, string legOperatingAirlineCode)
        {
            return flightAndFare
                .SelectTokens("flight.segments[*].legs[*].operatingAirline.code")
                .Any(code => code.ToString().Is(legOperatingAirlineCode));
        }

        public static string[] ReturnSummaryLinksForVSFlight(string json)
        {
            JObject jsonObject = JObject.Parse(json);

            return jsonObject.SelectTokens("$..slice.flightsAndFares[?( @.flight.segments[ ?( @.operatingAirline.code == 'VS' ) ])].fares[*].links[ ?( @.rel == 'summary') ].href").Select(x => x.ToString()).ToArray();
        }

        public static string[] FindAllFareOrSummaryLinksWithOneStop(string jsonResponse, FlightType flightType, string linkType = "fare")
        {
            JObject jsonObject = JObject.Parse(jsonResponse);

            string airlineCode = flightType.ToString();

            // Filter flights based on the segment and airline code conditions
            var filteredFlights = jsonObject.SelectTokens("$..slice.flightsAndFares[*]")
                .Where(flight =>
                {
                    var segments = flight.SelectToken("flight.segments") as JArray;
                    if (segments == null || segments.Count != 2)
                        return false;

                    var firstSegmentOperatingAirline = segments[0].SelectToken("operatingAirline.code")?.ToString();

                    return firstSegmentOperatingAirline == airlineCode;
                })
                .ToList();

            // Extract fare or summary links for the filtered flights
            var fareOrSummaryLinks = filteredFlights
                .SelectMany(flight =>
                    flight.SelectTokens($"fares[*].links[?(@.rel == '{linkType}')].href")
                          .Select(link => link.ToString())
                          .Where(link => !string.IsNullOrEmpty(link)))
                .ToArray();

            return fareOrSummaryLinks ?? Array.Empty<string>();
        }

        public static string[] FindAllFareOrSummaryLinksWithTwoStop(string jsonResponse, FlightType flightType, string linkType = "fare")
        {
            JObject jsonObject = JObject.Parse(jsonResponse);

            string airlineCode = flightType.ToString();

            // Filter flights based on the segment and airline code conditions
            var filteredFlights = jsonObject.SelectTokens("$..slice.flightsAndFares[*]")
                .Where(flight =>
                {
                    var segments = flight.SelectToken("flight.segments") as JArray;
                    if (segments == null || segments.Count != 3)
                        return false;

                    var firstSegmentOperatingAirline = segments[0].SelectToken("operatingAirline.code")?.ToString();

                    return firstSegmentOperatingAirline == airlineCode;
                })
                .ToList();

            // Extract fare or summary links for the filtered flights
            var fareOrSummaryLinks = filteredFlights
                .SelectMany(flight =>
                    flight.SelectTokens($"fares[*].links[?(@.rel == '{linkType}')].href")
                          .Select(link => link.ToString())
                          .Where(link => !string.IsNullOrEmpty(link)))
                .ToArray();

            return fareOrSummaryLinks ?? Array.Empty<string>();
        }

        public static string[] FindAllFareOrSummaryLinksWithLongLayOver(string jsonResponse, FlightType flightType, int connectionTimeDuration, string linkType = "fare")
        {
            JObject jsonObject = JObject.Parse(jsonResponse);

            string airlineCode = flightType.ToString();

            // Filter flights based on airline + connection > 8 hours
            var filteredFlights = jsonObject.SelectTokens("$..slice.flightsAndFares[*]")
                .Where(flight =>
                {
                    var segments = flight.SelectToken("flight.segments") as JArray;
                    if (segments == null || segments.Count == 0)
                        return false;

                    // Airline condition
                    var firstSegmentOperatingAirline = segments[0].SelectToken("operatingAirline.code")?.ToString();
                    if (firstSegmentOperatingAirline != airlineCode)
                        return false;

                    // Check if any segment has connection > connectionTimeDuration
                    foreach (var seg in segments)
                    {
                        var connection = (string)seg["connection"];
                        if (!string.IsNullOrEmpty(connection))
                        {
                            var duration = XmlConvert.ToTimeSpan(connection);
                            if (duration > TimeSpan.FromHours(connectionTimeDuration))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                })
                .ToList();

            // Extract fare or summary links for the filtered flights
            var fareOrSummaryLinks = filteredFlights
                .SelectMany(flight =>
                    flight.SelectTokens($"fares[*].links[?(@.rel == '{linkType}')].href")
                          .Select(link => link.ToString())
                          .Where(link => !string.IsNullOrEmpty(link)))
                .ToArray();

            return fareOrSummaryLinks ?? Array.Empty<string>();
        }
    }
}
