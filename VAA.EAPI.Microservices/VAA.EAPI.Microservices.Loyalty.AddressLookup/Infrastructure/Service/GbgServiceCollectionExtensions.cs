using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Interface;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.V3;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Service
{
    public static class GbgServiceCollectionExtensions
    {
        public static IServiceCollection AddGbgServices(this IServiceCollection services)
        {
            services.AddHttpClient();
            // Both gateways use IHttpConnector, not an injected typed HttpClient.
            services.AddSingleton<IGbgService, GbgService>();
            services.AddSingleton<IGbgV3Service, GbgV3Service>();
            return services;
        }
    }
}
