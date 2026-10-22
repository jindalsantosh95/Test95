using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using LocationType = VAA.EAPI.Automation.Core.Helpers.Enums.LocationType;

namespace VAA.EAPI.Automation.NDC.Helpers
{
    public static class OfferSearchRequestBuilderHelper
    {
        private static Random rnd = new Random();

        private readonly static string departureDate = OfferMicroserviceHelper.DepartureDate();
        private readonly static string returnDate = OfferMicroserviceHelper.ReturnDate(departureDate);
        private readonly static string dateThree = Convert.ToDateTime(returnDate).AddDays(6).ToString("yyyy-MM-dd");
        private readonly static string dateFour = Convert.ToDateTime(dateThree).AddDays(8).ToString("yyyy-MM-dd");
        private readonly static string dateFive = Convert.ToDateTime(dateFour).AddDays(7).ToString("yyyy-MM-dd");
        private readonly static string dateSix = Convert.ToDateTime(dateFive).AddDays(6).ToString("yyyy-MM-dd");


        public static string OfferSearchRequest(JourneyType journeyType, LocationType locationOne = LocationType.None, LocationType locationTwo = LocationType.None,
            LocationType locationThree = LocationType.None, LocationType locationFour = LocationType.None, LocationType locationFive = LocationType.None, LocationType locationSix = LocationType.None, LocationType locationSeven = LocationType.None, LocationType locationEight = LocationType.None, LocationType locationNine = LocationType.None, LocationType locationTen = LocationType.None, LocationType locationEleven = LocationType.None, LocationType locationTwelve = LocationType.None,
            List<string> fareFamilies = null, string cabinTypeCode = "", int numberOfadults = 0, int numberOfchildren = 0, int numberOfyouths = 0, int numberOfinfants = 0, string cityCode = "LON", string countryCode = "GB", string currencyCode = "GBP", string partyType = "Seller", string name = "Skyscanner", string agencyId = "91281035", string ordinal = "1", bool flexiDateSearch = false, bool calenderSearch = false, string offerId = "")
        {

            var SearchOriginDestinations = CreateOriginDestinations(journeyType, locationOne, locationTwo, locationThree, locationFour, locationFive, locationSix, locationSeven, locationEight, locationNine, locationTen, locationEleven, locationTwelve, departureDate, returnDate, dateThree, dateFour, dateFive, dateSix);

            var customers = new List<dynamic>();

            customers.AddRange(Enumerable.Range(1, numberOfadults).Select(i => new
            {
                custId = $"ADULT_{i}",
                ptc = "ADT"
            }));

            customers.AddRange(Enumerable.Range(1, numberOfyouths).Select(i => new
            {
                custId = $"YOUNG_{i}",
                ptc = "GBE"
            }));

            customers.AddRange(Enumerable.Range(1, numberOfchildren).Select(i => new
            {
                custId = $"CHILD_{i}",
                ptc = "CNN"
            }));

            customers.AddRange(Enumerable.Range(1, numberOfinfants).Select(i => new
            {
                custId = $"INFANT_{i}",
                ptc = "INF"
            }));

            var offerSearchRequest = new
            {
                Pos = new { cityCode = cityCode, countryCode = countryCode, currencyCode = currencyCode },
                Parties = new
                {
                    participants = CreateParticipants(partyType, name, agencyId, ordinal).ToList()
                },
                flightsearchRequest = new
                {
                    searchOriginDestinations = SearchOriginDestinations.ToList(),
                    cabinTypes = new List<dynamic>(),
                    fareFamilies = fareFamilies,
                    flexiDateSearch = flexiDateSearch,
                    calendarSearch = calenderSearch,
                    bundleOffer = false,
                    nonStopOnly = false,
                    refundableOnly = false,
                    checkInBaggageAllowance = true,
                    carryOnBaggageAllowance = true,
                    offerID = offerId,
                },
                customerDetails = customers,
                loyaltyMember = new { payWithPoints = false }
            };

            return JsonConvert.SerializeObject(offerSearchRequest);
        }

        public static string OfferPriceRequest(JourneyType journeyType, string departureDate = "", string returnDate = "", LocationType locationOne = LocationType.None, LocationType locationTwo = LocationType.None,
            LocationType locationThree = LocationType.None, LocationType locationFour = LocationType.None, LocationType locationFive = LocationType.None, LocationType locationSix = LocationType.None, LocationType locationSeven = LocationType.None, LocationType locationEight = LocationType.None, LocationType locationNine = LocationType.None, LocationType locationTen = LocationType.None, LocationType locationEleven = LocationType.None, LocationType locationTwelve = LocationType.None,
            List<string> fareFamilies = null, string cabinTypeCode = "", int numberOfadults = 0, int numberOfchildren = 0, int numberOfyouths = 0, int numberOfinfants = 0, string cityCode = "LON", string countryCode = "GB", string currencyCode = "GBP", string partyType = "Seller", string name = "Skyscanner", string agencyId = "91281035", string ordinal = "1", bool flexiDateSearch = false, bool calenderSearch = false, dynamic selectedOffer = null)
        {
            var originDestination = CreateOriginDestinations(journeyType, locationOne, locationTwo, locationThree, locationFour, locationFive, locationSix, locationSeven, locationEight, locationNine, locationTen, locationEleven, locationTwelve, departureDate, returnDate, dateThree, dateFour, dateFive, dateSix);

            var customers = CreateCustomerDetails(numberOfadults, numberOfchildren, numberOfyouths, numberOfinfants);


            var offerPriceRequest = new
            {
                pos = new { cityCode = cityCode, countryCode = countryCode, currencyCode = currencyCode },
                parties = new
                {
                    participants = CreateParticipants(partyType, name, agencyId, ordinal).ToList()
                },
                repriceQueries = new
                {
                    searchCriteria = new
                    {
                        checkInBaggageAllowance = true,
                        carryOnBaggageAllowance = true,
                        fareFamilies = fareFamilies
                    }

                },
                pricedOffer = selectedOffer,
                customerDetails = customers
            };

            return JsonConvert.SerializeObject(offerPriceRequest);
        }

