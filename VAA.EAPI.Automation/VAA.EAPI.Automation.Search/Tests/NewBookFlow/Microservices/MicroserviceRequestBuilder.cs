using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;
using BasketCreateRequest = VAA.EAPI.Automation.Search.Models.OfferSearch.BasketCreateRequest;
using BrandMSRequest = VAA.EAPI.Automation.Search.Models.BrandMS.BrandMSRequest;
using CabinType = VAA.EAPI.Automation.Search.Models.OfferSearch.CabinType;
using DeepLinkOfferMSRequest = VAA.EAPI.Automation.Search.Models.DeepLinks.DeepLinkOfferMSRequest;
using FlightDetail = VAA.EAPI.Automation.Search.Models.SustainabilityMS.FlightDetail;
using FlightSearchRequest = VAA.EAPI.Automation.Search.Models.OfferSearch.FlightSearchRequest;
using FlightSegment = VAA.EAPI.Automation.Search.Models.DeepLinks.FlightSegment;
using Itinerary = VAA.EAPI.Automation.Search.Models.BrandMS.Itinerary;
using LoyaltyMember = VAA.EAPI.Automation.Search.Models.DeepLinks.LoyaltyMember;
using OfferMSRequest = VAA.EAPI.Automation.Search.Models.OfferSearch.OfferMSRequest;
using OfferSearchHelper = VAA.EAPI.Automation.Search.Helpers.OfferSearchHelper;
using Parties = VAA.EAPI.Automation.Search.Models.OfferSearch.Parties;
using Person = VAA.EAPI.Automation.Search.Models.OfferSearch.Person;
using Pos = VAA.EAPI.Automation.Search.Models.OfferSearch.Pos;
using PriceMSRequest = VAA.EAPI.Automation.Search.Models.OfferSearch.PriceMSRequest;
using SearchOriginDestination = VAA.EAPI.Automation.Search.Models.OfferSearch.SearchOriginDestination;
using Segment = VAA.EAPI.Automation.Search.Models.BrandMS.Segment;
using SustainabilityMSRequest = VAA.EAPI.Automation.Search.Models.SustainabilityMS.SustainabilityMSRequest;
using TotalOfferedAmount = VAA.EAPI.Automation.Search.Models.DeepLinks.TotalOfferedAmount;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.Microservices
{
    public static class MicroserviceRequestBuilder
    {
        [Obsolete("Marked as obsolete in favour of config driven approach. Please use 'OfferSearchRequest(TestConfig config)'.")]
        public static string OfferSearchRequest(JourneyType journeyType, string departureDate = "", string returnDate = "", LocationType locationOne = LocationType.None, LocationType locationTwo = LocationType.None,
            LocationType locationThree = LocationType.None, LocationType locationFour = LocationType.None, LocationType locationFive = LocationType.None, LocationType locationSix = LocationType.None,
             int numberOfadults = 0, int numberOfchildren = 0, int numberOfyouths = 0, int numberOfinfants = 0, bool flexiDateSearch = false, bool calenderSearch = false, string offerId = "", bool rewardFlag = false, string promoCode = "", LocationType connectionAirportOne = LocationType.None, LocationType connectionAirportTwo = LocationType.None, LocationType connectionAirportThree = LocationType.None, string flightTwoDepartureDate = "", string flightThreeDepartureDate = "", string currentTripIndexId = "0")
        {
            var searchOriginDestination = OfferSearchHelper.CreateOriginDestination(journeyType, locationOne, locationTwo, locationThree, locationFour, departureDate, returnDate, connectionAirportOne, connectionAirportTwo);

            var customers = OfferSearchHelper.CreateCustomerDetails(numberOfadults, numberOfchildren, numberOfyouths, numberOfinfants);

            OfferMSRequest offerMSRequest = new()
            {
                Pos = new Pos() { cityCode = "LON", countryCode = "GB", currencyCode = "GBP" },
                Parties = new Parties(),
                FeatureFlagUseGraphQl = true,
                flightsearchRequest = new FlightSearchRequest()
                {
                    searchOriginDestinations = searchOriginDestination.ToList(),
                    cabinTypes = new List<CabinType>(),
                    fareFamilies = new List<string>(),
                    flexiDateSearch = flexiDateSearch,
                    calendarSearch = calenderSearch,
                    bundleOffer = false,
                    nonStopOnly = false,
                    refundableOnly = false,
                    checkInBaggageAllowance = true,
                    carryOnBaggageAllowance = true,
                    offerID = offerId,
                    awardSearch = rewardFlag,
                    promoCode = promoCode,
                    currentTripIndexId = currentTripIndexId
                },
                customerDetails = customers,
                loyaltyMember = new Models.OfferSearch.LoyaltyMember() { payWithPoints = true }
            };

            return JsonConvert.SerializeObject(offerMSRequest);
        }

        public static string OfferSearchRequest(TestConfig config)
        {
            var searchOriginDestination = OfferSearchHelper.CreateOriginDestination(config);

            var customers = OfferSearchHelper.CreateCustomerDetails(config);

            OfferMSRequest offerMSRequest = new()
            {
                Pos = new Pos() { cityCode = "LON", countryCode = "GB", currencyCode = "GBP" },
                Parties = new Parties(),
                FeatureFlagUseGraphQl = true,
                flightsearchRequest = new FlightSearchRequest()
                {
                    searchOriginDestinations = searchOriginDestination.ToList(),
                    cabinTypes = new List<CabinType>(),
                    fareFamilies = new List<string>(),
                    flexiDateSearch = config.Search.IsFlexiDateSearch,
                    calendarSearch = config.Search.IsCalendarSearch,
                    bundleOffer = false,
                    nonStopOnly = false,
                    refundableOnly = false,
                    checkInBaggageAllowance = true,
                    carryOnBaggageAllowance = true,
                    offerID = config.Internal.OfferId,
                    awardSearch = config.Search.IsRewardSearch,
                    promoCode = config.Search.PromoCode,
                    currentTripIndexId = config.Search.CurrentTripIndexId
                },
                customerDetails = customers,
                loyaltyMember = new Models.OfferSearch.LoyaltyMember() { payWithPoints = true }
            };

            return JsonConvert.SerializeObject(offerMSRequest);
        }

        [Obsolete("Marked as obsolete in favour of config driven approach. Please use 'PriceRequest(TestConfig config)'.")]
        public static string PriceRequest(JourneyType journeyType, string departureDate = "", string returnDate = "", LocationType locationOne = LocationType.None, LocationType locationTwo = LocationType.None,
            LocationType locationThree = LocationType.None, LocationType locationFour = LocationType.None, LocationType locationFive = LocationType.None, LocationType locationSix = LocationType.None,
            string cabinTypeCode = "", int numberOfadults = 0, int numberOfchildren = 0, int numberOfyouths = 0, int numberOfinfants = 0, string offerId = "", bool rewardFlag = false, string flightTwoDepartureDate = "", string flightThreeDepartureDate = "")
        {
            var originDestination = OfferSearchHelper.CreateOriginDestination(journeyType, locationOne, locationTwo, locationThree, locationFour, departureDate, returnDate);

            var customers = OfferSearchHelper.CreateCustomerDetails(numberOfadults, numberOfchildren, numberOfyouths, numberOfinfants);

            PriceMSRequest priceRequest = new()
            {
                Pos = new Pos(),
                Parties = new Parties(),
                flightsearchRequest = new FlightSearchRequest()
                {
                    searchOriginDestinations = originDestination.ToList(),
                    cabinTypes = new List<CabinType>(),
                    fareFamilies = new List<string>(),
                    offerID = offerId,
                    carryOnBaggageAllowance = true,
                    checkInBaggageAllowance = true,
                    awardSearch = rewardFlag,
                    currentTripIndexId = journeyType switch
                    {
                        JourneyType.OneWay => "1",
                        JourneyType.Return => "2",
                        JourneyType.MultiCity => "3",
                        _ => "",
                    }
                },
                customerDetails = customers
            };

            return JsonConvert.SerializeObject(priceRequest);
        }

        public static string PriceRequest(TestConfig config)
        {
            var originDestination = OfferSearchHelper.CreateOriginDestination(config);

            var customers = OfferSearchHelper.CreateCustomerDetails(config);

            PriceMSRequest priceRequest = new()
            {
                Pos = new Pos(),
                Parties = new Parties(),
                flightsearchRequest = new FlightSearchRequest()
                {
                    searchOriginDestinations = originDestination.ToList(),
                    cabinTypes = new List<CabinType>(),
                    fareFamilies = new List<string>(),
                    offerID = config.Internal.OfferId,
                    carryOnBaggageAllowance = true,
                    checkInBaggageAllowance = true,
                    awardSearch = config.Search.IsRewardSearch,
                    currentTripIndexId = config.Search.JourneyType switch
                    {
                        JourneyType.OneWay => "1",
                        JourneyType.Return => "2",
                        JourneyType.MultiCity => "3",
                        _ => "",
                    }
                },
                customerDetails = customers
            };

            return JsonConvert.SerializeObject(priceRequest);
        }

        public static string SustainabilityMSRequest(LocationType origin = LocationType.None, LocationType destination = LocationType.None, string flightNumber = "", string flightDate = "", string cabinType = "", int passengersCount = 1)
        {
            var flightDetails = new List<FlightDetail>()
            {
               new FlightDetail()
               {
                  RequestID = "",
                  OriginAirport = origin.ToString(),
                  DestinationAirport = destination.ToString(),
                  FlightNumber = flightNumber,
                  FlightDate = flightDate,
                  Cabin = cabinType
               }
            };

            var sustainabilityRequest = new SustainabilityMSRequest()
            {
                passengers = passengersCount,
                flightDetails = flightDetails
            };

            return JsonConvert.SerializeObject(sustainabilityRequest);
        }

        public static string CreateBrandMSRequest(HttpResponseMessage offerSearchResponse, string cabinTypeCode = "1", bool rewardFlag = false)
        {
            JToken offerSearchResult = JsonConvert.DeserializeObject<JToken>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var flightSegmentsRefIds = offerSearchResult.SelectTokens("$.offers[*].offerItems[*].services[*].flightSegmentsRefIds").Select(x => x.ToString()).ToArray();

            string[] segmentsRefIds = JsonConvert.DeserializeObject<string[]>(flightSegmentsRefIds[0]);

            List<Segment> segments = new List<Segment>();

            foreach (string flightSegmentsRefId in segmentsRefIds)
            {
                var segmentToken = offerSearchResult.SelectToken($"$.dataList.flightSegments[?(@.segmentId == '{flightSegmentsRefId}')]");

                string fareBasisCode = OfferSearchHelper.ReturnFareBasisCodeBasedOnFlightSegmentsRefId(offerSearchResponse, flightSegmentsRefId);

                if (segmentToken != null)
                {
                    var operatingFlightNumber = segmentToken.Value<string>("marketingFlightNumber");
                    var operatingCarrier = segmentToken.Value<string>("marketingCarrier");
                    var originAirportCode = segmentToken.Value<string>("originAirportCode");
                    var destinationAirportCode = segmentToken.Value<string>("destinationAirportCode");
                    var scheduledDepartureTime = segmentToken["scheduledDepartureTime"]["localDateTime"].Value<string>();

                    segments.Add(new Segment
                    {
                        carrier = operatingCarrier,
                        flightNumber = operatingFlightNumber,
                        departureDate = scheduledDepartureTime,
                        origin = originAirportCode,
                        destination = destinationAirportCode,
                        cabin = cabinTypeCode,
                        fareBasis = fareBasisCode
                    });
                }
            }

            var passengerTypeCodes = new List<string>();
            if (rewardFlag)
            {
                passengerTypeCodes.Add("PFC");
            }
            else
            {
                passengerTypeCodes.Add("ADT");
            }

            var itinerary = new List<Itinerary>()
            {
                new Itinerary()
                {
                    source = rewardFlag ? "1P" : null,
                    segments = segments,
                    passengerTypeCodes = passengerTypeCodes
                }
            };

            var brandMSRequest = new BrandMSRequest()
            {
                itinerary = itinerary,
                pseudoCityCode = rewardFlag ? "GZC" : null,
                accountCode = rewardFlag ? "FCREWARD" : null
            };

            return JsonConvert.SerializeObject(brandMSRequest);
        }

        [Obsolete("Marked as obsolete in favour of config driven approach. Please use 'CreateBasketRequest(TestConfig config)'.")]
        public static string CreateBasketRequest(LocationType origin = LocationType.None, LocationType destination = LocationType.None, int numberOfadults = 0, int numberOfchildren = 0, int numberOfyouths = 0, int numberOfinfants = 0)
        {
            var deptDateTime = DateTime.UtcNow.AddDays(166).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 130;

            string departureDate = OfferSearchHelper.RandomDate(deptDateTime, maxNumberOfDays);

            var originDestination = new List<SearchOriginDestination>
            {
                new SearchOriginDestination
                {
                    origin = origin.ToString(),
                    destination = destination.ToString(),
                    departureDate = departureDate
                }
            };

            var peopleDetails = new List<Person>();

            peopleDetails.AddRange(Enumerable.Range(1, numberOfadults).Select(i => new Person()
            {
                id = "",
                title = "",
                firstName = "",
                lastName = "",
                dateOfBirth = "",
                gender = "",
                type = "ADULT",
                validation = ""
            }));

            peopleDetails.AddRange(Enumerable.Range(1, numberOfyouths).Select(i => new Person()
            {
                id = "",
                title = "",
                firstName = "",
                lastName = "",
                dateOfBirth = "",
                gender = "",
                type = "YOUNG",
                validation = ""
            }));

            peopleDetails.AddRange(Enumerable.Range(1, numberOfchildren).Select(i => new Person()
            {
                id = "",
                title = "",
                firstName = "",
                lastName = "",
                dateOfBirth = "",
                gender = "",
                type = "CHILD",
                validation = ""
            }));

            peopleDetails.AddRange(Enumerable.Range(1, numberOfinfants).Select(i => new Person()
            {
                id = "",
                title = "",
                firstName = "",
                lastName = "",
                dateOfBirth = "",
                gender = "",
                type = "INFANT",
                validation = ""
            }));

            BasketCreateRequest basketCreateRequest = new()
            {
                flightsearchRequest = new FlightSearchRequest()
                {
                    searchOriginDestinations = originDestination,
                    cabinTypes = new List<CabinType>(),
                    fareFamilies = new List<string>(),
                    flexiDateSearch = false,
                    calendarSearch = false,
                    bundleOffer = false,
                    nonStopOnly = false,
                    refundableOnly = false,
                    checkInBaggageAllowance = true,
                    carryOnBaggageAllowance = true,
                    currentTripIndexId = "0"
                },
                people = peopleDetails
            };

            return JsonConvert.SerializeObject(basketCreateRequest);
        }

        public static string CreateBasketRequest(TestConfig config)
        {
            int numberOfadults = config.Search.Passengers.Adults.Count;

            var originDestination = new List<SearchOriginDestination>
            {
                new SearchOriginDestination
                {
                    origin = config.Search.Origin.ToString(),
                    destination = config.Search.Destination.ToString(),
                    departureDate = config.Search.DepartureDate,
                }
            };

            var peopleDetails = new List<Person>();

            peopleDetails.AddRange(Enumerable.Range(1, config.Search.Passengers.Adults.Count).Select(i => new Person()
            {
                id = "",
                title = "",
                firstName = "",
                lastName = "",
                dateOfBirth = "",
                gender = "",
                type = "ADULT",
                validation = ""
            }));

            peopleDetails.AddRange(Enumerable.Range(1, config.Search.Passengers.YoungAdults.Count).Select(i => new Person()
            {
                id = "",
                title = "",
                firstName = "",
                lastName = "",
                dateOfBirth = "",
                gender = "",
                type = "YOUNG",
                validation = ""
            }));

            peopleDetails.AddRange(Enumerable.Range(1, config.Search.Passengers.Children.Count).Select(i => new Person()
            {
                id = "",
                title = "",
                firstName = "",
                lastName = "",
                dateOfBirth = "",
                gender = "",
                type = "CHILD",
                validation = ""
            }));

            peopleDetails.AddRange(Enumerable.Range(1, config.Search.Passengers.Infants.Count).Select(i => new Person()
            {
                id = "",
                title = "",
                firstName = "",
                lastName = "",
                dateOfBirth = "",
                gender = "",
                type = "INFANT",
                validation = ""
            }));

            BasketCreateRequest basketCreateRequest = new()
            {
                flightsearchRequest = new FlightSearchRequest()
                {
                    searchOriginDestinations = originDestination,
                    cabinTypes = new List<CabinType>(),
                    fareFamilies = new List<string>(),
                    flexiDateSearch = false,
                    calendarSearch = false,
                    bundleOffer = false,
                    nonStopOnly = false,
                    refundableOnly = false,
                    checkInBaggageAllowance = true,
                    carryOnBaggageAllowance = true,
                    currentTripIndexId = "0"
                },
                people = peopleDetails
            };

            return JsonConvert.SerializeObject(basketCreateRequest);
        }

        public static string DeepLinkOfferSummaryRequest(JourneyType journeyType, HttpResponseMessage offerSummaryResponse, CabinName cabinName, int numberOfadults = 0, int numberOfchildren = 0, int numberOfyouths = 0, int numberOfinfants = 0)
        {
            var flightSegmentList = new List<FlightSegment>();

            var customerDetails = OfferSearchHelper.CreateCustomerDetails(numberOfadults, numberOfchildren, numberOfyouths, numberOfinfants);

            JToken summaryResult = JsonConvert.DeserializeObject<JToken>(offerSummaryResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var summaryTotalAmount = summaryResult.SelectToken("$.offers[?(@.selected == true)].salePrice[?(@.priceComponentType == 'BasePrice')].amount.monetaryAmount")?.ToString();

            var baseCurrency = summaryResult.SelectToken("$.offers[?(@.selected == true)].salePrice[?(@.priceComponentType == 'BasePrice')].amount.baseCurrency")?.ToString();

            var flightSegmentsRefIds = summaryResult.SelectTokens("$.offers[?(@.selected == true)].offerItems[*].services[*].flightSegmentsRefIds[*]").Select(x => x.ToString()).ToArray();

            foreach (var (flightSegmentRefId, index) in flightSegmentsRefIds.Select((id, idx) => (id, idx + 1)))
            {
                var segmentToken = summaryResult.SelectToken($"$.dataList.flightSegments[?(@.segmentId == '{flightSegmentRefId}')]");
                var classOfService = summaryResult.SelectToken($"$.offers[?(@.selected == true)].offerItems[*].services[*].fareDetails.fareSegments[?(@.flightSegmentRefID == '{flightSegmentRefId}')].bookingClass")?.ToString();
                var fareBasisCode = summaryResult.SelectToken($"$.offers[?(@.selected == true)].offerItems[*].services[*].fareDetails.fareSegments[?(@.flightSegmentRefID == '{flightSegmentRefId}')].fareBasisCode")?.ToString();

                if (segmentToken != null)
                {
                    var flightSegment = CreateFlightSegment(segmentToken, fareBasisCode, classOfService, index);

                    flightSegmentList.Add(flightSegment);
                }
            }

            var totalOfferedAmount = new TotalOfferedAmount
            {
                currency = baseCurrency,
                value = Convert.ToDouble(summaryTotalAmount)
            };

            var deepLinkOfferMSRequest = new DeepLinkOfferMSRequest
            {
                metaPartnerId = "Skyscanner",
                pos = new Pos { cityCode = "LON", countryCode = "GB", currencyCode = "GBP" },
                flightSegments = flightSegmentList,
                customerDetails = customerDetails,
                loyaltyMember = new LoyaltyMember { payWithPoints = true },
                currentTripIndexId = journeyType switch
                {
                    JourneyType.OneWay => "1",
                    JourneyType.Return => "2",
                    _ => "",
                },
                dominantSegmentBrandId = cabinName.ToString(),
                totalOfferedAmount = totalOfferedAmount
            };

            return JsonConvert.SerializeObject(deepLinkOfferMSRequest);
        }

        private static FlightSegment CreateFlightSegment(JToken segmentToken, string fareBasisCode, string classOfService, int segmentNumber)
        {
            var marketingCarrier = segmentToken.Value<string>("marketingCarrier");
            var marketingFlightNumber = segmentToken.Value<string>("marketingFlightNumber");
            var scheduledDepartureLocalDateTime = segmentToken["scheduledDepartureTime"]["localDateTime"].Value<string>();
            var originAirportCode = segmentToken.Value<string>("originAirportCode");
            var destinationAirportCode = segmentToken.Value<string>("destinationAirportCode");

            return new FlightSegment
            {
                sliceNumber = segmentNumber,
                segmentId = segmentNumber,
                marketingCarrierCode = marketingCarrier,
                marketingFlightNumber = marketingFlightNumber,
                originAirportCode = originAirportCode,
                destinationAirportCode = destinationAirportCode,
                marketingClassOfService = classOfService,
                scheduledDepartureLocalDateTime = scheduledDepartureLocalDateTime,
                fareBasisCode = fareBasisCode
            };
        }
    }
}