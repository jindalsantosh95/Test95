using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Helper;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Application.Command.StoreAppointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Application.Query.Store;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.Google;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.Store;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.StoreManager;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.Entities.Google;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.Store;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreAppointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment;

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
                services.AddHttpClient();

                services.AddSingleton<IHttpConnector, HttpConnector>();
                services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();
                services.AddSingleton<IServiceBusTopicMessageEventCommandHandler, ServiceBusTopicMessageEventCommandHandler>();
                services.AddSingleton<ITelemetryInitializer, TelemetryHelper>();

                services.AddScoped<IStoreAppointmentRepository, StoreAppointmentRepository>();
                services.AddSingleton<IStoreAppointmentEmailPublisher, StoreAppointmentEmailPublisher>();
                services.AddScoped<IStoreAppointmentHandler, StoreAppointmentHandler>();

                services.AddScoped<IStoreRepository, StoreRepository>();
                services.AddScoped<IGoogleService, GoogleService>();
                services.AddScoped<IStoreHandler, StoreHandler>();
                services.AddScoped<IStoreManagerService, StoreManagerRepository>();

                var connectionString = ConfigReader.Try(Constants.Environment.StoreAppointmentSqlConnectionString, string.Empty);
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    services.AddDbContext<StoreAppointmentDbContext>(options => options.UseSqlServer(connectionString));
                }
                else
                {
                    services.AddDbContext<StoreAppointmentDbContext>(options => options.UseInMemoryDatabase("StoreAppointmentPlaceholder"));
                }
            })
            .Build();

        return host.RunAsync();
    }
}