        public static string SeatsRetrievalRequest(int numberOfadults = 0, int numberOfchildren = 0, int numberOfyouths = 0, int numberOfinfants = 0, string currencyCode = "", string channelId = "", dynamic flightSegments = null)
        {
            var customers = CreateCustomerDetails(numberOfadults, numberOfchildren, numberOfyouths, numberOfinfants);

            var seatsRequest = new
            {
                context = new
                {
                    currencyCode = currencyCode,
                },
                flightSegments = flightSegments,
                channelId = channelId,
                customerDetails = customers
            };


            return JsonConvert.SerializeObject(seatsRequest);
        }

        private static List<dynamic> CreateCustomerDetails(int numberOfAdults, int numberOfChildren, int numberOfYouths, int numberOfInfants)
        {
            var customers = new List<dynamic>();

            customers.AddRange(GenerateCustomerList(numberOfAdults, "ADULT", "ADT"));
            customers.AddRange(GenerateCustomerList(numberOfYouths, "YOUNG", "GBE"));
            customers.AddRange(GenerateCustomerList(numberOfChildren, "CHILD", "CNN"));
            customers.AddRange(GenerateCustomerList(numberOfInfants, "INFANT", "INF"));

            return customers;
        }

        private static IEnumerable<dynamic> GenerateCustomerList(int count, string prefix, string ptc)
        {
            return Enumerable.Range(1, count).Select(i => new
            {
                custId = $"{prefix}_{i}",
                ptc = ptc
            });
        }

        public static dynamic[] CreateParticipants(string partyType, string name, string agencyId, string ordinal)
        {
            var participantsList = new dynamic[]
                {
                    new
                    {
                        partyType = partyType,
                        name = name,
                        agencyId = agencyId,
                        ordinal = ordinal
                    }
                };
            return participantsList;
        }

        public static dynamic[] CreateOriginDestinations(JourneyType journeyType, LocationType locationOne = LocationType.None, LocationType locationTwo = LocationType.None,
            LocationType locationThree = LocationType.None, LocationType locationFour = LocationType.None, LocationType locationFive = LocationType.None, LocationType locationSix = LocationType.None, LocationType locationSeven = LocationType.None, LocationType locationEight = LocationType.None, LocationType locationNine = LocationType.None, LocationType locationTen = LocationType.None, LocationType locationEleven = LocationType.None, LocationType locationTwelve = LocationType.None, string departureDate = "", string dateTwo = "", string dateThree = "", string dateFour = "", string dateFive = "", string dateSix = "")
        {
            var SearchOriginDestinations = journeyType switch
            {
                JourneyType.OneWay => new dynamic[]
                {
                    new
                    {
                        origin = locationOne.ToString(),
                        destination = locationTwo.ToString(),
                        departureDate = departureDate
                    }
                },
                JourneyType.Return => new dynamic[]
                {
                    new
                    {
                        origin = locationOne.ToString(),
                        destination = locationTwo.ToString(),
                        departureDate = departureDate
                    },
                    new
                    {
                        origin = (locationThree == LocationType.None) ? locationTwo.ToString() : locationThree.ToString(),
                        destination = (locationFour == LocationType.None) ? locationOne.ToString() : locationFour.ToString(),
                        departureDate = dateTwo
                    }
            },
                JourneyType.MultiCity => new[]
                {
                    locationOne != LocationType.None && locationTwo != LocationType.None
                    ? new
                    {
                        origin = locationOne.ToString(),
                        destination = locationTwo.ToString(),
                        departureDate = departureDate
                    }
                    : null,
                    locationThree != LocationType.None && locationFour != LocationType.None
                    ? new
                    {
                        origin = locationThree.ToString(),
                        destination = locationFour.ToString(),
                        departureDate = dateTwo
                    }
                    : null,
                    locationFive != LocationType.None && locationSix != LocationType.None
                    ? new
                    {
                        origin = locationFive.ToString(),
                        destination = locationSix.ToString(),
                        departureDate = dateThree
                    }
                    : null,
                    locationSeven != LocationType.None && locationEight != LocationType.None
                    ? new
                    {
                        origin = locationSeven.ToString(),
                        destination = locationEight.ToString(),
                        departureDate = dateFour
                    }
                    : null,
                    locationNine != LocationType.None && locationTen != LocationType.None
                    ? new
                    {
                        origin = locationNine.ToString(),
                        destination = locationTen.ToString(),
                        departureDate = dateFive
                    }
                    : null,
                    locationEleven != LocationType.None && locationTwelve != LocationType.None
                    ? new
                    {
                        origin = locationEleven.ToString(),
                        destination = locationTwelve.ToString(),
                        departureDate = dateSix
                    }
                    : null
            }.Where(segment => segment != null).ToArray(),
            };
            return SearchOriginDestinations;
        }
    }
}
