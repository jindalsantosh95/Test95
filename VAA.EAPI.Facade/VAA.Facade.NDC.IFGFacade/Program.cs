using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Services;
using VAA.Facade.NDC.IFGFacade.Application.Command;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Services.OrderDataMS;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Services.PaymentMS;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Services.ReferenceMS;

namespace VAA.Facade.NDC.IFGFacade
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
                    services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                    services.AddSingleton<IAirportDetailsQuery, AirportDetailsQueryHandler>();
                    services.AddSingleton<ICurrencyDetailsQuery, CurrencyDetailsQueryHandler>();
                    services.AddSingleton<IAuthorizeCommand, AuthorizeCommandHandler>();
                    services.AddSingleton<IOrderCommand, OrderCommandHandler>();
                    services.AddSingleton<IRefundCommand, RefundCommandHandler>();
                    services.AddSingleton<IPaymentMSAuthorizeCommand, PaymentMSAuthorizeCommandHandler>();
                    services.AddSingleton<IPaymentMSCaptureCommand, PaymentMSCaptureCommandHandler>();
                    services.AddSingleton<IPaymentMSRefundCommand, PaymentMSRefundCommandHandler>();
                    services.AddSingleton<IOrderDataQuery, OrderDataMSQueryHandler>();
                })
                .Build();

            return host.RunAsync();
        }
    }
}
