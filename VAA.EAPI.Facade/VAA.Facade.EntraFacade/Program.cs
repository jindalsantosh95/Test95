using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Services;
using VAA.Facade.EntraFacade.Application.Command;
using VAA.Facade.EntraFacade.Application.Interfaces;


namespace VAA.Facade.EntraFacade
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
                   services.AddSingleton<ICustomAuthExtensionAdaptor, CustomAuthExtensionAdaptor>();
               })
               .Build();

            return host.RunAsync();
        }
    }
}
