using MediatR;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.WcfServiceMessaging;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Services.Interfaces;
using VAA.EAPI.Infrastructure.Extentions;

namespace VAA.EAPI.Microservices.Retail.Passenger
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
                    worker.UseWhen<AuthenticationMiddleware>(x => !Debugger.IsAttached);
                })
                .ConfigureOpenApi()
                .ConfigureServices(services =>
                {
                    services.AddDefaultServices();
                    services.AddAutoMapper(Assembly.GetExecutingAssembly());
                    services.AddSingleton<ICacheConfig, CacheConfig>();
                    services.AddSingleton(typeof(IAir4PassengerV4), typeof(Air4PassengerV4));
                    services.AddSingleton(s => new PassengerClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.DeltaPassengerV4EndPoint)))));
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
                })
                .Build();

            return host.RunAsync();
        }
    }
}
