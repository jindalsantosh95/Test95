using MediatR;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Trip.Application;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate.interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic.interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.PlusGrade;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.PlusGrade.Interfaces;
using Environment = VAA.EAPI.Microservices.Retail.Trip.Constant.Environment;

namespace VAA.EAPI.Microservices.Retail.Trip
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddCommandLine(args);
                    builder.AddEnvironmentConfiguration();
                })
                .ConfigureFunctionsWorkerDefaults(worker =>
                {
                    worker.AddDefaultMiddleware();
                })
                .ConfigureOpenApi()
                .ConfigureServices(services =>
                {
                    services.AddDefaultServices();
                    services.AddHealthChecks();
                    services.AddSingleton<IDeltaTripsDetailsService, DeltaTripsDetailsService>();
                    services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                    services.AddSingleton(typeof(IDeltaTripsDetailsService), typeof(DeltaTripsDetailsService));
                    services.AddSingleton<ITripService, TripService>();
                    services.AddSingleton<IMessageCentreService, MessageCentreService>();
                    services.AddSingleton<IDeltaMessageDetailsService, DeltaMessageDetailsService>();
                    services.AddSingleton<IPassengerDetailsService, PassengerDetailsService>();
                    services.AddSingleton<IDeltaPassengerDetailsService, DeltaPassengerDetailsService>();
                    services.AddScoped<IUpsellOfferDetailsService, UpsellOfferDetailsService>();
                    services.AddScoped<IDeltaUpsellOfferDetailsService, DeltaUpsellOfferDetailsService>();
                    services.AddScoped<IDeeplinkService, DeeplinkService>();
                    services.AddScoped<IDeltaTripsDetailsPrivateService, DeltaTripsDetailsPrivateService>();
                    services.AddScoped<IDeltaMessageDetailsPrivateService, DeltaMessageDetailsPrivateService>();
                    services.AddScoped<IDeltaUpsellOfferDetailsPrivateService, DeltaUpsellOfferDetailsPrivateService>();
                    services.AddScoped<IDeltaPassengerDetailsPrivateService, DeltaPassengerDetailsPrivateService>();
                    services.AddStackExchangeRedisCache(options => options.Configuration = ConfigReader.Get(Environment.RedisConnectionString));
                    services.AddSingleton(typeof(IDistributedCacheService), typeof(DistributedCacheService));
                    services.AddSingleton(typeof(IAir4TokenGenerator), typeof(Air4TokenGenerator));
                    services.AddScoped<IDeltaEncryptionPrivateService, DeltaEncryptionPrivateService>();
                    services.AddScoped<IDeltaEncryptionService, DeltaEncryptionService>();
                    services.AddScoped(typeof(IBidForUpgradeOffersService), typeof(BidForUpgradeOffersService));
                })
                .Build();

            return host.RunAsync();
        }
    }
}
