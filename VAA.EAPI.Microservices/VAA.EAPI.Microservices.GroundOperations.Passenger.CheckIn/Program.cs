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
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Interface;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Service;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Infrastructure.Helper.Services;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn
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
                    services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                    services.AddSingleton(typeof(IAir4TokenGenerator), typeof(Air4TokenGenerator));
                    services.AddSingleton<IDeltaSsciService, DeltaSsciService>();
                    services.AddSingleton<IDeltaTrmService, DeltaTrmService>();
                    services.AddSingleton<ICheckInService, CheckInService>();
                    services.AddSingleton<ITravelRequirementsService, TravelRequirementsService>();
                    services.AddStackExchangeRedisCache(options => options.Configuration = ConfigReader.Get(Constants.Environment.Redisconnectionstring));
                    services.AddSingleton(typeof(IDistributedCacheService), typeof(DistributedCacheService));
                })
                .Build();

            return host.RunAsync();
        }
    }
}
