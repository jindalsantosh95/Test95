using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Interface;
using VAA.EAPI.GraphQL.Domain.Retailing.Models;
using VAA.EAPI.GraphQL.Domain.Retailing.Models.MealServiceMutaion;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Services
{
    public sealed class CateringProvider(IHttpConnector httpConnector) : ICateringProvider
    {
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));

        public async Task<PNR> IsEligible(PNR bookingInfo)
        {
            var url = $"{ConfigReader.Get("CateringProvider:CateringMsUri").TrimEnd('/')}/ValidateSegmentsMealEligibility";

            var headers = new Dictionary<string, string> { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get("CateringProvider:OcpKey") } };            

            return (await _httpConnector.SendAsync<PNR>(url, headers, string.Empty, HttpMethod.Post, Json.Serialize(bookingInfo))).Response;
        }

        public async Task<IEnumerable<Meal>> GetFlightMeals(DateOnly flightDate, string carrier, int flightNumber, string cabin)
        {          
            var headers = new Dictionary<string, string> { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get("CateringProvider:OcpKey") } };

            var request = new
            {
                FlightDate = flightDate,
                Carrier = carrier,
                FlightNo = flightNumber,
                Cabin = cabin
            };

            var url = $"{ConfigReader.Get("CateringProvider:CateringMsUri").TrimEnd('/')}/GetFlightMeals";

            return (await _httpConnector.SendAsync<IEnumerable<Meal>>(url, headers, string.Empty, HttpMethod.Post, Json.Serialize(request))).Response;
        }

        public async Task<string> AddMealOrder(MealOrders mealOrder)
        {
            var url = $"{ConfigReader.Get("CateringProvider:CateringMsUri").TrimEnd('/')}/CreateMealOrder";

            var headers = new Dictionary<string, string> { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get("CateringProvider:OcpKey") } };

            var response = await _httpConnector.SendAsync(url, headers, string.Empty, HttpMethod.Post, Json.Serialize(mealOrder));

            return response.Content.ReadAsStringAsync().Result;
        }

        public async Task<string> UpdateMealOrder(MealOrders mealOrder)
        {
            var url = $"{ConfigReader.Get("CateringProvider:CateringMsUri").TrimEnd('/')}/UpdateMealOrder";

            var headers = new Dictionary<string, string> { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get("CateringProvider:OcpKey") } };

            var response = await _httpConnector.SendAsync(url, headers, string.Empty, HttpMethod.Post, Json.Serialize(mealOrder));

            return response.Content.ReadAsStringAsync().Result;
        }
    }
}
