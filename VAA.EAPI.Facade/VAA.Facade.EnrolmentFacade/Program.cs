using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using VAA.Facade.EnrolmentFacade.Application.Command;
using VAA.Facade.EnrolmentFacade.Infrastructure.Interfaces;
using VAA.Facade.EnrolmentFacade.Infrastructure.Middleware;
using VAA.Facade.EnrolmentFacade.Infrastructure.Services;

namespace VAA.Facade.EnrolmentFacade
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
                .ConfigureFunctionsWorkerDefaults(builder =>
                {
                    builder.UseMiddleware<AuthenticationMiddleware>();
                })
                .ConfigureServices(services =>
                {
                    services.AddHttpClient();
                    services.AddLogging();
                    services.AddSingleton<IOAuthTokenProvider, OAuthTokenProvider>();
                    services.AddSingleton<IEnrolmentRequestCommandHandler, EnrolmentRequestCommandHandler>();
                    services.AddSingleton<IHttpServiceHandler, HttpServiceHandler>();
                    services.AddSingleton<IPnrLinkServiceHandler, PnrLinkServiceHandler>();
                })
                .Build();

            return host.RunAsync();
        }
    }
}
