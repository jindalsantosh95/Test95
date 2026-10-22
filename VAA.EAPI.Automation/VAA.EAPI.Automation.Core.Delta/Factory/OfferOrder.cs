using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Delta.Models.OfferSearch;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Core.Delta.Factory
{
    public static class OfferOrder
    {
        public static string OfferSearch(string deptDate, string requestPayload)
        {
            var payload = File.ReadAllText($"requests\\Body\\Delta\\OfferSearch\\{requestPayload}.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            if (requestPayload.Has("Return"))
            {
                payloadObject["offersCriteria"]["flightRequestCriteria"]["searchOriginDestination"][0]["departureLocalTs"] = deptDate;
                payloadObject["offersCriteria"]["flightRequestCriteria"]["searchOriginDestination"][1]["departureLocalTs"] = deptDate.Parse<DateTime>().AddDays(14).ToString("yyyy-MM-ddT00:00:00");
            }
            else
            {
                payloadObject["offersCriteria"]["flightRequestCriteria"]["searchOriginDestination"][0]["departureLocalTs"] = deptDate;
            }


            return JsonConvert.SerializeObject(payloadObject);
        }
        public static string OrderCreate(string offerid, string offerItemId, string totalAmt)
        {

            var payload = File.ReadAllText($"requests\\OrderCreate\\OrderCreate_Request.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["selectedOffers"][0]["offerId"] = offerid;
            payloadObject["selectedOffers"][0]["selectedOfferItems"][0]["offerItemId"] = offerItemId;
            payloadObject["selectedOffers"][0]["totalOfferAmt"]["currencyEquivalentPrice"]["currencyAmt"] = totalAmt;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string OfferSearchSummary(string offerid)
        {
            var payload = File.ReadAllText($"requests\\Body\\Delta\\OfferSearch\\OfferPrice.json");
            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["offersCriteria"]["flightRequestCriteria"]["selectedOfferId"] = offerid;
            payloadObject["offersCriteria"]["flightRequestCriteria"]["currentTripIndexId"] = 1;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string DeltaOfferSearchRequest(JourneyType journeyType, string departureDate = "", string returnDate = "", LocationType locationOne = LocationType.None, LocationType locationTwo = LocationType.None,
            LocationType locationThree = LocationType.None, LocationType locationFour = LocationType.None, LocationType locationFive = LocationType.None, LocationType locationSix = LocationType.None,
            string cabinTypeCode = "", int numberOfadults = 0, int numberOfchildren = 0, int numberOfyouths = 0, int numberOfinfants = 0, bool calenderSearch = false, string offerId = "", string currentTripIndexId = "0")
        {
            var searchOriginDestination = CreateOriginDestinations(journeyType, locationOne, locationTwo, locationThree, locationFour, locationFive, locationSix, departureDate, returnDate);

            var productGroups = new List<ProductGroup>()
            {
                new ProductGroup()
                {
                    productCategoryCode = "FLIGHTS"
                }
            };

            var customers = new List<Customer>();

            customers.AddRange(Enumerable.Range(1, numberOfadults).Select(i => new Customer()
            {
                passengerTypeCode = "ADT"
            }));

            customers.AddRange(Enumerable.Range(1, numberOfyouths).Select(i => new Customer()
            {
                passengerTypeCode = "GBE"
            }));

            customers.AddRange(Enumerable.Range(1, numberOfchildren).Select(i => new Customer()
            {
                passengerTypeCode = "CHD"
            }));

            customers.AddRange(Enumerable.Range(1, numberOfinfants).Select(i => new Customer()
            {
                passengerTypeCode = "INF"
            }));

            FlightRequestCriteria flightRequestCriteria = new()
            {
                selectedOfferId = offerId,
                calendarSearch = calenderSearch,
                searchOriginDestination = searchOriginDestination.ToList(),
                currentTripIndexId = currentTripIndexId
            };

            PricingCriteria pricingCriteria = new()
            {
                priceableIn = new List<string> { "CURRENCY" },
                currencyCode = "GBP",
                waiveChangeFee = false
            };

            OffersCriteria offersCriteria = new()
            {
                preferences = new(),
                pricingCriteria = pricingCriteria,
                resultsPageNum = 1,
                resultsPerRequestNum = 20,
                flightRequestCriteria = flightRequestCriteria
            };

            DeltaOfferSearchRequest deltaOfferSearchRequest = new()
            {
                productGroups = productGroups,
                offersCriteria = offersCriteria,
                customers = customers
            };

            return JsonConvert.SerializeObject(deltaOfferSearchRequest);
        }

        public static string DeltaOfferGraphqlRequest(JourneyType journeyType, string departureDate = "", string returnDate = "", LocationType locationOne = LocationType.None, LocationType locationTwo = LocationType.None,
           LocationType locationThree = LocationType.None, LocationType locationFour = LocationType.None, LocationType locationFive = LocationType.None, LocationType locationSix = LocationType.None,
           string cabinTypeCode = "", int numberOfadults = 0, int numberOfchildren = 0, int numberOfyouths = 0, int numberOfinfants = 0, bool calenderSearch = false, string offerId = "", string currentTripIndexId = "0")
        {
            var query = File.ReadAllText($"requests\\OfferSearch\\OfferGraphql.txt").Replace(@"\r\n", "\r\n");

            var searchOriginDestination = CreateOriginDestinations(journeyType, locationOne, locationTwo, locationThree, locationFour, locationFive, locationSix, departureDate, returnDate);

            var productGroups = new List<ProductGroup>()
            {
                new ProductGroup()
                {
                    productCategoryCode = "FLIGHTS"
                }
            };

            var customers = new List<Customer>();

            customers.AddRange(Enumerable.Range(1, numberOfadults).Select(i => new Customer()
            {
                passengerTypeCode = "ADT"
            }));

            customers.AddRange(Enumerable.Range(1, numberOfyouths).Select(i => new Customer()
            {
                passengerTypeCode = "GBE"
            }));

            customers.AddRange(Enumerable.Range(1, numberOfchildren).Select(i => new Customer()
            {
                passengerTypeCode = "CHD"
            }));

            customers.AddRange(Enumerable.Range(1, numberOfinfants).Select(i => new Customer()
            {
                passengerTypeCode = "INF"
            }));

            FlightRequestCriteria flightRequestCriteria = new()
            {
                selectedOfferId = offerId,
                calendarSearch = calenderSearch,
                searchOriginDestination = searchOriginDestination.ToList(),
                currentTripIndexId = currentTripIndexId
            };

            PricingCriteria pricingCriteria = new()
            {
                priceableIn = new List<string> { "CURRENCY" },
                currencyCode = "GBP",
                waiveChangeFee = false
            };

            OffersCriteria offersCriteria = new()
            {
                preferences = new(),
                pricingCriteria = pricingCriteria,
                resultsPageNum = 1,
                resultsPerRequestNum = 20,
                flightRequestCriteria = flightRequestCriteria
            };

            OfferSearchCriteria offerSearchCriteria = new()
            {
                productGroups = productGroups,
                offersCriteria = offersCriteria,
                customers = customers,
            };

            Variables variables = new()
            {
                offerSearchCriteria = offerSearchCriteria
            };

            DeltaOfferGraphqlRequest deltaOfferGraphqlRequest = new()
            {
                query = query,
                variables = variables
            };

            return JsonConvert.SerializeObject(deltaOfferGraphqlRequest);
        }

        public static List<Origin> GetOrigin(LocationType origin)
        {
            return new List<Origin>()
            {
                new Origin()
                {
                    airportCode = origin.ToString()
                }
            };
        }

        public static List<Destination> GetDestination(LocationType destination)
        {
            return new List<Destination>()
            {
                new Destination()
                {
                     airportCode = destination.ToString()
                }

            };
        }

        public static SearchOriginDestination[] CreateOriginDestinations(JourneyType journeyType, LocationType locationOne = LocationType.None, LocationType locationTwo = LocationType.None,
           LocationType locationThree = LocationType.None, LocationType locationFour = LocationType.None, LocationType locationFive = LocationType.None, LocationType locationSix = LocationType.None, string departureDate = "", string returnDate = "")
        {
            var SearchOriginDestinations = journeyType switch
            {
                JourneyType.OneWay => new SearchOriginDestination[]
                {
                                new()
                                {
                                    departureLocalTs = departureDate,
                                    destinations = GetDestination(locationTwo),
                                    origins = GetOrigin(locationOne)

                                }
                },
                JourneyType.Return => new SearchOriginDestination[]
                {
                                new()
                                {
                                    departureLocalTs = departureDate,
                                    destinations = GetDestination(locationTwo),
                                    origins = GetOrigin(locationOne)

                                },
                                new()
                                {
                                    departureLocalTs = returnDate,
                                    destinations = GetDestination(locationFour),
                                    origins = GetOrigin(locationThree)

                                }

                },
                JourneyType.MultiCity => new SearchOriginDestination[]
                {
                                new()
                                {
                                    origins = GetOrigin(locationOne),
                                    destinations = GetDestination(locationTwo),
                                    departureLocalTs = departureDate
                                },
                                new()
                                {
                                    origins = GetOrigin(locationThree),
                                    destinations = GetDestination(locationFour),
                                },
                                new()
                                {
                                    origins = GetOrigin(locationFive),
                                    destinations = GetDestination(locationSix),
                                }
                },
                _ => Array.Empty<SearchOriginDestination>()
            };
            return SearchOriginDestinations;
        }
    }
}
