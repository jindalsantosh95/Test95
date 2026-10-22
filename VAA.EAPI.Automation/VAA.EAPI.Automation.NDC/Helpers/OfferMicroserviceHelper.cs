using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.NDC.Helpers
{
    public static class OfferMicroserviceHelper
    {
        public static List<string> ReturnFareFamilies()
        {
            var fareFamilies = new List<FareFamilyType>
            {
                FareFamilyType.ECONOMYDELIGHT,
                FareFamilyType.ECON,
                FareFamilyType.ECONCLASSICFLEXIBLE,
                FareFamilyType.ECONDELIGHTFLEXIBLE,
                FareFamilyType.COMFORTPLUSPREMIUMECON,
                FareFamilyType.BUSFIRST,
                FareFamilyType.BUSFIRSTFLEXIBLE
            };

            return fareFamilies.Select(x => x.GetEnumDescription())
                               .ToList();
        }

        public static List<string> ReturnExpectedCabins()
        {
            var expectedCabinList = new List<string>
            {
                "Business", "Economy", "Premium Economy"
            };

            return expectedCabinList;
        }

        public static dynamic ReturnVirginSegmentId(HttpResponseMessage jsonResponse)
        {
            var offerPriceResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var offerIdsForVirginFlights = offerPriceResult
                .SelectTokens("$..*")
                .OfType<JObject>()
                .Where(segment =>
                    segment["odRefId"] != null &&
                    segment["segmentId"] != null &&
                    (segment["operatingCarrier"] == null || segment["operatingCarrier"].Type == JTokenType.Null))
                .Select(x => new
                {
                    segmentId = x["segmentId"].ToString(),
                    marketingCarrier = "VS",
                    marketingFlightNumber = x["marketingFlightNumber"].ToString(),
                    operatingCarrier = "VS",
                    operatingFlightNumber = x["marketingFlightNumber"].ToString(),
                    originAirportCode = x["originAirportCode"].ToString(),
                    destinationAirportCode = x["destinationAirportCode"].ToString(),
                    classOfService = "A",
                    scheduledArrivalTime = new
                    {
                        localDateTime = x["scheduledArrivalTime"]["localDateTime"].ToString()
                    },
                    scheduledDepartureTime = new
                    {
                        localDateTime = x["scheduledDepartureTime"]["localDateTime"].ToString()
                    }
                });

            return offerIdsForVirginFlights.FirstOrDefault();
        }

        public static dynamic ReturnFirstSelectedVirginOffer(HttpResponseMessage jsonResponse)
        {
            var offerSearchResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var segmentId = ReturnVirginSegmentId(jsonResponse).segmentId;

            var query = $"$.offers[?( @.offerItems[?( @.services[?( @.journeyRefIds[?( @ == $.dataList.journey[?( @.flightSegmentRefIds[?( @ == '{segmentId}' )] )].journeyId) ] ) ]  )] )]";

            var offer = offerSearchResult.SelectTokens(query).FirstOrDefault();

            var shoppingRefId = offerSearchResult.SelectToken("$.shoppingResponseId").ToString();

            var offerRefId = offer.SelectToken("$.offerId");

            var offerItemsBlock = (JArray)offer.SelectToken("$.offerItems");

            var offerItems = offerItemsBlock.Select(x => new
            {
                offerItemId = x.SelectToken("$.offerItemId").ToString(),
                serviceBeneficiaries = ((JArray)x.SelectToken("$.services[0].serviceBeneficiaries")).Select(y => y.ToString())
            });

            return new
            {
                selectedOffer = new
                {
                    shoppingRefId = shoppingRefId,
                    offerRefId = offerRefId,
                    offerItems = offerItems
                }
            };
        }

        public static dynamic ReturnFlightSegmentsForVirginFlights(HttpResponseMessage jsonResponse)
        {
            var offerPriceResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var segmentsWithoutOperatingCarrier = offerPriceResult
                .SelectTokens("$..*")
                .OfType<JObject>()
                .Where(segment =>
                    segment["odRefId"] != null &&
                    segment["segmentId"] != null &&
                    (segment["operatingCarrier"] == null || segment["operatingCarrier"].Type == JTokenType.Null))
                .Select(x => new 
                {
                    segmentId = x["segmentId"].ToString(),
                    marketingCarrier = "VS",
                    marketingFlightNumber = x["marketingFlightNumber"].ToString(),
                    operatingCarrier = "VS",
                    operatingFlightNumber = x["marketingFlightNumber"].ToString(),
                    originAirportCode = x["originAirportCode"].ToString(),
                    //originAirportName = x[""].ToString(),
                    destinationAirportCode = x["destinationAirportCode"].ToString(),
                    //destinationAirportName = x[""].ToString(),
                    classOfService = "A",
                    scheduledArrivalTime = new
                    {
                        localDateTime = x["scheduledArrivalTime"]["localDateTime"].ToString()
                    },
                    scheduledDepartureTime = new
                    {
                        localDateTime = x["scheduledDepartureTime"]["localDateTime"].ToString()
                    }
                });

            return segmentsWithoutOperatingCarrier;
        }

        public static string DepartureDate()
        {
            Random rnd = new Random();
            DateTime randomMonth = DateTime.Now.AddMonths(rnd.Next(5, 10));

            // Get the total number of days in the selected month
            int daysInMonth = DateTime.DaysInMonth(randomMonth.Year, randomMonth.Month);

            // Generate a random day within the selected month
            int randomDay = rnd.Next(1, daysInMonth + 1);

            // Create the final random date
            DateTime departureDate = new DateTime(randomMonth.Year, randomMonth.Month, randomDay);

            // Format the date as "yyyy-MM-ddTHH:mm:ss"
            string formattedDate = departureDate.ToString("yyyy-MM-dd");

            return formattedDate;
        }

        public static string ReturnDate(string departureDate)
        {
            string returnDate = departureDate.Parse<DateTime>().AddDays(7).ToString("yyyy-MM-dd");

            return returnDate;
        }
    }
}