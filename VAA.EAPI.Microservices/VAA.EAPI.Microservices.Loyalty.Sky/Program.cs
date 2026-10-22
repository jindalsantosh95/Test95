using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Reflection;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;
using VAA.EAPI.Infrastructure.PSS.Services;
using VAA.EAPI.Microservices.Loyalty.Sky.Application.Command;
using MediatR;
using VAA.EAPI.Microservices.Loyalty.Sky.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Extentions;

namespace VAA.EAPI.Microservices.Loyalty.Sky
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
                    worker.UseWhen<AuthorizationMiddleware>(x => !Debugger.IsAttached);
                })
                .ConfigureOpenApi()
                .ConfigureServices(services =>
                {
                    services.AddDefaultServices();
                    services.AddSingleton(SkyClient.GetSkyRetroCreditClient());
                    services.AddSingleton(typeof(ISkyTeamRetro), typeof(SkyTeamRetro));
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(CreateSkyTeamRetroCommandHandler));
                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
