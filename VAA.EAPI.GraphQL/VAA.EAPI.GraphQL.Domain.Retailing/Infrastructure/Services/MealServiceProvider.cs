using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Interface;
using VAA.EAPI.GraphQL.Domain.Retailing.Models.MealServiceMutaion;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Services
{
    public sealed class MealServiceProvider(IHttpConnector httpConnector) : IMealServiceProvider
    {
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));

        public async Task<MealOperationResponse> AddMeal(AddMealRequest request)
        {
            var headers = new Dictionary<string, string> { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get("MealServiceProvider:OcpKey") } };
                      
            return (await _httpConnector.SendAsync<MealOperationResponse>(GetResourceUrl("/meals/add"), headers, string.Empty, HttpMethod.Post, Json.Serialize(request))).Response;
        }

        public async Task<MealOperationResponse> CancelMeal(CancelMealRequest request)
        {
            var headers = new Dictionary<string, string> { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get("MealServiceProvider:OcpKey") } };

            return (await _httpConnector.SendAsync<MealOperationResponse>(GetResourceUrl("/meals/cancel"), headers, string.Empty, HttpMethod.Post, Json.Serialize(request))).Response;
        }

        private static string GetResourceUrl(string resource) => $"{ConfigReader.Get("MealServiceProvider:FlightServiceMsUri").Trim().TrimEnd('/')}/{resource?.Trim().TrimStart('/')}";
       
    }
}
