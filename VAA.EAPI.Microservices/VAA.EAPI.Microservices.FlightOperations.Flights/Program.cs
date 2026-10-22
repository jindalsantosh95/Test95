using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Interface;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Service;
using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Interface;
using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Service;
using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Interface;
using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Service;

namespace VAA.EAPI.Microservices.FlightOperations.Flights
{
    public sealed class Program
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
                    services.AddSingleton<IFlightsService, FlightsService>();
                    services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
                    services.AddSingleton<ISqlQueryService, SqlQueryService>();
                    services.AddSingleton<ICheckAircraftCategoryService, CheckAircraftCategoryService>();
                    services.AddSingleton<ICheckAircraftCategoryDbService, CheckAircraftCategoryDbService>();
                    services.AddSingleton<IAzureADServiceWrapper, AzureADServiceWrapper>();
                    services.AddSingleton<IFlightOperationsService, FlightOperationsService>();
                    services.AddSingleton<TimeProvider>(TimeProvider.System);
                    services.AddSingleton<IFlightStatusService, FlightStatusService>();
                })
                .Build();

            return host.RunAsync();
        }

        public static void ConfigureWorkerSerialization(WorkerOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            options.Serializer ??= new JsonObjectSerializer();
        }

    }
}
