using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Search.Models.DeepLinks;
using VAA.EAPI.Automation.Search.Models.GraphQL.RetrieveBasket;
using Variables = VAA.EAPI.Automation.Search.Models.DeepLinks.Variables;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.GraphQL
{
    public class GraphQLRequestBuilder
    {
        private static Random rnd = new Random();

        public static string RetrieveBasketGQLRequest(string basketId)
        {
            BasketGQLRequest retrieveBasketGQLRequest = new()
            {
                pqId = "retrieveBasketDetailsPqId",
                variables = new Models.GraphQL.RetrieveBasket.Variables()
                {
                    basketId = basketId
                }
            };

            return JsonConvert.SerializeObject(retrieveBasketGQLRequest);
        }

        public static string RetrieveBasketWithVoucherDetailsGQLRequest(string basketId)
        {
            JObject retrieveBasketGQLRequest = new JObject
            {
                ["operationName"] = "RetrieveBasket",
                ["query"] = @"
query RetrieveBasket($basketId: String) {
  retrieveBasket(basketId: $basketId) {
    basketId
    summary {
      membershipPoints
      hasSufficientPoints
      voucherPoints
      pointsToPay
      totalPrice
      priceCurrency
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
  }
}",
                ["variables"] = new JObject
                {
                    ["basketId"] = basketId
                }
            };

            return retrieveBasketGQLRequest.ToString(Formatting.None);
        }

        public static string DeepLinkSummaryGQLRequest(HttpResponseMessage summaryResponse, int numberOfadults = 1, int numberOfchildren = 0, int numberOfyouths = 0, int numberOfinfants = 0)
        {
            // Parse the JSON response
            JToken summaryResult = JsonConvert.DeserializeObject<JToken>(summaryResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            // Extract required fields from the response
            var totalAmount = summaryResult.SelectToken("$.data.searchSummary.price.amountIncludingTax")?.ToObject<double>();
            var baseCurrency = summaryResult.SelectToken("$.data.searchSummary.price.currency")?.ToString();
            var flightSegments = summaryResult.SelectTokens("$..flights[*].segments").Select(segment => segment.ToString()).ToArray();

            // Create flight segments list
            var flightSegmentList = CreateFlightSegments(flightSegments);

            // Create customer details
            var customers = new Customers()
            {
                noOfAdults = numberOfadults,
                noOfYoungAdults = numberOfyouths,
                noOfChildren = numberOfchildren,
                noOfInfants = numberOfinfants
            };

            // Create total offered amount
            var totalOfferedAmount = new TotalOfferedAmount
            {
                currency = baseCurrency,
                value = totalAmount ?? 0
            };

            // Create the DeepLinkSummaryGQLRequest object
            var deepLinkSummaryGQLRequest = new DeepLinkSummaryGQLRequest
            {
                pqId = "searchSummaryDeepLink",
                variables = new Variables
                {
                    request = new Request
                    {
                        metaPartnerId = "Skyscanner",
                        flightSegments = flightSegmentList,
                        totalOfferedAmount = totalOfferedAmount,
                        customers = customers,
                        dominantSegmentBrandId = string.Empty,
                        summaryLink = "https://www.virginatlantic.com/flight/summary"
                    }
                }
            };

            return JsonConvert.SerializeObject(deepLinkSummaryGQLRequest);
        }

        private static List<SegmentRequest> CreateFlightSegments(string[] flightSegments)
        {
            var segmentList = new List<SegmentRequest>();
            int sliceIndex = 0; // Initialize slice index

            foreach (var segmentJson in flightSegments)
            {
                if (!string.IsNullOrWhiteSpace(segmentJson))
                {
                    // Parse the JSON into a JArray
                    JArray flightsArray = JArray.Parse(segmentJson);
                    var segment = CreateFlightSegment(flightsArray, sliceIndex++);
                    segmentList.Add(segment);
                }
            }

            return segmentList;
        }

        private static SegmentRequest CreateFlightSegment(JArray flightsArray, int sliceNumber)
        {
            var segmentRequest = new SegmentRequest();

            // Iterate over the array of flights
            foreach (var flight in flightsArray)
            {
                // Extract flight segment details
                var operatingFlightNumber = flight["operatingFlightNumber"]?.ToString();
                var originAirportCode = flight["origin"]?["code"]?.ToString();
                var destinationAirportCode = flight["destination"]?["code"]?.ToString();
                var departureValue = flight["departure"]?.ToString();
                string scheduledDepartureTime = DateTime.TryParse(flight["departure"]?.ToString(), out DateTime departureDateTime) ? departureDateTime.ToString("yyyy-MM-ddTHH:mm:ss") : string.Empty;
                var fareBasisCode = flight["fareBasisCode"]?.ToString();
                var bookingClass = flight["bookingClass"]?.ToString();

                // Split operatingFlightNumber into letters and digits
                string carrierCode = new string(operatingFlightNumber.TakeWhile(char.IsLetter).ToArray());
                string flightNumber = new string(operatingFlightNumber.SkipWhile(char.IsLetter).ToArray());

                segmentRequest.sliceNumber = sliceNumber;
                segmentRequest.marketingCarrierCode = carrierCode;
                segmentRequest.marketingFlightNumber = flightNumber;
                segmentRequest.originAirportCode = originAirportCode;
                segmentRequest.destinationAirportCode = destinationAirportCode;
                segmentRequest.marketingClassOfService = bookingClass;
                segmentRequest.scheduledDepartureLocalDateTime = scheduledDepartureTime;
                segmentRequest.fareBasisCode = fareBasisCode;
            }

            return segmentRequest;
        }
    }
}
