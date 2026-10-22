using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.NDC.Tests.Microservices.Seats
{
    public class SeatsValidator
    {
        public static bool ValidateSeatOfferId(HttpResponseMessage response, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            JToken seatResult = JsonConvert.DeserializeObject<JToken>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string seatOfferid = seatResult.SelectToken("$.seatOfferId").ToString();

            if (seatOfferid.IsNullOrWhiteSpace())
            {
                result.Message.Add("Seat Offer Id was null or white space");
            }

            return result.IsValid;
        }

        public static bool ValidateFlightSegmentSeatMapCount(HttpResponseMessage response, int flightSegmentCount, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            JToken seatResult = JsonConvert.DeserializeObject<JToken>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            IEnumerable<JToken> flightSegmentSeatMaps = seatResult.SelectTokens("$.flightSegmentSeatMap[*].flightSegment");

            int flightSegmentSeatMapsCount = flightSegmentSeatMaps.Count();

            if (flightSegmentSeatMapsCount != flightSegmentCount)
            {
                result.Message.Add($"Flight segement seat map count {flightSegmentSeatMapsCount} does not match the flight segment count from offer price {flightSegmentCount}");
            }

            return result.IsValid;
        }

        public static bool ValidateSeatPtcs(string request, HttpResponseMessage response, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();
            
            JToken flightSearchRequest = JsonConvert.DeserializeObject<JToken>(request);

            JToken seatResult = JsonConvert.DeserializeObject<JToken>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string[] ptcs = flightSearchRequest.SelectTokens("$.customerDetails[*].ptc").Select(x => x.ToString()).Distinct().ToArray();

            string[] ptcsReturned = seatResult.SelectTokens("$.flightSegmentSeatMap[*].seatmap[*].seatRows[*].seatColumns[*].eligibility[*].customerPTC[*]").Select(x => x.ToString()).Distinct().ToArray();

            if (!ptcsReturned.All(x => ptcs.Contains(x)))
            {
                result.Message.Add($"PTCs did not match from the original request begin {ptcs} and the seat map response being {ptcsReturned}");
            }

            return result.IsValid;
        }

        public static bool ValidateSeatMapData(HttpResponseMessage response, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            JToken seatResult = JsonConvert.DeserializeObject<JToken>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string[] availableSeats = seatResult.SelectTokens("$.flightSegmentSeatMap[*].seatmap[*].seatRows[*].seatColumns[*].available").Select(x => x.ToString()).ToArray();

            int availableSeatsCount = availableSeats.Count();

            if (availableSeatsCount == 0)
            {
                result.Message.Add($"No available seats returned");
            }

            IEnumerable<string> ownerAirline = seatResult.SelectTokens("$.flightSegmentSeatMap[*].seatmap[*].seatRows[*].seatColumns[*].eligibility[*].offerItemId[*].ownerAirline").Select(x => x.ToString()).ToArray().Distinct();

            if (!ownerAirline.Contains("VS"))
            {
                result.Message.Add($"No available seats returned");
            }

            return result.IsValid;
        }

        public static bool ValidateCatalogueData(string request, HttpResponseMessage response, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            JToken flightSearchRequest = JsonConvert.DeserializeObject<JToken>(request);

            string expectedBaseCurrency = flightSearchRequest.SelectToken("$.Pos.currencyCode").ToString();

            JToken seatResult = JsonConvert.DeserializeObject<JToken>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            double[] seatPrices = seatResult.SelectTokens("$.flightSegmentSeatMap[*].catalogue[*].seatPrice.monetaryAmount").Select(x => x.ToString().Parse<double>()).ToArray();

            IEnumerable<string> baseCurrencies = seatResult.SelectTokens("$.flightSegmentSeatMap[*].catalogue[*].seatPrice.baseCurrency").Select(x => x.ToString()).ToArray().Distinct();

            int freeSeats = seatPrices.Count(x => x == 0);

            int paidSeats = seatPrices.Count(x => x > 0);

            if (freeSeats == 0)
            {
                result.Message.Add($"No free seats returned");
            }

            if (paidSeats == 0)
            {
                result.Message.Add($"No paid seats returned");
            }

            if (!baseCurrencies.Contains(expectedBaseCurrency))
            {
                result.Message.Add($"Base currency in Seat Map response being {baseCurrencies} is not what was expected being {expectedBaseCurrency}");
            }

            return result.IsValid;
        }
    }
}