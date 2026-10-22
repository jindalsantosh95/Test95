using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.CRM.Refunds.Application;
using VAA.EAPI.Microservices.CRM.Refunds.Application.Interfaces;
using VAA.EAPI.Microservices.CRM.Refunds.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.CRM.Refunds.Infrastructure.Services;

namespace VAA.EAPI.Microservices.CRM.Refunds;

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
                services.AddHttpClient();

                services.AddSingleton<IHttpConnector, HttpConnector>();
                services.AddSingleton<ID365RefundGateway, D365RefundGateway>();
                services.AddSingleton<IRefundNotificationPublisher, RefundNotificationPublisher>();
                services.AddSingleton<IRefundStatusCommandHandler, RefundStatusCommandHandler>();
                services.AddSingleton<ISubmitRefundCommandHandler, SubmitRefundCommandHandler>();
                services.AddSingleton(sp => new ServiceBusClient(ConfigReader.Get(Constants.Environment.EMailNotificationServiceBusConnection)));
            })
            .Build();

        return host.RunAsync();
    }
}
