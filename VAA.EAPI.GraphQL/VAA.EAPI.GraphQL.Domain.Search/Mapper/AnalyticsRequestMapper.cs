using Newtonsoft.Json;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Infrastructure.Services.Analytics;

namespace VAA.EAPI.GraphQL.Domain.Search.Mapper
{
    public static class AnalyticsRequestMapper
    {
        public static string MapFirstSliceRequest(FlightOfferRequest request)
        {
            if (request == null) return string.Empty;

            var trackRequestValues = new Dictionary<string, object>
            {
                { "slice", "1" }
            };

            if (request.FlightSearchRequest != null)
            {
                GetFlightRequestValues(request.FlightSearchRequest, trackRequestValues);
            }

            if (request.CustomerDetails != null)
            {
                GetCustomerDetails(request.CustomerDetails, trackRequestValues);
            }

            return SerializeAnalyticsRequest("First Slice Request", trackRequestValues);
        }

        public static string MapFirstSliceResponse(Guid basketId, int? totalOffers) =>
            SerializeAnalyticsRequest("First Slice Response", new Dictionary<string, object>
            {
                { "basketId", basketId.ToString() },
                { "totalOffers", totalOffers?.ToString() ?? string.Empty }
            });

        public static string MapNextSliceRequest(FlightOfferNextRequest request)
        {
            if (request == null) return string.Empty;

            var trackRequestValues = new Dictionary<string, object>
            {
                { "basketId", request.BasketId.ToString() }
            };

            if (request.FlightSearchRequest != null)
            {
                trackRequestValues["slice"] = request.FlightSearchRequest.CurrentTripIndexId;
                GetFlightRequestValues(request.FlightSearchRequest, trackRequestValues);
            }

            if (request.CustomerDetails != null)
            {
                GetCustomerDetails(request.CustomerDetails, trackRequestValues);
            }

            return SerializeAnalyticsRequest("Slice Request", trackRequestValues);
        }

        public static string MapNextSliceResponse(Guid basketId) =>
            SerializeAnalyticsRequest("Slice Response", new Dictionary<string, object>
            {
                { "basketId", basketId.ToString() }
            });

        public static string MapSummaryRequest(SearchSummaryRequest request) =>
            request == null ? string.Empty : Json.Serialize(new Dictionary<string, string>
            {
                { "basketId", request.BasketId.ToString() },
                { "summarylink", request.SummaryLink ?? string.Empty }
            });

        public static string MapSummaryResponse(Guid basketId) =>
            Json.Serialize(new Dictionary<string, string>
            {
                { "basketId", basketId.ToString() }
            });

        private static void GetFlightRequestValues(FlightSearchRequest flightSearchRequest, Dictionary<string, object> trackRequestValues)
        {
            var route = flightSearchRequest.SearchOriginDestinations?.FirstOrDefault();
            if (route != null)
            {
                trackRequestValues["origin"] = route.Origin;
                trackRequestValues["destination"] = route.Destination;
                trackRequestValues["flight_date"] = route.DepartureDate?.ToString() ?? string.Empty;
            }

            trackRequestValues["award_search"] = flightSearchRequest.AwardSearch.ToString();
            trackRequestValues["non_stop_only"] = flightSearchRequest.NonStopOnly.ToString();
        }

        private static void GetCustomerDetails(IEnumerable<Customer> customers, Dictionary<string, object> trackRequestValues)
        {
            var counts = customers
                .Select(customer => Enum.TryParse(customer.Ptc, out PtcEnums.PtcTypeEnum ptcType) ? ptcType : (PtcEnums.PtcTypeEnum?)null)
                .Where(ptcType => ptcType.HasValue)
                .GroupBy(ptcType => ptcType.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (PtcEnums.PtcTypeEnum ptc in Enum.GetValues(typeof(PtcEnums.PtcTypeEnum)))
            {
                trackRequestValues[ptc.ToString().ToLower()] = counts.GetValueOrDefault(ptc, 0).ToString();
            }
        }


        private static string SerializeAnalyticsRequest(string subject, Dictionary<string, object> data) => JsonConvert.SerializeObject(new TrackAnalyticsMessage("GraphQL", subject, data));
    }
}
