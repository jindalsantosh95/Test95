using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Application;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health
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
                    services.AddSingleton(typeof(IHealthReportCommand), typeof(HealthReportCommandHandler));
                    services.AddSingleton<IHealthCheckAirShoppingRepository, HealthCheckAirShoppingRepository>();
                    services.AddSingleton<IHealthCheckAirShoppingService, HealthCheckAirShoppingService>();
                    services.AddSingleton<IHealthCheckOfferPriceRepository, HealthCheckOfferPriceRepository>();
                    services.AddSingleton<IHealthCheckOfferPriceService, HealthCheckOfferPriceService>();
                    services.AddSingleton<IHealthCheckOrderCreateRepository, HealthCheckOrderCreateRepository>();
                    services.AddSingleton<IHealthCheckOrderCreateService, HealthCheckOrderCreateService>();
                    services.AddSingleton<IHealthCheckSeatAvailabilityRepository, HealthCheckSeatAvailabilityRepository>();
                    services.AddSingleton<IHealthCheckSeatAvailabilityService, HealthCheckSeatAvailabilityService>();
                    services.AddSingleton<IHealthCheckOrderChangeRepository, HealthCheckOrderChangeRepository>();
                    services.AddSingleton<IHealthCheckOrderChangeService, HealthCheckOrderChangeService>();
                    services.AddSingleton<IHealthCheckOrderHistoryRepository, HealthCheckOrderHistoryRepository>();
                    services.AddSingleton<IHealthCheckOrderHistoryService, HealthCheckOrderHistoryService>();
                    services.AddSingleton<IHealthCheckOrderQuoteRepository, HealthCheckOrderQuoteRepository>();
                    services.AddSingleton<IHealthCheckOrderQuoteService, HealthCheckOrderQuoteService>();
                    services.AddSingleton<IHealthCheckOrderReshopRepository, HealthCheckOrderReshopRepository>();
                    services.AddSingleton<IHealthCheckOrderReshopService, HealthCheckOrderReshopService>();
                    services.AddSingleton<IHealthCheckOrderListRepository, HealthCheckOrderListRepository>();
                    services.AddSingleton<IHealthCheckOrderListService, HealthCheckOrderListService>(); 
                    services.AddSingleton<IHealthCheckOrderRetrieveRepository, HealthCheckOrderRetrieveRepository>();
                    services.AddSingleton<IHealthCheckOrderRetrieveService, HealthCheckOrderRetrieveService>();
                    services.AddSingleton<IHealthCheckOCNRepository, HealthCheckOCNRepository>();
                    services.AddSingleton<IHealthCheckOCNService, HealthCheckOCNService>();
                    services.AddSingleton<IHealthCheckServiceListRepository, HealthCheckServiceListRepository>();
                    services.AddSingleton<IHealthCheckServiceListService, HealthCheckServiceListService>();

                    services.AddDbContext<HealthReportDbContext>(options => options.UseSqlServer(ConfigReader.Get(Constants.Environment.HealthReportSynapseDbConnString)));
                })
                .Build();

            return host.RunAsync();
        }
    }
}
