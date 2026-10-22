using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Services;
using VAA.Facade.WalletFacade.Application.Command;

namespace VAA.Facade.WalletFacade
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var host = new HostBuilder()
               .ConfigureAppConfiguration(configurationBuilder =>
               {
                   configurationBuilder.AddCommandLine(args);
               })
               .ConfigureFunctionsWorkerDefaults(worker =>
               {
                   worker.UseMiddleware<FunctionContextAccessorMiddleware>();
               })
               .ConfigureServices(services =>
               {
                   services.AddDefaultServices();
                   services.AddSingleton<IOAuthTokenProvider, OAuthTokenProvider>();
                   services.AddSingleton<IHttpConnector, HttpConnector>();
                   services.AddMediatR(config =>
                   {
                       config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                       config.RegisterServicesFromAssemblyContaining(typeof(RegisterPassCommand));

                   });
               })
               .Build();

            return host.RunAsync();
        }
    }
}
