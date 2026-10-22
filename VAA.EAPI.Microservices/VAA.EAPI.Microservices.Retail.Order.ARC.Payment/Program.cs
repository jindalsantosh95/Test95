using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Application.Command;
using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment
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
                    services.AddSingleton<IHttpConnector, HttpConnector>();
                    services.AddSingleton<ICheckStatusQuery, CheckStatusQueryHandler>();
                    services.AddSingleton<ITransactionService, TransactionServiceHandler>();
                    services.AddSingleton<ITransactionCommand, TransactionCommandHandler>();
                })
                .Build();

            return host.RunAsync();
        }
    }
}
