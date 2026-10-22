using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Application.Command;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Services.IFG;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment
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
                    services.AddSingleton<IAuthorizePaymentCommand, AuthorizePaymentCommandHandler>();
                    services.AddSingleton<IOrderPaymentCommand, OrderPaymentCommandHandler>();
                    services.AddSingleton<IPaymentAuthorizeCommand, PaymentAuthorizeCommandHandler>();
                    services.AddSingleton<IPaymentOrderCommand, PaymentOrderCommandHandler>();
                    services.AddSingleton<IOrderExchangePaymentCommand, OrderExchangePaymentCommandHandler>();
                    services.AddSingleton<IOrderRefundCommand, OrderRefundCommandHandler>();
                    services.AddSingleton<IPaymentOrderExchangeCommand, PaymentOrderExchangeCommandHandler>();
                    services.AddSingleton<IPaymentOrderRefundCommand, PaymenyOrderRefundCommandHandler>();
                    services.AddSingleton<IAuthorizeCancelPaymentCommand, AuthorizeCancelPaymentCommandHandler>();
                    services.AddSingleton<IPaymentAuthorizeCancelCommand, PaymentAuthorizeCancelCommandHandler>();
                    services.AddSingleton<IHttpConnector, HttpConnector>();
                })
                .Build();

            return host.RunAsync();
        }
    }
}
